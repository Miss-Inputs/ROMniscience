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
    class VirtualBoy : Handler {
        public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
            {"vb", "Virtual Boy ROM"}
        };

        public override string name => "Virtual Boy";

        public override void addROMInfo(ROMInfo info, ROMFile file) {
            info.addInfo("Platform", name);
            InputStream s = file.stream;
            s.Seek(-544, System.IO.SeekOrigin.End); //Yeah, this one's a bit weird

            string title = s.read(20, MainProgram.shiftJIS).TrimEnd(' ');
            info.addInfo("Internal name", title);
            byte[] reserved = s.read(5);
            info.addInfo("Reserved", reserved, true);
            string makerCode = s.read(2, Encoding.ASCII);
            info.addInfo("Manufacturer", makerCode, NintendoCommon.LICENSEE_CODES);
            string productCode = s.read(4, Encoding.ASCII);
            info.addInfo("Product code", productCode);
            //I don't know what to do about the game type, since it's all V so far
            info.addInfo("Type", productCode[0], true);
            string shortTitle = productCode.Substring(1, 2);
            info.addInfo("Short title", shortTitle);
            char region = productCode[3];
            info.addInfo("Region", region, NintendoCommon.REGIONS);
            int version = s.read();
            info.addInfo("Version", version);
        }
    }
}
