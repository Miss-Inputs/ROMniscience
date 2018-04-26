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
	class GDISheet : CueSheet {
		//Haha fuck it's yet another format where there's no actual documentation whatsoever and I have to figure things out myself god dammit
		//Ugh fuckety fuck fuck fuck I wonder if there's a thing that automatically searches GitHub for comments containing the word "fuck"
		//Alright so from what I can gather it's something like this, and I am probably so fucking wrong it's not funny so if you use this class for code-as-documentation yourself and things aren't working for you, then it's probably not your fault
		//You have a line at the start with the number of tracks, which isn't actually that useful since without it we would just read to the end of the file anyway
		//Then you have one line for each track and it's split by whitespace like this
		//track number
		//maybe some kind of offset or "LBA" as the cool kids say? It doesn't look like I need it (unless I was writing to an actual CD I guess)
		//A number where 0 means audio or otherwise non-data and 4 means data (why? what? Why 4? Who made this?)
		//Sector size
		//Filename (needs double quotes if includes spaces of course)
		//Another number which is always 0 and I don't know if it's some kind of secret pregap information that only certain discs use or something
		//I guess I can work with that unless that 0 breaks the world

		private IList<CueFile> _filenames = new List<CueFile>();
		public override IList<CueFile> filenames => _filenames;

		static readonly Regex GDI_LINE_REGEX = new Regex(
			@"^(?<trackNumber>\d+)\s+(?<unknown1>\S+)\s+(?<type>\d)\s+(?<sectorSize>\d+)\s+(?:""(?<name>.+)""|(?<name>\S+))\s+(?<unknown2>.+)$"
		);

		public GDISheet(Stream cueStream) {
			using(var sr = new StreamReader(cueStream)) {
				sr.ReadLine();
				//Number of tracks, but we won't use that

				while (!sr.EndOfStream) {
					string line = sr.ReadLine();
					if (line == null) {
						break;
					}

					var match = GDI_LINE_REGEX.Match(line);
					if (match.Success) {
						//int trackNumber = int.Parse(match.Groups["trackNumber"].Value);
						bool isData = int.Parse(match.Groups["type"].Value) == 4;
						int sectorSize = int.Parse(match.Groups["sectorSize"].Value);
						string filename = match.Groups["name"].Value;

						_filenames.Add(new CueFile(filename, sectorSize, isData));
					}
				}
			}
		}

	}
}
