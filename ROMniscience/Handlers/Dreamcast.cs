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
	class Dreamcast : CDBasedSystem {
		//http://mc.pp.se/dc/ip0000.bin.html
		public override string name => "Dreamcast";

		static readonly Regex DEVICE_INFO_REGEX = new Regex(@"^(?<checksum>[\dA-Fa-f]{4}) GD-ROM(?<discNum>\d+)/(?<totalDiscs>\d+) *$");
		static readonly Regex VERSION_REGEX = new Regex(@"^V(?<major>\d)\.(?<minor>\d{3})$");

		public static void parsePeripheralInfo(ROMInfo info, int peripherals) {
			info.addInfo("Uses Windows CE?", (peripherals & (1 << 0)) > 0);
			info.addInfo("Supports VGA box?", (peripherals & (1 << 4)) > 0);
			info.addInfo("Supports other expansions", (peripherals & (1 << 8)) > 0); //Well that's just mysterious, thanks
			info.addInfo("Supports rumble", (peripherals & (1 << 9)) > 0); //The "Dreamcast Jump Pack"/"Vibration Pack"/"Puru Puru Pack" accessory
			info.addInfo("Supports microphone", (peripherals & (1 << 10)) > 0);
			info.addInfo("Supports memory card", (peripherals & (1 << 11)) > 0);
			info.addInfo("Requires Start + A + B + dpad", (peripherals & (1 << 12)) > 0);
			info.addInfo("Requires C button", (peripherals & (1 << 13)) > 0);
			info.addInfo("Requires D button", (peripherals & (1 << 14)) > 0);
			info.addInfo("Requires X button", (peripherals & (1 << 15)) > 0);
			info.addInfo("Requires Y button", (peripherals & (1 << 16)) > 0);
			info.addInfo("Requires Z button", (peripherals & (1 << 17)) > 0);
			info.addInfo("Requires expanded dpad", (peripherals & (1 << 18)) > 0);
			info.addInfo("Requires analog R trigger", (peripherals & (1 << 19)) > 0);
			info.addInfo("Requires analog L trigger", (peripherals & (1 << 20)) > 0);
			info.addInfo("Requires analog horizontal", (peripherals & (1 << 21)) > 0);
			info.addInfo("Requires analog vertical", (peripherals & (1 << 22)) > 0);
			info.addInfo("Requires expanded analog horizontal", (peripherals & (1 << 23)) > 0);
			info.addInfo("Requires expanded analog vertical", (peripherals & (1 << 24)) > 0);
			info.addInfo("Supports gun", (peripherals & (1 << 25)) > 0);
			info.addInfo("Supports mouse", (peripherals & (1 << 26)) > 0);
			info.addInfo("Supports keyboard", (peripherals & (1 << 27)) > 0);
		}

		static int calcChecksum(byte[] buf) {
			int n = 0xffff;

			foreach(byte b in buf) {
				n ^= (b << 8);

				for(int c = 0; c < 8; ++c) {
					if((n & 0x8000) > 0) {
						n = (n << 1) ^ 4129;
					} else {
						n = (n << 1);
					}
				}

			}

			return n & 0xffff;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream) {
			string hardwareID = stream.read(16, Encoding.ASCII);
			if (!"SEGA SEGAKATANA ".Equals(hardwareID)) {
				info.addInfo("Platform", hardwareID);
				return;
			}
			info.addInfo("Platform", name);

			string copyright = stream.read(16, Encoding.ASCII);
			//Seems to always be SEGA ENTERPRISES, so that's no fun
			info.addInfo("Copyright", copyright);

			string deviceInfo = stream.read(16, Encoding.ASCII);
			var deviceInfoMatches = DEVICE_INFO_REGEX.Match(deviceInfo);
			int checksum = 0;
			if (deviceInfoMatches.Success) {
				checksum = Convert.ToInt32(deviceInfoMatches.Groups["checksum"].Value, 16);
				info.addInfo("Disc number", int.Parse(deviceInfoMatches.Groups["discNum"].Value, NumberStyles.None));
				info.addInfo("Number of discs", int.Parse(deviceInfoMatches.Groups["totalDiscs"].Value, NumberStyles.None));
			}

			char[] areaSymbols = stream.read(8, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("Region", areaSymbols, Saturn.REGIONS);

			string peripheralsAsString = stream.read(8, Encoding.ASCII).TrimEnd(' ');
			try {
				int peripherals = Convert.ToInt32(peripheralsAsString, 16);
				parsePeripheralInfo(info, peripherals);
			} catch (FormatException) {

			}

			byte[] productNumberAndVersion = stream.read(16);
			info.addInfo("Checksum", checksum, ROMInfo.FormatMode.HEX, true);
			int calculatedChecksum = calcChecksum(productNumberAndVersion);
			info.addInfo("Calculated checksum", calculatedChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksum valid?", checksum == calculatedChecksum);

			string productNumber = Encoding.ASCII.GetString(productNumberAndVersion, 0, 10).TrimEnd(' ');
			info.addInfo("Product code", productNumber);

			string version = Encoding.ASCII.GetString(productNumberAndVersion, 10, 6).TrimEnd(' ');
			var versionMatches = VERSION_REGEX.Match(version);
			if (versionMatches.Success) {
				info.addInfo("Version", version.Substring(1));
				info.addInfo("Major version", versionMatches.Groups["major"].Value);
				info.addInfo("Minor version", versionMatches.Groups["minor"].Value);
			} else {
				info.addInfo("Version", version);
			}

			string releaseDate = stream.read(16, Encoding.ASCII).TrimEnd(' ');
			if (DateTime.TryParseExact(releaseDate, "yyyyMMdd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime date)) {
				info.addInfo("Date", date);
				info.addInfo("Year", date.Year);
				info.addInfo("Month", DateTimeFormatInfo.CurrentInfo.GetMonthName(date.Month));
				info.addInfo("Day", date.Day);
			} else {
				info.addInfo("Date", releaseDate.TrimEnd(' '));
			}

			string bootFilename = stream.read(16, Encoding.ASCII).TrimEnd(' ');
			//Seems to be 0WINCEOS.BIN if Windows CE is used and 1ST_BOOT.BIN otherwise
			info.addInfo("Boot filename", bootFilename);

			string maker = stream.read(16, Encoding.ASCII).TrimEnd(' ');
			if ("SEGA ENTERPRISES".Equals(maker)) {
				info.addInfo("Publisher", "Sega");
			} else if(maker.StartsWith("SEGA LC-")) {
				maker = maker.Substring("SEGA LC-".Length);
				info.addInfo("Publisher", maker, SegaCommon.LICENSEES);
			} else { 
				info.addInfo("Publisher", maker);
			}

			string interalName = stream.read(128, Encoding.ASCII).TrimEnd(' ');
			info.addInfo("Internal name", interalName);
		}
	}
}
