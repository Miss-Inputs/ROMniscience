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
	class Gameboy : Handler {
		const int GB_NINTENDO_LOGO_CRC32 = 0x46195417;


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
			{"gbx", "Nintendo Game Boy ROM + GBX footer"},
		};

		public override string name => "Game Boy";

		static bool isNintendoLogoEqual(byte[] nintendoLogo) {
			return Datfiles.CRC32.crc32(nintendoLogo) == GB_NINTENDO_LOGO_CRC32;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			InputStream f = file.stream;

			f.Seek(-4, SeekOrigin.End);
			string magic = f.read(4, Encoding.ASCII);
			bool isGBX = false;
			if ("GBX!".Equals(magic)) {
				//See also: http://hhug.me/gbx/1.0
				isGBX = true;
				f.Seek(-16, SeekOrigin.End);
				int footerSize = f.readIntBE();
				int majorVersion = f.readIntBE();
				int minorVersion = f.readIntBE();
				info.addInfo("GBX footer size", footerSize, ROMInfo.FormatMode.SIZE);
				info.addInfo("GBX version", String.Format("{0}.{1}", majorVersion, minorVersion));

				if (majorVersion == 1) {
					f.Seek(-footerSize, SeekOrigin.End);
					string mapperID = f.read(4, Encoding.ASCII).TrimEnd('\0');
					bool hasBattery = f.read() == 1;
					bool hasRumble = f.read() == 1;
					bool hasTimer = f.read() == 1;
					info.addInfo("Mapper", mapperID);
					info.addInfo("Has battery", hasBattery);
					info.addInfo("Has rumble", hasRumble);
					info.addInfo("Has timer", hasTimer);

					int unused = f.read();
					info.addInfo("GBX unused", unused, true);

					int gbxRomSize = f.readIntBE();
					int gbxRamSize = f.readIntBE();
					info.addInfo("ROM size", gbxRomSize, ROMInfo.FormatMode.SIZE);
					info.addInfo("Save size", gbxRamSize, ROMInfo.FormatMode.SIZE);

					byte[] gbxFlags = f.read(32);
					info.addInfo("GBX flags", gbxFlags, true);
				}
			}

			f.Position = 0x100;

			info.addInfo("Platform", name);
			byte[] startVector = f.read(4);
			info.addInfo("Entry point", startVector, true);
			byte[] nintendoLogo = f.read(48);
			info.addInfo("Nintendo logo", nintendoLogo, true);
			info.addInfo("Nintendo logo valid?", isNintendoLogoEqual(nintendoLogo));

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
			if (CGB_FLAGS.ContainsKey(title[15])) {
				titleLength = 15;
				info.addInfo("Is colour", title[15], CGB_FLAGS);
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

				//The other way would be to check if there are extra characters beyond the first null character, because
				//you're not allowed to have a null character in the middle of a title so if I see characters after that, then
				//it's the manufacturer code (which is always in the same place)
				//So if the null character appears inside the 11 bytes, then it definitely ends the string, and then we can
				//just check to see if there's a manufacturer code afterwards
				int lastNullCharIndex = Array.IndexOf(title, 0);
				if (title[15] == 0xc0 || ((lastNullCharIndex != -1 && lastNullCharIndex <= 11) && title[14] != 0)) {
					titleLength = 11;
					string productCode = Encoding.ASCII.GetString(title, 11, 4);
					info.addInfo("Product code", productCode);
					//No documentation I've found at all knows what the product type means! It looks like it works the same way
					//as GBA, right down to V being the product type for rumble-enabled games and Kirby's Tilt and Tumble
					//using K. How about that?
					//Anyway yeah it all works out except for homebrews that stuff up my heuristic and don't really have
					//product codes, and Robopon games which have a H for game type? That probably means something involving their infrared stuff
					char gameType = productCode[0];
					info.addInfo("Type", gameType, GBA.GBA_GAME_TYPES);
					string shortTitle = productCode.Substring(1, 2);
					info.addInfo("Short title", shortTitle);
					char region = productCode[3];
					info.addInfo("Region", region, NintendoCommon.REGIONS);
				}
			}

			//Now we can add what's left of the title
			info.addInfo("Internal name", Encoding.ASCII.GetString(title, 0, titleLength).TrimEnd('\0', ' '));


			string licenseeCode = f.read(2, Encoding.ASCII);
			bool isSGB = f.read() == 3;
			info.addInfo("Super Game Boy Enhanced?", isSGB);

			int cartType = f.read();
			int romSize = f.read();
			int ramSize = f.read();
			if (isGBX) {
				info.addInfo("Original ROM type", cartType, CART_TYPES, true);
				info.addInfo("Original ROM size", romSize, ROM_SIZES, ROMInfo.FormatMode.SIZE, true);
				info.addInfo("Original save size", ramSize, RAM_SIZES, ROMInfo.FormatMode.SIZE, true);
			} else {
				info.addInfo("ROM type", cartType, CART_TYPES);
				info.addInfo("ROM size", romSize, ROM_SIZES, ROMInfo.FormatMode.SIZE);
				info.addInfo("Save size", ramSize, RAM_SIZES, ROMInfo.FormatMode.SIZE);
			}

			int destinationCode = f.read();
			info.addInfo("Destination code", destinationCode); //Don't want to call this "Region", it's soorrrta what it is but only sorta. 0 is Japan and anything else is non-Japan basically

			int oldLicensee = f.read();
			if (oldLicensee == 0x33) {
				info.addInfo("Manufacturer", licenseeCode, NintendoCommon.LICENSEE_CODES);
				info.addInfo("Uses new licensee", true);
			} else {
				info.addInfo("Manufacturer", String.Format("{0:X2}", oldLicensee), NintendoCommon.LICENSEE_CODES);
				info.addInfo("Uses new licensee", false);
			}
			int version = f.read();
			info.addInfo("Version", version);
			int checksum = f.read();
			info.addInfo("Checksum", checksum, true);
			int calculatedChecksum = calcChecksum(f);
			info.addInfo("Calculated checksum", calculatedChecksum, true);
			info.addInfo("Checksum valid?", checksum == calculatedChecksum);


		}

		//public int 

		public int calcChecksum(InputStream f) {
			int x = 0;
			long originalPos = f.Position;
			try {
				f.Position = 0x134;
				while (f.Position <= 0x14c) {
					x = (((x - f.read()) & 0xff) - 1) & 0xff; //TODO Shouldn't this just work with unsigned bytes
				}
			} finally {
				f.Position = originalPos;
			}
			return x;
		}

	}
}
