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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class Gamecube : Handler {
		//While this is a disc-based format, we don't need to do anything funny to implement it since
		//it uses non-standard discs which are only represented in one format (well, .gcz too, but there's
		//no fiddling around with sectors or whatever), so it all works out well and everyone's happy
		//http://hitmen.c02.at/files/yagcd/yagcd/frames.html
		//http://wiibrew.org/wiki/Wii_Disc
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"iso", "Nintendo GameCube disc"},
			{"gcm", "Nintendo GameCube disc"},
		};
		//TODO: I guess it couldn't hurt to do .dol, but that's not a priority

		public override string name => "Nintendo GameCube";

		static bool isGamecubeMagic(byte[] b) {
			return b[0] == 0xc2 && b[1] == 0x33 && b[2] == 0x9f && b[3] == 0x3d;
		}

		static bool isWiiMagic(byte[] b) {
			return b[0] == 0x5d && b[1] == 0x1c && b[2] == 0x9e && b[3] == 0xa3;
		}

		public enum DiscRegions : int {
			NTSC_J = 0,
			NTSC_U = 1,
			PAL = 2,
			NTSC_K = 4, //Wii only
		}

		public static void parseGamecubeHeader(ROMInfo info, InputStream s) {
			string productCode = s.read(4, Encoding.ASCII);
			info.addInfo("Product code", productCode);
			char gameType = productCode[0];
			info.addInfo("Type", gameType, NintendoCommon.DISC_TYPES);
			string shortTitle = productCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char region = productCode[3];
			info.addInfo("Region", region, NintendoCommon.REGIONS);

			string maker = s.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", maker, NintendoCommon.LICENSEE_CODES);

			int discNumber = s.read() + 1; //Used for multi-disc games, but otherwise 0
			info.addInfo("Disc number", discNumber);

			int version = s.read();
			info.addInfo("Version", version);

			bool audioStreaming = s.read() > 0;
			info.addInfo("Audio streaming?", audioStreaming, true);

			int streamBufferSize = s.read();
			info.addInfo("Streaming buffer size", streamBufferSize, true);

			byte[] unused = s.read(14);
			info.addInfo("Unused", unused, true);

			byte[] wiiMagic = s.read(4);
			info.addInfo("Wii magic", wiiMagic, true);

			byte[] magic = s.read(4);
			info.addInfo("Magic", magic, true);

			info.addInfo("Platform", isGamecubeMagic(magic) ? "GameCube" : isWiiMagic(wiiMagic) ? "Wii" : "Unknown");

			string gameName = s.read(0x3e0, Encoding.ASCII).TrimEnd('\0'); //Okay, there's no way that can be right
			info.addInfo("Internal name", gameName);

		}

		static string getNullTerminatedString(byte[] b, int offset) {
			StringBuilder sb = new StringBuilder();
			for (int i = offset; i < b.Length; ++i) {
				if (b[i] == 0) {
					break;
				}
				sb.Append(Encoding.ASCII.GetString(b, i, 1));
			}
			return sb.ToString();
		}

		struct FSTEntry {
			//Since this is all just looking for opening.bnr, which is at the root, we're ignoring directories and whatnot
			public bool isDirectory;
			public int filenameOffset;
			public int fileOffset;
			public int fileLength;
			public string name;

			public FSTEntry(byte[] b, byte[] filenameTable) {
				isDirectory = b[0] > 0;
				filenameOffset = (b[1] << 16) | (b[2] << 8) | b[3];
				fileOffset = (b[4] << 24) | (b[5] << 16) | (b[6] << 8) | b[7];
				fileLength = (b[8] << 24) | (b[9] << 16) | (b[10] << 8) | b[11];
				if (filenameOffset >= filenameTable.Length) {
					name = String.Format("<Invalid offset: {0}>", filenameOffset);
				} else {
					try {
						name = getNullTerminatedString(filenameTable, filenameOffset);
					} catch (Exception ex) {
						name = String.Format("<Exception: {0}>", ex);
					}
				}
			}
		}

		IList<FSTEntry> parseFST(byte[] fst) {
			int numEntries = (fst[8] << 24) | (fst[9] << 16) | (fst[10] << 8) | fst[11];

			byte[] filenameTable = new byte[fst.Length - (numEntries * 12)];
			Array.Copy(fst, numEntries * 12, filenameTable, 0, filenameTable.Length);

			IList<FSTEntry> list = new List<FSTEntry>();
			for (int i = 1; i < numEntries; ++i) {
				byte[] temp = new byte[12];
				Array.Copy(fst, i * 12, temp, 0, 12);
				list.Add(new FSTEntry(temp, filenameTable));
			}
			return list;
		}

		byte[] getBanner(InputStream s, IList<FSTEntry> fst) {
			foreach (var entry in fst) {
				if ("opening.bnr".Equals(entry.name)) {
					s.Position = entry.fileOffset;
					return s.read(entry.fileLength);
				}
			}
			return null;
		}

		//TODO Refactor these 3 methods into one
		static int convert3BitColor(int c) {
			int n = c * (256 / 0b111);
			return n > 255 ? 255 : n;
		}
		static int convert4BitColor(int c) {
			int n = c * (256 / 0b1111);
			return n > 255 ? 255 : n;
		}
		static int convert5BitColor(int c) {
			int n = c * (256 / 0b11111);
			return n > 255 ? 255 : n;
		}

		static Bitmap convertBannerIcon(byte[] banner) {
			Bitmap b = new Bitmap(96, 32);
			using (Graphics g = Graphics.FromImage(b)) {
				g.FillRectangle(Brushes.Black, 0, 0, b.Width, b.Height);
			}

			int i = 0;
			for (int y = 0; y < 32; y += 4) {
				for (int x = 0; x < 96; x += 4) {
					for (int tile_y = 0; tile_y < 4; ++tile_y) {
						for (int tile_x = 0; tile_x < 8; tile_x += 2) {
							int offset = 32 + i + tile_x;
							int color = (banner[offset] << 8) | banner[offset + 1];

							//I hate RGB5A3
							int alpha, red, green, blue;
							if ((color & 32768) == 0) {
								alpha = convert3BitColor((color & 0b0111_0000_0000_0000) >> 12);
								red = convert4BitColor((color & 0b0000_1111_0000_0000) >> 8);
								green = convert4BitColor((color & 0b0000_0000_1111_0000) >> 4);
								blue = convert4BitColor(color & 0b0000_0000_0000_1111);
							} else {
								alpha = 255;
								red = convert5BitColor((color & 0b0_11111_00000_00000) >> 10);
								green = convert5BitColor((color & 0b0_00000_11111_00000) >> 5);
								blue = convert5BitColor(color & 0b0_00000_00000_11111);
							}
							b.SetPixel(x + (tile_x / 2), y + tile_y, Color.FromArgb(alpha, red, green, blue));
						}
						i += 8;
					}
				}
			}
			return b;
		}

		static string[] languageNames = { "English", "German", "French", "Spanish", "Italian", "Dutch" };
		static Encoding windows1252 = Encoding.GetEncoding(1252);
		//It probably uses Latin-1 actually, but this is the closest we can have without potentially breaking things on non-standard .NET platforms

		static void parseBanner(ROMInfo info, byte[] banner, int region) {
			string bannerMagic = Encoding.ASCII.GetString(banner, 0, 4);
			info.addInfo("Banner magic", bannerMagic, true);

			info.addInfo("Icon", convertBannerIcon(banner));

			if ("BNR1".Equals(bannerMagic)) {
				//TODO These variable names fuckin suck and this whole thing needs refactoring
				if (region == (int)DiscRegions.NTSC_J) {
					string title = MainProgram.shiftJIS.GetString(banner, 0x1820, 32);
					string title2 = MainProgram.shiftJIS.GetString(banner, 0x1840, 32);
					info.addInfo("Japanese short title", title + Environment.NewLine + title2);

					string title3 = MainProgram.shiftJIS.GetString(banner, 0x1860, 64);
					string title4 = MainProgram.shiftJIS.GetString(banner, 0x18a0, 64);
					info.addInfo("Japanese title line", title3 + Environment.NewLine + title4);

					string title5 = MainProgram.shiftJIS.GetString(banner, 0x18e0, 128);
					info.addInfo("Japanese description", title5);
				} else {
					string title = windows1252.GetString(banner, 0x1820, 32);
					string title2 = windows1252.GetString(banner, 0x1840, 32);
					info.addInfo("English short title", title + Environment.NewLine + title2);

					string title3 = windows1252.GetString(banner, 0x1860, 64);
					string title4 = windows1252.GetString(banner, 0x18a0, 64);
					info.addInfo("English title", title3 + Environment.NewLine + title4);

					string title5 = windows1252.GetString(banner, 0x18e0, 128);
					info.addInfo("English description", title5);
				}
			} else if ("BNR2".Equals(bannerMagic)) {
				int baseOffset = 0x1820;

				for (int i = 0; i < 5; ++i) {
					string title = windows1252.GetString(banner, baseOffset + (i * 320), 32);
					string title2 = windows1252.GetString(banner, baseOffset + (i * 320) + 32, 32);
					info.addInfo(languageNames[i] + " short title", title + Environment.NewLine + title2);

					string title3 = windows1252.GetString(banner, baseOffset + (i * 320) + 64, 64);
					string title4 = windows1252.GetString(banner, baseOffset + (i * 320) + 128, 64);
					info.addInfo(languageNames[i] + " title", title3 + Environment.NewLine + title4);

					string title5 = windows1252.GetString(banner, baseOffset + (i * 320) + 192, 128);
					info.addInfo(languageNames[i] + " description", title5);
				}
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			InputStream s = file.stream;
			parseGamecubeHeader(info, s);

			s.Position = 0x400;

			int debugMonitorOffset = s.readIntBE();
			info.addInfo("Debug monitor offset", debugMonitorOffset, true);

			int debugMonitorLoadAddress = s.readIntBE();
			info.addInfo("Debug monitor load address", debugMonitorLoadAddress, true);

			byte[] unused2 = s.read(24);
			info.addInfo("Unused 2", unused2, true);

			int bootDOLOffset = s.readIntBE();
			info.addInfo("Boot DOL offset", bootDOLOffset, true);

			int fstOffset = s.readIntBE();
			info.addInfo("FST offset", fstOffset, true);

			int fstSize = s.readIntBE();
			info.addInfo("FST size", fstSize, ROMInfo.FormatMode.SIZE, true);

			int fstMaxSize = s.readIntBE();
			info.addInfo("FST maximum size", fstMaxSize, ROMInfo.FormatMode.SIZE, true);

			int userPosition = s.readIntBE();
			info.addInfo("User position", userPosition, true);

			int userSize = s.readIntBE();
			info.addInfo("User size", userSize, ROMInfo.FormatMode.SIZE, true);

			s.Position = 0x458;
			int region = s.readIntBE();
			try {
				info.addInfo("Region code", Enum.GetName(typeof(DiscRegions), region));
			} catch (InvalidCastException) {
				info.addInfo("Region code", String.Format("Unknown {0}", region));
			}

			s.Position = 0x2440;
			string apploaderDate = s.read(9, Encoding.ASCII);
			info.addInfo("Apploader date", apploaderDate);

			if (fstOffset > 0 && fstSize > 12 && fstSize < (128 * 1024 * 1024)) {
				s.Position = fstOffset;
				byte[] fst = s.read(fstSize);

				byte[] banner = getBanner(s, parseFST(fst));
				if (banner != null) {
					parseBanner(info, banner, region);
				}
			}
		}
	}
}
