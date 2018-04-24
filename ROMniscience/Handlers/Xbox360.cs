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

		public static readonly IDictionary<string, string> GENRES = new Dictionary<string, string>() {
			//These are probably wrong, and are just my own research
			{"116010000", "Action & Adventure"},
			{"116040000", "Puzzle & Trivia"},
			{"116070000", "Classics"},
		};

		public static readonly IDictionary<int, string> ESRB_RATINGS = new Dictionary<int, string>() {
			//These might also be wrong
			{2, "Everyone"},
			{6, "Teen"},
			{8, "Mature"},
			//1 might be Early Childhood?
			//E10+ was only invented later on I think but it's probably 3 4 or 5
		};

		public static readonly IDictionary<int, string> PEGI_RATINGS = new Dictionary<int, string>() {
			{0, "3+"},
			{4, "7+"},
			{9, "12+"},
			{13, "16+"},
			{14, "18+"},
		};

		public static readonly IDictionary<int, string> PEGI_FINLAND_RATINGS = new Dictionary<int, string>() {
			{0, "3+"},
			{4, "7+"},
			{8, "12+"},
			{12, "16+"},
			{14, "18+"},
		};

		public static readonly IDictionary<int, string> PEGI_PORTGUAL_RATINGS = new Dictionary<int, string>() {
			{1, "3+"},
			{3, "7+"},
			{9, "12+"},
			{13, "16+"},
			{14, "18+"},
		};

		public static readonly IDictionary<int, string> PEGI_UK_RATINGS = new Dictionary<int, string>() {
			//BBFLC but maybe not? It confuses me quite a bit
			{0, "3+"},
			{4, "7+"},
			{9, "12+"},
			{12, "16+ (12)"}, //Doom
			{13, "16+ (13)"}, //LR: FFXIII demo and CSI: Deadly Intent (TODO figure out what's going on here)
			{14, "18+"},
		};

		public static readonly IDictionary<int, string> CERO_RATINGS = new Dictionary<int, string>() {
			{0, "A"},
			{2, "B"},
			{4, "C"},
			//6 = D? 8 = Z?
		};

		public static readonly IDictionary<int, string> USK_RATINGS = new Dictionary<int, string>() {
			{0, "No age restriction"},
			{2, "6+"},
			{4, "12+"},
			{6, "16+"}
			//1 = 3+? 7 = 18+?
		};

		public static readonly IDictionary<int, string> OFLC_AU_RATINGS = new Dictionary<int, string>() {
			//Might actually be AGB, which was formed slightly after the Xbox 360 was released. The ratings themselves stayed the same though, so it shouldn't matter
			{0, "G"},
			{2, "PG"},
			{4, "M"},
			{5, "M + Online interactivity"}, //LR:FF13 demo has this, the full game is rated M and has "Gaming experience may change online" in the descriptor
			{6, "MA15+"},
			//8 = R18+ (although that was only really introduced after the 360's lifespan)?
		};

		public static readonly IDictionary<int, string> OFLC_NZ_RATINGS = new Dictionary<int, string>() {
			{0, "G"},
			{2, "PG"},
			{4, "R13+"},
			{6, "R18+"},
			{16, "M"}, //But it's not restricted (just advised for 16 and over), and confusing enough that the OFLC has to have a web page attempting to explain it. Anyway, it's non-linear and this annoys me for some reason
			//There's R15+ and R16+ in there, theoretically
		};

		public static readonly IDictionary<int, string> BRAZIL_RATINGS = new Dictionary<int, string>() {
			//Only Doom has a rating, so this is all I can know of
			//This really doesn't seem right at all but that's what the Marketplace website says if you view Doom's page with the country set to Brazil
			{80, "16+"},
		};

		public static readonly IDictionary<int, string> FBP_RATINGS = new Dictionary<int, string>() {
			{6, "A"},
			{10, "13"},
			{13, "16"}, //Sorta making a big assumption here. There is a 16+ rating for FBP, but this value is just seen in CSI: Deadly Intent which I can't find any actual FBP rating info on (it's been removed from the Xbox Marketplace in South Africa seemingly), just a South African website which has a box art with the PEGI UK rating on it, and it has a value of 13 in the UK and released as 16+ so this is probably about right
			{14, "18"},
		};


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
					//Uhh so I did a little bit of guessing here
					//The numbers clearly aren't just the age ratings, but there's some kind of table going on, so all the numbers mean different arbitrary things for different rating systems. And 255 means it's not there, that's all I can figure out, really. Otherwise the code speaks for itself? Or I mean like I hope it does, because what I really mean is that I suck at explaining it with normal words
					//What's interesting is that 255 is the OFLC (AU) rating for Boom Boom Rocket and it's quite obviously released here because I wouldn't have it otherwise and ???
					//I can't help but feel this can be refactored a bit more neatly, but eh, never mind that for now...
					if (data[0] != 255) {
						info.addInfo("ERSB rating", data[0], ESRB_RATINGS);
					}
					if (data[1] != 255) {
						info.addInfo("PEGI rating", data[1], PEGI_RATINGS);
					}
					if (data[2] != 255) {
						info.addInfo("PEGI (Finland) rating", data[2], PEGI_FINLAND_RATINGS);
					}
					if (data[3] != 255) {
						info.addInfo("PEGI (Portgual) rating", data[3], PEGI_PORTGUAL_RATINGS);
					}
					if (data[4] != 255) {
						info.addInfo("PEGI (UK) rating", data[4], PEGI_UK_RATINGS);
					}
					if (data[5] != 255) {
						info.addInfo("CERO rating", data[5], CERO_RATINGS);
					}
					if (data[6] != 255) {
						info.addInfo("USK rating", data[6], USK_RATINGS);
					}
					if (data[7] != 255) {
						info.addInfo("OLFC (AU) rating", data[7], OFLC_AU_RATINGS); //Stopped being used in 2005, but it's really just under new management but the same thing
					}
					if (data[8] != 255) {
						info.addInfo("OLFC (NZ) rating", data[8], OFLC_NZ_RATINGS);
					}
					if (data[9] != 255) {
						info.addInfo("KMRB rating", data[9]); //Stopped being used in 2006, and it doesn't look like any game I know of has a Korean rating anyway
					}
					if (data[10] != 255) {
						info.addInfo("Brazil rating", data[10], BRAZIL_RATINGS);
					}
					if (data[11] != 255) {
						info.addInfo("FBP rating", data[11], FBP_RATINGS); //South African ratings board
					}
					//In theory, there can be up to 63 ratings, but do you really want me to copypaste 3 lines 52 more times just to see if there's any secret ratings that probably aren't even used? Too bad, I'm not gonna
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
			if (nameAttrib != null) {
				info.addInfo(combinePrefix(prefix, "Name"), nameAttrib.Value);
			}

			//Path is the name of the xex file, but we already know where that is so don't mind that (although is there an XBLA game with this ArcadeInfo.xml file and multiple .xex files, and the main one this ArcadeInfo.xml is applicable is the one referred to and not the other ones? Probably not)

			var iconAttrib = titleInfo.Attribute("ImagePath");
			if (iconAttrib != null) {
				string iconName = iconAttrib.Value;
				if (file.hasSiblingFile(iconName)) {
					var icon = System.Drawing.Image.FromStream(file.getSiblingFile(iconName));
					info.addInfo(combinePrefix(prefix, "Icon"), icon);
				}
			}

			//MaxCred = 200 but I don't know what that does. Credits? You don't have credits in Banjo-Kazooie, kid

			var minPlayersAttrib = titleInfo.Attribute("MinPlayers");
			if (minPlayersAttrib != null) {
				info.addInfo(combinePrefix(prefix, "Minimum players"), int.Parse(minPlayersAttrib.Value));
			}

			var maxPlayersAttrib = titleInfo.Attribute("MaxPlayers");
			if (maxPlayersAttrib != null) {
				info.addInfo(combinePrefix(prefix, "Maximum players"), int.Parse(maxPlayersAttrib.Value));
			}

			var scoreNameAttrib = titleInfo.Attribute("ScoreName");
			if (scoreNameAttrib != null) {
				//Used for leaderboards, for example for Banjo-Kazooie this is "Jiggies" as it compares how many jiggies you've collected compared to your friends
				info.addInfo(combinePrefix(prefix, "Scoring name"), scoreNameAttrib.Value);
			}

			var descriptionElement = titleInfo.Element("Description");
			if (descriptionElement != null) {
				info.addInfo(combinePrefix(prefix, "Description"), descriptionElement.Value);
			}

			//Multiple AchievementInfo elements, which have no content and only ID attributes (some unique number for each AchievementInfo element) and an ImagePath pointing to an icon in this directory that represents the achievement

			//Leaderboard I don't quite understand, it's like <Leaderboard view="12" column="2" type="int" sort="Descending" />

			int i = 1;
			foreach (var genreElement in titleInfo.Elements("Genre")) {
				var genreAttrib = genreElement.Attribute("genreId");
				if (genreAttrib != null) {
					//TODO: How are genres encoded into numbers like this? They don't seem to be consistent with what shows up in Game Details on an actual console, unless I need to redump my stuff because it's been updated or something
					//Banjo-Kazooie: 116070000 > Platformer, Classics
					//Banjo-Tooie: 116010000, 116070000 > Action-Adventure, Classics
					//Blade Kitten: <no value> > Action-Adventure
					//Bliss Island: 116040000, 116090000 > <nothing>
					//Boom Boom Rocket: 116010000 > Action-Adventure
					//Coffeetime Crosswords: 116040000 > <nothing>
					//Doritos Crash Course: <no value> > Platformer, Racing & Flying
					//ilomilo: <no value> > Platformer, Puzzle & Trivia
					//Interpol: 116090000 > Family, Puzzle & Trivia
					//Marble Blast Ultra: <no value> > <nothing>
					//OutRun Online Arcade: 116130000 > <nothing>
					//Phantasy Star II: 116070000 > Classics
					//Rez HD: 116160000 > Shooter
					//Sam & Max Save the World: 116040000 > Classics, Puzzle & Trivia
					//Scott Pilgrim vs. The World <no value> > <nothing>
					//Track & Field: 116030000 > Classics
					//Uno: <no value> > <nothing>
					//Daytona USA demo: <no value> > Classics, Racing & Flying
					//Deathspank demo: <no value> > Action & Adventure, Role Playing
					//Doom (1993) demo: <no value> > Shooter, Classics
					//Peggle demo: 116040000 > Puzzle & Trivia
					//Peggle 2 demo: <no value> > Family, Puzzle & Trivia
					//Sonic the Hedgehog (1991) demo: 116070000 > Classics
					//So as you can see it's a bit messed up, there are games of the same genre internally that don't display as the same genre, or there's games that display as the same genre but aren't the same internally. Either there's been updates and if I dump them again it'll start making sense, or the Xbox 360 just goes to Xbox Live to get genre information and ignores whatever's here anyway.
					//Having said that, there seems to be a few values which we can deduce as most likely being correct
					info.addInfo(combinePrefix(prefix, i == 1 ? "Genre" : "Genre " + i), genreAttrib.Value, GENRES);
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

			foreach (var titleInfo in arcadeInfo.Elements("TitleInfo")) {
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
