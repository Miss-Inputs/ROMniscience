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
    //Mostly referenced from 7800header (GPL, included with 7800AsmDevKit)

    class Atari7800 : Handler {
        public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
            {"a78", "Atari 7800 ROM"},
            {"bin", "Atari 7800 ROM"},
            {"rom", "Atari 7800 ROM"},
        };
        public override string name => "Atari 7800";

        public readonly static IDictionary<int, string> CONTROLLER_TYPES = new Dictionary<int, string>() {
            {0, "None"},
            {1, "Joystick"},
            {2, "Light gun"},
            {3, "Paddle"},
            {4, "Trackball"},
        };

        public readonly static IDictionary<int, string> SAVE_TYPES = new Dictionary<int, string>() {
            {0, "None"},
            {1, "High Score Cart"},
            {2, "SaveKey / AtariVOX"},

			//Have also found 255 inside "7800 Dev BIOS"
			//Asteroids has 252 for this and controller 1 type, 3 for TV type, and that's allegedly a good dump
			//Hmm... if I didn't know any better I'd say there's a different ROM format, maybe it's headerless
		};

        public readonly static IDictionary<int, string> TV_TYPES = new Dictionary<int, string>() {
            {0, "NTSC"},
            {1, "PAL"},
        };

        public override bool shouldSkipHeader(ROMFile rom) {
            InputStream s = rom.stream;
            long pos = s.Position;
            try {
                s.Position = 1;
                string atari7800Magic = s.read(16, Encoding.ASCII);
                return "ATARI7800\0\0\0\0\0\0\0".Equals(atari7800Magic);
            } finally {
                s.Position = pos;
            }
        }

        public override int skipHeaderBytes() {
            return 0x80;
        }

        public override void addROMInfo(ROMInfo info, ROMFile file) {
            info.addInfo("Platform", name);
            InputStream s = file.stream;

            int headerVersion = s.read();
            string atari7800Magic = s.read(16, Encoding.ASCII); //Should be "ATARI7800" null padded
            if (!"ATARI7800\0\0\0\0\0\0\0".Equals(atari7800Magic)) {
                info.addInfo("Detected format", "Non-headered");
                return;
            }
            info.addInfo("Detected format", "Headered");
            info.addInfo("Magic", atari7800Magic, true);
            info.addInfo("Header version", headerVersion);


            string title = s.read(32, Encoding.ASCII).TrimEnd('\0', ' ');
            info.addInfo("Internal name", title);

            int romSize = s.readIntBE(); //Excluding this header
            info.addInfo("ROM size", romSize, ROMInfo.FormatMode.SIZE);

            int specialCartType = s.read();
            info.addInfo("Special cart type data", specialCartType);

            int cartType = s.read();
            info.addInfo("Cart type data", cartType);
            //TODO This seems like it would be better suited as a flags enum
            bool pokey = (cartType & 1) > 0;
            info.addInfo("Contains POKEY chip?", pokey);
            bool supercartBankSwitched = (cartType & 2) > 0;
            info.addInfo("Supercart bank switched?", supercartBankSwitched);
            bool supercartRAMat4000 = (cartType & 4) > 0;
            info.addInfo("Supercart RAM at 0x4000?", supercartRAMat4000);
            bool romAt4000 = (cartType & 8) > 0;
            info.addInfo("ROM at 0x4000?", romAt4000);
            bool bank6At4000 = (cartType & 16) > 0;
            info.addInfo("Bank 6 at 0x4000?", bank6At4000);

            int controller1Type = s.read();
            info.addInfo("Controller 1 type", controller1Type, CONTROLLER_TYPES);
            int controller2Type = s.read();
            info.addInfo("Controller 2 type", controller2Type, CONTROLLER_TYPES);

            int tvType = s.read();
            info.addInfo("TV type", tvType, TV_TYPES);
            int saveType = s.read();
            info.addInfo("Save type", saveType, SAVE_TYPES);
            byte[] reserved = s.read(4);
            info.addInfo("Reserved", reserved, true);
            bool expansionModuleRequired = (s.read() & 1) > 0;
            info.addInfo("Expansion module required?", expansionModuleRequired);

            //Then there's a 28 byte string which literally says "ACTUAL CART DATA STARTS HERE" in ASCII, and indeed
            //the actual cart data starts there
        }
    }
}
