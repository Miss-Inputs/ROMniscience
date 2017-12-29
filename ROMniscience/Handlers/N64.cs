﻿/*
 * The MIT License
 *
 * Copyright 2017 Megan Leet (Zowayix).
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	//Info mostly from http://en64.shoutwiki.com/wiki/ROM, but also a whole bunch of various forum posts around
	//the internet which all disagree with each other. I ended up doing a lot of original research by actually
	//reading the ROM headers myself to see what works out
	class N64: Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"z64", "Nintendo 64 ROM"},
			//TODO Support v64/byteswapped n64
			//TODO 64DD (.ndd) once I have that figured out
			};
		public override string name => "Nintendo 64";

		readonly static IDictionary<char, string> N64_REGIONS = new Dictionary<char, string> {
			{'\0', "Homebrew"},
			{'7', "Beta"}, //Used by Starcraft 64 beta, but not any other betas from what I can tell... might not really be valid
			{'A', "Asia"},
			{'B', "Brazil"},
			{'C', "China"},
			{'D', "Germany"},
			{'E', "USA"},
			{'F', "France"},
			{'G', "Gateway 64 NTSC"},
			{'H', "Netherlands"},
			{'I', "Italy"},
			{'J', "Japan"},
			{'K', "Korea"},
			{'L', "Gateway 64 PAL"},
			{'N', "Canada"},
			{'P', "Europe"}, //P for PAL I guess
			{'S', "Spain"},
			{'U', "Australia"},
			{'W', "Scandanavia"},
			{'X', "Europe (X)"},
			{'Y', "Europe (Y)"},
		};

		readonly static IDictionary<char, string> N64_MEDIA_TYPES = new Dictionary<char, string> {
			{'\0', "Homebrew"},
			{'N', "Cartridge"},
			{'C', "Cartridge with 64DD expansion"}, //F-Zero X was the only game that ended up having an expansion, but Pocket Monsters 
			//Stadium and Ocarina of Time use this as well (since they were going to have expansions which more or less ended up being Pokemon Stadium (international) and Majora's Mask)
			{'D', "64DD disk"}, //64DD disk dumps use a different format entirely, but there's a hack of SimCity 64 to make it function as a normal cart/ROM file which uses this
			{'E', "64DD expansion for cartridge"},
			{'Z', "Seta Aleck64 arcade board"}, //While these would usually be MAME romsets, it's possible to extract the file representing the game and it's just a byteswapped N64 rom
			//Some other ones that might not be valid:
			//M: Dragon Sword prototype (not the Aug 25 1999 one, but the one in No-Intro)
			//7: GameShark Pro (region code is also 'p' and the short title has a garbage character so the whole thing is probs junk)
			//1: Starcraft 64 beta
			//X: Tristar 64 BIOS, CD64 BIOS
			//A: Turok 3 Jun 6 2000 beta (the whole game code is ABCD so probably just the developers using a placeholder)
			//Presumably iQue would have a different one as well but I think they haven't even been dumped yet? So who knows

		};

		enum N64ROMFormat {
			Z64, N64, V64, UNKNOWN
		}

		enum N64CICChip {
			//Apparently they're actually called this, though only ShoutWiki makes reference to these names
			//TODO: Looks like you crc32 the entire bootcode to identify the CIC

			//6101, only used by Star Fox 64. Doesn't relocate the entry point, crc32 = 0x6170a4a1
			STARF,
			//7102, only used by Lylat Wars. Possibly works the same way as STARF?
			LYLAT,
			//6102-7101 (Super Mario 64, CD64 BIOS, apparently the standard for NTSC), crc32 = 0x90bb6cb5
			//Doesn't relocate entry point
			MARIO,
			//6103-7103 (Banjo-Kazooie, Paper Mario.. the rest of the chips seemed to be named after games that use them, I wonder if Diddy Kong Racing uses it)
			//Subtracts 0x0100000 from entry entry point, crc32 = 0x0b050ee0
			DIDDY,
			//6105-7105 (Ocarina of Time, Majora's Mask), crc32 = 0x98bc2c86
			//Doesn't relocate entry point
			ZELDA,
			//6106-7106 (Yoshi's Story, F-Zero X), crc32 = 0xacc8580a
			//Subtracts 0x0200000 from entry point
			YOSHI, 
		}

		static N64ROMFormat detectFormat(byte[] header) {
			if(header[0] == 0x80 && header[1] == 0x37 && header[2] == 0x12 && header[3] == 0x40) {
				return N64ROMFormat.Z64;
			} else if(header[0] == 0x37 && header[1] == 0x80 && header[2] == 0x40 && header[3] == 0x12) {
				return N64ROMFormat.V64;
			} else if(header[0] == 0x40 && header[1] == 0x12 && header[2] == 0x37 && header[3] == 0x80) {
				return N64ROMFormat.N64;
			}
			return N64ROMFormat.UNKNOWN;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Nintendo 64");

			InputStream s = file.stream;
			byte[] header = s.read(4);
			info.addInfo("Detected format", detectFormat(header));
			int clockRate = s.readIntBE(); //0 = default, apparently the low nibble isn't read
			info.addExtraInfo("Clock rate", clockRate);
			int programCounter = s.readIntBE(); //This technically is the entry point but the CIC chip might alter that
			info.addExtraInfo("Entry point", programCounter);
			int release = s.readIntBE();
			info.addExtraInfo("Release address", release); //What the fuck does that even mean
			int crc1 = s.readIntBE(); //TODO: Calculate the checksum, see http://n64dev.org/n64crc.html... this is gonna be hell
			int crc2 = s.readIntBE();
			info.addExtraInfo("CRC1", crc1);
			info.addExtraInfo("CRC2", crc2);
			byte[] unknown = s.read(8); //Should be 0 filled, console probably doesn't read it though
			info.addExtraInfo("Unknown", unknown);

			string name;
			try {
				//Yes it's Shift-JIS and if you say it's ASCII I will smack you on the head with a copy of Mario no Photopi
				name = s.read(20, Encoding.GetEncoding("shift_jis")).Trim('\0');
			} catch (ArgumentException ae) {
				//Bugger (why isn't this natively supported by .NET anyway?)
				System.Diagnostics.Trace.TraceWarning(ae.Message);
				name = s.read(20, Encoding.ASCII).Trim('\0');
			}
			info.addInfo("Internal name", name);

			byte[] unknown2 = s.read(4);
			info.addExtraInfo("Unknown 2", unknown2);
			byte[] unknown3 = s.read(3);
			info.addExtraInfo("Unknown 3", unknown3
				);
			//A lot of N64 documentation seems to think the media type (or in the case of
			//n64dev, the manufacturer which is not what these bytes are for) is 4 bytes, but it's
			//just one byte, these are just there, all I know is that Custom Robo's fan
			//translation patch changes this and messes up parsing the header if I think that
			//the media type is 4 bytes
			string gameCode = s.read(4, Encoding.ASCII); //Just alphanumeric but ASCII will do
			info.addInfo("Product code", gameCode);
			char mediaType = gameCode[0];
			info.addInfo("Type", mediaType, N64_MEDIA_TYPES);
			string shortTitle = gameCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char region = gameCode[3];
			info.addInfo("Region", region, N64_REGIONS);
			int version = s.read();
			info.addInfo("Version", version);

			byte[] bootCode = s.read(4032);
			info.addExtraInfo("Boot code", bootCode);
			//Might be a way to detect save type, also number of players and rumble (Project64 shows me
			//the latter two for some ROMs it explicitly says it doesn't have in its database so it knows something
			//I don't know)
		}

	}
}
