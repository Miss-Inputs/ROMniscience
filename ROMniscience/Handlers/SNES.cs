/*
 * The MIT License
 *
 * Copyright 2018 Megan Leet (Zowayix).
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
 * 
 * Also this:
 * Copyright 2017 Fabio Priuli, Cowering

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class SNES : Handler {
		//https://web.archive.org/web/20150726095841/http://romhack.wikia.com/wiki/SNES_header 
		//https://web.archive.org/web/20150519154456/http://romhack.wikia.com/wiki/SMC_header
		//https://en.wikibooks.org/wiki/Super_NES_Programming/SNES_memory_map
		//http://patpend.net/technical/snes/sneskart.html
		//http://problemkaputt.de/fullsnes.htm#snescartridgeromheader

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"sfc", "Super Nintendo Entertainment System ROM"},
			{"smc", "SNES ROM with Super Magicom header"},
			{"swc", "SNES ROM with Super Wild Card header"},
			{"fig", "SNES ROM with Pro Fighter header"},
			//These two just seem to use .sfc anyway most of the time, but they're a thing
			{"bs", "SNES Satellaview ROM"},
			{"st", "Sufami Turbo ROM"},
		};

		public override string name => "Super Nintendo Entertainment System";

		public static readonly IDictionary<int, string> ROM_LAYOUTS = new Dictionary<int, string>() {
			{0x20, "LoROM"},
			{0x21, "HiROM"},
			{0x30, "LoROM + FastROM"},
			{0x31, "HiROM + FastROM"},
			{0x32, "ExLoROM"},
			{0x35, "ExHiROM"},
		};

		public static readonly IDictionary<int, string> ROM_TYPES = new Dictionary<int, string>() {
			{0x00, "ROM only"},
			{0x01, "ROM + RAM"},
			{0x02, "ROM + RAM + Battery"},
			{0x03, "DSP-1"},
			{0x04, "DSP-1 + RAM"}, //Apparently never existed
			{0x05, "DSP-1 + RAM + Battery"}, //Also used for SD Gundam GX, which actually uses the DSP-3, and Dungeon Master which uses DSP-2. Maybe it has nothing to do with save RAM at all and it indicates DSP version, or maybe it's not DSP-1 specifically and it's any DSP version...
			{0x12, "ROM + Battery (0x12)"}, //Is this ever used?
			{0x13, "SuperFX"},
			{0x14, "SuperFX (0x14)"}, //Used by Doom... could be GSU-2 maybe? I'd need Yoshi's Island or Winter Gold to know
			{0x15, "SuperFX + Battery"},
			{0x1a, "SuperFX + Battery (0x1A)"},
			{0x25, "OBC-1"},
			{0x32, "SA-1 + Battery (0x32)"},
			{0x34, "SA-1"},
			{0x35, "SA-1 + Battery"},
			{0x43, "S-DD1"},
			{0x45, "S-DD1 + RAM + Battery"},
			{0x55, "ROM + RTC + RAM + Battery"}, //S-RTC used in Daikaijuu Monogatari II
			{0xe3, "ROM + RAM + Gameboy hardware"},
			{0xe5, "Satellaview BS-X BIOS"},
			{0xf3, "CX4"},
			{0xf5, "ST018"},
			{0xf6, "ROM + ST010/ST011"},
			{0xf9, "SPC7110"},
		};

		public static readonly IDictionary<char, string> GAME_TYPES = new Dictionary<char, string> {
			{'A', "Game"},
			{'B', "Nintendo Power downloadable game"},
			{'Z', "Game with expansion cart"},
		};

		public static readonly IDictionary<int, string> REGIONS = new Dictionary<int, string>() {
			{0, "Japan"},
			{1, "USA"},
			{2, "Europe"}, //Also includes Oceania + Asia, for example the Australian versions of the TMNT games use this (they're different because we don't call it Teenage Mutant Hero Turtles), but anyway consistency
			{3, "Sweden + Scandinavia"},
			{4, "Finland"},
			{5, "Denmark"},
			{6, "France"},
			{7, "Netherlands"},
			{8, "Spain"},
			{9, "Germany"}, //Also includes Austria + Switzerland (supposedly)
			{10, "Italy"},
			{11, "Hong Kong + China"}, //Should this just be Hong Kong?
			{12, "Indonesia"},
			{13, "Korea"},
			{15, "Canada"},
			{16, "Brazil"},
			{17, "Australia"},
		};

		private static IDictionary<int, long> generateROMSizeDict() {
			var d = new Dictionary<int, long>();
			for (int i = 0; i < 255; ++i) {
				d.Add(i, (1 << i) * 1024);
			}
			return d;
		}
		public static readonly IDictionary<int, long> ROM_RAM_SIZES = generateROMSizeDict();

		static readonly IDictionary<int, int> SATELLAVIEW_BOOTS_LEFT = new Dictionary<int, int>() {
			{0xfc, 5},
			{0xbc, 4},
			{0x9c, 3},
			{0x8c, 2},
			{0x84, 1},
			{0x80, 0},
		};

		static readonly IDictionary<int, string> SATELLAVIEW_EXECUTION_AREAS = new Dictionary<int, string>() {
			{0, "Flash"},
			{1, "Copy to PSRAM"},
		};

		int scoreHeader(WrappedInputStream s, long offset) {
			//Well, this is a fun one. It's adapted from MAME devices/bus/snes/snes_slot.cpp (snes_validate_infoblock(), to be precise), which doesn't have much license
			//information, except for this:
			// license:BSD-3-Clause
			// copyright-holders:Fabio Priuli,Cowering
			//So that's why I put that BSD header up there, and I hope that satisfies everything license wise, and that no lawyers will hunt me down and kill me
			//Anyway, there's basically no other way to do this. It seems to just be what every single SNES emulator does
			//I don't even know what the heck is going on down here
			
			if(s.Length < offset) {
				//Well I added this check here at least, because if the ROM isn't even that big then
				//there's a good chance that offset isn't valid
				return 0;
			}

			int score = 0;

			s.Position = offset + 0x3c;
			int resetVector = s.readShortLE();

			s.Position = offset + 0x1c;
			int inverseChecksum = s.readShortLE();
			int checksum = s.readShortLE();

			long resetOpcodeOffset = ((uint)(offset & -0x7fff)) | (ushort)(resetVector & 0x7ffff);
			s.Position = resetOpcodeOffset;
			int resetOpcode = s.read();

			s.Position = offset + 0x15;
			int mapper = s.read() & ~0x10;

			if (resetVector < 0x8000) {
				return 0;
			}

			if ((new int[] { 0x78, 0x18, 0x38, 0x9c, 0x4c, 0x5c }.Contains(resetOpcode))) {
				score += 8;
			}

			if (new int[] { 0xc2, 0xe2, 0xad, 0xae, 0xac, 0xaf, 0xa9, 0xa2, 0xa0, 0x20, 0x22 }.Contains(resetOpcode)) {
				score += 4;
			}

			if (new int[] { 0x40, 0x60, 0x6b, 0xcd, 0xec, 0xcc}.Contains(resetOpcode)) {
				score -= 4;
			}

			if (new int[] { 0x00, 0x02, 0xdb, 0x42, 0xff }.Contains(resetOpcode)) {
				score -= 8;
			}

			//Okay now that we aren't talking opcodes I can at least make sense of this part
			//Here we check that the checksum and inverse checksum add up, because they're meant to do that (although I guess some unlicensed carts might not)
			//Anyway, if you actually wanted to make sense of this function, you'd just look at MAME anyway instead of my amateur explanations
			if((checksum + inverseChecksum) == 0xffff && (checksum != 0) && (inverseChecksum != 0)) {
				score += 4;
			}

			if(offset == 0x7fc0 && (mapper == 0x20 || mapper == 0x22) ){
				score += 2;
			}
			if(offset == 0xffc0 && mapper == 0x21) {
				score += 2;
			}
			if(offset == 0x40ffc0 && mapper == 0x25) {
				score += 2;
			}

			s.Position = offset + 0x16;
			if (s.read() < 8) {
				//Check if ROM type is a normal value
				score++;
			}
			if(s.read() < 16) {
				//Check if ROM size is a normal value (what even is normal anyway the SNES is weird)
				score++;
			}
			if(s.read() < 8) {
				//Check if SRAM size is a normal value
				score++;
			}
			if(s.read() < 14) {
				//Check if region is normal
				score++;
			}
			if(s.read() == 0x33) {
				//Check if extended header is used, because it so commonly is
				score += 2;
			}

			return score < 0 ? 0 : score;
		}

		long findHeaderOffset(WrappedInputStream s) {
			long offset1 = 0x7fc0;
			long offset2 = 0xffc0;
			long offset3 = 0x40ffc0;

			int offset1Score = 0, offset2Score = 0, offset3Score = 0;

			long length = s.Length;
			if (length > offset1) {
				offset1Score = scoreHeader(s, offset1);
			}
			if (length > offset2) {
				offset2Score = scoreHeader(s, offset2);
			}
			if (length > offset3) {
				offset3Score = scoreHeader(s, offset3);
			}

			if(offset3Score > 0) {
				//If it has that much space it's more likely that it is indeed ExHiROM
				offset3Score += 4;
			}

			if((offset1Score >= offset2Score) && (offset1Score >= offset3Score)) {
				return offset1;
			} else if(offset2Score > offset3Score) {
				return offset2;
			} else {
				return offset3;
			}
		}

		public static void parseBSHeader(WrappedInputStream s, ROMInfo info, long offset) {
			s.Position = offset;

			string name = s.read(16, MainProgram.shiftJIS).TrimEnd('\0', ' ');
			info.addInfo("Internal name", name);

			byte[] blockAllocation = s.read(4);
			info.addInfo("Block allocation flags", blockAllocation, true); //TODO (at the moment this confuzzles me)

			int limitedStarts = s.readShortLE();
			if ((limitedStarts & 0x8000) > 0) {
				info.addInfo("Boots left", "Unlimited");
			} else {
				info.addInfo("Boots left", limitedStarts, SATELLAVIEW_BOOTS_LEFT);
			}

			byte[] date = s.read(2);
			int month = (date[0] & 0b11110000) >> 4;
			int day = (date[1] & 0b11111000) >> 3;
			info.addInfo("Month", (month != 0 && month < 13) ? System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(month) : String.Format("Unknown ({0})", month));
			info.addInfo("Day", day);

			int romType = s.read();
			info.addInfo("Mapper", romType, ROM_LAYOUTS);

			int flags = s.read();
			info.addInfo("SoundLink enabled", (flags & 16) == 0);
			int executionArea = (flags & 96) >> 5;
			info.addInfo("Execution area", executionArea);
			info.addInfo("Skip intro", (flags & 128) > 0);

			int fakeLicensee = s.read();
			//Always 0x33

			int version = s.read();
			info.addInfo("Version", version);
			//superfamicom.org says: Version Number is an extension. Actual format is 1 + ord(val($ffdb))/10. (what the heckie)

			int checksum = s.readShortLE();
			int inverseChecksum = s.readShortLE();
			info.addInfo("Checksum", checksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Inverse checksum", inverseChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksums add up?", checksum + inverseChecksum == 0xffff);
			//TODO calculate checksum

			byte[] unknown = s.read(4);
			info.addInfo("Unknown", unknown, true);

			s.Position = offset - 16;
			string licensee = s.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", licensee, NintendoCommon.LICENSEE_CODES);

			//Is there a product code in here? Who knows
			info.addInfo("Unknown 2", s.read(8), true);
		}

		public static void parseSNESHeader(WrappedInputStream s, ROMInfo info, long offset) {
			s.Position = offset;

			//Finally now I can get on with the fun stuff

			//It's not ASCII
			string name = s.read(21, MainProgram.shiftJIS).TrimEnd('\0', ' ');
			info.addInfo("Internal name", name);

			int layout = s.read();
			info.addInfo("Mapper", layout, ROM_LAYOUTS);

			int type = s.read();
			info.addInfo("ROM type", type, ROM_TYPES);

			int romSize = s.read();
			info.addInfo("ROM size", romSize, ROM_RAM_SIZES, ROMInfo.FormatMode.SIZE);

			int ramSize = s.read();
			info.addInfo("Save size", ramSize, ROM_RAM_SIZES, ROMInfo.FormatMode.SIZE);

			int countryCode = s.read();
			
			int licenseeCode = s.read();
			bool usesExtendedHeader = false;
			if (licenseeCode == 0x33) {
				//WHY"D YOU HAVE TO GO AND MAKE EVERYTHING SO COMPLICATED
				usesExtendedHeader = true;
			} else {
				info.addInfo("Manufacturer", licenseeCode.ToString("X2"), NintendoCommon.LICENSEE_CODES);
			}
			info.addInfo("Uses extended header", usesExtendedHeader);

			int version = s.read();
			info.addInfo("Version", version);

			ushort inverseChecksum = (ushort)s.readShortLE();
			info.addInfo("Inverse checksum", inverseChecksum, ROMInfo.FormatMode.HEX, true);
			ushort checksum = (ushort)s.readShortLE();
			info.addInfo("Checksum", checksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksums add up?", checksum + inverseChecksum == 0xffff);
			int calculatedChecksum = calcChecksum(s);
			info.addInfo("Calculated checksum", calculatedChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksum valid?", checksum == calculatedChecksum);

			if (usesExtendedHeader) {
				//Heck you
				s.Position = offset - 0x10;

				string makerCode = s.read(2, Encoding.ASCII);
				info.addInfo("Manufacturer", makerCode, NintendoCommon.LICENSEE_CODES);

				string productCode = s.read(4, Encoding.ASCII);
				info.addInfo("Product code", productCode);

				if (isProductCodeValid(productCode)) {
					info.addInfo("Type", productCode[0], GAME_TYPES);
					info.addInfo("Short title", productCode.Substring(1, 2));
					info.addInfo("Region", productCode[3], NintendoCommon.REGIONS);
				} else {
					if(productCode[2] == ' ' && productCode[3] == ' ') {
						info.addInfo("Short title", productCode.TrimEnd(' '));
					}
					info.addInfo("Region", countryCode, REGIONS);
				}

				byte[] reserved = s.read(6);
				info.addInfo("Reserved", reserved, true);
				int expansionFlashSize = (1 << s.read()) * 1024; //Should this be left as 0 if the raw value is 0?
				info.addInfo("Expansion Flash size", expansionFlashSize, ROMInfo.FormatMode.SIZE);
				int expansionRAMSize = (1 << s.read()) * 1024;
				info.addInfo("Expansion RAM size", expansionRAMSize, ROMInfo.FormatMode.SIZE);
				int specialVersion = s.readShortLE();
				info.addInfo("Special version", specialVersion);
			} else {
				info.addInfo("Region", countryCode, REGIONS);
			}
		}

		public static int calcChecksum(WrappedInputStream s) {
			//Note that this probably requires a bit more nuance, especially for funny games with funny mappers (Star Ocean, etc)
			//but also fails for some perfectly normal games like Earthbound and Home Improvement, unless that's just how they are
			long pos = s.Position;
			long len = s.Length;
			try {
				s.Position = len % 1024; //Avoid starting at weird places for copier headered ROMs
				int checksum = 0;
				while (s.Position < len) {
					checksum = (checksum + s.read()) & 0xffff;
				}
				return checksum;
			} finally {
				s.Position = pos;
			}
		}

		public static bool isProductCodeValid(string code) {
			if(code.Equals("MENU") || code.Equals("XBND")) {
				//These indicate Nintendo Power or X-Band Modem BIOS respectively, and don't contain anything fun
				return false;
			}

			//TODO Return false if it's not alphanumeric

			if(code.TrimEnd(' ').Length != 4) {
				return false;
			}

			return true;
		}

		public static void parseSufamiTurboHeader(ROMInfo info, WrappedInputStream s) {
			//Well this is a lot more straightforward and relaxing
			//Some Sufami Turbo games have an ordinary SNES header, but most of them don't

			s.Position = 0;

			string magic = s.read(14, Encoding.ASCII);
			info.addInfo("Magic", magic, true); //Should be "BANDAI SFC-ADX"

			s.Seek(2, SeekOrigin.Current); //Skip zero-filled padding
			string title = s.read(14, MainProgram.shiftJIS).TrimEnd(' ');
			info.addInfo("Internal name", title);
			s.Seek(2, SeekOrigin.Current);
			byte[] entryPoint = s.read(4);
			info.addInfo("Entry point", entryPoint, ROMInfo.FormatMode.HEX, true);
			s.Position = 0x30; //Skip over all these vectors whatevs
			byte[] gameID = s.read(3);
			info.addInfo("Game ID", gameID);
			int seriesIndex = s.read();
			info.addInfo("Index within series", seriesIndex);
			int romSpeed = s.read();
			info.addInfo("ROM speed", romSpeed == 1 ? "Fast (3.58MHz)" : "Slow (2.68MHz)");
			int features = s.read();
			info.addInfo("Features", features);
			int romSize = s.read() * 128 * 1024;
			info.addInfo("ROM size", romSize, ROMInfo.FormatMode.SIZE);
			int saveSize = s.read() * 2 * 1024;
			info.addInfo("Save size", saveSize, ROMInfo.FormatMode.SIZE);
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);

			WrappedInputStream s = file.stream;
			if (".st".Equals(file.extension)) {
				parseSufamiTurboHeader(info, s);
				return;
			}

			long offset = 0;
			long length = file.length;

			if (length % 1024 == 512) {
				//We have a frickin' copier header

				s.Position = 8;
				int magic1 = s.read();
				int magic2 = s.read();
				int magic3 = s.read();
				if (magic1 == 0xaa && magic2 == 0xbb && magic3 == 4) {
					info.addInfo("Detected format", "Super Wild Card");
					s.Position = 2;

					int swcFlags = s.read();
					info.addInfo("Jump to 0x8000", (swcFlags & 0x80) == 0x80);
					info.addInfo("Split file but not last part", (swcFlags & 0x40) == 0x40);
					if ((swcFlags & 0x30) == 0x30) {
						offset = 0x101c0;
					} else {
						offset = 0x81c0;
					}
					//Everything else should be in the _real_ ROM header anyway
				} else {
					if ("fig".Equals(file.extension)) {
						info.addInfo("Detected format", "Pro Fighter");
						s.Position = 2;
						bool isSplit = s.read() == 0x40;
						bool isHiROM = s.read() == 0x80;
						byte[] dspSettings = s.read(2);

						info.addInfo("Split file but not last part", isSplit);
						info.addInfo("DSP-1 settings", dspSettings);
						if (isHiROM) {
							offset = 0x101c0;
						} else {
							offset = 0x81c0;
						}
					} else {
						//I'll just assume it's SMC until I see anyone use any copier header that isn't SMC, SWC, or FIG
						info.addInfo("Detected format", "Super Magicom");
						s.Position = 2;
						int flags = s.read();
						if ((flags & 0x30) == 0x30) {
							offset = 0x101c0;
						} else {
							offset = 0x81c0;
						}
					}
				}
			} else {
				info.addInfo("Detected format", "Plain");
			}
			if(length < 0x7fc0) {
				//There is no header, since the file is too small to have any of the known header offsets
				//This only really happens for some homebrew stuff
				return;
			}

			if (offset == 0) {
				//If we haven't detected it from a copier header
				offset = findHeaderOffset(s);
			}

			if ("bs".Equals(file.extension)) {
				parseBSHeader(s, info, offset);
			} else {
				parseSNESHeader(s, info, offset);
			}
		}
	}
}
