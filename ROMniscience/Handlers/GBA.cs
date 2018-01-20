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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	//Mostly adapted from http://problemkaputt.de/gbatek.htm#gbacartridgeheader
	class GBA: Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"gba","Nintendo Game Boy Advance ROM" },
			{"bin","Nintendo Game Boy Advance ROM" },
			{"srl","Nintendo Game Boy Advance ROM" },
			{"mb","Nintendo Game Boy Advance multiboot ROM" },
		};
		public override string name => "Game Boy Advance";

		public static readonly IDictionary<char, string> GBA_GAME_TYPES = new Dictionary<char, string> {
			{'A', "Normal game (older)"},
			{'B', "Normal game (newer)"},
			{'C', "Normal game (unused)"}, //Why do I get the feeling it's not unused?
			{'F', "Famicom/Classic NES series"},
			{'K', "Acceleration sensor"}, //Yoshi's Universal Gravitation, Koro Koro Puzzle
			{'P', "e-Reader"},
			{'R', "Gyro sensor"}, //WarioWare: Twisted
			{'U', "Solar sensor"}, //Boktai: The Sun is in Your Hands
			{'V', "Rumble"}, //Drill Dozer
			{'M', "GBA Video"}, //Also used by mb2gba and any multiboot roms converted by it
			{'T', "Test cart"}, //AGS Aging Cartridge
			{'Z', "DS expansion"}, //Daigassou! Band-Brothers - Request Selection (it's just a slot 2 device for a DS game, but it has a GBA ROM header surprisingly)
			//Have also seen J for the Pokemon Aurora Ticket distribution cart, and G for GameCube multiboot images (they just use the product code of the GameCube disc they were from usually)
		};

		public static readonly IDictionary<char, string> GBA_GAME_REGIONS = new Dictionary<char, string> {
			{'J', "Japan"},
			{'P', "Europe"},
			{'E', "USA"},
			{'D', "Germany"},
			{'F', "France"},
			{'I', "Italy"},
			{'S', "Spain"},
			{'X', "Europe (X)"},
		};

		public static readonly IDictionary<int, string> GBA_MULTIBOOT_MODES = new Dictionary<int, string> {
			{0, "Not multiboot"},
			{1, "Joybus"},
			{2, "Normal"},
			{3, "Multiplay"},
		};

		int calculateChecksum(InputStream f) {
			long origPos = f.Position;
			try {
				int x = 0;
				f.Seek(0xa0, SeekOrigin.Begin);
				while(f.Position < 0xbc) {
					x = (x - f.read()) & 0xff;
				}
				return (x - 0x19) & 0xff;
			} finally {
				f.Seek(origPos, SeekOrigin.Begin);
			}
		}

		readonly static byte[] SAPPY_SELECTSONG = {
			0x00, 0xB5, 0x00, 0x04, 0x07, 0x4A, 0x08, 0x49,
			0x40, 0x0B, 0x40, 0x18, 0x83, 0x88, 0x59, 0x00,
			0xC9, 0x18, 0x89, 0x00, 0x89, 0x18, 0x0A, 0x68,
			0x01, 0x68, 0x10, 0x1C, 0x00, 0xF0};

		bool attemptDetectSappy(InputStream s) {
			//Adapted from sappy_detector.c from GBAMusRipper by Bregalad and CaptainSwag101
			//FIXME This is way too slow to be usable
			long originalPos = s.Position;
			try {
				s.Seek(0xe4, SeekOrigin.Begin); //We know it's not in the header anywhere so skip over that
				while(s.Position <= s.Length - SAPPY_SELECTSONG.Length) {
					byte[] buf = s.read(SAPPY_SELECTSONG.Length);
					if(SAPPY_SELECTSONG.SequenceEqual(buf)) {
						return true;
					}
					s.Seek(-SAPPY_SELECTSONG.Length + 1, SeekOrigin.Current);
				}


			} finally {
				s.Seek(originalPos, SeekOrigin.Begin);
			}
			return false;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);
			InputStream f = file.stream;

			byte[] entryPoint = f.read(4);
			info.addExtraInfo("Entry point", entryPoint);
			byte[] nintendoLogo = f.read(156);
			info.addExtraInfo("Nintendo logo", nintendoLogo);
			//TODO: Get the proper Nintendo logo that the BIOS validates against, which would be in the BIOS image somewhere
			//TODO: Bits 2 and 7 of nintendoLogo[0x99] enable debugging functions when set (undefined instruction exceptions are sent
			//to a user handler identified using the device type)
			//0x9b bits 0 and 1 also have some crap in them but I don't even know

			string title = f.read(12, Encoding.ASCII).Trim('\0');
			info.addInfo("Internal name", title);
			string gameCode = f.read(4, Encoding.ASCII);
			info.addInfo("Product code", gameCode);
			char gameType = gameCode[0];
			info.addInfo("Type", gameType, GBA_GAME_TYPES);
			string shortTitle = gameCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char region = gameCode[3];
			info.addInfo("Region", region, GBA_GAME_REGIONS);

			string makerCode = f.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", makerCode, NintendoHandheldCommon.LICENSEE_CODES);
			int fixedValue = f.read();
			info.addExtraInfo("Fixed value", fixedValue);
			info.addInfo("Fixed value valid?", fixedValue == 0x96);

			//This indicates the required hardware, should be 0 but it's possible that
			//some prototype/beta/multiboot/other weird ROMs have something else
			int mainUnitCode = f.read();
			info.addInfo("Main unit code", mainUnitCode);

			//If bit 7 is set, the debugging handler entry point is at 0x9fe2000 and not 0x9ffc000, normally this will be 0
			int deviceType = f.read();
			info.addInfo("Device type", deviceType);

			byte[] reserved = f.read(7); //Should be all 0
			info.addExtraInfo("Reserved", reserved);
			int version = f.read();
			info.addInfo("Version", version);
			int checksum = f.read();
			info.addExtraInfo("Checksum", checksum);
			info.addInfo("Checksum valid?", checksum == calculateChecksum(f));
			byte[] reserved2 = f.read(2);
			info.addExtraInfo("Reserved 2", reserved2);
			byte[] multibootEntryPoint = f.read(4);
			info.addExtraInfo("Multiboot entry point", multibootEntryPoint);
			int multibootMode = f.read();
			info.addInfo("Multiboot mode", multibootMode, GBA_MULTIBOOT_MODES);
			int multibootSlaveID = f.read();
			info.addInfo("Multiboot slave ID", multibootSlaveID);
			//0xe0 contains a joybus entry point if joybus stuff is set, meh

			//TODO Try and detect save type
			//info.addInfo("Sound driver", attemptDetectSappy(f) ? "Sappy" : "Unknown");
			//TODO Krawall is open source, see if we can detect that
		}
	}
}
