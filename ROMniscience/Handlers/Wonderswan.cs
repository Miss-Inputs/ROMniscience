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
using ROMniscience.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
    class Wonderswan : Handler {
        //https://www.zophar.net/fileuploads/2/10805ixdtg/wstech23.txt

        public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
            {"ws" , "Bandai WonderSwan ROM"},
            {"wsc", "Bandai WonderSwan Color ROM"}
        };

        public override string name => "Bandai WonderSwan";

        public static readonly IDictionary<int, long> ROM_SIZES = new Dictionary<int, long> {
            {2, 512 * 1024},
            {3, 1024 * 1024},
            {4, 2 * 1024 * 1024},
            {6, 4 * 1024 * 1024},
            {8, 8 * 1024 * 1024},
            {9, 16 * 1024 * 1024},
        };

        public static readonly IDictionary<int, long> RAM_SIZES = new Dictionary<int, long> {
            {0, 0},
            {1, 8 * 1024},
            {2, 32 * 1024 },
            {3, 128 * 1024},
            {4, 256 * 1024},
            {5, 64 * 1024},
            {0x10, 128},
            {0x20, 2 * 1024},
            {0x50, 1024},
        };

        public override void addROMInfo(ROMInfo info, ROMFile file) {
            info.addInfo("Platform", name);
            InputStream s = file.stream;
            s.Seek(-10, System.IO.SeekOrigin.End);

            int devID = s.read();
            info.addInfo("Developer ID", devID);
            bool isColor = s.read() == 1;
            info.addInfo("Is colour", isColor);
            int cartID = s.read();
            info.addInfo("Cart ID", cartID);
            int unknown = s.read();
            info.addExtraInfo("Unkown", unknown);
            int romSize = s.read();
            info.addInfo("ROM size", romSize, ROM_SIZES, ROMInfo.FormatMode.SIZE);
            int ramSize = s.read();
            info.addInfo("Save size", ramSize, RAM_SIZES, ROMInfo.FormatMode.SIZE);
            info.addInfo("Save type", ramSize >= 10 ? "EEPROM" : ramSize == 0 ? "None" : "SRAM");
            bool isRotated = (s.read() & 1) == 1;
            info.addInfo("Rotated screen?", isRotated);
            bool hasRTC = s.read() == 1;
            info.addInfo("Has RTC", hasRTC);
            //TODO checksum
        }
    }
}
