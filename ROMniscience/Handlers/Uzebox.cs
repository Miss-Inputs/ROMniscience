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
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class Uzebox : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"bin", "Uzebox ROM"},
			{"uze", "Uzebox ROM"},
		};

		public override string name => "Uzebox";

		public static IDictionary<int, string> TARGETS => new Dictionary<int, string> {
			{0, "ATmega644"},
			{1, "ATmega1284"}, //Reserved
		};

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Uzebox");
			var s = file.stream;
			s.Position = 0;

			string magic = s.read(6, Encoding.ASCII); //Should be UZEBOX
			info.addInfo("Magic", magic);
			if (!"UZEBOX".Equals(magic)) {
				//Not all ROMs have this 512-byte header
				//TODO: Add "has header" "skip header" etc blah stuff what am I even doing
				return;
			}

			int headerVersion = s.read();
			info.addInfo("Header version", headerVersion);
			int target = s.read();
			info.addInfo("Target", target, TARGETS);
			int programSize = s.readIntLE();
			info.addInfo("ROM size", programSize, ROMInfo.FormatMode.SIZE);
			short year = s.readShortLE();
			info.addInfo("Year", year);
			string name = s.read(32, Encoding.ASCII).TrimEnd('\0'); //Presumably ASCII?
			info.addInfo("Internal name", name); //Not really internal name, meant for display actually...
			string author = s.read(32, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Manufacturer", author);

			//Supposedly never used
			byte[] iconData = s.read(256);
			info.addInfo("Icon data", iconData, true);

			int crc32 = s.readIntLE();
			info.addInfo("Checksum", crc32, ROMInfo.FormatMode.HEX, true);
			bool snesMouseMarker = s.read() == 1;
			info.addInfo("Requires SNES mouse?", snesMouseMarker);

			//Supposedly unused
			string description = s.read(64, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Description", description);
		}
	}
}
