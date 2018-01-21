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
using System.IO;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class Gameboy : Handler{
		static readonly byte[] GB_NINTENDO_LOGO = {0xce, 0xed, 0x66, 0x66, 0xcc, 0x0d, 0x00, 0x0b, 0x03, 0x73, 0x00, 0x83, 0x00, 0x0c, 0x00, 0x0d,
			0x00, 0x08, 0x11, 0x1f, 0x88, 0x89, 0x00, 0x0e, 0xdc, 0xcc, 0x6e, 0xe6, 0xdd, 0xdd, 0xd9, 0x99,
			0xbb, 0xbb, 0x67, 0x63, 0x6e, 0x0e, 0xec, 0xcc, 0xdd, 0xdc, 0x99, 0x9f, 0xbb, 0xb9, 0x33, 0x3e };

		static readonly IDictionary<int, string> CART_TYPES = new Dictionary<int, string>() {
			{0, "ROM only"},
			{1, "MBC1"},
			{2, "MBC1 + RAM"},
			{3, "MBC1 + RAM + Battery"},
			{5, "MBC2"},
			{6, "MBC2 + Battery"},
			{8, "ROM + RAM"},
			{9, "ROM + RAM + Battery"},
			{0xb, "MMM01"},
			{0xc, "MMM01 + RAM"},
			{0xd, "MMM01 + RAM + Battery"},
			{0xf, "MBC3 + Timer + Battery"},
			{0x10, "MBC3 + Timer + RAM + Battery"},
			{0x11, "MBC3"},
			{0x12, "MBC3 + RAM"},
			{0x13, "MBC3 + RAM + Battery"},
			{0x15, "MBC4" }, //Apparently this doesnt actually exist or something? GiiBiiAdvance and Mooneye authors claim that Pandocs is wrong
			{0x16, "MBC4 + RAM"},
			{0x17, "MBC4 + RAM + Battery"},
			{0x19, "MBC5"},
			{0x1a, "MBC5 + RAM"},
			{0x1b, "MBC5 + RAM + Battery"},
			{0x1c, "MBC5 + Rumble"},
			{0x1d, "MBC5 + Rumble + RAM"},
			{0x1e, "MBC5 + Rumble + RAM + Battery"},
			{0x20, "MBC6"},
			{0x22, "MBC7 + Accelerometer + Rumble + RAM + Battery"},
			{0xfc, "Pocket Camera / Gameboy Camera"},
			{0xfd, "Bandai TAMA5"},
			{0xfe, "HuC3"},
			{0xff, "HuC1 + RAM + Battery"}
		};

		static readonly IDictionary<int, long> ROM_SIZES = new Dictionary<int, long>() {
			{0, 32 * 1024}, //The only one that doesn't need bankswitching, used for simple games like Tetris
			{1, 64 * 1024},
			{2, 128 * 1024},
			{3, 256 * 1024},
			{4, 512 * 1024},
			{5, 1024 * 1024},
			{6, 2 * 1024 * 1024},
			{7, 4 * 1024 * 1024}, //Very rarely used except for some really late titles like Harry Potter
			{8, 8 * 1024 * 1024}, //Even more rarely used, I've only seen the homebrew video player thing use this
			
			//These ones seem to be only used for multicarts
			{0x52, (9 * 1024 * 1024) / 8},
			{0x53, (10 * 1024 * 1024) / 8},
			{0x54, (12 * 1024 * 1024) / 8},

			//There's also a multicart that is actually 16MB but I don't think it says that in the ROM header
		};

		static readonly IDictionary<int, long> RAM_SIZES = new Dictionary<int, long>() {
			{0, 0},
			{1, 2 * 1024},
			{2, 8 * 1024},
			{3, 32 * 1024},
			{4, 128 * 1024},
			{5, 64 * 1024 },

			//There is also 32 used by Sonic 3D Blast 5 (bootleg), and 8 used by
			//Wonderworm Willy (homebrew). Also 255 used by Pro Action Replay, but that fills the
			//entire header with 255 for no reason, so ignore that
		};

		static readonly IDictionary<int, string> CGB_FLAGS = new Dictionary<int, string>() {
			{0, "Normal"},
			{0x80, "GBC enhanced"},
			{0xc0, "GBC only"},
			//Some GB docs also mention 0x42 and 0x44 but they're unused
		};

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"gb", "Nintendo Game Boy ROM"},
			{"gbc", "Nintendo Game Boy Color ROM"},
		};

		public override string name => "Game Boy";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			InputStream f = file.stream;
			long originalPos = f.Position;
			try {
				f.Seek(0x100, SeekOrigin.Begin);

				info.addInfo("Platform", name);
				byte[] startVector = f.read(4);
				info.addExtraInfo("Entry point", startVector);
				byte[] nintendoLogo = f.read(48);
				info.addExtraInfo("Nintendo logo", nintendoLogo);
				info.addInfo("Nintendo logo valid?", GB_NINTENDO_LOGO.SequenceEqual(nintendoLogo));

				//Hoo boy this is gonna be tricky hold my... I don't have a beer right now
				byte[] title = f.read(16);
				//This gets tricky because only early games use the full 16 characters and then
				//at some point the last byte became the CGB flag, and then afterwards 4 characters
				//became the product code leaving only 11 characters for the title and there's not
				//really a 100% accurate heuristic to detect if the game uses 11 or 15 characters
				//Most emulators and whatnot use 11 if the game uses a new licensee code and 15
				//otherwise, but stuff like Pokemon Yellow and Gameboy Camera disprove that theory
				int titleLength = 16;
				//At least we can reliably detect if the game uses a CGB flag or not because the only
				//two valid values aren't valid inside titles
				if(CGB_FLAGS.ContainsKey(title[15])) {
					titleLength = 15;
					info.addInfo("Game Boy Color flag", title[15], CGB_FLAGS);
					//Here's the tricky part... well, we know that any game old enough to not
					//have a CGB flag isn't going to have a product code either, because those are new
					//and also I looked at every single commercially released GB/GBC ROM I have to figure out
					//what works and what doesn't
					//We also know that any game that uses the _old_ licensee code _isn't_ going to have a
					//product code, but a game that uses the new licensee code might have a product code and
					//also might not as previously mentioned
					//We can also see that any game that is exclusive to the Game Boy Color will have a
					//product code - but not necessarily a game that is merely GBC enhanced but GB compatible
					//With that in mind... for now, I'll only use 11 characters + product code if I know for sure it has one
					if(title[15] == 0xc0) {
						titleLength = 11;
						string productCode = Encoding.ASCII.GetString(title, 11, 4);
						info.addInfo("Product code", productCode);
						//No documentation I've found at all knows what the product type means! It looks like it works the same way
						//as GBA, right down to V being the product type for rumble-enabled games and Kirby's Tilt and Tumble
						//using K. How about that?
						char gameType = productCode[0];
						info.addInfo("Type", gameType, GBA.GBA_GAME_TYPES);
						string shortTitle = productCode.Substring(1, 2);
						info.addInfo("Short title", shortTitle);
						char region = productCode[3];
						info.addInfo("Region", region, GBA.GBA_GAME_REGIONS);
					}
				}

				//Now we can add what's left of the title
				info.addInfo("Internal name", Encoding.ASCII.GetString(title, 0, titleLength).TrimEnd('\0', ' '));
	
				string licenseeCode = f.read(2, Encoding.ASCII);
				bool isSGB = f.read() == 3;
				info.addInfo("Super Game Boy Enhanced?", isSGB);
				int cartType = f.read();
				info.addInfo("ROM type", cartType, CART_TYPES);

				int romSize = f.read();
				info.addInfo("ROM size", romSize, ROM_SIZES, ROMInfo.FormatMode.SIZE);

				int ramSize = f.read();
				info.addInfo("Save size", ramSize, RAM_SIZES, ROMInfo.FormatMode.SIZE);

				int destinationCode = f.read();
				info.addInfo("Destination code", destinationCode); //Don't want to call this "Region", it's soorrrta what it is but only sorta. 0 is Japan and anything else is non-Japan basically

				int oldLicensee = f.read();
				if(oldLicensee == 0x33) {
					info.addInfo("Manufacturer", licenseeCode, NintendoCommon.LICENSEE_CODES);
					info.addInfo("Uses new licensee", true);
				} else {
					info.addInfo("Manufacturer", String.Format("{0:X2}", oldLicensee), NintendoCommon.LICENSEE_CODES);
					info.addInfo("Uses new licensee", false);
				}
				int version = f.read();
				info.addInfo("Version", version);
				int checksum = f.read();
				info.addExtraInfo("Checksum", checksum);
				info.addInfo("Checksum valid?", checksum == calcChecksum(f));
			} finally {
				f.Seek(originalPos, SeekOrigin.Begin);
			}
		}

		public int calcChecksum(InputStream f) {
			int x = 0;
			long originalPos = f.Position;
			try {
				f.Seek(0x134, SeekOrigin.Begin);
				while(f.Position <= 0x14c) {
					x = (((x - f.read()) & 0xff) - 1) & 0xff; //TODO Shouldn't this just work with unsigned bytes
				}
			} finally {
				f.Seek(originalPos, SeekOrigin.Begin);
			}
			return x;
		}

	}
}
