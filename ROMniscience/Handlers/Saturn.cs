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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class Saturn : CDBasedSystem {
		public override string name => "Sega Saturn";

		public static readonly Dictionary<char, string> REGIONS = new Dictionary<char, string>() {
			{'J', "Japan"},
			{'T', "Asia (NTSC)"}, //Taiwan/Philippines/not Korea?
			{'U', "USA"}, //Also includes Canada, which isn't the USA, but nobody tells me off for any other system that works that way. Supposedly includes Brazil, but I don't think it does
			{'E', "Europe"}, //Not sure if this includes Australia, and it shouldn't, but it always does
				
			{'B', "Brazil"},
			{'A', "Asia (PAL)"},
			{'K', "Korea"},
			{'L', "Latin America"},
		};

		public static readonly Dictionary<char, string> PERIPHERALS = new Dictionary<char, string>() {
			{'J', "Control Pad"},
			{'A', "Analog Controller"},
			{'M', "Mouse"},
			{'K', "Keyboard"},
			{'S', "Steering Controller"},
			{'T', "Multitap"},
			//There's an E in the localization prototypes of Deep Fear and Shining Force III, and the Panzer Dragoon Saga demo? Hmm
			//I want to think it has something to do with backup memory? But then maybe you wouldn't have that on a demo disc; and I thought the backup memory cartridge just worked with all games anyway
			//I have looked at every single peripheral there is and there doesnt' seem to be anything compatible with those games in particular, so unless it's some kind of development thing?
		};

		static readonly Regex CD_REGEX = new Regex(@"^CD-(?<discNum>\d+)/(?<totalDiscs>\d+) *$");
		static readonly Regex VERSION_REGEX = new Regex(@"^V(?<major>\d)\.(?<minor>\d{3})$");

		public override void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream) {
			string hardwareID = stream.read(16, Encoding.ASCII);
			if (!("SEGA SEGASATURN ".Equals(hardwareID))) {
				//Sorry kid, every single Saturn disc has this here, even the betas and.. if there was homebrews, even they would have to
				//Well, if there was a disc that somehow didn't (it would have to be used with a HLE emulator or some weird modchip), it wouldn't have anything useful down here either
				info.addInfo("Platform", hardwareID);
				return;
			}
			info.addInfo("Platform", "Sega Saturn");

			string makerID = stream.read(16, Encoding.ASCII).TrimEnd(' ');
			if (makerID.StartsWith("SEGA TP ")) {
				string maker = makerID.Substring("SEGA TP ".Length);
				info.addInfo("First party", false);
				maker = Regex.Replace(maker, "^T-0", "T-");
				info.addInfo("Publisher", maker, SegaCommon.LICENSEES);
			} else if ("SEGA ENTERPRISES".Equals(makerID)) {
				info.addInfo("First party", true);
				info.addInfo("Publisher", "Sega");
			} else {
				info.addInfo("First party", false);
				info.addInfo("Publisher", makerID);
			}

			string productNumber = stream.read(10, Encoding.ASCII).TrimEnd(' ');
			info.addInfo("Product code", productNumber);

			string version = stream.read(6, Encoding.ASCII);
			var versionMatches = VERSION_REGEX.Match(version);
			if (versionMatches.Success) {
				info.addInfo("Version", version.Substring(1));
				info.addInfo("Major version", versionMatches.Groups["major"].Value);
				info.addInfo("Minor version", versionMatches.Groups["minor"].Value);
			} else {
				info.addInfo("Version", version);
			}

			string releaseDate = stream.read(8, Encoding.ASCII);
			if (DateTime.TryParseExact(releaseDate, "yyyyMMdd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime date)) {
				info.addInfo("Date", date);
				info.addInfo("Year", date.Year);
				info.addInfo("Month", DateTimeFormatInfo.CurrentInfo.GetMonthName(date.Month));
				info.addInfo("Day", date.Day);
			} else {
				info.addInfo("Date", releaseDate.TrimEnd(' '));
			}

			string deviceInfo = stream.read(8, Encoding.ASCII);
			var deviceInfoMatches = CD_REGEX.Match(deviceInfo);
			if (deviceInfoMatches.Success) {
				info.addInfo("Disc number", int.Parse(deviceInfoMatches.Groups["discNum"].Value, NumberStyles.None));
				info.addInfo("Number of discs", int.Parse(deviceInfoMatches.Groups["totalDiscs"].Value, NumberStyles.None));
			}

			char[] compatibleAreaSymbol = stream.read(10, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("Region", compatibleAreaSymbol, REGIONS);

			stream.Position = 0x50; //Skip 6 characters for some reason... was there supposed to be 16 possible countries at some point?
			char[] peripherals = stream.read(16, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("Compatible peripherals", peripherals, PERIPHERALS);

			string gameTitle = stream.read(112, Encoding.ASCII).TrimEnd(' ');
			//Note that / : and - are used as delimiters
			//Supposedly, it's possible to do something like J:JapaneseNameU:USAName
			info.addInfo("Internal name", gameTitle);
		}
	}
}
