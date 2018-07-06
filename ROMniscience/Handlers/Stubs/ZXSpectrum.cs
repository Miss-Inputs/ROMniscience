/*
 * The MIT License
 *
 * Copyright 2018 Megan Leet(Zowayix).
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
	class ZXSpectrum : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"dsk", "ZX Spectrum +3 floppy image"},
			{"ipf", "ZX Spectrum +3 Interchangable Preservation Format disk image"},
			{"trd", "ZX Spectrum TR-DOS disk file"},
			{"scl", "ZX Spectrum TR-DOS disk file"},
			{"mdf", "ZX Spectrum Microdrive magnetic tape image"},
			{"udi", "ZX Spectrum Ultra Disk Image"}, //http://scratchpad.wikia.com/wiki/Spectrum_emulator_file_format:_udi
			{"img", "ZX Spectrum G+DOS +D image"},
			{"mgt", "ZX Spectrum G+DOS MasterDOS image"}, //Same as .img but in a different order
			{"fdi", "ZX Spectrum floppy disk image"}, //http://www.worldofspectrum.org/faq/reference/formats.htm
			//TODO: d77, d88, ldd, dfi, imd, mfi, mfm, td0, cqm, cqi

			//Are snapshots really suitable for inclusion? I guess I might as well
			{"sna", "ZX Spectrum Mirage Microdrive snapshot"}, //https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#SNA
			{"snp", "ZX Spectrum snapshot"},
			{"sp", "Spectrum/VGASpec snapshot"}, //https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#SP
			{"z80", "ZX Spectrum snapshot"}, //http://www.worldofspectrum.org/faq/reference/z80format.htm https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#Z80
			{"szx", "ZX Spectrum zx-state snapshot"}, //http://www.spectaculator.com/docs/zx-state/intro.shtml
			{"plusd", "ZX Spectrum +D snapshot"},
			{"zxs", "zx32 snapshot"},
			{"slt", "ZX Spectrum Super Level Loader snapshot"}, //http://www.worldofspectrum.org/faq/reference/formats.htm https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#SLT
			{"ach", "!Speccy snapshot"},
			{"prg", "SpecEm snapshot"},
			{"raw", "ZX Spectrum memory dump"},
			{"sem", "ZX Spectrum-Emulator snapshot"},
			{"sit", "Sinclair snapshot"},
			{"snx", "ZX Spectrum extended Mirage Microdrive snapshot"},
			{"zx", "KGB snapshot"},
			{"zx82", "Speculator 97 snapshot"}, //https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#ZX82
			//TODO: frz

			{"tap", "ZX Spectrum tape image"}, //http://www.zx-modules.de/fileformats/tapformat.html //https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#TAPZ
			{"tzx", "ZX Spectrum raw tape image"},
			{"sta", "Speculator tape image"},
			{"ltp", "Nuclear ZX tape image"},
			{"blk", "ZX Spectrum tape image"}, //Same as .tap
			{"itm", "Intermega tape image"}, //https://web.archive.org/web/20120330020033/http://www.nvg.ntnu.no/sinclair/faq/fileform.html#ITM

			{"rom", "ZX Spectrum Interface II ROM"},
			{"dck", "ZX Spectrum Timex dock ROM"},

			{"hdf", "ZX Spectrum IDE hard disk image"},

			{"scr", "ZX Spectrum screen dump"}, //I swear I've seen games/demos released in this format, although I don't know how that wold even remotely work
			//rzx, air = input recordings, but does that really count as a ROM of any kind? I'm a bit ehhh as it is about the snapshots
		};

		public override string name => "Sinclair ZX Spectrum";
	}
}
