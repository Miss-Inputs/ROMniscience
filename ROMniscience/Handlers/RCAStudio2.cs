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
	//https://archive.kontek.net/studio2.classicgaming.gamespy.com/cart.htm
	//Holy heck that was hard to find but I did look at the things in a hex editor and I was like YES there is something here
	class RCAStudio2 : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"st2", "RCA Studio II ROM"},
		};

		public override string name => "RCA Studio II";

		public readonly static IDictionary<string, string> AUTHORS = new Dictionary<string, string>() {
			//Yes really
			{"PR", "Paul Robson"},
			{"JD", "John Dondzilla"},
			{"JW", "Joseph Weisbacker"},
			{"AM", "Andrew Modla"},
			{"??", "Someone"},
			{"LR", "Lee Romanow"}
		};

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);
			InputStream s = file.stream;

			string magic = s.read(4, Encoding.ASCII);
			info.addInfo("Magic", magic, true); //Should be "RCA2"

			int blocks = s.read();
			info.addInfo("Number of 256K blocks", blocks, true);
			info.addInfo("ROM size", blocks * 256 * 1024, ROMInfo.FormatMode.SIZE); //The first block is the header, though

			int format = s.read();
			info.addInfo("Header version", format);

			int videoDriver = s.read();
			info.addInfo("Video driver", videoDriver); //Normally 0 for standard Studio 2 driver

			int reserved = s.read();
			info.addInfo("Reserved", reserved, true);

			string author = s.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", author, AUTHORS);

			string dumper = s.read(2, Encoding.ASCII);
			info.addInfo("Dumper", dumper, AUTHORS); //That's just how it works for some reason

			byte[] reserved2 = s.read(4);
			info.addInfo("Reserved 2", reserved2, true);

			string catalogue = s.read(10, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Product code", catalogue);

			byte[] reserved3 = s.read(6);
			info.addInfo("Reserved 3", reserved3, true);

			string title = s.read(32, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Internal name", title);
		}
	}
}
