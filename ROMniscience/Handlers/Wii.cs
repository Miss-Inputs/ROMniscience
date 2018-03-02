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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class Wii : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"iso", "Nintendo Wii disc"},
		};

		public override string name => "Wii";

		//Japan, USA, Reservedland, Germany, Europe, Finland, Portgual, UK, Australia, and South Korea respectively (although Finland uses normal European PEGI now as I understand it)
		readonly static Tuple<string, IDictionary<int, string>>[] RATING_NAMES = {
			new Tuple<string, IDictionary<int, string>>("CERO", DS.CERO_RATINGS),
			new Tuple<string, IDictionary<int, string>>("ERSB", DS.ESRB_RATINGS),
			new Tuple<string, IDictionary<int, string>>("<reserved>", null),
			new Tuple<string, IDictionary<int, string>>("USK", DS.USK_RATINGS),
			new Tuple<string, IDictionary<int, string>>("PEGI (Europe)", DS.PEGI_RATINGS),
			new Tuple<string, IDictionary<int, string>>("FBFC", null),
			new Tuple<string, IDictionary<int, string>>("PEGI (Portgual)", DS.PEGI_PORTUGAL_RATINGS),
			new Tuple<string, IDictionary<int, string>>("PEGI", DS.PEGI_UK_RATINGS),
			new Tuple<string, IDictionary<int, string>>("AGCB", DS.AGCB_RATINGS),
			new Tuple<string, IDictionary<int, string>>("GRB", DS.GRB_RATINGS),
		};
		public static void parseRatings(ROMInfo info, byte[] ratings) {
			//Seems to be kinda different than DSi ratings
			//There seems to be something more to this; for example the USA version of
			//Super Smash Bros. Brawl contains 45 for the ESRB rating instead of
			//13 (it's rated Teen in the USA), which indicates that bit 5 is set to 1 and
			//therefore does something and I don't know what; as does Bomberman Blast
			//but therefore we'll only use bits 0-4 for now
			//(Possibly bit 5 indicates online interactivity?)

			for (int i = 0; i < 16; ++i) {
				int rating = ratings[i];
				string ratingName;
				if (i >= RATING_NAMES.Length) {
					ratingName = "Unknown rating " + (i - RATING_NAMES.Length);
				} else {
					ratingName = RATING_NAMES[i].Item1 + " rating";
				}

				if ((rating & 0x40) > 0) {
					info.addInfo(ratingName + " bit 6", rating & 0x40, true);
				}
				if ((rating & 0x20) > 0) {
					info.addInfo(ratingName + " bit 5", rating & 0x20, true);
				}

				if (rating != 0x80) {
					int ratingValue = rating & 0x1f;
					if (i < RATING_NAMES.Length && RATING_NAMES[i].Item2 != null) {
						info.addInfo(ratingName, ratingValue, RATING_NAMES[i].Item2);
					} else {
						info.addInfo(ratingName, ratingValue);
					}

				}
			}

		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			InputStream s = file.stream;
			Gamecube.parseGamecubeHeader(info, s);

			s.Position = 0x40000;

			int totalPartitions = 0;
			bool containsUpdate = false;
			int partitionIndex = 0;
			for (int partitionGroup = 0; partitionGroup < 4; ++partitionGroup) {
				int partitions = s.readIntBE();
				//info.addInfo("Number of partitions", partitions);
				totalPartitions += partitions;

				int partitionInfoOffset = s.readIntBE() << 2;
				for (int i = 0; i < partitions; ++i) {
					long pos = s.Position;
					try {
						s.Position = partitionInfoOffset + (i * 8) + 4;

						byte[] partitionType = s.read(4);
						string type;
						if (partitionType[0] == 0) {
							if (partitionType[3] == 0) {
								type = "Game data";
							} else if (partitionType[3] == 1) {
								containsUpdate = true;
								type = "Update";
							} else if (partitionType[3] == 2) {
								type = "Channel";
							} else {
								type = String.Format("Unknown ({0})", partitionType[3]);
							}
						} else {
							type = Encoding.ASCII.GetString(partitionType);
						}
						info.addInfo(String.Format("Partition {0} type", partitionIndex), type, true);
						//TODO: Read TMD from partition
					} finally {
						s.Position = pos;
					}
					++partitionIndex;
				}
			}
			info.addInfo("Number of partitions", totalPartitions);
			info.addInfo("Contains update partition", containsUpdate);

			s.Position = 0x4e000;
			int region = s.readIntBE();
			info.addInfo("Region code", region, NintendoCommon.DISC_REGIONS);

			byte[] unused = s.read(12);
			info.addInfo("Unused region data", unused);

			byte[] ratings = s.read(16);
			parseRatings(info, ratings);
		}
	}
}
