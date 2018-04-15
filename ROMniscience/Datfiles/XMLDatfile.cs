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

					var crc32Attrib = romNode.Attribute("crc");
					rom.crc32 = null;
					if(crc32Attrib != null) {
						rom.crc32 = Convert.ToInt32(crc32Attrib.Value, 16);
					}

					rom.md5 = parseHexBytes(romNode.Attribute("md5")?.Value);
					rom.sha1 = parseHexBytes(romNode.Attribute("sha1")?.Value);
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

		private static byte[] parseHexBytes(string s) {
			//TODO Why the fuck this slow
			if(s == null) {
				return new byte[0];
			}

			if(s.Length % 2 == 1) {
				s = '0' + s;
			}
			char[] chars = s.ToCharArray();

			byte[] b = new byte[s.Length / 2];
			for(int i = 0; i < s.Length; i += 2) {
				string nybble = new string(new char[] { s[i], s[i + 1] });
				b[i / 2] = Convert.ToByte(nybble, 16);
			}
			return b;
		}

		private static bool byteArraysEqual(byte[] b1, byte[] b2) {
			if(b1 == null) {
				return b2 == null;
			}
			if(b2 == null) {
				return b1 == null;
			}

			int length = b1.Length;
			if(length != b2.Length) {
				return false;
			}

			for(int i = 0; i < length; ++i) {
				if(b1[i] != b2[i]) {
					return false;
				}
			}
			return true;
		}

		public IdentifyResult identify(int crc32, byte[] md5, byte[] sha1) {
			foreach(Game game in games) {
				foreach(ROM rom in game.roms) {
					//Still need to check all three in case a datfile only has a checksum for one of them

					if(byteArraysEqual(sha1, rom.sha1)) {
						return new IdentifyResult(this, game, rom);
					}

					if(byteArraysEqual(md5, rom.md5)) {
						return new IdentifyResult(this, game, rom);
					}

					if(crc32 == rom.crc32) {
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
			using(IO.WrappedInputStream s = new IO.WrappedInputStream(new FileInfo(file).OpenRead())) {
				var result = datFiles.identify(s, 0);
				Console.WriteLine("Datfile: {0}", result.datfile.name);
				Console.WriteLine("Game: {0}", result.game.name);
				Console.WriteLine("ROM: {0}", result.rom.name);
			}
		}
	}


}
