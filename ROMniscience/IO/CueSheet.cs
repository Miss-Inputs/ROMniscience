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

namespace ROMniscience.IO {
	class CueSheet {

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

		IList<CueFile> _filenames = new List<CueFile>();
		public IList<CueFile> filenames => _filenames;

		//<type> is BINARY for little endian, MOTOROLA for big endian, or AIFF/WAV/MP3. Generally only BINARY will be used (even audio tracks are usually ripped as raw binary)
		static readonly Regex FILE_REGEX = new Regex(@"^\s*FILE\s+(?:""(?<name>.+)""|(?<name>\S+))\s+(?<type>.+)\s*$", RegexOptions.IgnoreCase);

		//<mode> is defined here: https://www.gnu.org/software/ccd2cue/manual/html_node/MODE-_0028Compact-Disc-fields_0029.html#MODE-_0028Compact-Disc-fields_0029 but generally only AUDIO, MODE1/<size>, and MODE2/<size> are used
		static readonly Regex TRACK_REGEX = new Regex(@"^\s*TRACK\s+(?<number>\d+)\s+(?<mode>.+)\s*$", RegexOptions.IgnoreCase);
		public CueSheet(Stream cueSheet) {
			using (var sr = new StreamReader(cueSheet)) {
				string currentFile = null;
				string currentMode = null;

				while (!sr.EndOfStream) {
					string line = sr.ReadLine();
					if (line == null) {
						break;
					}

					var match = FILE_REGEX.Match(line);
					if (match.Success) {
						if (currentFile != null && currentMode != null) {
							_filenames.Add(new CueFile(currentFile, currentMode));
							currentFile = null;
							currentMode = null;
						}

						currentFile = match.Groups["name"].Value;
					} else {
						//Yeah, see what I mean? We're just gonna use the first track/mode of each file for simplicity until I'm forced to not do that
						//Hence we only bother checking for a new track if it's a new file
						if (currentMode == null) {
							match = TRACK_REGEX.Match(line);
							if (match.Success) {
								currentMode = match.Groups["mode"].Value;
							}
						}
					}
				}

				_filenames.Add(new CueFile(currentFile, currentMode));
			}
		}
	}
}
