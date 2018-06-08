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
	class Commodore64 : Handler {
		//http://vice-emu.sourceforge.net/vice_16.html#SEC349
		//http://vice-emu.sourceforge.net/vice_16.html#SEC314
		//http://vice-emu.sourceforge.net/vice_16.html#SEC327
		//http://ist.uwaterloo.ca/~schepers/formats/D64.TXT

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			//Whoa! There is a _lot_ of formats here and they're all documented. I love that. Makes all the other communities for consoles and computers look bad.
			//There's even more in https://ist.uwaterloo.ca/~schepers/formats.html but I may be going out of scope, whatever the scope even is... but like... whoa

			{"crt", "Commodore 64 cartridge"}, 
			{"d64", "Commodore 64 disk image"}, 
			{"t64", "Commodore 64 tape image"}, 
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

		public readonly static IDictionary<short, string> CARTRIDGE_TYPES = new Dictionary<short, string>() {
			{0, "Normal"},
			{1, "Action Replay"},
			{2, "KCS Power Cartridge"},
			{3, "Final Cartridge III"},
			{4, "Simons' Basic"},
			{5, "Ocean type 1"},
			{6, "Expert Cartridge"},
			{7, "Fun Play / Power Play"},
			{8, "Super Games"},
			{9, "Atomic Power"},
			{10, "Epyx Fastload"},
			{11, "Westermann Learning"},
			{12, "Rex Utility"},
			{13, "Final Cartridge I"},
			{14, "Magic Formel"},
			{15, "C64GS / System 3"}, //Designed specifically for the C64GS, and seem to not actually run properly on a normal C64, or at least with MAME?
			{16, "Warp Speed"},
			{17, "Dinamic"},
			{18, "[Super] Zaxxon"},
			{19, "Magic Desk / Domark / HES"},
			{20, "Super Snapshot v5"},
			{21, "Comal-80"},
			{22, "Structured BASIC"},
			{23, "Ross"},
			{24, "Dela EP64"},
			{25, "Dela EP7x8"},
			{26, "Dela EP256"},
			{27, "Rex EP256"},
			{28, "Mikro Assembler"},
			{29, "Final Cartridge Plus"},
			{30, "Action Replay 4"},
			{31, "StarDOS"},
			{32, "EasyFlash"},
			{33, "EasyFlash Xbank"},
			{34, "Capture"},
			{35, "Action Replay 3"},
			{36, "Retro Replay"},
			{37, "MMC64"},
			{38, "MMC Replay"},
			{39, "IDE64"},
			{40, "Super Snapshot v4"},
			{41, "IEEE-488"},
			{42, "Game Killer"},
			{43, "Prophet64"},
			{44, "EXOS"},
			{45, "Freeze Frame"},
			{46, "Freeze machine"},
			{47, "Snapshot64"},
			{48, "Super Explode v5.0"},
			{49, "Magic Voice"},
			{50, "Action Replay 2"},
			{51, "Mach 5"},
			{52, "Diashow-Maker"},
			{53, "Pagefox"},
			{54, "Kingsoft"},
			{55, "Silverrock 128k"},
			{56, "Formel 64"},
			{57, "RGCD"},
			{58, "RR-Net MK3"},
			{59, "EasyCalc"},
			{60, "GMod2"},
		};

		static bool isCCS64CartMagic(byte[] magic) {
			return Encoding.ASCII.GetString(magic.Take(16).ToArray()).Equals("C64 CARTRIDGE   ");
		}

		public static void parseCCS64Cart(ROMInfo info, WrappedInputStream s) {
			s.Position = 0x10;
			int headerLength = s.readIntBE();
			info.addInfo("Header size", headerLength);

			short version = s.readShortBE();
			info.addInfo("Version", version); //Is this actually the kind of version I'm thinking of, or is it more like the header version?

			short cartType = s.readShortBE();
			info.addInfo("Type", cartType, CARTRIDGE_TYPES);
			info.addInfo("Platform", cartType == 15 ? "Commodore 64GS" : "Commodore 64");

			int exromLineStatus = s.read();
			info.addInfo("EXROM line status", exromLineStatus == 0 ? "Active" : "Inactive");

			int gameLineStatus = s.read();
			info.addInfo("Game line status", gameLineStatus == 0 ? "Active" : "Inactive");

			byte[] reserved = s.read(6);
			info.addInfo("Reserved", reserved, true);

			string name = s.read(32, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Internal name", name);
			//TODO Read CHIP packets (not entirely trivial as there can be more than one)
		}

		static bool isT64Magic(byte[] magic) {
			string magicString = Encoding.ASCII.GetString(magic.Take(31).ToArray());
			return magicString.Equals("C64 tape image file".PadRight(31, '\0')) || magicString.Equals("C64S tape image file".PadRight(31, '\0'));
		}

		public static void parseT64(ROMInfo info, WrappedInputStream s) {
			info.addInfo("Platform", "Commodore 64");
			s.Position = 32;

			short version = s.readShortLE();
			info.addInfo("Version", version);

			short dirEntries = s.readShortLE();
			info.addInfo("Number of files", dirEntries);

			short usedEntries = s.readShortLE();
			info.addInfo("Number of used entries", usedEntries);

			short reserved = s.readShortLE();
			info.addInfo("Reserved", reserved, true);

			string name = s.read(24, Encoding.ASCII).TrimEnd(' ');
			info.addInfo("Internal name", name);
		}

		static bool isD64Magic(WrappedInputStream stream) {
			if (stream.Length < 0x16500) {
				return false;
			}
			stream.Position = 0x16500;
			byte[] magic = stream.read(4);

			//Actually, what we're really doing is checking that track and sector of first directory sector is 18 and 1 respectively and that Disk DOS version is 0x41. There's not really any magic here since it's a raw dump
			return magic[0] == 18 && magic[1] == 01 && magic[2] == 0x41 && magic[3] == 00;
		}

		public static void parseBAMArea(ROMInfo info, WrappedInputStream s, long offset) {
			s.Position = offset;

			int firstTrack = s.read();
			int firstSector = s.read();
			info.addInfo("First directory track", firstTrack);
			info.addInfo("First directory sector", firstSector);

			int diskType = s.read();
			info.addInfo("Disk type", diskType);

			int unused = s.read();
			info.addInfo("Unused", unused, true);

			//TODO Individual BAM entries

			s.Position = offset + 0x90;
			//Official documentation says it goes up to 16 bytes, and the rest are 2-byte 0xa0 padding, 2-byte disk ID, 1 byte unused 0xa0, and "2A" for DOS type. Howevevr, that seems to not actually be the case half the time, as people just put whatever they want here, especially cracking groups
			string diskLabel = Encoding.ASCII.GetString(s.read(27).Select(b => b == 0xa0 ? (byte)0x20 : b).ToArray()).TrimEnd(' ');
			info.addInfo("Internal name", diskLabel);

		}

		public static void parseD64(ROMInfo info, WrappedInputStream stream) {
			info.addInfo("Platform", "Commodore 64");
			parseBAMArea(info, stream, 0x16500);
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			byte[] magic = file.stream.read(32);
			if (isCCS64CartMagic(magic)) {
				info.addInfo("Detected format", "CCS64 cartridge");
				parseCCS64Cart(info, file.stream);
			} else if (isT64Magic(magic)) {
				info.addInfo("Detected format", "T64");
				parseT64(info, file.stream);
			} else if (isD64Magic(file.stream)) {
				info.addInfo("Detected format", "D64");
				parseD64(info, file.stream);
			} else {
				info.addInfo("Detected format", "Unknown");
				info.addInfo("Platform", "Commodore 64");
			}
		}
	}
}
