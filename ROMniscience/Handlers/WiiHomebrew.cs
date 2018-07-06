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
using System.Drawing;
using System.Xml.Linq;
using System.Globalization;


namespace ROMniscience.Handlers {
	class WiiHomebrew : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"dol", "Wii executable"},
			{"elf", "Wii ELF executable"},
		};

		public override string name => "Wii Homebrew";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			//TODO: Read .elf header
			info.addInfo("Platform", name);
			if (file.hasSiblingFile("icon.png")) {
				var iconStream = file.getSiblingFile("icon.png");
				var icon = Image.FromStream(iconStream);
				info.addInfo("Icon", icon);
			}

			if (file.hasSiblingFile("meta.xml")) {
				var metaXML = XDocument.Load(file.getSiblingFile("meta.xml"));
				//TODO: Some apps have invalid XML according to this, but they show up in the HBC just fine... and some don't and really do have broken XML and we should catch the exception in that case

				var app = metaXML.Element("app");
				//<app> has a version attribute, but it is always "1"
				string name = app.Element("name")?.Value;
				string coder = app.Element("coder")?.Value;
				if (coder == null) {
					coder = app.Element("author")?.Value;
				}
				string version = app.Element("version")?.Value;
				string releaseDate = app.Element("release_date")?.Value;
				string shortDescription = app.Element("short_description")?.Value;
				string longDescription = app.Element("long_description")?.Value;
				string arguments = app.Element("arguments")?.Value;

				//Note that in the HBC source release, these two things seem to be interpreted exactly the same way
				bool ahbAccess = app.Element("ahb_access") != null;
				bool noIOSReload = app.Element("no_ios_reload") != null;

				if (name != null) {
					//Well, we call it an internal name for consistency. But is that really accurate? It is by definition external, after all; should I just rename it to "Name"?
					info.addInfo("Internal name", name);
				}
				if (coder != null) {
					//As with Master System homebrew headers, should I call this "Author" and have a distinction with that and "Manufacturer" or should those be combined into a thing which is cleverly worded enough to be both?
					info.addInfo("Author", coder);
				}
				if (version != null) {
					info.addInfo("Version", version);
				}
				if (releaseDate != null) {
					//Officially, only the last one is correct. But you don't actually expect people to follow standards, do you?
					//There are also some where it's text with the month name not even spelled correctly, some where I don't even know what they were trying to do because any conceivable format results in them having a month above 12 or hour above 23 or something like that, and some are ambiguous between dd/mm/yyyy and the wrong date format, and why are you people like this?
					string[] formats = { "yyyyMMdd", "yyyyMMddHHmm", "yyyyMMdd000", "yyyyMMdd00000", "yyyy-MM-dd", "yyyyMMddHHmmss" };
					if (DateTime.TryParseExact(releaseDate, formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime date)) {
						info.addInfo("Date", date);
						info.addInfo("Year", date.Year);
						info.addInfo("Month", DateTimeFormatInfo.CurrentInfo.GetMonthName(date.Month));
						info.addInfo("Day", date.Day);
					} else {
						info.addInfo("Date", releaseDate);
					}
				}
				if (shortDescription != null) {
					info.addInfo("Description", shortDescription);
				}
				if (longDescription != null) {
					//Yeah you don't want this in a table
					info.addInfo("Long description", longDescription.Replace("\n", Environment.NewLine), true);
				}
				if (arguments != null) {
					info.addInfo("Arguments", arguments);
				}
				info.addInfo("AHB access", ahbAccess);
				info.addInfo("No IOS reload", noIOSReload);
			}
		}
	}
}
