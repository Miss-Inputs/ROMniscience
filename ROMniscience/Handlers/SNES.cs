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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class SNES: Handler {
		//https://web.archive.org/web/20150726095841/http://romhack.wikia.com/wiki/SNES_header 
		//https://web.archive.org/web/20150519154456/http://romhack.wikia.com/wiki/SMC_header
		//https://en.wikibooks.org/wiki/Super_NES_Programming/SNES_memory_map
		//http://patpend.net/technical/snes/sneskart.html

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"sfc", "Super Nintendo Entertainment System ROM"},
			{"smc", "SNES ROM with Super Magicom header"},
			{"swc", "SNES ROM with Super Wild Card header"},
			{"fig", "SNES ROM with Pro Fighter header"},
			//These two just seem to use .sfc anyway most of the time, but they're a thing
			{"bs", "SNES Satellaview ROM"},
			{"st", "Sufami Turbo ROM"},
		};

		public override string name => "Super Nintendo Entertainment System";

		public static readonly IDictionary<int, string> ROM_LAYOUTS = new Dictionary<int, string>() {
			{0x20, "LoROM"},
			{0x21, "HiROM"},
			{0x30, "LoROM + FastROM"},
			{0x31, "HiROM + FastROM"},
			{0x32, "ExLoROM"},
			{0x35, "ExHiROM"},
		};

		public static readonly IDictionary<int, string> ROM_TYPES = new Dictionary<int, string>() {
			{0x00, "ROM only"},
			{0x01, "ROM + RAM"},
			{0x02, "ROM + Save RAM"},
			{0x03, "DSP-1"},
			{0x04, "DSP-1 + RAM"},
			{0x05, "DSP-1 + Save RAM"}, //Also used for SD Gundam GX, which actually uses the DSP3. Maybe it has nothing to do with save RAM at all...
			{0x12, "ROM + Save RAM"},
			{0x13, "SuperFX"},
			{0x14, "SuperFX (0x14)"}, //Used by Doom... could be GSU-2 maybe? I'd need Yoshi's Island or Winter Gold to know
			{0x15, "SuperFX + Save RAM"},
			{0x1a, "SuperFX + Save RAM (0x1A)"},
			{0x34, "SA-1"},
			{0x35, "SA-1 (0x35)"}, //Kirby Super Star, Kirby's Dream Land 3, and Super Mario RPG all use this... only this homebrew zoomer thing uses 0x34
			{0xe3, "ROM + RAM + Gameboy hardware"},
			{0xf6, "ROM + ST011"},
		};

		public static readonly IDictionary<int, string> REGIONS = new Dictionary<int, string>() {
			{0, "Japan"},
			{1, "USA"},
			{2, "Europe + Oceania + Asia"},
			{3, "Sweden"},
			{4, "Finland"},
			{5, "Denmark"},
			{6, "France"},
			{7, "Holland"},
			{8, "Spain"},
			{9, "Germany + Austria + Switzerland"},
			{10, "Italy"},
			{11, "Hong Kong + China"},
			{12, "Indonesia"},
			{13, "South Korea"},
		};

		private static IDictionary<int, long> generateROMSizeDict() {
			var d = new Dictionary<int, long>();
			for(int i = 0; i < 255; ++i) {
				d.Add(i, (1 << i) * 1024);
			}
			return d;
		}
		public static readonly IDictionary<int, long> ROM_RAM_SIZES = generateROMSizeDict();

		private static Encoding getTitleEncoding() {
			try {
				//IT"S FUCKING SHIFT JIS WHY DOES EVERY SINGLE __FUCKING__ PIECE OF DOCUMENTATION SAY IT"S ASCII DID YOU EVEN LOOK TO SEE IF ANY SNES GAMES AT ALL USE HALF_WIDTH KANA AND NOT NECESSARILY ASCII CHARACTERS I BET YOU FUCKING DIDN"T BECAUSE GUESS WHAT THERE ARE GAMES THAT DO THAT AND GUESS WHAT KANA CAN"T BE REPRESENTED IN ASCII SO IT"S NOT!!! FUCKING!!! ASCIIII!!!!!!!! YOU JACKASSES
				return Encoding.GetEncoding("shift_jis");
			} catch(ArgumentException ae) {
				//Bugger
				System.Diagnostics.Trace.TraceWarning(ae.Message);
				return Encoding.ASCII;
			}
		}
		private static readonly Encoding titleEncoding = getTitleEncoding();

		public static void parseSNESHeader(InputStream s, ROMInfo info, long offset) {
			s.Seek(offset, SeekOrigin.Begin);

			//Finally now I can get on with the fun stuff
			string name = s.read(21, titleEncoding).TrimEnd('\0', ' ');
			info.addInfo("Internal name", name);

			int layout = s.read();
			info.addInfo("Mapper", layout, ROM_LAYOUTS);

			int type = s.read();
			info.addInfo("Type", type, ROM_TYPES);

			int romSize = s.read();
			info.addInfo("ROM size", romSize, ROM_RAM_SIZES, ROMInfo.FormatMode.SIZE);

			int ramSize = s.read();
			info.addInfo("Save size", ramSize, ROM_RAM_SIZES, ROMInfo.FormatMode.SIZE);

			int countryCode = s.read();
			info.addInfo("Region", countryCode, REGIONS);

			int licenseeCode = s.read();
			bool usesExtendedHeader = false;
			if(licenseeCode == 0x33) {
				//WHY"D YOU HAVE TO GO AND MAKE EVERYTHING SO COMPLICATED
				usesExtendedHeader = true;
			} else {
				info.addInfo("Manufacturer", licenseeCode.ToString("X2"), NintendoCommon.LICENSEE_CODES);
			}
			info.addInfo("Uses extended header", usesExtendedHeader);

			int version = s.read();
			info.addInfo("Version", version);

			//TODO Calculate this stuff and check if valid and whatever
			byte[] checksum = s.read(2);
			info.addExtraInfo("Checksum", checksum);
			byte[] checksumComplement = s.read(2);
			info.addExtraInfo("Checksum complement", checksum);

			if(usesExtendedHeader) {
				//Heck you
				s.Seek(offset - 0x10, SeekOrigin.Begin);

				string makerCode = s.read(2, Encoding.ASCII);
				info.addInfo("Manufacturer", makerCode, NintendoCommon.LICENSEE_CODES);

				string productCode = s.read(4, Encoding.ASCII);
				info.addInfo("Product code", productCode);

				byte[] unknown = s.read(10);
				//It seems to be 0 filled except in bootlegs
				info.addExtraInfo("Unknown", unknown);
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);

			InputStream s = file.stream;
			bool isHeadered = false;

			long offset;
			if(file.length % 1024 != 0) {
				//We have a frickin' header
				//TODO Read these headers, detect which type exactly
				info.addInfo("Detected format", "Headered");
				isHeadered = true;
			} else {
				info.addInfo("Detected format", "Plain");
			}
			//TODO If file size < 0x7fc0 (there are a few 32KB homebrews), don't try and read a header that isn't even there

			s.Seek(isHeadered ? 0xffd5 + 512 : 0xffd5, SeekOrigin.Current);
			if((s.read() & 0x21) == 0x21) {
				//TODO This method of detecting HiROM/LoROM sucks and is not okay and doesn't even work most of the time
				offset = 0xffc0;
			} else {
				offset = 0x7fc0;
			}

			if(isHeadered) {
				offset += 512;
			}

			parseSNESHeader(s, info, offset);
		}
	}
}
