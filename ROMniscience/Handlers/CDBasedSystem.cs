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
			{"iso", "2048-byte sector CD image"},
			{"bin", "CD image file"},
			{"img", "CD image file"},
		};
		
		public abstract void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream);

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if ("iso".Equals(file.extension)) {
				//Straight 2048 byte sectors, nothing to do here
				info.addInfo("Sector size", 2048);
				addROMInfo(info, file, file.stream);
			} else if ("bin".Equals(file.extension) || "img".Equals(file.extension)) {
				//Just to be annoying, there can be .bin/.img formats with 2048 byte sectors instead of 2352
				int sectorSize;
				//We'll check the cue first
				string cueFilename = Path.ChangeExtension(file.name, "cue");
				if (file.hasSiblingFile(cueFilename)) {
					using (var cue = file.getSiblingFile(cueFilename)) {
						string mode = parseCueSheet(cue);
						sectorSize = int.Parse(mode.Split('/').Last());
					}
				} else {
					//Assume 2352 if there isn't a cue there (which there should be, but y'know)
					sectorSize = 2352;
				}
				info.addInfo("Sector size", sectorSize);

				if (sectorSize == 2352) {
					addROMInfo(info, file, new CDInputStream(file.stream));
				} else {
					addROMInfo(info, file, file.stream);
				}
			} else {
				//If we end up here, the handler decided to override or add to filetypeMap, so it's their problem
				addROMInfo(info, file, file.stream);
			}
 
		}

		static Regex trackLineRegex = new Regex(@"^\s*TRACK\s+\d+\s+(?<mode>MODE./\d+)\s*$");
		static string parseCueSheet(Stream cueSheet) {
			//TODO This is very hacky and bad!!
			using(var sr = new StreamReader(cueSheet)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();
					if(line == null) {
						break;
					}

					var matches = trackLineRegex.Match(line);
					if (matches.Success) {
						return matches.Groups["mode"].Value;
					}
				}
				return null;
			}
		}
	}
}
