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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class NES: Handler {
		//https://wiki.nesdev.com/w/index.php/INES
		//https://wiki.nesdev.com/w/index.php/NES_2.0
		//TODO: https://wiki.nesdev.com/w/index.php/TNES and http://wiki.nesdev.com/w/index.php/UNIF maybe?

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"nes", "Nintendo Entertainment System ROM"},
			{"fds", "Nintendo Famicom Disk System disk image" },
		};
		public override string name => "Nintendo Entertainment System";

		public static readonly IDictionary<char, string> FDS_TYPES = new Dictionary<char, string> {
			{' ', "Game"},
			{'E', "Promotional game"},
			{'R', "Reduced price"},
		};

		public static void parseiNES(ROMInfo info, WrappedInputStream s) {
			s.Position = 4; //Don't need to read the header magic again

			int prgSize = s.read();
			int chrSize = s.read();
			//0 means it uses CHR RAM instead

			int flags = s.read();
			string mirroring = (flags & 1) == 1 ? "Vertical" : "Horizontal";
			info.addInfo("Has battery", (flags & 2) == 2);
			info.addInfo("Contains trainer", (flags & 4) == 4);
			bool ignoreMirroring = (flags & 8) == 8;
			if(ignoreMirroring) {
				info.addInfo("Four screen VRAM", true);
			} else {
				info.addInfo("Four screen VRAM", false);
				info.addInfo("Mirroring", mirroring);
			}
			int mapperLow = (flags & 0b11110000) >> 4;

			int flags2 = s.read();
			info.addInfo("VS Unisystem", (flags2 & 1) == 1);
			info.addInfo("PlayChoice-10", (flags2 & 2) == 2);
			int mapperHigh = flags2 & 0b11110000;
			if((flags2 & 0x0c) == 0x0c) {
				//This is the fun part
				//FIXME: This basically is guaranteed to be broken but I don't have NES 2.0 stuff to test with
				info.addInfo("Detected format", "NES 2.0");

				int flags3 = s.read();
				info.addInfo("Submapper", flags3 & 0b11110000 >> 4);
				int mapperHi2 = flags3 & 0b00001111;
				int mapper = (mapperHi2 << 8) & mapperHigh & (mapperLow >> 4);
				info.addInfo("Mapper", mapper);

				int flags4 = s.read();
				int prgSizeHi = flags4 & 0b00001111;
				int chrSizeHi = flags4 & 0b11110000;

				info.addInfo("PRG ROM size", ((prgSizeHi << 8) & prgSize) * 16 * 1024, ROMInfo.FormatMode.SIZE);
				info.addInfo("CHR ROM size", ((chrSizeHi << 8) & chrSize) * 8 * 1024, ROMInfo.FormatMode.SIZE);

				//TODO: Bytes 10 to 14. I can't be stuffed and I also don't have any NES 2.0 ROMs so I'm programming all of this blind basically
			} else {
				info.addInfo("Detected format", "iNES");
				info.addInfo("Mapper", mapperHigh | mapperLow);
				info.addInfo("PRG ROM size", prgSize * 16 * 1024, ROMInfo.FormatMode.SIZE);
				info.addInfo("CHR ROM size", chrSize * 8 * 1024, ROMInfo.FormatMode.SIZE);

				int ramSize = s.read() * 8 * 1024;
				info.addInfo("PRG RAM size", ramSize, ROMInfo.FormatMode.SIZE);

				int flags3 = s.read();
				info.addInfo("TV type", (flags3 & 1) == 1 ? "PAL" : "NTSC");
				info.addInfo("Byte 9 reserved", flags3 & 0xfe);

				//Byte 10 isn't actually part of the specification so screw it
				info.addInfo("Reserved", s.read(6));
			}
		}

		static int decodeBCD(int i) {
			int hi = (i & 0xf0) >> 4;
			int lo = i & 0x0f;
			return ((hi * 10) + lo);
		}

		public static void parseFDS(ROMInfo info, WrappedInputStream s) {
			int blockCode = s.read(); //Always 1
			info.addInfo("Block code", blockCode, true);

			string magic = s.read(14, Encoding.ASCII);
			info.addInfo("Magic", magic, true);

			int manufacturer = s.read();
			info.addInfo("Manufacturer", manufacturer.ToString("X2"), NintendoCommon.LICENSEE_CODES);

			string productCode = s.read(3, Encoding.ASCII); //I bet you $5 this is Shift-JIS
			info.addInfo("Product code", productCode);

			char type = s.read(1, Encoding.ASCII)[0];
			info.addInfo("Type", type, FDS_TYPES);

			int version = s.read();
			info.addInfo("Version", version);

			int sideNumber = s.read();
			info.addInfo("Side number", sideNumber + 1);

			int diskNumber = s.read();
			info.addInfo("Disk number", diskNumber + 1);

			int diskType = s.read();
			info.addInfo("Disk type", diskType); //Allegedly 0 = FMC, 1 = FSC (has shutter)

			int unknown = s.read();
			info.addInfo("Unknown", unknown, true); //Could be the colour of disk

			int startupFileNumber = s.read();
			info.addInfo("Startup file", startupFileNumber);

			byte[] unknown2 = s.read(5); //Seemingly always 0xff filled except where everything is 0 filled
			info.addInfo("Unknown 2", unknown2, true);

			byte[] date = s.read(3);
			int year = 1925 + decodeBCD(date[0]); //Because Showa
			int month = decodeBCD(date[1]);
			int day = decodeBCD(date[2]);

			info.addInfo("Year", year); //FDS Zelda has 2011 here, so that's weird; it has 0x86 as the raw byte and maybe it means 1986 there and not Showa 86
			info.addInfo("Month", (month != 0 && month < 13) ? System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(month) : String.Format("Unknown ({0})", month));
			info.addInfo("Day", day);

			int countryCode = s.read();
			info.addInfo("Country code", countryCode, true); //Supposedly; it's always 0x49

			int unknown3 = s.read();
			info.addInfo("Unknown 3", unknown3, true); //Speculated to be a region code

			int unknown4 = s.read();
			info.addInfo("Unknown 4", unknown4, true); //Seemingly always 0

			byte[] unknown5 = s.read(2); //Seemingly always 00 02
			info.addInfo("Unknown 5", unknown5, true); 

			byte[] unknown6 = s.read(5);
			info.addInfo("Unknown 6", unknown6, true);

			byte[] rewrittenDiskDate = s.read(3); //For non-rewritten disks, this is just equal to the other date
			int rewrittenYear = 1925 + decodeBCD(rewrittenDiskDate[0]);
			int rewrittenMonth = decodeBCD(rewrittenDiskDate[1]);
			int rewrittenDay = decodeBCD(rewrittenDiskDate[2]);

			info.addInfo("Rewritten disk year", rewrittenYear);
			info.addInfo("Rewritten disk month", (rewrittenMonth != 0 && rewrittenMonth < 13) ? System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(rewrittenMonth) : String.Format("Unknown ({0})", rewrittenMonth));
			info.addInfo("Rewritten disk day", rewrittenDay);

			int unknown7 = s.read();
			info.addInfo("Unknown 7", unknown7, true);

			int unknown8 = s.read();
			info.addInfo("Unknown 8", unknown8, true); //Seemingly always 0x80

			byte[] diskWriterSerial = s.read(2);
			info.addInfo("Disk Writer serial number", diskWriterSerial);

			int unknown9 = s.read(); //Seemingly always 7
			info.addInfo("Unknown 9", unknown9, true);

			int diskRewriteCount = s.read();
			info.addInfo("Disk rewrite count", diskRewriteCount);

			int actualDiskSide = s.read(); //wut
			info.addInfo("Actual disk side", actualDiskSide);

			int unknown10 = s.read();
			info.addInfo("Unknown 10", unknown10, true);

			int rawPrice = s.read();
			if(diskRewriteCount > 0) {
				//I don't really even know what's going on here at this point
				info.addInfo("Price", String.Format("{0}円", 500 + (100 * rawPrice)));
			} else {
				info.addInfo("Price", "3400円");
				info.addInfo("Includes peripherals", rawPrice == 3);
			}

			//There is a CRC here as well but .fds files don't include it
		}

		byte[] getHeaderMagic (WrappedInputStream s) {
			long pos = s.Position;
			try {
				s.Position = 0;
				return s.read(4);
			} finally {
				s.Position = pos;
			}
		}

		bool isRawFDS(WrappedInputStream s) {
			long pos = s.Position;
			try {
				s.Position = 1;
				return "*NINTENDO-HVC*".Equals(s.read(14, Encoding.ASCII));
			}
			finally {
				s.Position = pos;
			}
		}

		bool isINES(byte[] magic) {
			//Could also be NES 2.0 which works the same way more or less
			//Wii U VC apparently uses 00 as the fourth byte instead of 1A but we still know what you're up to Nintendo
			return magic[0] == 0x4E && magic[1] == 0x45 && magic[2] == 0x53 && (magic[3] == 0x1A || magic[3] == 0x00);
		}

		bool isFwNES(byte[] magic) {
			return magic[0] == 0x46 && magic[1] == 0x44 && magic[2] == 0x53 && magic[3] == 0x1A;
		}

		public override bool shouldSkipHeader(ROMFile rom) {
			byte[] magic = getHeaderMagic(rom.stream);
			return isINES(magic) || isFwNES(magic);
		}

		public override int skipHeaderBytes() {
			return 16;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);

			WrappedInputStream s = file.stream;
			byte[] headerMagic = getHeaderMagic(s);

			if (isINES(headerMagic)) {
				parseiNES(info, s);
			} else if (isFwNES(headerMagic)) {
				info.addInfo("Detected format", "fwNES");

				s.Position = 4;
				info.addInfo("Number of sides", s.read());

				s.Position = 0x10;
				parseFDS(info, s);
			} else if (isRawFDS(s)) {
				info.addInfo("Detected format", "Raw FDS");

				s.Position = 0;
				parseFDS(info, s);
			} else {
				info.addInfo("Detected format", "Unknown");
			}
		}
	}
}
