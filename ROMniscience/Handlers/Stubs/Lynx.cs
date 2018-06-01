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

namespace ROMniscience.Handlers.Stubs {
	class Lynx: StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"lnx", "Atari Lynx ROM"}
		};

		public override string name => "Atari Lynx";

		public override bool shouldSkipHeader(ROMFile rom) {
			WrappedInputStream s = rom.stream;
			long pos = s.Position;
			try {
				//I hope this doesn't result in false positives. Might be worth checking the file size modulo 64 or something clever like that? Not sure if that works
				string magic = s.read(4, Encoding.ASCII);
				return "LYNX".Equals(magic);
			} finally {
				s.Position = pos;
			}
		}

		public override int skipHeaderBytes() {
			return 64;
		}
	}
}
