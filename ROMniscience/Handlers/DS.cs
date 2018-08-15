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
using ROMniscience.IO;
using System.Drawing;

namespace ROMniscience.Handlers {
	class DS : Handler {
		//Mostly adapted from http://problemkaputt.de/gbatek.htm

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"nds", "Nintendo DS ROM"},
			{"dsi", "Nintendo DS ROM (DSi exclusive)"},
			{"ids", "Nintendo iQue DS ROM"},
			{"plg", "Supercard DSTwo plugin"},
		};
		public override string name => "Nintendo DS";

		public readonly static IDictionary<int, string> UNIT_CODES = new Dictionary<int, string>() {
			{0, "Nintendo DS"},
			{2, "Nintendo DS + DSi"},
			{3, "Nintendo DSi"},
		};

		public readonly static IDictionary<int, string> REGIONS = new Dictionary<int, string>() {
			//This actually matters, because if it's set to China then the DS
			//will display a glowing "ONLY FOR iQue DS" error message, and seemingly that's all that's stopping you playing it
			//DSi and 3DS won't care (the region locking on DSi stuff works differently)
			//Korea doesn't seem to make a difference though, in fact Pokemon White 2 (USA) has it set to that for some reason, and there are some Korean games (Touch Dictionary etc) that use Normal anyway
			//Have also seen 65 (wat?)
			{0, "Normal"},
			{1, "DSi app"},
			{0x40, "Korea"},
			{0x80, "China"},
		};

		public readonly static IDictionary<char, string> GAME_TYPES = new Dictionary<char, string>() {
			{'A', "Game"},
			{'B', "Game (B)"},
			{'C', "Game (C)"},
			{'D', "DSi exclusive game"},
			{'H', "DSiWare system utility"},
			{'I', "DS game with infrared"}, //e.g Pokemon HeartGold/SoulSilver, Pokemon gen 5, I think Personal Trainer: Walking
			{'K', "DSiWare game"},
			{'N', "Downloadable demo"}, //Basically always seen with a product code of NTRJ
			{'T', "Game (T)"}, //This is a bit weird, only seen in Pokemon Card Game Asobitai DS and Puyo Puyo 20th anniversary so far. What makes those games special? Who knows
			{'U', "DS utility/educational game/uncommon hardware"}, //Learn With Pokemon: Typing Adventure uses this
			{'V', "DSi enhanced DS game"}, //e.g. Assassin's Creed 2: Discovery, Pokemon Conquest
			{'Y', "Game (Y)"},
		};

		public readonly static IDictionary<int, string> BANNER_VERSIONS = new Dictionary<int, string>() {
			{1, "Original"},
			{2, "With Chinese title"}, //Ironically, only Chinese New Super Mario Bros uses this
			{3, "With Chinese and Korean titles"},
			{0x103, "With Chinese and Korean titles and DSi animated icon"},
		};

		[Flags]
		public enum DSiRegionFlags : uint {
			//This stuff actually is checked and is how the region locking works
			Japan = 1 << 0,
			USA = 1 << 1,
			Europe = 1 << 2, //NOT Australia! Most games in either Europe or Australia use both flags, except for the DSi browser
			Australia = 1 << 3,
			China = 1 << 4,
			Korea = 1 << 5,
		}

		public static readonly IDictionary<int, string> DSI_TYPES = new Dictionary<int, string>() {
			{0, "Cartridge"},
			{4, "DSiWare"},
			{5, "System fun tools"}, //wat (seen in this DSi Demonstration thingy that was tucked away in my 3DS's internal storage somewhere)
			{15, "Non-executable data file"},
			{21, "System base tools"},
			{23, "System menu"},
		};

		public static Bitmap decodeDSIcon(byte[] bitmap, byte[] palette) {
			//Convert the palette data into 16-bit ints, because I forgot to
			//parse it like that when I read it, and maybe I should do that
			short[] palette16 = new short[16];
			for (int i = 0; i < 16; ++i) {
				palette16[i] = (short)(palette[i * 2] + (palette[i * 2 + 1] << 8));
			}

			Color[] actualPalette = new Color[16];
			for (int i = 0; i < 16; ++i) {
				int r = (palette16[i] & 0b000000000011111) << 3;
				int g = (palette16[i] & 0b000001111100000) >> 2;
				int b = (palette16[i] & 0b111110000000000) >> 7;

				int alpha = 0xff;
				if (i == 0) {
					alpha = 0;
				}
				actualPalette[i] = Color.FromArgb(alpha, r, g, b);
			}

			Bitmap icon = new Bitmap(32, 32);
			int pos = 0;
			for (int tile_y = 0; tile_y < 4; ++tile_y) {
				for (int tile_x = 0; tile_x < 4; ++tile_x) {
					for (int y = 0; y < 8; ++y) {
						for (int x = 0; x < 4; ++x) {
							icon.SetPixel((x * 2) + (8 * tile_x), y + 8 * tile_y, actualPalette[bitmap[pos] & 0x0f]);
							icon.SetPixel((x * 2 + 1) + (8 * tile_x), y + 8 * tile_y, actualPalette[(bitmap[pos] & 0xf0) >> 4]);
							pos++;
						}
					}
				}
			}
			return icon;
		}

		bool isPassMeEntryPoint(byte[] b) {
			return b[0] == 0xc8 && b[1] == 0x60 && b[2] == 0x4f && b[3] == 0xe2 && b[4] == 0x01 && b[5] == 0x70 && b[6] == 0x8f && b[7] == 0xe2;
		}

		bool isDecryptedSecureAreaID(byte[] b) {
			return b[0] == 0xff && b[1] == 0xde && b[2] == 0xff && b[3] == 0xe7 && b[4] == 0xff && b[5] == 0xde && b[6] == 0xff && b[7] == 0xe7;
		}

		public static void parseBanner(ROMInfo info, WrappedInputStream s, long bannerOffset) {
			s.Position = bannerOffset;
			short bannerVersion = s.readShortLE();
			info.addInfo("Banner version", bannerVersion, BANNER_VERSIONS);
			if (BANNER_VERSIONS.ContainsKey(bannerVersion)) {
				short bannerChecksum = s.readShortLE(); //CRC16 of 0x20 to 0x83f (icons and titles)
				short bannerChecksum2 = s.readShortLE(); //CRC16 of 0x20 to 0x93f (icons and titles including Chinese)
				short bannerChecksum3 = s.readShortLE(); //CRC16 of 0x20 to 0xa3f (icons and titles including Chinese + Korea)
				short bannerChecksum4 = s.readShortLE(); //CRC16 of 0x1240 to 0x23bf (i.e. DSi animated icon)

				info.addInfo("Banner checksum", bannerChecksum, ROMInfo.FormatMode.HEX, true);
				short calculatedBannerChecksum = calcCRC16(s, bannerOffset + 0x20, bannerOffset + 0x83f);
				info.addInfo("Calculated banner checksum", calculatedBannerChecksum, ROMInfo.FormatMode.HEX, true);
				info.addInfo("Banner checksum valid?", bannerChecksum == calculatedBannerChecksum);

				if (bannerVersion >= 2) {
					info.addInfo("Banner checksum 2", bannerChecksum2, ROMInfo.FormatMode.HEX, true);
					short calculatedBannerChecksum2 = calcCRC16(s, bannerOffset + 0x20, bannerOffset + 0x93f);
					info.addInfo("Calculated banner checksum 2", calculatedBannerChecksum2, ROMInfo.FormatMode.HEX, true);
					info.addInfo("Banner checksum 2 valid?", bannerChecksum2 == calculatedBannerChecksum2);
				}

				if (bannerVersion >= 3) {
					info.addInfo("Banner checksum 3", bannerChecksum3, ROMInfo.FormatMode.HEX, true);
					short calculatedBannerChecksum3 = calcCRC16(s, bannerOffset + 0x20, bannerOffset + 0xa3f);
					info.addInfo("Calculated banner checksum 3", calculatedBannerChecksum3, ROMInfo.FormatMode.HEX, true);
					info.addInfo("Banner checksum 3 valid?", bannerChecksum3 == calculatedBannerChecksum3);
				}

				if (bannerVersion >= 0x103) {
					info.addInfo("Banner checksum 4", bannerChecksum4, ROMInfo.FormatMode.HEX, true);
					short calculatedBannerChecksum4 = calcCRC16(s, bannerOffset + 0x1240, bannerOffset + 0x23bf);
					info.addInfo("Calculated banner checksum 4", calculatedBannerChecksum4, ROMInfo.FormatMode.HEX, true);
					info.addInfo("Banner checksum 4 valid?", bannerChecksum4 == calculatedBannerChecksum4);
				}

				byte[] bannerReserved = s.read(0x16); //Should be zero filled
				info.addInfo("Banner reserved", bannerReserved, true);

				byte[] iconBitmap = s.read(0x200);
				byte[] iconPalette = s.read(0x20);
				info.addInfo("Icon", decodeDSIcon(iconBitmap, iconPalette));

				string japaneseTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				info.addInfo("Japanese title", japaneseTitle);
				string englishTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				info.addInfo("English title", englishTitle);
				string frenchTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				info.addInfo("French title", frenchTitle);
				string germanTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				info.addInfo("German title", germanTitle);
				string italianTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				info.addInfo("Italian title", italianTitle);
				string spanishTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
				info.addInfo("Spanish title", spanishTitle);
				if (bannerVersion >= 2) {
					string chineseTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
					info.addInfo("Chinese title", chineseTitle);
				}
				if (bannerVersion >= 3) {
					string koreanTitle = s.read(256, Encoding.Unicode).TrimEnd('\0').Replace("\n", Environment.NewLine);
					info.addInfo("Korean title", koreanTitle);
				}

				//Should be zero filled, but it's not
				byte[] titleReserved = s.read(0x800);

				if (bannerVersion >= 0x103) {
					//Same format as DS icon, but animated
					IList<byte[]> dsiIconBitmaps = new List<byte[]>();
					for (int i = 0; i < 8; ++i) {
						dsiIconBitmaps.Add(s.read(0x200));
					}
					IList<byte[]> dsiIconPalettes = new List<byte[]>();
					for (int i = 0; i < 8; ++i) {
						dsiIconPalettes.Add(s.read(0x20));
					}

					for (int i = 0; i < 64; ++i) {
						ushort sequence = (ushort)s.readShortLE();
						if (sequence == 0) {
							break;
						}

						bool flipVertically = (sequence & 0x8000) > 0;
						bool flipHorizontally = (sequence & 0x4000) > 0;

						int paletteIndex = (sequence & 0x3800) >> 11;
						int bitmapIndex = (sequence & 0x700) >> 8;

						Bitmap frame = decodeDSIcon(dsiIconBitmaps[bitmapIndex], dsiIconPalettes[paletteIndex]);
						if (flipHorizontally) {
							frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
						}
						if (flipVertically) {
							frame.RotateFlip(RotateFlipType.RotateNoneFlipY);
						}
						info.addInfo("DSi icon frame " + (i + 1), frame, true);

						//Duration: sequence & 0xff (in 60Hz units, so I guess how many frames this lasts at 60fps?) but we have no method of storing that at the moment
					}
				}
			}
		}

		public static void parseDSiHeader(ROMInfo info, WrappedInputStream s) {
			s.Position = 0x1b0;
			int regionFlags = s.readIntLE();
			//There's only 6 bits used, everything else is reserved. What a good use of 5 bytes!
			if (regionFlags == -1) {
				//Hmm... Pokemon gen 5 games (I sure do talk about them a lot, huh? Well, they're weird. And they're good games) use 0xffffffef / -17 here, actually; explain that one nerd (bit 27 I guess? But then what the heck)
				info.addInfo("Region", "Region free");
			} else {
				info.addInfo("Region", Enum.ToObject(typeof(DSiRegionFlags), regionFlags).ToString());
			}

			s.Position = 0x210;
			int usedROMSize = s.readIntLE();
			info.addInfo("Used ROM size", usedROMSize, ROMInfo.FormatMode.SIZE);

			info.addInfo("DSi reserved", s.read(4), true);
			info.addInfo("DSi reserved 2", s.read(4), true);
			info.addInfo("DSi reserved 3", s.read(4), true);

			int modcryptOffset = s.readIntLE();
			info.addInfo("Modcrypt area 1 offset", modcryptOffset, ROMInfo.FormatMode.HEX, true);
			int modcryptSize = s.readIntLE();
			info.addInfo("Modcrypt area 1 size", modcryptSize, ROMInfo.FormatMode.SIZE, true);
			int modcryptOffset2 = s.readIntLE();
			info.addInfo("Modcrypt area 2 offset", modcryptOffset2, ROMInfo.FormatMode.HEX, true);
			int modcryptSize2 = s.readIntLE();
			info.addInfo("Modcrypt area 2 size", modcryptSize2, ROMInfo.FormatMode.SIZE, true);

			string emagCode = s.read(4, Encoding.ASCII);
			info.addInfo("Game code backwards", emagCode, true);
			int dsiType = s.read();
			info.addInfo("Filetype", dsiType, DSI_TYPES);
			byte[] titleIDReserved = s.read(3); //Usually 00 03 00 for some reason
			info.addInfo("DSi title ID reserved", titleIDReserved, true);

			int publicSaveSize = s.readIntLE();
			info.addInfo("DSiWare public.sav filesize", publicSaveSize, ROMInfo.FormatMode.SIZE);
			int privateSaveSize = s.readIntLE();
			info.addInfo("DSiWare private.sav filesize", publicSaveSize, ROMInfo.FormatMode.SIZE);

			info.addInfo("DSi reserved 4", s.read(176), true);

			byte[] ratings = s.read(16);
			NintendoCommon.parseRatings(info, ratings, true);
		}

		public static void addSupercardDS2PluginInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Supercard DSTwo");
			string iconFilename = System.IO.Path.ChangeExtension(file.name, "bmp");
			//The icon is actually pointed to in the .ini file, but it's in a native DS format (starts with fat1:/) so it won't be of any use unless running on an actual DSTwo. Luckily, the filename is always the same as the .plg but with a .bmp extension; and this is the kind of convention that nobody would dare break
			var icon = Image.FromStream(file.getSiblingFile(iconFilename));
			info.addInfo("Icon", icon);

			string iniFilename = System.IO.Path.ChangeExtension(file.name, "ini");
			using (var sr = new System.IO.StreamReader(file.getSiblingFile(iniFilename))) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();
					if (line == null) {
						break;
					}

					if (line.ToLowerInvariant().StartsWith("name=")) {
						//Once again, not really internal. I kinda want to rename this column globally, it's already kinda wordy and a mouthful and I don't like that
						info.addInfo("Internal name", line.Split('=')[1]);
						break;
					}

				}
			}
		}

		static short calcCRC16(WrappedInputStream s, long start, long end) {
			long pos = s.Position;
			try {
				s.Position = start;
				int length = (int)((end - start) + 1);
				byte[] data = s.read(length);
				return CRC16.crc16(data);
			} finally {
				s.Position = pos;
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if ("plg".Equals(file.extension)) {
				addSupercardDS2PluginInfo(info, file);
				return;
			}

			WrappedInputStream s = file.stream;

			s.Position = 0xc0;
			bool passMe = isPassMeEntryPoint(s.read(8));
			s.Position = 0;
			info.addInfo("PassMe", passMe);

			string title = s.read(12, Encoding.ASCII).TrimEnd('\0');

			string gameCode = s.read(4, Encoding.ASCII);
			char gameType = gameCode[0];
			string shortTitle = gameCode.Substring(1, 2);
			char country = gameCode[3];

			string makerCode = s.read(2, Encoding.ASCII);

			if (!passMe) {
				info.addInfo("Internal name", title);
				info.addInfo("Product code", gameCode);
				info.addInfo("Type", gameType, GAME_TYPES);
				info.addInfo("Short title", shortTitle);
				info.addInfo("Country", country, NintendoCommon.COUNTRIES);
				info.addInfo("Publisher", makerCode, NintendoCommon.LICENSEE_CODES);
			}

			int unitCode = s.read();
			info.addInfo("Device type", unitCode, UNIT_CODES);
			info.addInfo("Platform", unitCode == 3 ? "DSi" : "DS");
			int encryption_seed = s.read(); //From 0 to 7, usually 0
			info.addInfo("Encryption seed", encryption_seed, true);
			long romSize = (128 * 1024) << s.read();
			info.addInfo("ROM size", romSize, ROMInfo.FormatMode.SIZE);

			//Should be 0 filled (Pokemon Black 2 doesn't 0 fill it, so maybe it doesn't have to be)
			byte[] reserved = s.read(7);
			info.addInfo("Reserved", reserved, true);
			//Should be 0 normally, but used somehow on DSi
			int reserved2 = s.read();
			info.addInfo("Reserved 2", reserved2, true);

			int regionCode = s.read();
			if (unitCode >= 2) {
				//DSi has its own region locking system, and will happily play iQue games etc
				info.addInfo("DS region", regionCode, REGIONS);
			} else {
				info.addInfo("Region", regionCode, REGIONS);
			}
			int version = s.read();
			info.addInfo("Version", version);
			int autostart = s.read(); //Bit 2 skips health and safety screen when autostarting the game
			info.addInfo("Autostart param", autostart, ROMInfo.FormatMode.HEX, true);

			int arm9Offset = s.readIntLE();
			info.addInfo("ARM9 offset", arm9Offset, ROMInfo.FormatMode.HEX, true);
			int arm9Entry = s.readIntLE();
			info.addInfo("ARM9 entry point", arm9Entry, ROMInfo.FormatMode.HEX, true);
			int arm9RAMAddress = s.readIntLE();
			info.addInfo("ARM9 RAM address", arm9RAMAddress, ROMInfo.FormatMode.HEX, true);
			int arm9Size = s.readIntLE();
			info.addInfo("ARM9 size", arm9Size, ROMInfo.FormatMode.SIZE, true);

			int arm7Offset = s.readIntLE();
			info.addInfo("ARM7 offset", arm7Offset, ROMInfo.FormatMode.HEX, true);
			int arm7Entry = s.readIntLE();
			info.addInfo("ARM7 entry point", arm7Entry, ROMInfo.FormatMode.HEX, true);
			int arm7RAMAddress = s.readIntLE();
			info.addInfo("ARM7 RAM address", arm7RAMAddress, ROMInfo.FormatMode.HEX, true);
			int arm7Size = s.readIntLE();
			info.addInfo("ARM7 size", arm7Size, ROMInfo.FormatMode.SIZE, true);

			int filenameTableOffset = s.readIntLE();
			info.addInfo("Filename table offset", filenameTableOffset, ROMInfo.FormatMode.HEX, true);
			int filenameTableSize = s.readIntLE();
			info.addInfo("Filename table size", filenameTableSize, ROMInfo.FormatMode.SIZE, true);
			int fatOffset = s.readIntLE();
			info.addInfo("File allocation table offset", fatOffset, ROMInfo.FormatMode.HEX, true);
			int fatSize = s.readIntLE();
			info.addInfo("File allocation table size", fatSize, ROMInfo.FormatMode.SIZE, true);
			int fileARM9OverlayOffset = s.readIntLE();
			info.addInfo("File ARM9 overlay offset", fileARM9OverlayOffset, ROMInfo.FormatMode.HEX, true);
			int fileARM9OverlaySize = s.readIntLE();
			info.addInfo("File ARM9 overlay size", fileARM9OverlaySize, ROMInfo.FormatMode.SIZE, true);
			int fileARM7OverlayOffset = s.readIntLE();
			info.addInfo("File ARM7 overlay offset", fileARM7OverlayOffset, ROMInfo.FormatMode.HEX, true);
			int fileARM7OverlaySize = s.readIntLE();
			info.addInfo("File ARM7 overlay size", fileARM7OverlaySize, ROMInfo.FormatMode.SIZE, true);

			byte[] normalCommandSetting = s.read(4); //For port 0x40001A4 (ROMCTRL), usually 0x00586000
			info.addInfo("Normal command setting", normalCommandSetting, true);
			byte[] key1CommandSetting = s.read(4); //For port 0x40001A4 (ROMCTRL), usually 0x001808f8
			info.addInfo("KEY1 command cetting", key1CommandSetting, true);

			int bannerOffset = s.readIntLE();
			info.addInfo("Banner offset", bannerOffset, ROMInfo.FormatMode.HEX, true);

			short secureAreaChecksum = s.readShortLE();
			info.addInfo("Secure area checksum", secureAreaChecksum, ROMInfo.FormatMode.HEX, true);
			short calculatedSecureAreaChecksum = calcCRC16(s, 0x4000, 0x7fff);
			info.addInfo("Calculated secure area checksum", calculatedSecureAreaChecksum, ROMInfo.FormatMode.HEX, true);
			//Do not be alarmed if this is not valid, since most DS dumps erase the secure area
			info.addInfo("Secure area checksum valid?", secureAreaChecksum == calculatedSecureAreaChecksum);

			short secureAreaDelay = s.readShortLE(); //131kHz units, 0x051e = 10ms, 0x0d7e = 26ms
			info.addInfo("Secure area delay (ms)", secureAreaDelay / 131, true);

			int arm9AutoLoadRAMAddress = s.readIntLE();
			info.addInfo("ARM9 auto load RAM address", arm9AutoLoadRAMAddress, ROMInfo.FormatMode.HEX, true);
			int arm7AutoLoadRAMAddress = s.readIntLE();
			info.addInfo("ARM7 auto load RAM address", arm7AutoLoadRAMAddress, ROMInfo.FormatMode.HEX, true);

			byte[] secureAreaDisable = s.read(8); //Usually 0 filled
			info.addInfo("Secure area disable", secureAreaDisable, true);

			int usedROMSize = s.readIntLE(); //Excludes DSi area, so we add the info item later to determine the meaning
			int romHeaderSize = s.readIntLE();
			info.addInfo("Header size", romHeaderSize, ROMInfo.FormatMode.SIZE);

			byte[] reserved3 = s.read(0x38); //0 filled except on DSi which uses first 12 bytes for some purpose
			info.addInfo("Reserved 3", reserved3, true);
			byte[] nintendoLogo = s.read(0x9c); //Same as on GBA (I think?)
			info.addInfo("Nintendo logo", nintendoLogo, true);

			short nintendoLogoChecksum = s.readShortLE(); //CRC16 of nintendoLogo, should be 0xcf56?
			info.addInfo("Nintendo logo checksum", nintendoLogoChecksum, ROMInfo.FormatMode.HEX, true);
			short calculatedNintendoLogoChecksum = CRC16.crc16(nintendoLogo);
			info.addInfo("Calculated Nintendo logo checksum", calculatedNintendoLogoChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Nintendo logo checksum valid?", nintendoLogoChecksum == calculatedNintendoLogoChecksum);

			short headerChecksum = s.readShortLE(); //CRC16 of header up until here (first 0x15d bytes)
			info.addInfo("Header checksum", headerChecksum, ROMInfo.FormatMode.HEX, true);
			short calculatedHeaderChecksum = calcCRC16(s, 0, 0x15d);
			info.addInfo("Calculated header checksum", calculatedHeaderChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Header checksum valid?", headerChecksum == calculatedHeaderChecksum);

			int debugROMOffset = s.readIntLE();
			info.addInfo("Debug ROM offset", debugROMOffset, ROMInfo.FormatMode.HEX, true);
			int debugSize = s.readIntLE();
			info.addInfo("Debug ROM size", debugSize, ROMInfo.FormatMode.SIZE, true);
			int debugRAMAddress = s.readIntLE();
			info.addInfo("Debug RAM address", debugRAMAddress, ROMInfo.FormatMode.HEX, true);

			//Both zero filled, who cares
			byte[] reserved4 = s.read(4);
			info.addInfo("Reserved 4", reserved4, true);
			byte[] reserved5 = s.read(0x90);
			info.addInfo("Reserved 5", reserved5, true);

			if (unitCode >= 2) {
				info.addInfo("Used ROM size excluding DSi area", usedROMSize, ROMInfo.FormatMode.SIZE);
				parseDSiHeader(info, s);
			} else {
				info.addInfo("Used ROM size", usedROMSize, ROMInfo.FormatMode.SIZE);
			}

			info.addInfo("Homebrew?", arm9Offset < 0x4000);
			if (arm9Offset >= 0x4000 && arm9Offset < 0x8000) {
				info.addInfo("Contains secure area", true);
				s.Position = 0x4000;
				//Secure area, please leave all your electronic devices at the front counter before entering
				byte[] secureAreaID = s.read(8);
				info.addInfo("Multiboot", secureAreaID.All(x => x == 0));
				info.addInfo("Decrypted", isDecryptedSecureAreaID(secureAreaID));

			} else {
				info.addInfo("Contains secure area", false);
			}
			s.Position = 0x1000;
			//See also: https://twitter.com/Myriachan/status/964580936561000448
			//This should be true, but it's often false, especially for No-Intro verified dumps
			info.addInfo("Contains Blowfish encryption tables", s.read(8).Any(x => x != 0));

			bool containsWifi = false;
			if (fatOffset > 0 && fatSize > 0 && filenameTableOffset > 0 && filenameTableSize > 0) {
				//TODO: Negative fatOffset/fatSize/etc values might mean it is actually there, just really big (and unsigned). But we should check for absurdly big values that are invalid before doing that
				var fs = readNitroFS(s, (uint)fatOffset, (uint)fatSize, (uint)filenameTableOffset, (uint)filenameTableSize);
				info.addFilesystem(fs);
				if (fs.contains("dwc")) {
					var dwc = (FilesystemDirectory)fs.getChild("dwc");
					containsWifi = dwc.contains("utility.bin");
				}
			}
			info.addInfo("Contains WFC setup", containsWifi);
			parseBanner(info, s, bannerOffset);
		}

		public static Tuple<uint, uint> getFatInfo(WrappedInputStream s, uint fatOffset, ushort fileID) {
			long origPos = s.Position;
			try {
				s.Position = fatOffset + (fileID * 8);

				uint startOffset = (uint)s.readIntLE();
				uint endOffset = (uint)s.readIntLE();
				uint size = endOffset - startOffset;
				return new Tuple<uint, uint>(startOffset, size);
			} finally {
				s.Position = origPos;
			}
		}

		public static void readNitroSubFNT(FilesystemDirectory fs, WrappedInputStream s, uint fatOffset, uint fatSize, uint fntOffset, uint fntSize, uint subTableOffset, ushort firstID) {

			long origPos = s.Position;
			try {
				ushort currentFileID = firstID;
				s.Position = fntOffset + subTableOffset;
				while (true) {
					int length = s.read();
					if (length == -1 || length == 0 || length == 0x80) {
						break;
					}

					if (length < 0x80) {
						ushort fileID = currentFileID;
						currentFileID++;
						string name = s.read(length, Encoding.ASCII);
						var offsetAndSize = getFatInfo(s, fatOffset, fileID);
						fs.addChild(name, offsetAndSize.Item1, offsetAndSize.Item2);
					} else {
						length -= 0x80;

						string name = s.read(length, Encoding.ASCII);
						ushort dirID = (ushort)((ushort)s.readShortLE() & 0x0fff);
						FilesystemDirectory folder = new FilesystemDirectory() {
							name = name,
						};
						readNitroMainFNT(folder, s, fatOffset, fatSize, fntOffset, fntSize, dirID * (uint)8);
						fs.addChild(folder);
					}
				}
			} finally {
				s.Position = origPos;
			}
		}

		public static void readNitroMainFNT(FilesystemDirectory fs, WrappedInputStream s, uint fatOffset, uint fatSize, uint fntOffset, uint filenameTableSize, uint tableOffset) {
			//tableOffset being the offset _into_ the FNT, not the offset _of_ the FNT (which is fntOffset). That's confusing. I'll figure out how to be less confusing later

			long origPos = s.Position;
			try {
				s.Position = fntOffset + tableOffset;
				uint subTableOffset = (uint)s.readIntLE();
				ushort firstID = (ushort)s.readShortLE();
				if(firstID > 0xefff) {
					//Shouldn't go higher than that, apparently
					return;
				}
				//TODO: For tableOffset = 0 (root), read number of directories (short LE here):
				//Validate that <= 4096 directories
				//Validate that number of directories * 8 < fntSize

				readNitroSubFNT(fs, s, fatOffset, fatSize, fntOffset, filenameTableSize, subTableOffset, firstID);
			} finally {
				s.Position = origPos;
			}
		}

		public static FilesystemDirectory readNitroFS(WrappedInputStream s, uint fatOffset, uint fatSize, uint fntOffset, uint fntSize) {
			//Number of directories = fnt[6], but we don't need that info
			FilesystemDirectory fs = new FilesystemDirectory {
				name = "NitroFS"
			};
			readNitroMainFNT(fs, s, fatOffset, fatSize, fntOffset, fntSize, 0);
			return fs;
		}
	}
}
