/*
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
	//But also https://github.com/LuigiBlood/64dd/wiki/System-Area
	class N64 : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"z64", "Nintendo 64 ROM"},
			{"v64", "Nintendo 64 Doctor 64 ROM"}, //Byteswapped
			{"n64", "Nintendo 64 word swapped ROM"},
			{"ndd", "64DD retail disk"},
			{"ddd", "64DD development disk"},
		};
		public override string name => "Nintendo 64";

		readonly static IDictionary<char, string> COUNTRIES = new Dictionary<char, string> {
			//This could plausibly use the same country codes as everything else... or could it?
			{'\0', "Homebrew"},
			{'A', "Asia"}, //Or is it worldwide?
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
			{'Y', "Europe (Y)"}, //Is this valid?
		};

		readonly static IDictionary<char, string> N64_MEDIA_TYPES = new Dictionary<char, string> {
			{'C', "Cartridge with 64DD expansion"}, //F-Zero X was the only game that ended up having an expansion, but Pocket Monsters 
			//Stadium and Ocarina of Time use this as well (since they were going to have expansions which more or less ended up being Pokemon Stadium (international) and Majora's Mask)
			{'D', "64DD disk"}, //64DD disk dumps use a different format entirely, but there's a hack of SimCity 64 to make it function as a normal cart/ROM file which uses this
			{'E', "64DD expansion for cartridge"},
			{'H', "Homebrew"},
			{'N', "Cartridge"},
			{'Z', "Seta Aleck64 arcade board"}, //While these would usually be MAME romsets, it's possible to extract the file representing the game and it's just a byteswapped N64 rom
			{'\0', "Homebrew"},
			//Some other ones that might not be valid (since product codes only make sense for officially released products):
			//M: Dragon Sword prototype (not the Aug 25 1999 one, but the one in No-Intro)
			//7: GameShark Pro (region code is also 'p' and the short title has a garbage character so the whole thing is probs junk)
			//1: Starcraft 64 beta
			//X: Tristar 64 BIOS, CD64 BIOS
			//A: Turok 3 Jun 6 2000 beta (the whole game code is ABCD so probably just the developers using a placeholder)
			//Presumably iQue would have a different one as well but I think they haven't even been dumped yet? So who knows

		};

		enum N64ROMFormat {
			Z64, N64, V64, UNKNOWN, JAPAN_NDD, USA_NDD
		}

		//CIC chips... should this be an enum? Eh, it'll take me longer to figure out if it should be an enum and how it would work instead of just taking the easy route and making them constants
		const uint CIC_LYLAT = 0x27fdf31; //
		const uint CIC_6101 = 0xfb631223; //Star Fox 64 (PAL equivalent is 7102, which would be CIC_LYLAT? Or have I confused myself somewhere)
		const uint CIC_6102 = 0x57c85244; //PAL equivalent: 7101 (standard)
		const uint CIC_6103 = 0x497e414b; //PAL equivalent: 7103 (Banjo-Kazooie/Paper Mario)
		const uint CIC_6105 = 0x49f60e96; //PAL equivalent: 7105 (Ocarina of Time)
		const uint CIC_6106 = 0xd5be5580; //PAL equivalent: 7106 (F-Zero X, Yoshi's Story)
		const uint CIC_64DD = 0x3bc19870; //Apparently it's called "5137"? I dunno

		static N64ROMFormat detectFormat(byte[] header) {
			if (header[0] == 0x80 && header[1] == 0x37 && header[2] == 0x12 && header[3] == 0x40) {
				return N64ROMFormat.Z64;
			} else if (header[0] == 0x37 && header[1] == 0x80 && header[2] == 0x40 && header[3] == 0x12) {
				return N64ROMFormat.V64;
			} else if (header[0] == 0x40 && header[1] == 0x12 && header[2] == 0x37 && header[3] == 0x80) {
				return N64ROMFormat.N64;
			} else if (header[0] == 0xe8 && header[1] == 0x48 && header[2] == 0xd3 && header[3] == 0x16) {
				return N64ROMFormat.JAPAN_NDD;
			} else if (header[0] == 0x22 && header[1] == 0x63 && header[2] == 0xee && header[3] == 0x56) {
				//What retail USA disks are out there, though? Even the translations use the Japanese header
				return N64ROMFormat.USA_NDD;
			}
			return N64ROMFormat.UNKNOWN;
		}

		public static void parseN64ROM(WrappedInputStream s, ROMInfo info) {
			int clockRate = s.readIntBE(); //0 = default, apparently the low nibble isn't read
			info.addInfo("Clock rate", clockRate, ROMInfo.FormatMode.HEX, true);
			int programCounter = s.readIntBE(); //This technically is the entry point but the CIC chip might alter that
			info.addInfo("Entry point", programCounter, ROMInfo.FormatMode.HEX, true);
			int release = s.readIntBE();
			info.addInfo("Release address", release, ROMInfo.FormatMode.HEX, true); //What the fuck does that even mean
			uint crc1 = (uint)s.readIntBE();
			uint crc2 = (uint)s.readIntBE();
			info.addInfo("CRC1", crc1, ROMInfo.FormatMode.HEX, true);
			info.addInfo("CRC2", crc2, ROMInfo.FormatMode.HEX, true);
			byte[] unknown = s.read(8); //Should be 0 filled, console probably doesn't read it though
			info.addInfo("Unknown", unknown, true);

			//The N64 does use Shift-JIS for its internal names, and if anyone says it is
			//ASCII I will smack them on the head with a copy of Densha de Go 64
			string name = s.read(20, MainProgram.shiftJIS).TrimEnd('\0');
			info.addInfo("Internal name", name);

			byte[] unknown2 = s.read(4);
			info.addInfo("Unknown 2", unknown2, true);
			byte[] unknown3 = s.read(3);
			info.addInfo("Unknown 3", unknown3, true);

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
			char country = gameCode[3];
			info.addInfo("Country", country, COUNTRIES);
			int version = s.read();
			info.addInfo("Version", version);

			int[] bootCode = new int[1008];
			uint bootCodeChecksum = 0;
			for (var i = 0; i < 1008; ++i) {
				bootCode[i] = s.readIntBE();
				bootCodeChecksum = (uint)(bootCodeChecksum + bootCode[i]) & 0xffffffff;
			}
			//Not sure why I insisted on making bootCode a separate array, but I'm sure I'll remember the reason later... until then, there's no point in adding an info item which just shows up as System.Int32[]
			//info.addInfo("Boot code", bootCode, true);

			switch (bootCodeChecksum) {
				case CIC_LYLAT:
				case CIC_6101: //Star Fox 64 (USA), or at least rev A; for some reason Lylat Wars ends up with a different boot code checksum even though it'd be the same chip except PAL
					info.addInfo("CIC chip", "6101/7102 (Star Fox 64)");
					break;
				case CIC_6102:
					info.addInfo("CIC chip", "6102/7101 (standard, Super Mario 64 etc)");
					break;
				case CIC_6103:
					info.addInfo("CIC chip", "6103/7103 (Banjo-Kazooie, Paper Mario etc)");
					break;
				case CIC_6105:
					info.addInfo("CIC chip", "6105/7105 (Ocarina of Time etc)");
					break;
				case CIC_6106:
					info.addInfo("CIC chip", "6106/7106 (F-Zero X, Yoshi's Story etc)");
					break;
				case CIC_64DD:
					info.addInfo("CIC chip", "5137 (64DD cartridge conversion)");
					break;
				default:
					info.addInfo("CIC chip", String.Format("Unknown {0:X}", bootCodeChecksum));
					break;

					//Others:
					//64DD modem: D1055850 (IIRC, this doesn't actually have a CIC chip at all)
					//2C21F6CA in most Aleck64 games hacked to run on retail N64 carts via Everdrive, although Tower & Shaft uses 1950CEA5 and Star Soldier Vanishing Earth Arcade uses AC11F6CA
					//Vivid Dolls ripped from the MAME romset without further modifications: F80BF620
			}

			Tuple<uint, uint> calculatedChecksum = calcChecksum(s, bootCodeChecksum);
			info.addInfo("Calculated CRC1", calculatedChecksum.Item1, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Calculated CRC2", calculatedChecksum.Item2, ROMInfo.FormatMode.HEX, true);
			info.addInfo("CRC1 valid?", calculatedChecksum.Item1 == crc1);
			info.addInfo("CRC2 valid?", calculatedChecksum.Item2 == crc2);

			//Might be a way to detect save type (probably not)
		}

		public static Tuple<uint, uint> calcChecksum(WrappedInputStream s, uint cicType) {
			//What the fuck
			uint seed;
			switch (cicType) {
				case CIC_6106:
					seed = 0x1fea617a;
					break;
				case CIC_6105:
					seed = 0xdf26f436;
					break;
				case CIC_6103:
					seed = 0xa3886759;
					break;
				default:
					seed = 0xf8ca4ddc;
					break;
			}

			uint[] t = new uint[6];
			t[0] = t[1] = t[2] = t[3] = t[4] = t[5] = seed;

			s.Position = 0x1000;
			//What the fucking dicks
			//Why isn't this even documented properly anywhere other than "use this tool" well how about I don't wanna use that tool
			uint d, r;
			for (int i = 0x1000; i < 0x101000; i += 4) {
				d = (uint)s.readIntBE();
				if ((t[5] + d) < t[5]) {
					t[3]++;
				}
				t[5] += d;
				t[2] ^= d;
				r = rol(d, (int)(d & 0x1f));
				t[4] += r;
				if (t[1] > d) {
					t[1] ^= r;
				} else {
					t[1] ^= t[5] ^ d;
				}

				if (cicType == CIC_6105) {
					//What the arse titty balls
					long pos = s.Position;
					s.Position = 0x750 + (i & 0xff);
					t[0] += (uint)s.readIntBE() ^ d;
					s.Position = pos;
				} else {
					t[0] += t[4] ^ d;
				}

			}

			//What the absolute fuck I'm telling my therapist about this shit
			if (cicType == CIC_6103) {
				return new Tuple<uint, uint>((t[5] ^ t[3]) + t[2], (t[4] ^ t[1]) + t[0]);
			} else if (cicType == CIC_6106) {
				return new Tuple<uint, uint>((t[5] * t[3]) + t[2], (t[4] * t[1]) + t[0]);
			} else {
				return new Tuple<uint, uint>(t[5] ^ t[3] ^ t[2], t[4] ^ t[1] ^ t[0]);
			}
		}

		public static uint rol(uint x, int n) {
			return (x << n) | (x >> (64 - n));
		}

		public static void parse64DDDiskInfo(ROMInfo info, WrappedInputStream s) {
			//The naming of this function is a solid argument for snake_case everywhere
			s.Position = 0x43670; //I don't know why here, but it is

			string gameCode = s.read(4, Encoding.ASCII);
			info.addInfo("Product code", gameCode);
			char mediaType = gameCode[0];
			info.addInfo("Type", mediaType, N64_MEDIA_TYPES);
			string shortTitle = gameCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char country = gameCode[3];
			info.addInfo("Country", country, COUNTRIES);

			int version = s.read();
			info.addInfo("Version", version);

			int diskNumber = s.read();
			info.addInfo("Disk number", diskNumber);

			int usesMFS = s.read();
			info.addInfo("Uses MFS", usesMFS != 0);

			int diskUse = s.read();
			info.addInfo("Disk use", diskUse);

			byte[] factoryLineNumber = s.read(8);
			//In BCD apparently
			info.addInfo("Factory line number", factoryLineNumber);

			byte[] productionTime = s.read(8);
			//BCD, but what format is it in even then? Is it a big ol' 64 bit timestamp?
			info.addInfo("Production time", productionTime);

			string companyCode = s.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", companyCode, NintendoCommon.LICENSEE_CODES);

			string freeArea = s.read(6, MainProgram.shiftJIS);
			info.addInfo("Memo", freeArea);
		}

		static bool isDiskFormat(N64ROMFormat format) {
			return format == N64ROMFormat.JAPAN_NDD || format == N64ROMFormat.USA_NDD;
		}

		static bool isDiskExtension(string ext) {
			if (ext == null) {
				return false;
			}
			return ext.Equals(".ndd") || ext.Equals(".ddd");
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Nintendo 64");

			WrappedInputStream s = file.stream;
			byte[] header = s.read(4);
			N64ROMFormat format = detectFormat(header);
			info.addInfo("Detected format", detectFormat(header));

			if (!isDiskFormat(format) && isDiskExtension(file.extension)) {
				//Some kind of 64DD disk with an unknown format (might be a dev disk or something, or a bad dump with no system area)
				return;
			} else if (isDiskFormat(format)) {
				parse64DDDiskInfo(info, s);
			} else if (format == N64ROMFormat.V64) {
				ByteSwappedInputStream swappedInputStream = new ByteSwappedInputStream(s);
				parseN64ROM(swappedInputStream, info);
			} else {
				parseN64ROM(s, info);
			}
			//Haha I'm sure word swapping will be a lot of fun
		}

	}
}
