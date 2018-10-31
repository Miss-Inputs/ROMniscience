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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class _3DS: Handler {
		//https://www.3dbrew.org/wiki/NCCH
		//https://www.3dbrew.org/wiki/NCSD
		//https://3dbrew.org/wiki/3DSX_Format
		//https://www.3dbrew.org/wiki/Serials
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"3ds", "Nintendo 3DS cart"},
			{"cci", "Nintendo 3DS CTR Cart Image"}, //Actual format of .3ds
			{"3dsx", "Nintendo 3DS homebrew"}, //Often seen with .smdh to store metadata
			{"cia", "3DS downloadable title (CTR Importable Archive)"},
			{"cfa", "Nintendo 3DS CTR File Archive"},
			{"csu", "Nintendo 3DS CTR System Update"},
			{"cxi", "Nintendo 3DS CTR Executable Image"},
		};
		public override string name => "Nintendo 3DS";

		const int MEDIA_UNIT = 0x200;

		[Flags]
		public enum RegionFlags : uint {
			Japan = 1 << 0,
			USA = 1 << 1,
			Europe = 1 << 2,
			Australia = 1 << 3, //Ends up going unused (PAL games have this region but it isn't checked), as the European 3DS ends up being sold here
			China = 1 << 4,
			Korea = 1 << 5,
			Taiwan = 1 << 6,
		}

		public static readonly IDictionary<char, string> CATEGORIES = new Dictionary<char, string> {
			{'P', "Cartridge"}, //Or downloadable version
			{'N', "Digital"}, //eShop games, or system applications
			{'M', "DLC"},
			{'T', "Trial"}, //eShop demo
			{'U', "Update"},
		};

		public static readonly IDictionary<char, string> GAME_TYPES = new Dictionary<char, string> {
			{'A', "Game"},
			{'B', "Game (B)"},
			{'C', "New 3DS exclusive"}, //Also in the default product code
			{'E', "Game (Card2)"}, //Would be nice if I knew what that actually meant
			{'H', "Built-in application"},
			{'J', "eShop"},
			{'K', "eShop (K)"}, //Seen on Pokemon Shuffle and Pokemon Rumble World.. hmm. Might just be a case of running out of unique product codes
			{'P', "GBA Virtual Console"},
			{'Q', "GBC Virtual Console"},
			{'S', "3D Classics"}, //Seen on Shin Chan Vol 1, so that's probably wrong
			{'T', "NES Virtual Console"},
			//N: Seen in Pokemon ORAS Special Demo, Pokemon Dream Radar, Poke Transporter (but not Pokemon Bank, which is J)
			//R: Seen in Pokemon Blue, so could mean "GB VC" or "VC with link cable stuff"
		};

		private static string combinePrefix(string prefix, string s, bool preserveCase = false) {
			if (String.IsNullOrEmpty(prefix)) {
				return s;
			} else {
				if (preserveCase) {
					return prefix + " " + s;
				} else {
					return prefix + " " + char.ToLowerInvariant(s[0]) + s.Substring(1);
				}
			}
		}

		private static FilesystemFile getIconFile(FilesystemDirectory partition) {
			if (partition.contains("ExeFS")) {
				var exefs = (FilesystemDirectory)partition.getChild("ExeFS");
				if (exefs.contains("icon")) {
					return (FilesystemFile)exefs.getChild("icon");
				}
			}
			return null;
		}

		public static void parseNCCH(ROMInfo info, WrappedInputStream s, string prefix, long offset = 0) {
			//"NCCD" magic at 0x100
			s.Position = offset + 0x104;
			uint contentSize = (uint)s.readIntLE() * MEDIA_UNIT;
			info.addInfo(combinePrefix(prefix, "Content size"), contentSize, ROMInfo.FormatMode.SIZE);

			byte[] partitionID = s.read(8);
			info.addInfo(combinePrefix(prefix, "Partition ID"), partitionID);

			string makerCode = s.read(2, Encoding.ASCII);
			info.addInfo(combinePrefix(prefix, "Publisher"), makerCode, NintendoCommon.LICENSEE_CODES);

			short version = s.readShortLE();
			info.addInfo(combinePrefix(prefix, "NCCH version", true), version); //Seemingly not quite the same as card version... always 2?

			//Some stuff about a hash

			s.Position = offset + 0x118;
			byte[] programID = s.read(8);
			info.addInfo(combinePrefix(prefix, "Program ID"), programID);

			//Some stuff about a reserved and a logo SHA-256
			s.Position = offset + 0x150;
			string productCode = s.read(16, Encoding.ASCII).TrimEnd('\0'); //Not just ABBC anymore! It's now CTR-P-ABBC... albeit that's 10 chars?
			info.addInfo(combinePrefix(prefix, "Product code"), productCode);
			if (productCode.Length == 10 && !productCode.Equals("CTR-P-CTAP")) {
				info.addInfo(combinePrefix(prefix, "Category"), productCode[4], CATEGORIES);
				info.addInfo(combinePrefix(prefix, "Type"), productCode[6], GAME_TYPES);
				info.addInfo(combinePrefix(prefix, "Short title"), productCode.Substring(7, 2));
				info.addInfo(combinePrefix(prefix, "Country"), productCode[9], NintendoCommon.COUNTRIES);
			}
			s.Position = offset + 0x180;
			int extendedHeaderSize = s.readIntLE(); //NOT in media units!
			info.addInfo(combinePrefix(prefix, "Extended header size"), extendedHeaderSize, ROMInfo.FormatMode.SIZE);

			//Something about a reserved and an extended header SHA-256

			s.Position = offset + 0x188;
			byte[] flags = s.read(8);

			bool isData = (flags[5] & 1) > 0;
			bool isExecutable = (flags[5] & 2) > 0;
			bool isCXI = !(isData & !isExecutable);
			bool isSystemUpdate = (flags[5] & 4) > 0;
			bool isElectronicManual = (flags[5] & 8) > 0;
			bool isTrial = (flags[5] & 16) > 0; //This isn't set on trials...
			bool isZeroKeyEncrypted = (flags[7] & 1) > 0;
			bool isDecrypted = (flags[7] & 4) > 0;

			info.addInfo(combinePrefix(prefix, "Is CXI"), isCXI);
			info.addInfo(combinePrefix(prefix, "Is data"), isData);
			info.addInfo(combinePrefix(prefix, "Is executable"), isExecutable);
			info.addInfo(combinePrefix(prefix, "Is system update"), isSystemUpdate);
			info.addInfo(combinePrefix(prefix, "Is electronic manual"), isElectronicManual); //TODO This just goes to show we should make some of this stuff extra if it's not the main thing ("Electronic manual is electronic manual" = true)
			info.addInfo(combinePrefix(prefix, "Is trial"), isTrial);
			info.addInfo(combinePrefix(prefix, "Is encrypted with 0 key"), isZeroKeyEncrypted);
			info.addInfo(combinePrefix(prefix, "Is decrypted"), isDecrypted);

			long plainRegionOffset = (uint)s.readIntLE() * MEDIA_UNIT + offset;
			long plainRegionSize = (uint)s.readIntLE() * MEDIA_UNIT;
			long logoRegionOffset = (uint)s.readIntLE() * MEDIA_UNIT + offset;
			long logoRegionSize = (uint)s.readIntLE() * MEDIA_UNIT;
			long exeFSOffset = (uint)s.readIntLE() * MEDIA_UNIT + offset;
			long exeFSSize = (uint)s.readIntLE() * MEDIA_UNIT;
			s.Seek(8, SeekOrigin.Current); //Skip over ExeFS hash region size and reserved
			long romFSOffset = (uint)s.readIntLE() * MEDIA_UNIT + offset;
			long romFSSize = (uint)s.readIntLE() * MEDIA_UNIT;

			FilesystemDirectory partition = new FilesystemDirectory() {
				name = prefix ?? "Main partition"
			};
			info.addFilesystem(partition);

			if (plainRegionSize > 0) {
				info.addInfo(combinePrefix(prefix, "Plain region offset"), plainRegionOffset, ROMInfo.FormatMode.HEX);
				info.addInfo(combinePrefix(prefix, "Plain region size"), plainRegionSize, ROMInfo.FormatMode.SIZE);

				parsePlainRegion(info, s, prefix, plainRegionOffset);
			}

			if(exeFSSize > 0) {
				info.addInfo(combinePrefix(prefix, "ExeFS offset", true), exeFSOffset, ROMInfo.FormatMode.HEX);
				info.addInfo(combinePrefix(prefix, "ExeFS size", true), exeFSSize, ROMInfo.FormatMode.SIZE);

				if (isDecrypted) {
					//If the ROM is encrypted, it'll be all garbled, so there's not much we can do there...
					parseExeFS(info, s, partition, exeFSOffset);
				}
			}
			if(romFSSize > 0) {
				info.addInfo(combinePrefix(prefix, "RomFS offset", true), romFSOffset, ROMInfo.FormatMode.HEX);
				info.addInfo(combinePrefix(prefix, "RomFS size", true), romFSSize, ROMInfo.FormatMode.SIZE);

				if (isDecrypted) {
					parseRomFS(info, s, partition, romFSOffset);
				}
			}

			var icon = getIconFile(partition);
			if(icon != null) {
				parseSMDH(info, s, prefix, icon.offset);
			}

			if (isCXI & isDecrypted) {
				s.Position = offset + 0x200;
				string appTitle = s.read(8, Encoding.ASCII).TrimEnd('\0');
				info.addInfo(combinePrefix(prefix, "Internal name"), appTitle);

				s.Position = offset + 0x20d;
				int extendedFlags = s.read();
				info.addInfo(combinePrefix(prefix, "Compressed ExeFS code"), (extendedFlags & 1) > 0, true);
				info.addInfo(combinePrefix(prefix, "SD application", true), (extendedFlags & 2) > 0, true);

				short remasterVersion = s.readShortLE();
				info.addInfo(combinePrefix(prefix, "Remaster version"), remasterVersion, true);

				s.Position = offset + 0x450;
				//This makes no sense - it should be at offset + 0x240 according to documentation, but it isn't
				var dependencyList = new List<string>();
				for (int i = 0; i < 48; ++i) {
					string dependency = s.read(8, Encoding.ASCII).TrimEnd('\0');
					if (dependency.Length == 0) {
						break;
					}
					dependencyList.Add(dependency);
				}
				info.addInfo("Dependencies", String.Join(", ", dependencyList), true);

				s.Position = offset + 0x3c0;
				//TODO: Add readLongLE and readLongBE to WrappedInputStream
				//This wouldn't work if you used this on a big endian C# environment I would think
				ulong saveDataSize = BitConverter.ToUInt64(s.read(8), 0);
				info.addInfo("Save size", saveDataSize, ROMInfo.FormatMode.SIZE);

				//TODO: Access control info stuff https://www.3dbrew.org/wiki/NCCH/Extended_Header

			}
		}

		public static void parsePlainRegion(ROMInfo info, WrappedInputStream s, string prefix, long offset = 0) {
			s.Position = offset;

			//TODO: Only read up to plainRegionSize bytes
			List<string> libs = new List<string>();
			while (true) {
				int c = s.read();
				if (c == -1 | c == 0) {
					break;
				}

				StringBuilder sb = new StringBuilder();
				sb.Append((char)c);
				while (true) {
					int cc = s.read();
					if(cc == -1 | cc == 0) {
						break;
					}
					sb.Append((char)cc);
				}
				libs.Add(sb.ToString());
			}
			info.addInfo(combinePrefix(prefix, "Libraries used"), String.Join(", ", libs));
		}

		static int roundUpToMultiple(int n, int f) {
			return (int)(Math.Ceiling((double)n / f) * f);
		}

		private static void iterateRomFSEntry(WrappedInputStream s, FilesystemDirectory currentDirectory, byte[] metadataEntry, long directoryMetadataOffset, long fileMetadataOffset, long fileOffset) {
			uint firstChildDirectoryOffset = BitConverter.ToUInt32(metadataEntry, 8);
			uint firstFileOffset = BitConverter.ToUInt32(metadataEntry, 12);

			if (firstChildDirectoryOffset != 0xffffffff) {
				s.Position = directoryMetadataOffset + firstChildDirectoryOffset;
				while (true) {
					byte[] childDirectoryMetadata = s.read(24);
					uint nextSiblingDirectory = BitConverter.ToUInt32(childDirectoryMetadata, 4);

					uint nameLength = BitConverter.ToUInt32(childDirectoryMetadata, 20);
					string name = s.read((int)nameLength, Encoding.Unicode);

					FilesystemDirectory childDir = new FilesystemDirectory() {
						name = name
					};
					currentDirectory.addChild(childDir);
					iterateRomFSEntry(s, childDir, childDirectoryMetadata, directoryMetadataOffset, fileMetadataOffset, fileOffset);

					if(nextSiblingDirectory == 0xffffffff) {
						break;
					}
					s.Position = directoryMetadataOffset + nextSiblingDirectory;
				}
			}

			if (firstFileOffset != 0xffffffff) {
				s.Position = fileMetadataOffset + firstFileOffset;
				while (true) {
					byte[] childFileMetadata = s.read(32);
					uint nextSiblingFile = BitConverter.ToUInt32(childFileMetadata, 4);
					ulong childFileOffset = BitConverter.ToUInt64(childFileMetadata, 8);
					ulong childFileSize = BitConverter.ToUInt64(childFileMetadata, 16);

					uint nameLength = BitConverter.ToUInt32(childFileMetadata, 28);
					string name = s.read((int)nameLength, Encoding.Unicode);

					currentDirectory.addChild(name, (long)childFileOffset + fileOffset, (long)childFileSize);

					if (nextSiblingFile == 0xffffffff) {
						break;
					}
					s.Position = fileMetadataOffset + nextSiblingFile;
				}
			}
		}

		public static void parseRomFS(ROMInfo info, WrappedInputStream s, FilesystemDirectory partition, long offset = 0) {
			FilesystemDirectory romfs = new FilesystemDirectory() {
				name = "RomFS"
			};

			s.Position = offset;
			string magic = s.read(4, Encoding.ASCII);
			if (!magic.Equals("IVFC")) {
				return;
			}

			s.Position = offset + 8;
			int masterHashSize = s.readIntLE();
			s.Position = offset + 0x4c;
			int level3BlockSize = s.readIntLE();
			int level3HashBlockSize = 1 << level3BlockSize;
			int level3Offset = roundUpToMultiple(0x60 + masterHashSize, level3HashBlockSize);

			s.Position = offset + level3Offset + 4;
			//Header size should be 0x28...
			int directoryHashTableOffset = s.readIntLE();
			int directoryHashTableLength = s.readIntLE();
			int directoryMetadataOffset = s.readIntLE();
			int directoryMetadataLength = s.readIntLE();

			int fileHashTableOffset = s.readIntLE();
			int fileHashTableLength = s.readIntLE();
			int fileMetadataOffset = s.readIntLE();
			int fileMetadataLength = s.readIntLE();

			int fileDataOffset = s.readIntLE();

			long baseOffset = offset + level3Offset;
			s.Position = baseOffset + directoryMetadataOffset;
			byte[] rootDirectory = s.read(directoryMetadataLength);
			iterateRomFSEntry(s, romfs, rootDirectory, baseOffset + directoryMetadataOffset, baseOffset + fileMetadataOffset, baseOffset + fileDataOffset);

			partition.addChild(romfs);
		}

		public static void parseExeFS(ROMInfo info, WrappedInputStream s, FilesystemDirectory partition, long offset = 0) {
			FilesystemDirectory exefs = new FilesystemDirectory() {
				name = "ExeFS"
			};

			s.Position = offset;
			for(int i = 0; i < 10; ++i) {
				string filename = s.read(8, Encoding.ASCII).TrimEnd('\0');
				long fileOffset = (uint)s.readIntLE() + offset + 0x200; //Add ExeFS header as well
				long fileSize = (uint)s.readIntLE();

				if (fileSize > 0) {
					exefs.addChild(filename, fileOffset, fileSize);
				}
			}
			partition.addChild(exefs);
		}

		static readonly string[] titleLanguages = {"Japanese", "English", "French", "German", "Italian", "Spanish", "Simplified Chinese", "Korean", "Dutch", "Portugese", "Russian", "Traditional Chinese", "Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5"};
		public static void parseSMDH(ROMInfo info, WrappedInputStream s, string prefix, long offset = 0) {
			s.Position = offset + 4;
			short version = s.readShortLE();
			info.addInfo("SMDH version", version);

			s.Position = offset + 8;

			for (int i = 0; i < 16; ++i) {
				byte[] shortNameBytes = s.read(0x80);
				if (shortNameBytes.All(b => b == 0)) {
					s.Seek(0x180, SeekOrigin.Current);
					continue;
				}

				string shortName = Encoding.Unicode.GetString(shortNameBytes).TrimEnd('\0').Replace("\n", Environment.NewLine);
				string longName = s.read(0x100, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				string publisherName = s.read(0x80, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);

				string key = combinePrefix(prefix, titleLanguages[i], true);
				info.addInfo(key + " short name", shortName);
				info.addInfo(key + " long name", longName);
				info.addInfo(key + " publisher name", publisherName);
			}

			byte[] ratings = s.read(16);
			NintendoCommon.parseRatings(info, ratings, true);

			int region = s.readIntLE();
			if (region == 0x7fffffff) {
				info.addInfo(combinePrefix(prefix, "Region"), "Region free");
			} else {
				info.addInfo(combinePrefix(prefix, "Region"), Enum.ToObject(typeof(RegionFlags), region).ToString());
			}

			//Stuff used in online connectivity
			byte[] matchMakerID = s.read(4);
			byte[] matchMakerBitID = s.read(8);
			info.addInfo(combinePrefix(prefix, "Match maker ID"), matchMakerID, true);
			info.addInfo(combinePrefix(prefix, "Match maker BIT ID"), matchMakerBitID, true);

			int flags = s.readIntLE();
			info.addInfo(combinePrefix(prefix, "Visible on Home Menu"), (flags & 1) > 0);
			info.addInfo(combinePrefix(prefix, "Auto-boot"), (flags & 2) > 0);
			info.addInfo(combinePrefix(prefix, "Uses 3D"), (flags & 4) > 0); //For parental controls use, doesn't actually stop an application using 3D
			info.addInfo(combinePrefix(prefix, "Requires EULA"), (flags & 8) > 0);
			info.addInfo(combinePrefix(prefix, "Autosave on exit"), (flags & 16) > 0);
			info.addInfo(combinePrefix(prefix, "Uses extended banner"), (flags & 32) > 0);
			info.addInfo(combinePrefix(prefix, "Region rating required"), (flags & 64) > 0); //weh
			info.addInfo(combinePrefix(prefix, "Warn about save data"), (flags & 128) > 0); //Just changes the warning when closing an application to "Do you want to close blah (Unsaved data will be lost.)"
			info.addInfo(combinePrefix(prefix, "Record application usage"), (flags & 256) > 0); //This is unset on developer/customer service tools to stop them showing up in the activity log, apparently
			info.addInfo(combinePrefix(prefix, "Disable SD card save backup"), (flags & 1024) > 0);
			info.addInfo(combinePrefix(prefix, "New 3DS exclusive"), (flags & 4096) > 0);

			int eulaMajorVersion = s.read();
			int eulaMinorVersion = s.read();
			info.addInfo("EULA version", eulaMajorVersion + "." + eulaMinorVersion);

			s.Position = offset + 0x2034;
			byte[] cecID = s.read(4);
			info.addInfo("CEC (StreetPass) ID", cecID);

			s.Position = offset + 0x2040;
			byte[] iconData = s.read(0x480);
			byte[] largeIconData = s.read(0x1200);
			info.addInfo("Small icon", decodeIcon(iconData, 24, 24));
			info.addInfo("Icon", decodeIcon(largeIconData, 48, 48));
		}

		static byte[] tileOrder = {
			0, 1, 8, 9, 2, 3, 10, 11,
			16, 17, 24, 25, 18, 19, 26, 27,
			4, 5, 12, 13, 6, 7, 14, 15,
			20, 21, 28, 29, 22, 23, 30, 31,
			32, 33, 40, 41, 34, 35, 42, 43,
			48, 49, 56, 57, 50, 51, 58, 59,
			36, 37, 44, 45, 38, 39, 46, 47,
			52, 53, 60, 61, 54, 55, 62, 63
		};
		public static Bitmap decodeIcon(byte[] data, int width, int height) {
			//Assumes RGB565, although allegedly there can be other encodings
			Bitmap bitmap = new Bitmap(width, height);
			int i = 0;
			for(int tile_y = 0; tile_y < height; tile_y += 8) {
				for(int tile_x = 0; tile_x < width; tile_x += 8) {
					for(int tile = 0; tile < 8 * 8; ++tile) {
						int x = tile_x + (tileOrder[tile] & 0b0000_0111);
						int y = tile_y + ((tileOrder[tile] & 0b1111_1000) >> 3);

						int pixel = data[i] | (data[i + 1] << 8);

						int b = ((pixel >> 0) & 0x1f) << 3;
						int g = ((pixel >> 5) & 0x3f) << 2;
						int r = ((pixel >> 11) & 0x1f) << 3;
						bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
						i += 2;
					}
				}
			}
			return bitmap;
		}

		private static Dictionary<int, string> PARTITION_NAMES = new Dictionary<int, string> {
			{0, "Executable content"},
			{1, "Electronic manual"},
			{2, "Download Play child"},
			{6, "New 3DS update data"},
			{7, "Update data"},
		};

		public static void parseNCSD(ROMInfo info, ROMFile file, bool isCCI) {
			var s = file.stream;
			s.Position = 0x104;

			long size = (uint)s.readIntLE() * MEDIA_UNIT;
			info.addInfo("ROM size", size, ROMInfo.FormatMode.SIZE);

			byte[] mediaID = s.read(8);
			info.addInfo("Media ID", mediaID, true); //What does this mean?

			byte[] partitionTypes = s.read(8);
			info.addInfo("Partition types", partitionTypes, true);

			byte[] partitionCryptTypes = s.read(8);
			info.addInfo("Partition crypt types", partitionCryptTypes, true);

			long[] partitionOffsets = new long[8], partitionLengths = new long[8];
			for(int i = 0; i < 8; ++i) {
				partitionOffsets[i] = (uint)s.readIntLE() * MEDIA_UNIT;
				partitionLengths[i] = (uint)s.readIntLE() * MEDIA_UNIT;
			}

			if (isCCI) {
				//There's an additional header here but ehh don't think I need it (except 8-byte flags at 0x188, flags[6] might be needed for media unit size? But that seems to always be 0)
				s.Position = 0x312;
				short version = s.readShortLE();
				info.addInfo("Version", version);

				for (int i = 0; i < 8; ++i) {
					string partitionName = "Partition " + (i + 1);
					if (PARTITION_NAMES.ContainsKey(i)) {
						partitionName = PARTITION_NAMES[i];
					}

					if(partitionLengths[i] > 0 && partitionOffsets[i] > 0) {
						info.addInfo(combinePrefix(partitionName, "partition offset"), partitionOffsets[i], ROMInfo.FormatMode.HEX);
						info.addInfo(combinePrefix(partitionName, "partition length"), partitionLengths[i], ROMInfo.FormatMode.SIZE);

						//We use no prefix here with "main" partition so it'll just say "Product code" or "Version" instead of "Executable content version" etc
						parseNCCH(info, s, i == 0 ? null : partitionName, partitionOffsets[i]);
					}
				}
			}
		}

		public static void parse3DSX(ROMInfo info, ROMFile file) {
			WrappedInputStream s = file.stream;

			s.Position = 4;
			short headerSize = s.readShortLE();
			bool hasExtendedHeader = headerSize > 32;
			info.addInfo("Header size", headerSize, ROMInfo.FormatMode.SIZE);

			//meh..... don't really care about the rest of the 3dsx header, it's basically just a boneless .elf
			bool lookForSMDHFile = true;
			if (hasExtendedHeader) {
				s.Position = 32;
				uint smdhOffset = (uint)s.readIntLE();
				uint smdhSize = (uint)s.readIntLE();
				info.addInfo("SMDH offset", smdhOffset, ROMInfo.FormatMode.HEX);
				info.addInfo("SMDH size", smdhSize, ROMInfo.FormatMode.SIZE);

				if(smdhSize > 0) {
					lookForSMDHFile = false;
					parseSMDH(info, s, null, smdhOffset);
				}
			}

			if (lookForSMDHFile) {
				string smdhName = Path.ChangeExtension(file.name, "smdh");
				if (file.hasSiblingFile(smdhName)) {
					var smdh = file.getSiblingFile(smdhName);
					parseSMDH(info, smdh, null);
				}
			}
		}

		static bool isNCSDMagic(byte[] b) {
			//"NCSD" in ASCII
			return b[0] == 78 && b[1] == 67 && b[2] == 83 && b[3] == 68;
		}

		static bool isNCCHMagic(byte[] b) {
			return b[0] == 78 && b[1] == 67 && b[2] == 67 && b[3] == 72;
		}

		static bool is3DSXMagic(byte[] b) {
			return b[0] == 51 && b[1] == 68 && b[2] == 83 && b[3] == 88;
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);

			var s = file.stream;
			s.Position = 0x100;
			byte[] magic = s.read(4);
			if (isNCSDMagic(magic)) {
				info.addInfo("Detected format", "NCSD");
				parseNCSD(info, file, true);
			} else if (isNCCHMagic(magic)) {
				info.addInfo("Detected format", "NCCH");
				parseNCCH(info, file.stream, null);
			} else {
				s.Position = 0;
				magic = s.read(4);
				if (is3DSXMagic(magic)) {
					info.addInfo("Detected format", "3DSX");
					parse3DSX(info, file);
				}
			}
			//TODO CIA https://www.3dbrew.org/wiki/CIA
		}
	}
}
