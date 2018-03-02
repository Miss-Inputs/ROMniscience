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
			try {
				info.addInfo("Region code", Enum.GetName(typeof(Gamecube.DiscRegions), region));
			} catch (InvalidCastException) {
				info.addInfo("Region code", String.Format("Unknown {0}", region));
			}

			byte[] unused = s.read(12);
			info.addInfo("Unused region data", unused);

			//There seems to be something more to this; for example the USA version of
			//Super Smash Bros. Brawl contains 45 for the ESRB rating instead of
			//13 (it's rated Teen in the USA), which indicates that bit 6 is set to 1 and
			//therefore does something and I don't know what; no other game seems to do that
			//but therefore we'll only use the last 5 bits for now
			//(Possibly bit 6 indicates online interactivity?)

			int cero = s.read();
			if (cero != 0x80) {
				info.addInfo("CERO rating", cero & 0x1f, DS.CERO_RATINGS);
			}

			int esrb = s.read();
			if (esrb != 0x80) {
				info.addInfo("ESRB rating", esrb & 0x1f, DS.ESRB_RATINGS);
			}

			int reservedRating = s.read();
			if (reservedRating != 0x80) {
				info.addInfo("<reserved> rating", reservedRating);
			}

			int usk = s.read();
			if (usk != 0x80) {
				info.addInfo("USK rating", usk & 0x1f, DS.USK_RATINGS);
			}

			int pegi = s.read();
			if (pegi != 0x80) {
				info.addInfo("PEGI (Europe) rating", pegi & 0x1f, DS.PEGI_RATINGS);
			}


			int finland = s.read();
			if (finland != 0x80) {
				info.addInfo("Finland rating", finland & 0x1f);
			}

			int pegiPortugal = s.read();
			if (pegiPortugal != 0x80) {
				info.addInfo("PEGI (Portugal) rating", pegiPortugal & 0x1f, DS.PEGI_PORTUGAL_RATINGS);
			}

			int pegiUK = s.read();
			if (pegiUK != 0x80) {
				info.addInfo("PEGI rating", pegiUK & 0x1f, DS.PEGI_UK_RATINGS);
			}

			int agcb = s.read();
			if (agcb != 0x80) {
				info.addInfo("AGCB rating", agcb & 0x1f, DS.AGCB_RATINGS);
			}

			int grb = s.read();
			if (grb != 0x80) {
				info.addInfo("GRB rating", grb & 0x1f, DS.GRB_RATINGS);
			}

			//Filled with 0x80, maybe these are other countries with rating systems that no game anyone knows of has a rating for?
			byte[] unused2 = s.read(6);
			info.addInfo("Unused region data 2", unused2, true);
		}
	}
}
