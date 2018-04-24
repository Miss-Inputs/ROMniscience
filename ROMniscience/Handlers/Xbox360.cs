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
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ROMniscience.Handlers {
	class Xbox360 : Handler {
		//http://www.free60.org/wiki/XEX
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"iso", "Xbox 360 disc image"},
			{"xex", "Xbox 360 executable"},
			//.xex is in downloadable stuff, or at least what I ended up dumping from my hard drive. Seems to be always called default.xex, and for XBL there's an ArcadeInfo.xml file which is interesting? But that's not there for demos of full games
			//TODO: Homebrew uses .ccgame and sometimes .exe but sometimes .application? Not sure what's going on there
		};

		public override string name => "Xbox 360";

		static bool isDiscMagic(byte[] magic) {
			return magic[0] == 0x58 && magic[1] == 0x53 && magic[2] == 0x46 && magic[3] == 0x1a;
		}

		static bool isXEXMagic(byte[] magic) {
			return Encoding.ASCII.GetString(magic).Equals("XEX2");
		}

		public static void parseXEXFlags(ROMInfo info, uint flags) {
			info.addInfo("Title module", (flags & 1) > 0);
			info.addInfo("Exports to title", (flags & 2) > 0);
			info.addInfo("System debugger", (flags & 4) > 0);
			info.addInfo("DLL module", (flags & 8) > 0);
			info.addInfo("Module patch", (flags & 16) > 0);
			info.addInfo("Full patch", (flags & 32) > 0);
			info.addInfo("Delta patch", (flags & 64) > 0);
			info.addInfo("User mode", (flags & 128) > 0);

		}

		static uint bytesToUintBE(byte[] b, int offset = 0) {
			return (uint)((b[0 + offset] << 24) | (b[1 + offset] << 16) | (b[2 + offset] << 8) | b[3 + offset]);
		}

		static void addXEXInfo(ROMInfo info, uint id, byte[] data) {
			switch (id) {
				case 0x10001:
					info.addInfo("Load address", bytesToUintBE(data), ROMInfo.FormatMode.HEX, true);
					break;
				case 0x10100:
					info.addInfo("Entry point", bytesToUintBE(data), ROMInfo.FormatMode.HEX, true);
					break;
				case 0x10201:
					info.addInfo("Base address", bytesToUintBE(data), ROMInfo.FormatMode.HEX, true);
					break;
				case 0x18002:
					info.addInfo("Checksum", bytesToUintBE(data), ROMInfo.FormatMode.HEX, true);
					info.addInfo("Filetime", bytesToUintBE(data, 4), ROMInfo.FormatMode.HEX, true); //TODO Parse this
					break;
				case 0x183ff:
					info.addInfo("Original PE name", Encoding.ASCII.GetString(data));
					break;
				case 0x40006:
					info.addInfo("Media ID", data.Take(4).ToArray(), true); //What is this exactly?

					//TODO What's the correct format here?
					info.addInfo("Version as bytes", data.Skip(4).Take(4).ToArray(), true);
					info.addInfo("Base version as bytes", data.Skip(8).Take(4).ToArray(), true);

					string maker = Encoding.ASCII.GetString(data, 12, 2);
					info.addInfo("Manufacturer", maker, MicrosoftCommon.LICENSEE_CODES);
					int titleID = (data[14] << 8) | data[15];
					info.addInfo("Title ID", titleID); //Not entirely sure what this does... could be some kind of product code?
					break;
				case 0x40310:
					//TODO: There's some more to do be done here. The ratings seem to be indexes into some kind of array or enum or something, rather than minimum ages directly. Also some of these ratings boards stopped being used in the Xbox 360's lifespan so for parental controls to keep working they would have to support both I guess, such as OFLC AU (2005) or KMRB (2006); we can possibly cross-reference ArcadeInfo.xml data to figure out what means what
					info.addInfo("ERSB rating", data[0]);
					info.addInfo("PEGI rating", data[1]);
					info.addInfo("PEGI (Finland) rating", data[2]);
					info.addInfo("PEGI (Portgual) rating", data[3]);
					info.addInfo("PEGI (BBFC) rating", data[4]);
					info.addInfo("CERO rating", data[5]);
					info.addInfo("USK rating", data[6]);
					info.addInfo("OLFC (AU) rating", data[7]);
					info.addInfo("OLFC (NZ) rating", data[8]);
					info.addInfo("KMRB rating", data[9]);
					info.addInfo("Brazil rating", data[10]);
					info.addInfo("FBP rating", data[11]); //South African ratings board
					break;
				case 0x40404:
					//Seems to be zeroed out for every .xex I have, so I dunno
					info.addInfo("LAN key", data);
					break;
				case 0x406ff:
					info.addInfo("Multidisc IDs", data);
					break;
				case 0x407ff:
					info.addInfo("Alternate media IDs", data);
					break;
				default:
					info.addInfo("Header 0x" + id.ToString("X2"), data, true);
					info.addInfo("Header 0x" + id.ToString("X2") + " as ASCII", Encoding.ASCII.GetString(data), true);
					info.addInfo("Header 0x" + id.ToString("X2") + " as UTF-8", Encoding.UTF8.GetString(data), true);
					info.addInfo("Header 0x" + id.ToString("X2") + " as UTF-16BE", Encoding.BigEndianUnicode.GetString(data), true);
					break;
			}
		}
		
		private static string combinePrefix(string prefix, string s) {
			if (String.IsNullOrEmpty(prefix)) {
				return s;
			} else {
				return prefix + " " + char.ToLowerInvariant(s[0]) + s.Substring(1);
			}
		}

		public static void parseTitleInfo(ROMInfo info, ROMFile file, XElement titleInfo) {
			var localeAttrib = titleInfo.Attribute("locale");
			string prefix = String.Empty;
			if (localeAttrib != null) {
				try {
					//We use .Parent here to get just the language, and not the country specific to that language, so that it's consistent with other multi-language columns in other handlers
					prefix = System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(localeAttrib.Value).Parent.DisplayName;
				} catch (System.Globalization.CultureNotFoundException) {
					prefix = localeAttrib.Value;
				}
			}

			//ID = some numeric value? For example, Banjo-Kazooie is 1480657236 and Banjo-Tooie is 1480657237
			//This might actually just be the same as the Title ID in the .xex, if you convert that number to a byte array with big endian, and then as usual it's 2-char manufacturer and 2-byte something else

			var nameAttrib = titleInfo.Attribute("Name");
			if(nameAttrib != null) {
				info.addInfo(combinePrefix(prefix, "Name"), nameAttrib.Value);
			}

			//Path is the name of the xex file, but we already know where that is so don't mind that (although is there an XBLA game with this ArcadeInfo.xml file and multiple .xex files, and the main one this ArcadeInfo.xml is applicable is the one referred to and not the other ones? Probably not)

			var iconAttrib = titleInfo.Attribute("ImagePath");
			if(iconAttrib != null) {
				string iconName = iconAttrib.Value;
				if (file.hasSiblingFile(iconName)) {
					var icon = System.Drawing.Image.FromStream(file.getSiblingFile(iconName));
					info.addInfo(combinePrefix(prefix, "Icon"), icon);
				}
			}

			//MaxCred = 200 but I don't know what that does. Credits? You don't have credits in Banjo-Kazooie, kid

			var minPlayersAttrib = titleInfo.Attribute("MinPlayers");
			if(minPlayersAttrib != null) {
				info.addInfo(combinePrefix(prefix, "Minimum players"), int.Parse(minPlayersAttrib.Value));
			}

			var maxPlayersAttrib = titleInfo.Attribute("MaxPlayers");
			if(maxPlayersAttrib != null) {
				info.addInfo(combinePrefix(prefix, "Maximum players"), int.Parse(maxPlayersAttrib.Value));
			}

			var scoreNameAttrib = titleInfo.Attribute("ScoreName");
			if(scoreNameAttrib != null) {
				//Used for leaderboards, for example for Banjo-Kazooie this is "Jiggies" as it compares how many jiggies you've collected compared to your friends
				info.addInfo(combinePrefix(prefix, "Scoring name"), scoreNameAttrib.Value);
			}

			var descriptionElement = titleInfo.Element("Description");
			if(descriptionElement != null) {
				info.addInfo(combinePrefix(prefix, "Description"), descriptionElement.Value);
			}

			//Multiple AchievementInfo elements, which have no content and only ID attributes (some unique number for each AchievementInfo element) and an ImagePath pointing to an icon in this directory that represents the achievement

			//Leaderboard I don't quite understand, it's like <Leaderboard view="12" column="2" type="int" sort="Descending" />

			int i = 0;
			foreach(var genreElement in titleInfo.Elements("Genre")) {
				var genreAttrib = genreElement.Attribute("genreID");
				if(genreAttrib != null) {
					//TODO: How are genres encoded into numbers like this?
					//Banjo-Kazooie displays as "Platformer, Classics" in the 360 dashboard and is 116070000 for example; could be a bitfield of some kind?
					//Also there can be multiple elements despite a single element apparently representing more than one genre (Banjo-Tooie), or even none (Blade Kitten); I'll have to remember what those show up as on the actual system
					info.addInfo(combinePrefix(prefix, i == 0 ? "Genre ID" : "Genre ID " + i), genreAttrib.Value);
				}
				++i;
			}

			var parentalControl = titleInfo.Element("Parental-Control");
			if (parentalControl != null) {
				foreach (var system in parentalControl.Elements("System")) {
					info.addInfo(combinePrefix(prefix, system.Attribute("id").Value + " rating descriptor"), system.Value);
					var ratingImageAttribute = system.Attribute("ImagePath");
					if (ratingImageAttribute != null) {
						string ratingImagePath = ratingImageAttribute.Value;
						if (file.hasSiblingFile(ratingImagePath)) {
							var ratingImage = System.Drawing.Image.FromStream(file.getSiblingFile(ratingImagePath));
							info.addInfo(combinePrefix(prefix, system.Attribute("id").Value + " rating image"), ratingImage); ;
						}
					}
				}
			}
		}

		public static void parseArcadeInfo(ROMInfo info, ROMFile file, Stream xmlFile) {
			var xml = XDocument.Load(xmlFile);
			var arcadeInfo = xml.Element("ArcadeInfo");

			info.addInfo("XDK version", arcadeInfo.Attribute("xdkVersion")?.Value);
			info.addInfo("Project version", arcadeInfo.Attribute("projectVersion")?.Value);

			foreach(var titleInfo in arcadeInfo.Elements("TitleInfo")) {
				parseTitleInfo(info, file, titleInfo);
			}

		}

		public static void parseXEX(ROMInfo info, ROMFile file) {
			if (file.hasSiblingFile("ArcadeInfo.xml")) {
				parseArcadeInfo(info, file, file.getSiblingFile("ArcadeInfo.xml"));
			}

			var s = file.stream;
			s.Position = 4;
			uint flags = (uint)s.readIntBE();
			parseXEXFlags(info, flags);

			s.Position = 0x14;
			uint headerCount = (uint)s.readIntBE();

			for (uint i = 0; i < headerCount; ++i) {
				uint headerID = (uint)s.readIntBE();
				uint headerSize = headerID & 0xff;
				byte[] headerData = s.read(4);

				if (headerSize == 0 || headerSize == 1) {
					addXEXInfo(info, headerID, headerData);
				} else {
					long pos = s.Position;
					try {
						
						uint offset = bytesToUintBE(headerData);
						s.Position = offset;

						byte[] data;
						if (headerSize == 0xff) {
							headerSize = (uint)s.readIntBE() - 4;
						} else {
							headerSize = headerSize * 4;
						}
						data = s.read((int)headerSize);

						addXEXInfo(info, headerID, data);
						
					} finally {
						s.Position = pos;
					}
				}
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			var stream = file.stream;
			byte[] magic = stream.read(4);

			if (isDiscMagic(magic)) {
				info.addInfo("Detected format", "Disc");
				//Not really much we can do at this point... We could check for "God2Iso v" at 0x7a69, I guess, to see if that's a disc built from a ripped hard drive or USB and converted. I'm gonna presume rips via fancy hacked drives don't do that
				return;
			} else if (isXEXMagic(magic)) {
				info.addInfo("Detected format", "XEX");
				parseXEX(info, file);
			} else {
				info.addInfo("Detected format", "Unknown");
			}
		}
	}
}
