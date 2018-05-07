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
			{'K', "eShop (K)"}, //Supposedly seen on Mighty Gunvolt, according to 3dbrew but is that the eShop version or standalone version
			{'S', "3D Classics"}, 
			{'P', "GBA Virtual Console"},
			{'T', "NES Virtual Console"}, //Is this used for Game Boy/Game Gear VC too?
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
			//Maybe I should skip this if it's CTR-P-CTAP
			info.addInfo(combinePrefix(prefix, "Product code"), productCode);
			info.addInfo(combinePrefix(prefix, "Category"), productCode[4], CATEGORIES);
			info.addInfo(combinePrefix(prefix, "Type"), productCode[6], GAME_TYPES);
			info.addInfo(combinePrefix(prefix, "Short title"), productCode.Substring(7, 2));
			info.addInfo(combinePrefix(prefix, "Country"), productCode[9], NintendoCommon.COUNTRIES);


			//Something about a reserved and an extended header SHA-256

			s.Position = offset + 0x188;
			byte[] flags = s.read(8);
			bool isData = (flags[5] & 1) > 0;
			bool isExecutable = (flags[5] & 2) > 0;
			bool isCXI = !(isData & !isExecutable);
			bool isSystemUpdate = (flags[5] & 4) > 0;
			bool isElectronicManual = (flags[5] & 8) > 0;
			bool isTrial = (flags[5] & 16) > 0;
			bool isDecrypted = (flags[7] & 4) > 0;

			info.addInfo(combinePrefix(prefix, "Is CXI"), isCXI);
			info.addInfo(combinePrefix(prefix, "Is data"), isData);
			info.addInfo(combinePrefix(prefix, "Is executable"), isExecutable);
			info.addInfo(combinePrefix(prefix, "Is system update"), isSystemUpdate);
			info.addInfo(combinePrefix(prefix, "Is electronic manual"), isElectronicManual); //TODO This just goes to show we should make some of this stuff extra if it's not the main thing ("Electronic manual is electronic manual" = true)
			info.addInfo(combinePrefix(prefix, "Is trial"), isTrial);
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

			if (plainRegionSize > 0) {
				info.addInfo(combinePrefix(prefix, "Plain region offset"), plainRegionOffset, ROMInfo.FormatMode.HEX);
				info.addInfo(combinePrefix(prefix, "Plain region size"), plainRegionSize, ROMInfo.FormatMode.SIZE);

				parsePlainRegion(info, s, prefix, plainRegionOffset);
			}

			if(exeFSSize > 0) {
				info.addInfo(combinePrefix(prefix, "ExeFS offset"), exeFSOffset, ROMInfo.FormatMode.HEX);
				info.addInfo(combinePrefix(prefix, "ExeFS size"), exeFSSize, ROMInfo.FormatMode.SIZE);

				if (isDecrypted) {
					//If the ROM is encrypted, it'll be all garbled, so there's not much we can do there...
					parseExeFS(info, s, prefix, exeFSOffset);
				}
			}
			//Should look into RomFS once we start doing filesystem browsing, albeit it also won't work with encrypted dumps

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
				//TODO Rest of the extended header https://www.3dbrew.org/wiki/NCCH/Extended_Header
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

		public static void parseExeFS(ROMInfo info, WrappedInputStream s, string prefix, long offset = 0) {
			s.Position = offset;
			for(int i = 0; i < 10; ++i) {
				string filename = s.read(8, Encoding.ASCII).TrimEnd('\0');
				long fileOffset = (uint)s.readIntLE() + offset + 0x200; //Add ExeFS header as well
				long fileSize = (uint)s.readIntLE();

				if (fileSize > 0) {
					info.addInfo(combinePrefix(prefix, "File name " + i), filename);
					info.addInfo(combinePrefix(prefix, "File offset " + i), fileOffset, ROMInfo.FormatMode.HEX);
					info.addInfo(combinePrefix(prefix, "File size " + i), fileSize, ROMInfo.FormatMode.SIZE);
					//banner contains some kinda 3D graphics and a sound
					if ("icon".Equals(filename)) {
						long pos = s.Position;
						try {
							parseSMDH(info, s, prefix, fileOffset);
						} finally {
							s.Position = pos;
						}
					}
				}
			}
		}

		static readonly string[] titleLanguages = {"Japanese", "English", "French", "German", "Italian", "Spanish", "Simplified Chinese", "Korean", "Dutch", "Portugese", "Russian", "Traditional Chinese", "Unknown 1", "Unknown 2", "Unknown 3", "Unknown 4", "Unknown 5"};
		public static void parseSMDH(ROMInfo info, WrappedInputStream s, string prefix, long offset = 0) {
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

				if(partitionLengths[0] > 0) {
					info.addInfo("Executable content partition offset", partitionOffsets[0], ROMInfo.FormatMode.HEX);
					info.addInfo("Executable content partition length", partitionLengths[0], ROMInfo.FormatMode.SIZE);

					parseNCCH(info, s, null, partitionOffsets[0]); //We use no prefix here to consider this the "main" partition so it'll just say "Product code" or "Version" instead of "Executable content version" etc
				}

				if (partitionLengths[1] > 0) {
					info.addInfo("Electronic manual partition offset", partitionOffsets[1], ROMInfo.FormatMode.HEX);
					info.addInfo("Electronic manual partition length", partitionLengths[1], ROMInfo.FormatMode.SIZE);

					parseNCCH(info, s, "Electronic manual", partitionOffsets[1]);
				}

				if (partitionLengths[2] > 0) {
					info.addInfo("Download Play child partition offset", partitionOffsets[2], ROMInfo.FormatMode.HEX);
					info.addInfo("Download Play child partition length", partitionLengths[2], ROMInfo.FormatMode.SIZE);

					parseNCCH(info, s, "Download Play child", partitionOffsets[2]);
				}

				if (partitionLengths[6] > 0) {
					info.addInfo("New 3DS update data partition offset", partitionOffsets[6], ROMInfo.FormatMode.HEX);
					info.addInfo("New 3DS update data partition length", partitionLengths[6], ROMInfo.FormatMode.SIZE);

					parseNCCH(info, s, "New 3DS update data", partitionOffsets[6]);
				}

				if (partitionLengths[7] > 0) {
					info.addInfo("Update data partition offset", partitionOffsets[7], ROMInfo.FormatMode.HEX);
					info.addInfo("Update data partition length", partitionLengths[7], ROMInfo.FormatMode.SIZE);

					parseNCCH(info, s, "Update data", partitionOffsets[7]);
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
