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
            {0x02, "ROM + Save RAM"},
            {0x03, "DSP-1"},
            {0x04, "DSP-1 + RAM"},
            {0x05, "DSP-1 + Save RAM"}, //Also used for SD Gundam GX, which actually uses the DSP3. Maybe it has nothing to do with save RAM at all...
			{0x12, "ROM + Save RAM"},
            {0x13, "SuperFX"},
            {0x14, "SuperFX (0x14)"}, //Used by Doom... could be GSU-2 maybe? I'd need Yoshi's Island or Winter Gold to know
			{0x15, "SuperFX + Save RAM"},
            {0x1a, "SuperFX + Save RAM (0x1A)"},
            {0x34, "SA-1"},
            {0x35, "SA-1 (0x35)"}, //Kirby Super Star, Kirby's Dream Land 3, and Super Mario RPG all use this... only this homebrew zoomer thing uses 0x34
			{0xe3, "ROM + RAM + Gameboy hardware"},
            {0xf6, "ROM + ST011"},
        };

        public static readonly IDictionary<int, string> REGIONS = new Dictionary<int, string>() {
            {0, "Japan"},
            {1, "USA"},
            {2, "Europe + Oceania + Asia"},
            {3, "Sweden"},
            {4, "Finland"},
            {5, "Denmark"},
            {6, "France"},
            {7, "Holland"},
            {8, "Spain"},
            {9, "Germany + Austria + Switzerland"},
            {10, "Italy"},
            {11, "Hong Kong + China"},
            {12, "Indonesia"},
            {13, "South Korea"},
        };

        private static IDictionary<int, long> generateROMSizeDict() {
            var d = new Dictionary<int, long>();
            for (int i = 0; i < 255; ++i) {
                d.Add(i, (1 << i) * 1024);
            }
            return d;
        }
        public static readonly IDictionary<int, long> ROM_RAM_SIZES = generateROMSizeDict();

        int scoreHeader(InputStream s, long offset) {
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

            s.Seek(offset + 0x3c, SeekOrigin.Begin);
            int resetVector = s.readShortLE();

            s.Seek(offset + 0x1c, SeekOrigin.Begin);
            int inverseChecksum = s.readShortLE();
            int checksum = s.readShortLE();

            long resetOpcodeOffset = ((uint)(offset & -0x7fff)) | (ushort)(resetVector & 0x7ffff);
            s.Seek(resetOpcodeOffset, SeekOrigin.Begin);
            int resetOpcode = s.read();

            s.Seek(offset + 0x15, SeekOrigin.Begin);
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

            s.Seek(offset + 0x16, SeekOrigin.Begin);
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

        long findHeaderOffset(InputStream s) {
            long offset1 = 0x7fc0;
            long offset2 = 0xffc0;
            long offset3 = 0x40ffc0;

            int offset1Score = 0, offset2Score = 0, offset3Score = 0;

            if (s.Length > offset1) {
                offset1Score = scoreHeader(s, offset1);
            }
            if (s.Length > offset2) {
                offset2Score = scoreHeader(s, offset2);
            }
            if (s.Length > offset3) {
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

        public static void parseSNESHeader(InputStream s, ROMInfo info, long offset) {
            s.Seek(offset, SeekOrigin.Begin);

            //Finally now I can get on with the fun stuff

            //It's not ASCII
            string name = s.read(21, MainProgram.shiftJIS).TrimEnd('\0', ' ');
            info.addInfo("Internal name", name);

            int layout = s.read();
            info.addInfo("Mapper", layout, ROM_LAYOUTS);

            int type = s.read();
            info.addInfo("Type", type, ROM_TYPES);

            int romSize = s.read();
            info.addInfo("ROM size", romSize, ROM_RAM_SIZES, ROMInfo.FormatMode.SIZE);

            int ramSize = s.read();
            info.addInfo("Save size", ramSize, ROM_RAM_SIZES, ROMInfo.FormatMode.SIZE);

            int countryCode = s.read();
            info.addInfo("Region", countryCode, REGIONS);

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

            //TODO Calculate this stuff and check if valid and whatever
            byte[] checksumComplement = s.read(2);
            info.addExtraInfo("Checksum complement", checksumComplement);
            byte[] checksum = s.read(2);
            info.addExtraInfo("Checksum", checksum);

            if (usesExtendedHeader) {
                //Heck you
                s.Seek(offset - 0x10, SeekOrigin.Begin);

                string makerCode = s.read(2, Encoding.ASCII);
                info.addInfo("Manufacturer", makerCode, NintendoCommon.LICENSEE_CODES);

                string productCode = s.read(4, Encoding.ASCII);
                info.addInfo("Product code", productCode);

                byte[] unknown = s.read(10);
                //It seems to be 0 filled except in bootlegs
                info.addExtraInfo("Unknown", unknown);
            }
        }

        public override void addROMInfo(ROMInfo info, ROMFile file) {
            info.addInfo("Platform", name);

            InputStream s = file.stream;

            long offset = 0;

            if (file.length % 1024 == 512) {
                //We have a frickin' copier header

                s.Seek(8, SeekOrigin.Begin);
                int magic1 = s.read();
                int magic2 = s.read();
                int magic3 = s.read();
                if (magic1 == 0xaa && magic2 == 0xbb && magic3 == 4) {
                    info.addInfo("Detected format", "Super Wild Card");
                    s.Seek(2, SeekOrigin.Begin);

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
                    if (file.extension.Equals(".fig")) {
                        info.addInfo("Detected format", "Pro Fighter");
                        s.Seek(2, SeekOrigin.Begin);
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
                        s.Seek(2, SeekOrigin.Begin);
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
            //TODO If file size < 0x7fc0 (there are a few 32KB homebrews), don't try and read a header that isn't even there

            if (offset == 0) {
                //If we haven't detected it from a copier header
                //s.Seek(0xffd5, SeekOrigin.Current);
                //if ((s.read() & 0x21) == 0x21) {
                //    //TODO This method of detecting HiROM/LoROM sucks and is not okay and doesn't even work most of the time
                //    offset = 0xffc0;
                //} else {
                //    offset = 0x7fc0;
                //}
                offset = findHeaderOffset(s);
            }

            parseSNESHeader(s, info, offset);
        }
    }
}
