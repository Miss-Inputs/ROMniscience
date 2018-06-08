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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class ColecoVision: Handler {

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"col", "ColecoVision ROM"}
		};

		public override string name => "ColecoVision";

		static string truncate(string s, int length) {
			return s.Length > length ? s.Substring(0, length) : s;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			var s = file.stream;
			info.addInfo("Platform", name);

			byte[] cartCheck = s.read(2);
			bool skipBIOS = cartCheck[0] != 0xAA && cartCheck[1] != 0x55;

			if (!skipBIOS) {
				s.Position = 0x24;
				string title = s.read(60, Encoding.ASCII);
				string[] titleLines = title.Split('/');

				titleLines[0] = truncate(titleLines[0], 28);
				titleLines[1] = truncate(titleLines[1], 28);
				titleLines[2] = truncate(titleLines[2], 4);

				if (Regex.IsMatch(titleLines[0], @"^\x1d\s?\d+")) {
					//According to the manual you are supposed to not do this, but you dang bet companies did it
					//Sometimes, anyway
					//This heuristic doesn't always work and such you end up with the two lines being... well, not swapped around, but I end up parsing them as meaning what they don't actually mean, if that makes sense
					//I guess I shouldn't really do this
					info.addInfo("Internal name", titleLines[1].Replace('\x1d', '©'));
					info.addInfo("Copyright", titleLines[0].Replace('\x1d', '©'));
				} else {
					info.addInfo("Internal name", titleLines[0].Replace('\x1d', '©'));
					info.addInfo("Copyright", titleLines[1].Replace('\x1d', '©'));

					var matches = Regex.Match(titleLines[1], @"^PRESENTS (.+)'S$");
					if (matches.Success) {
						//This seems ugly, but it's not really wrong
						info.addInfo("Publisher", matches.Groups[1]);
					}
				}

				if (int.TryParse(titleLines[2], out int year)) {
					info.addInfo("Year", year); //Add it as an int if we can, just in case one day I decide to do something where type matters
				} else {
					info.addInfo("Year", titleLines[2]);
				}
			}
		}
	}
}
