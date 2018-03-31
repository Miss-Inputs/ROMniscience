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
			WrappedInputStream s = file.stream;
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
			info.addInfo("Unused region data", unused, true);

			byte[] ratings = s.read(16);
			NintendoCommon.parseRatings(info, ratings, false);
		}
	}
}
