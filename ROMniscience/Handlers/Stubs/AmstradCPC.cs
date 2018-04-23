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
	class AmstradCPC : StubHandler {
		//I guess this'll cover GX4000 too
		//TODO: There's info here http://www.cpcwiki.eu/index.php/Disk_image_file_format
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"sna", "Amstrad CPC snapshot"},
			{"dsk", "Amstrad CPC disk image"},
			{"ipf", "Amstrad CPC Interchangable Preservation Format disk image"},
			{"cdt", "Amstrad CPC tape image"},
			{"cpr", "Amstrad CPC cartridge"}, //GX4000 uses this, as does CPC Plus
			{"mfi", "Amstrad CPC MESS Floppy Image"},
			{"mfm", "Amstrad CPC HxCFloppyEmulator floppy image"}
			//Others from MAME: d77, d88, 1dd, dfi, imd; prn for something printer-related, and you can also use .wav for tapes but I don't think I can really add that in
		};

		public override string name => "Amstrad CPC";
	}
}
