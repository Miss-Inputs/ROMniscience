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
	class NeoGeoPocket: Handler {
		//http://www.devrs.com/ngp/files/ngpctech.txt
		//There's also another document that has "DoNotLink" in the URL, so I guess I'd better not link it

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"ngp", "Neo Geo Pocket ROM"},
			{"ngc", "Neo Geo Pocket ROM"}
		};

		public override string name => "Neo Geo Pocket";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			InputStream s = file.stream;

			string copyrightInfo = s.read(28, Encoding.ASCII);
			info.addInfo("Copyright", copyrightInfo, true);
			//For first party games this should say that, and for third party games it should say " LICENSED BY SNK CORPORATION"
			info.addInfo("First party", "COPYRIGHT BY SNK CORPORATION".Equals(copyrightInfo));

			byte[] entryPoint = s.read(4);
			info.addInfo("Entry point", entryPoint, true);

			int gameNumber = s.readShortLE();
			info.addInfo("Product code", gameNumber.ToString("X2"));

			int version = s.read();
			info.addInfo("Version", version);

			bool isColor = s.read() == 0x10;
			info.addInfo("Is colour", isColor);

			string internalName = s.read(12, Encoding.ASCII);
			info.addInfo("Internal name", internalName);

			byte[] reserved = s.read(16);
			info.addInfo("Reserved", reserved); //Should be 0 filled
		}
	}
}
