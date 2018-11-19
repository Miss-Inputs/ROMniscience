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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ROMniscience.IO.CueSheets {
	abstract class CueSheet {

		public static readonly IList<string> CUE_EXTENSIONS = new List<string>{
			//I guess we'd put "ccd" in this list once we implement that
			"cue", "gdi"
		};
		public static bool isCueExtension(String extension) {
			if (String.IsNullOrEmpty(extension)) {
				return false;
			}

			if (extension[0] == '.') {
				return CUE_EXTENSIONS.Contains(extension.Substring(1).ToLowerInvariant());
			}
			return CUE_EXTENSIONS.Contains(extension.ToLowerInvariant());
		}

		public class CueFile {
			public string filename { get; set; }
			public int sectorSize { get; set; }
			public CueFile(string filename, int sectorSize, bool isData, int trackNumber) {
				this.filename = filename;
				this.sectorSize = sectorSize;
				this.isData = isData;
				this.trackNumber = trackNumber;
				//Do we need index in cue sheet? Probs not, would be identical to track number unless you're being a frickin' weirdo anyway
			}

			public bool isData { get; set; }
			public int trackNumber { get; set; }
		}

		public abstract IList<CueFile> filenames {
			get;
		}

		public static CueSheet create(Stream cueSheet, string extension) {
			if(extension[0] == '.') {
				extension = extension.Substring(1);
			}
			extension = extension.ToLowerInvariant();

			if ("cue".Equals(extension)) {
				return new TextCueSheet(cueSheet);
			}
			if ("gdi".Equals(extension)) {
				return new GDISheet(cueSheet);
			}

			throw new ArgumentException("Can't create " + extension + " cue sheet", extension);
		}
	}
}
