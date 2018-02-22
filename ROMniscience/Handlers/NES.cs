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

		public static void parseiNES(ROMInfo info, InputStream s) {
            s.Position = 4; //Don't need to read the header magic again

			int prgSize = s.read();
			int chrSize = s.read();
			//0 means it uses CHR RAM instead

			int flags = s.read();
			string mirroring = (flags & 1) == 1 ? "Vertical" : "Horizontal";
			info.addInfo("Contains battery", (flags & 2) == 2);
			info.addInfo("Contains trainer", (flags & 4) == 4);
			bool ignoreMirroring = (flags & 8) == 8;
			if(ignoreMirroring) {
				info.addInfo("Four screen VRAM", true);
			} else {
				info.addInfo("Four screen VRAM", false);
				info.addInfo("Mirroring", mirroring);
			}
			int mapperLow = flags & 0b11110000;

			int flags2 = s.read();
			info.addInfo("VS Unisystem", (flags2 & 1) == 1);
			info.addInfo("PlayChoice-10", (flags2 & 2) == 2);
			int mapperHigh = flags & 0b11110000;
			if((flags2 & 0x0c) == 0x0c) {
				//This is the fun part
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
				info.addInfo("Mapper", mapperHigh & (mapperLow >> 4));
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

        byte[] getHeaderMagic (InputStream s) {
            long pos = s.Position;
            try {
                s.Position = 0;
                return s.read(4);
            } finally {
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

			InputStream s = file.stream;
            byte[] headerMagic = getHeaderMagic(s);

			if(isINES(headerMagic)) {
				parseiNES(info, s);
			} else if(isFwNES(headerMagic)) {
				//TODO I'm too lazy at the moment to add the number of sides of disks, which I might as well do at some point
				info.addInfo("Detected format", "fwNES");
			} else {
				info.addInfo("Detected format", "Unknown");
			}
		}
	}
}
