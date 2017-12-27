/*
 * The MIT License
 *
 * Copyright 2017 Megan Leet (Zowayix).
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
using System.IO;
using System.Xml.Linq;

namespace ROMniscience.Datfiles {
	class XMLDatfile {
		public string name {
			get;
		}
		public string description {
			get;
		}
		public string version {
			get;
		}
		public string author {
			get;
		}
		public string homepage {
			get;
		}
		public string url {
			get;
		}

		public IList<Game> games {
			get;
		}

		public XMLDatfile(string path) {
			var doc = XDocument.Load(path).Element("datafile");
			var header = doc.Element("header");

			name = header.Element("name")?.Value;
			description = header.Element("description")?.Value;
			version = header.Element("version")?.Value;
			author = header.Element("author")?.Value;
			homepage = header.Element("homepage")?.Value;
			url = header.Element("url")?.Value;

			games = new List<Game>();

			foreach(XElement gameNode in doc.Elements("game")) {
				Game game = new Game {
					name = gameNode.Attribute("name")?.Value, //Technically this is mandatory according to the DTD, but I'd rather it not crash
					description = gameNode.Element("description")?.Value,
					category = gameNode.Element("category")?.Value,

					roms = new List<ROM>()
				};
				foreach(XElement romNode in gameNode.Elements("rom")) {
					ROM rom = new ROM {
						name = romNode.Attribute("name")?.Value
					};
					string sizeString = romNode.Attribute("size")?.Value;
					long actualSize = 0;
					if("0".Equals(sizeString)) {
						//Empty files are not of use to us
						continue;
					} else if(sizeString != null && long.TryParse(sizeString, out actualSize)) {
						rom.size = actualSize;
					}

					rom.crc32 = romNode.Attribute("crc")?.Value;
					rom.md5 = romNode.Attribute("md5")?.Value;
					rom.sha1 = romNode.Attribute("sha1")?.Value;
					rom.status = romNode.Attribute("status")?.Value;
					if(rom.status == null) {
						//The DTD (which I don't feel like using and don't really need to use) defines this as the default
						rom.status = "good";
					}

					game.roms.Add(rom);
				}

				games.Add(game);
			}

		}

		public class IdentifyResult {
			public XMLDatfile datfile {
				get; set;
			}
			public Game game {
				get; set;
			}
			public ROM rom {
				get; set;
			}

			public IdentifyResult(XMLDatfile datfile, Game game, ROM rom) {
				this.datfile = datfile;
				this.game = game;
				this.rom = rom;
			}
		}

		public IdentifyResult identify(int crc32, byte[] md5, byte[] sha1) {
			foreach(Game game in games) {
				foreach(ROM rom in game.roms) {
					//TODO: See if there are actually any CRC32/MD5 hash collisions across dat files
					//from No-Intro, Redump, and various other places. If not then it'll be safe to
					//just use those which would be faster than SHA-1

					if(BitConverter.ToString(sha1).Replace("-", "").Equals(rom.sha1?.ToUpperInvariant())){
						return new IdentifyResult(this, game, rom);
					}

					if(BitConverter.ToString(md5).Replace("-", "").Equals(rom.md5?.ToUpperInvariant())) {
						return new IdentifyResult(this, game, rom);
					}

					if(crc32.ToString("X2").Equals(rom.crc32?.ToUpperInvariant())) {
						return new IdentifyResult(this, game, rom);
					}
				}
			}
			return null;
		}

		public static void Main(string[] args) {
			//Just here for testing but I suppose you could use it as a testing thing if you really wanted
			string datFolder = args[0];
			string file = args[1];

			var datFiles = DatfileCollection.loadFromFolder(new DirectoryInfo(datFolder));
			using(ROMniscience.IO.FileInputStream f = new IO.FileInputStream(new FileInfo(file))) {
				var result = datFiles.identify(f);
				Console.WriteLine("Datfile: {0}", result.datfile.name);
				Console.WriteLine("Game: {0}", result.game.name);
				Console.WriteLine("ROM: {0}", result.rom.name);
			}
		}
	}


}
