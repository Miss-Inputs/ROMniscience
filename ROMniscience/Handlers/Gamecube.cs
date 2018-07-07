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
using System.Globalization;
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
			{"dol", "Nintendo GameCube executable"},
			{"tgc", "Nintendo GameCube embedded disc"},
		};

		public override string name => "Nintendo GameCube";

		static bool isGamecubeMagic(byte[] b) {
			return b[0] == 0xc2 && b[1] == 0x33 && b[2] == 0x9f && b[3] == 0x3d;
		}

		static bool isWiiMagic(byte[] b) {
			return b[0] == 0x5d && b[1] == 0x1c && b[2] == 0x9e && b[3] == 0xa3;
		}

		public static void parseGamecubeHeader(ROMInfo info, WrappedInputStream s) {
			string productCode = s.read(4, Encoding.ASCII);
			info.addInfo("Product code", productCode);
			char gameType = productCode[0];
			info.addInfo("Type", gameType, NintendoCommon.DISC_TYPES);
			string shortTitle = productCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char country = productCode[3];
			info.addInfo("Country", country, NintendoCommon.COUNTRIES);

			string maker = s.read(2, Encoding.ASCII);
			info.addInfo("Publisher", maker, NintendoCommon.LICENSEE_CODES);

			int discNumber = s.read() + 1; //Used for multi-disc games, but otherwise 0
			info.addInfo("Disc number", discNumber);

			int version = s.read();
			info.addInfo("Version", version);

			bool audioStreaming = s.read() > 0;
			info.addInfo("Audio streaming?", audioStreaming, true);

			int streamBufferSize = s.read();
			info.addInfo("Streaming buffer size", streamBufferSize, ROMInfo.FormatMode.SIZE, true);

			byte[] unused = s.read(14);
			info.addInfo("Unused", unused, true);

			byte[] wiiMagic = s.read(4);
			info.addInfo("Wii magic", wiiMagic, true);

			byte[] magic = s.read(4);
			info.addInfo("Magic", magic, true);

			info.addInfo("Platform", isGamecubeMagic(magic) ? "GameCube" : isWiiMagic(wiiMagic) ? "Wii" : "Unknown Nintendo optical disc-based system");

			string gameName = s.read(0x60, Encoding.ASCII).TrimEnd('\0');
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

		public static void readFileEntry(byte[] entry, byte[] fnt, int virtualOffset, Dictionary<int, FilesystemDirectory> parentDirs, int index, Dictionary<int, FilesystemFile> filesToAdd, Dictionary<int, int> directoryNextIndexes) {
			bool isDirectory = entry[0] > 0;
			//FIXME: Should check this type flag, sometimes garbage files end up here where they have neither 0 (file) or 1 (directory). It seems to be on beta discs and such so it's probably caused by incorrect header entries causing the FST parsing to stuff up

			int filenameOffset = (entry[1] << 16) | (entry[2] << 8) | entry[3];
			string name = getNullTerminatedString(fnt, filenameOffset);
			if (isDirectory) {
				var dir = new FilesystemDirectory {
					name = name
				};
				parentDirs.Add(index, dir);
				int parentIndex = (entry[4] << 24) | (entry[5] << 16) | (entry[6] << 8) | entry[7];
				int nextIndex = (entry[8] << 24) | (entry[9] << 16) | (entry[10] << 8) | entry[11]; //If I'm understanding all this correctly (I probably am not): The next index out of all the indexes that isn't a child of this directory; i.e. all indexes between here and next index are in this directory
				directoryNextIndexes.Add(index, nextIndex);
				
				//TODO: There has never been a case where parentDirs doesn't contain parentIndex other than the aforementioned garbage files, but we really should make this more robust
				parentDirs[parentIndex].addChild(dir);
			} else {
				int fileOffset = (entry[4] << 24) | (entry[5] << 16) | (entry[6] << 8) | entry[7];
				int fileLength = (entry[8] << 24) | (entry[9] << 16) | (entry[10] << 8) | entry[11];

				var file = new FilesystemFile {
					name = name,
					offset = fileOffset + virtualOffset,
					size = fileLength
				};
				filesToAdd.Add(index, file);
			}
		}

		public static void readRootFST(FilesystemDirectory fs, WrappedInputStream s, int fstOffset, int fstSize, int virtualOffset) {
			//TODO: Check against FST max size in header, and also Wii RAM limit (since this is also used on Wii I think, otherwise I'd say GameCube RAM limit) (88MB for Wii, 24MB system / 43MB total for GameCube) as it would be impossible for real hardware to read a bigger filesystem
			//TODO: Wii shifts offset by one
			long origPos = s.Position;
			Dictionary<int, FilesystemDirectory> parentDirectories = new Dictionary<int, FilesystemDirectory> {
				{0, fs}
			};

			try {
				s.Position = fstOffset + 8;
				//1 byte flag at fstOffset + 0: Filesystem root _must_ be a directory
				//Name offset is irrelevant since the root doesn't really have a name (I think it's just set to 0 anyway)
				//Parent index would also be irrelevant because it doesn't have a parent by definition
				//TODO: Throw error or something if not a directory
				//TODO: Throw error if number of entries * 12 > fstSize
				int numberOfEntries = s.readIntBE();
				int fntOffset = fstOffset + (numberOfEntries * 12);
				int fntSize = fstSize - ((numberOfEntries) * 12);
				s.Position = fntOffset;
				byte[] fnt = s.read(fntSize);
				s.Position = fstOffset + 12;

				//Due to weirdness, we need to figure out which directories these files go in afterwards. It's really weird. I just hope it makes enough sense for whatever purpose you're reading this code for.
				Dictionary<int, FilesystemFile> filesToAdd = new Dictionary<int, FilesystemFile>();
				Dictionary<int, int> directoryNextIndexes = new Dictionary<int, int> {
					{0, numberOfEntries}
				};

				for (int i = 0; i < numberOfEntries - 1; ++i) {
					byte[] entry = s.read(12);
					readFileEntry(entry, fnt, virtualOffset, parentDirectories, i + 1, filesToAdd, directoryNextIndexes);
				}

				//Now that we have the directory structure, add the files to them
				//This sucks, by the way
				//Like it's not that my code sucks (although it probably kinda does), it's like... I hate the GameCube filesystem
				foreach(var fileToAdd in filesToAdd){
					int fileIndex = fileToAdd.Key;
					FilesystemFile file = fileToAdd.Value;

					for(int potentialParentIndex = fileIndex -1; potentialParentIndex >= 0; potentialParentIndex--) {
						if (directoryNextIndexes.ContainsKey(potentialParentIndex)) {
							if (directoryNextIndexes[potentialParentIndex] > fileIndex) {
								var parentDir = parentDirectories[potentialParentIndex];
								parentDir.addChild(file);
								break;
							}
						}
					}
				}
			} finally {
				s.Position = origPos;
			}
		}

		public static FilesystemDirectory readGamecubeFS(WrappedInputStream s, int fstOffset, int fstSize, int virtualOffset) {
			FilesystemDirectory fs = new FilesystemDirectory {
				name = "GameCube Filesystem"
			};
			readRootFST(fs, s, fstOffset, fstSize, virtualOffset);
			return fs;
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
				//FIXME: Sonic Adventure DX (Prototype - Review) (hidden-palace.org).gcz is a completely NTSC USA disc with English gameplay and text and audio and whatnot, but has a Japanese banner for some reason. Not sure if I would consider this to be just the disc being weird, or if it's worth fixing, would need to know what it does on real hardware I guess, but anyway it displays mojibake here (and in Dolphin, fwiw) because it's actually using Shift-JIS text that we're decoding as Latin-1 (because disc region is NTSC-U). Can't use Shift-JIS everywhere because then it'd not work with extended ASCII characters (accented letters mostly) on actual NTSC-U discs
				if (region == 0) { //NTSC-J
					string title = MainProgram.shiftJIS.GetString(banner, 0x1820, 32);
					string title2 = MainProgram.shiftJIS.GetString(banner, 0x1840, 32);
					info.addInfo("Japanese short title", title + Environment.NewLine + title2);

					string title3 = MainProgram.shiftJIS.GetString(banner, 0x1860, 64);
					string title4 = MainProgram.shiftJIS.GetString(banner, 0x18a0, 64);
					info.addInfo("Japanese title", title3 + Environment.NewLine + title4);

					string title5 = MainProgram.shiftJIS.GetString(banner, 0x18e0, 128);
					info.addInfo("Japanese description", title5.Replace("\n", Environment.NewLine));
				} else {
					string title = windows1252.GetString(banner, 0x1820, 32);
					string title2 = windows1252.GetString(banner, 0x1840, 32);
					info.addInfo("English short title", title + Environment.NewLine + title2);

					string title3 = windows1252.GetString(banner, 0x1860, 64);
					string title4 = windows1252.GetString(banner, 0x18a0, 64);
					info.addInfo("English title", title3 + Environment.NewLine + title4);

					string title5 = windows1252.GetString(banner, 0x18e0, 128);
					info.addInfo("English description", title5.Replace("\n", Environment.NewLine));
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
					info.addInfo(languageNames[i] + " description", title5.Replace("\n", Environment.NewLine));
				}
			}
		}

		static void parseGamecubeDisc(ROMInfo info, WrappedInputStream s, int startOffset, int fileOffset, bool isEmbedded) {
			s.Position = startOffset;
			parseGamecubeHeader(info, s);

			s.Position = 0x400 + startOffset;

			int debugMonitorOffset = s.readIntBE();
			info.addInfo("Debug monitor offset", debugMonitorOffset - startOffset, ROMInfo.FormatMode.HEX, true);

			int debugMonitorLoadAddress = s.readIntBE();
			info.addInfo("Debug monitor load address", debugMonitorLoadAddress, ROMInfo.FormatMode.HEX, true);

			byte[] unused2 = s.read(24);
			info.addInfo("Unused 2", unused2, true);

			int bootDOLOffset, fstOffset, fstSize, fstMaxSize;
			if (!isEmbedded) {
				bootDOLOffset = s.readIntBE();
				fstOffset = s.readIntBE();
				fstSize = s.readIntBE();
				fstMaxSize = s.readIntBE();
			} else {
				long pos = s.Position;
				try {
					s.Position = 16;
					fstOffset = s.readIntBE();
					fstSize = s.readIntBE();
					fstMaxSize = s.readIntBE();
					bootDOLOffset = s.readIntBE();
				} finally {
					s.Position = pos;
				}
			}
			info.addInfo("Boot DOL offset", bootDOLOffset, ROMInfo.FormatMode.HEX, true);
			info.addInfo("FST offset", fstOffset, ROMInfo.FormatMode.HEX, true);
			info.addInfo("FST size", fstSize, ROMInfo.FormatMode.SIZE, true);
			info.addInfo("FST maximum size", fstMaxSize, ROMInfo.FormatMode.SIZE, true);


			int userPosition = s.readIntBE();
			info.addInfo("User position", userPosition, ROMInfo.FormatMode.HEX, true);

			int userSize = s.readIntBE();
			info.addInfo("User size", userSize, ROMInfo.FormatMode.SIZE, true);

			s.Position = 0x458 + startOffset;
			int region = s.readIntBE();
			info.addInfo("Region", region, NintendoCommon.REGIONS);

			s.Position = 0x2440 + startOffset;
			string apploaderDate = s.read(16, Encoding.ASCII).Trim('\0');
			if (DateTime.TryParseExact(apploaderDate, "yyyy/MM/dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime date)) {
				info.addInfo("Date", date);
				info.addInfo("Year", date.Year);
				info.addInfo("Month", DateTimeFormatInfo.CurrentInfo.GetMonthName(date.Month));
				info.addInfo("Day", date.Day);
			} else {
				info.addInfo("Date", apploaderDate);
			}

			if (fstOffset > 0 && fstSize > 12 && fstSize < (128 * 1024 * 1024)) {
				var fs = readGamecubeFS(s, fstOffset, fstSize, fileOffset);
				if (fs.contains("opening.bnr")) {
					var banner = (FilesystemFile)fs.getChild("opening.bnr");
					s.Position = banner.offset;
					byte[] bannerData = s.read((int)banner.size);
					parseBanner(info, bannerData, region);
				}
				info.addFilesystem(fs);
			}
		}

		static bool isTGCMagic(byte[] magic) {
			return magic[0] == 0xae && magic[1] == 0x0f && magic[2] == 0x38 && magic[3] == 0xa2;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if ("dol".Equals(file.extension)) {
				//TODO Parse what little info there is out of dol (could use this for boot .dol and Wii homebrew .dol too I guess)
				info.addInfo("Platform", "GameCube");
				return;
			}

			WrappedInputStream s = file.stream;

			byte[] magic = s.read(4);
			if (isTGCMagic(magic)) {
				s.Position = 8;
				int tgcHeaderSize = s.readIntBE();
				info.addInfo("TGC header size", tgcHeaderSize, ROMInfo.FormatMode.SIZE);

				//What the fuck? What does Dolphin know that YAGD doesn't and didn't decide to tell the rest of the class?
				s.Position = 0x24;
				int realOffset = s.readIntBE();
				s.Position = 0x34;
				int virtualOffset = s.readIntBE();
				int fileOffset = realOffset - virtualOffset;
				parseGamecubeDisc(info, s, tgcHeaderSize, fileOffset, true);
			} else {
				parseGamecubeDisc(info, s, 0, 0, false);
			}
		}

	}
}
