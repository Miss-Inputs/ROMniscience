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
