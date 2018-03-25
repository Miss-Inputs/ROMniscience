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

namespace ROMniscience.Handlers.Stubs {
	class Vic20 : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"d64", "Commodore VIC-20 disk image"},
			{"x64", "Commodore VIC-20 image"},
			{"t64", "Commodore VIC-20 tape image"},
			{"prg", "Commodore VIC-20 binary executable"},
			{"c64", "Commodore VIC-20 binary executable"},
			{"tap", "Commodore VIC-20 raw tape image"},
			{"p00", "Commodore VIC-20 program file" }
		};

		public override bool shouldSkipHeader(ROMFile rom) {
			//Yay, annoyance time! There's .prg files as normal, but then there's also .a0 and .60 and etc files that are all used by No-Intro... well, from what I can tell, these are actually just binary executables except there's normally a 2-byte load address at the beginning, so of course people take that off because headers are evil apparently. How those work is that the load address is determined by the file extension (.a0 = load at 0xa000, .60 = load at 0x6000, .a001 = load at 0xa001 apparently), so that means there's theoretically an infinite number of file extensions and aaaa thanks I hate it
			return "prg".Equals(rom.extension) && (rom.length % 256) == 2;
		}

		public override int skipHeaderBytes() {
			return 2;
		}

		public override string name => "Commodore VIC-20";
	}
}
