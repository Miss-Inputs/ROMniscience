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
			{"p00", "Commodore VIC-20 program file"},
			{"crt", "Commodore VIC-20 ROM"},
			{"20", "Commodore VIC-20 ROM (load at 0x2000)"},
			{"40", "Commodore VIC-20 ROM (load at 0x4000)"},
			{"60", "Commodore VIC-20 ROM (load at 0x6000)"},
			{"70", "Commodore VIC-20 ROM (load at 0x7000)"},
			{"a0", "Commodore VIC-20 ROM (load at 0xA000)"},
			{"b0", "Commodore VIC-20 ROM (load at 0xB000)"}, //Luckily, there's just these 6... or at least according to MAME, anyway
		};

		public override bool shouldSkipHeader(ROMFile rom) {
			//Okay kids, so what's happening here is that there's cartridges (crt) and executables (prg), but the thing is they have to be loaded at a specific address. So what happens is that you can have a 2-byte header at the beginning which has the load address in 2-byte little endian, or you can put the load address in the extension and leave the original file untouched (which is what No-Intro does). Hopefully that makes sense because I don't feel like explaining stuff
			//Although, maybe you don't want to rename a .crt that does have this header to something like .a0, which you might do if you skip this header and have a headered cartridge identified as something which doesn't... but ehhhhh. When I make this a not stub handler, I should just detect the presence of the header and whatnot, and that should make things nice and user-friendly
			return ("prg".Equals(rom.extension) || "crt".Equals(rom.extension)) && (rom.length % 256) == 2;
		}

		public override int skipHeaderBytes() {
			return 2;
		}

		public override string name => "Commodore VIC-20";
	}
}
