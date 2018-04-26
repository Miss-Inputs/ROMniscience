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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	abstract class CDBasedSystem : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			//Only put standalone file types in here, i.e. that don't consist of a TOC and other files; others will be handled by ROMScanner directly.
			//I suppose currently that would prevent you from zipping a cue sheet, but would you really want to do that? Does anyone do that? Who does that?
			{"iso", name + " 2048-byte sector CD image"},
			//TODO: Support .chd files, which maybe we can pretend are archives of .cue and .bin tracks to maintain checksums
			//TODO: .toc .nrg and .cdr are in MAME so they can't be _that_ proprietary, but they're low priority because they're weird. .ccd if it's documented somewhere (okay so it sort of is but it's proprietary and unofficial, see https://www.gnu.org/software/ccd2cue/manual/html_node/CCD-sheet-format.html#CCD-sheet-format); need .gdi for Dreamcast too
		};

		public static readonly IDictionary<string, string> genericCueSheetFiletypeMap = new Dictionary<string, string>() {
			//This will be used for files that are either cuesheets themselves, or non-data tracks that are referred to in cuesheets. Word everything as though it had a thing prefixed to it, so in all lowercase basically
			//Although if you miss something it'll just show up as "<system> <ext> file" for the filetype name
			{"cue", "cue sheet"},
			{"bin", "non-data track"},
			{"mp3", "MP3 audio track"},
			{"ogg", "Ogg Vorbis audio track"},
		};
		
		public override string getFiletypeName(string extension) {
			//This stops data tracks being read as "Unknown". Caveat is that if you override filetypeMap in the actual handler to add something like BIOSes or executables with a .bin extension, you'll get .bin data tracks recognized as a BIOS or executable accordingly
			string b = base.getFiletypeName(extension);
			if(b == null) {
				return name + " data track";
			}
			return b;
		}

		public abstract void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream);

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			//.iso files are 2048 sectors, and since they're read as normal ROM files without any special handling in ROMScanner, we'll specify that sector size here; for cue sheets we've already read that so we just do the thing
			int sectorSize = 2048;
			if (file.cdSectorSize != 0) {
				sectorSize = file.cdSectorSize;
			}

			info.addInfo("Sector size", sectorSize);

			if (sectorSize == 2352) {
				addROMInfo(info, file, new CDInputStream(file.stream));
			} else {
				addROMInfo(info, file, file.stream);
			}

		}

	}
}
