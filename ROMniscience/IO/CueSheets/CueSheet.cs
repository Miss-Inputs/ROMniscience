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
			//I guess we'd put "gdi" and "ccd" in this list once we implement those
			"cue"
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
			//TODO Include track number and index; right now we are assuming track 1 is the data part and the only data part we want to look at. This is, in fact, not true just to piss me off; Bandai Playdia discs have two data tracks and PC Engine CD discs have the data on track 2 (track 1 is always an audio track, probably to tell people off for putting the disc in an audio CD player)
			public string filename { get; set; }
			public string mode { get; set; }
			public CueFile(string filename, string mode) {
				this.filename = filename;
				this.mode = mode;
			}

			public bool isData => mode != null && mode.ToUpperInvariant().StartsWith("MODE");
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
			throw new ArgumentException("Can't create " + extension + " cue sheet", extension);
		}
	}
}
