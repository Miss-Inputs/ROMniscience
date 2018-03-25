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
	class AtariST : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"st", "Atari ST disk image"}, //Simple copy of the readable area of a disk
			{"msa", "Atari ST Magic Shadow Archiver image"}, //Like ST but compressed
			{"dim", "Atari ST FastCopyPro disk image"}, //Like ST but compressed... differently
			{"stt", "Steem SSE disk image"}, //Supposedly supports copy-protected disks
			{"stx", "Atari ST PASTI disk image"}, //Supposedly supports copy-protected disks
			{"scp", "Atari ST SuperCard Pro disk image"}, //Supposedly is a low-level copy
			{"stg", "Steem SSE ST Gost disk image"},
			{"stw", "Steem SSE ST Write image"},
			{"hfe", "HxC2001 Atari ST disk image"},
			
			//Oof, this format is... interesting. So, No-Intro uses it for the Atari ST datfile, because it's a low-level exact copy and whatnot, but it's also seemingly a very closed format. It seems to be invented by some "Software Preservation Society" organization, which doesn't seem to provide any documentation on the format whatsoever, and all they have is a library with restrictive license terms (no commercial use, which I don't care _that_ much about but the Free Software Definition and the Debian Free Software Guidelines do, so if I were to use said library it might restrict how ROMniscience can be used), seemingly no source available, and all the downloads seem to be broken anyway. Somehow FS-UAE and WinUAE and such use it though, and they're all GPLv2, so I don't know what's even going on there... anyway, point being, I'm not going to touch it, I'm just going to acknowledge it as being a format that Atari ST disks are preserved in
			{"ipf", "Atari ST Interchangable Preservation Format disk image"},
			{"ctr", "Atari ST KyroFlux CT Raw image"}, //Another format made by Software Preservation Society and I'm not even going to bother
		};

		public override string name => "Atari ST";
	}
}
