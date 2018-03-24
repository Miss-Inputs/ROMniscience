using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class Commodore64 : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			//Whoa! There is a _lot_ of formats here and they're all documented. I love that. Makes all the other communities for consoles and computers look bad.
			//There's even more in https://ist.uwaterloo.ca/~schepers/formats.html but I may be going out of scope, whatever the scope even is... but like... whoa

			{"crt", "Commodore 64 cartridge"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC349
			{"d64", "Commodore 64 disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC327
			{"t64", "Commodore 64 tape image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC314
			{"g64", "Commodore 64 GCR-encoded disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC318
			{"p64", "Commodore 64 NRZI flux pulse disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC321
			{"x64", "Commodore 64 image"}, //Any other type of image but with a 64 byte header http://vice-emu.sourceforge.net/vice_16.html#SEC332
			{"d71", "Commodore 64 1571 disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC333
			{"d81", "Commodore 64 1581 disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC336
			{"d80", "Commodore 64 8050 disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC342
			{"d82", "Commodore 64 8250 disk image"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC345

			{"p00", "Commodore 64 program file"}, //http://vice-emu.sourceforge.net/vice_16.html#SEC348 //TODO: Technically this goes from 00 to 99 (should generate this with a loop I guess), also should include S00 (SEQ) / U00 (USR) / R00 (REL) but I have no idea what those are

			{"rel", "Commodore 64 relative file"}, //https://ist.uwaterloo.ca/~schepers/formats/REL.TXT

			{"d2m", "Commodore 64 CMD FD2000 image"}, //https://ist.uwaterloo.ca/~schepers/formats/D2M-DNP.TXT
			{"dnp", "Commodore 64 CMD Disk Native Partition image"},

			{"prg", "Commodore 64 binary executable"}, //https://ist.uwaterloo.ca/~schepers/formats/BINARY.TXT
			{"bin", "Commodore 64 binary executable without load address"}, //Well, supposedly, although this also appears in No-Intro's list of cartridges so I dunno

			{"tap", "Commodore 64 raw tape image"}, //https://ist.uwaterloo.ca/~schepers/formats/TAP.TXT

			{"nib", "Commodore 64 NIBTOOLS disk image"} //Oof! There's one format which isn't documented, and it's used for No-Intro's Commodore - 64 (PP) datfile, whatever PP means. It's a low-level copy so that's cool I guess, but the only thing I can find about how to read it is this: https://github.com/markusC64/nibtools/tree/upstream which is luckily open source but doesn't specify a license and I'd rather not cause trouble if they'd rather I not use it or reverse engineer it
		};

		public override string name => "Commodore 64";
	}
}
