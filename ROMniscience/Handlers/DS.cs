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
	class DS: Handler {
		//Mostly adapted from http://problemkaputt.de/gbatek.htm

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"nds", "Nintendo DS ROM"},
			{"dsi", "Nintendo DS ROM (DSi exclusive)"},
			{"ids", "Nintendo iQue DS ROM"},
			//TODO DSiWare (.cia?), maybe SuperCard DSTwo .plg files
		};
		public override string name => "Nintendo DS";

		public readonly static IDictionary<int, string> UNIT_CODES = new Dictionary<int, string>() {
			{0, "Nintendo DS"},
			{2, "Nintendo DS + DSi"},
			{3, "Nintendo DSi"},
		};

		public readonly static IDictionary<int, string> REGION_CODES = new Dictionary<int, string>() {
			//This actually matters, because if it's set to China then the DS
			//will display a glowing "ONLY FOR iQue DS" error message, and seemingly that's all that's stopping you playing it
			//DSi and 3DS won't care
			//Korea doesn't seem to make a difference though, in fact Pokemon White 2 (USA) has it set to that for some reason
			{0, "Normal"},
			{0x40, "Korea"},
			{0x80, "China"}, 
		};

		public readonly static IDictionary<char, string> GAME_TYPES = new Dictionary<char, string>() {
			{'A', "DS game"},
			{'B', "DS game (B)"},
			{'C', "DS game (C)"},
			{'D', "DSi exclusive game"},
			{'H', "DSiWare system utility"},
			{'I', "DS game with infrared"}, //e.g Pokemon HeartGold/SoulSilver, Pokemon gen 5, I think Personal Trainer: Walking
			{'K', "DSiWare game"},
			{'N', "Nintendo Channel demo"},
			{'T', "DS game (T)"},
			{'U', "DS utility/educational game/uncommon hardware"}, //Learn With Pokemon: Typing Adventure uses this
			{'V', "DSi enhanced DS game"}, //e.g. Assassin's Creed 2: Discovery, Pokemon Conquest
			{'Y', "DS game (Y)"},
		};

		public readonly static IDictionary<char, string> REGIONS = new Dictionary<char, string>() {
			//Not the same as REGION_CODES, go figure
			{'A', "Asia"},
			{'B', "N/A"}, //Not so sure about this one
			{'C', "China"},
			{'D', "Germany"},
			{'E', "USA"},
			{'F', "France"},
			{'G', "N/A (G)"},
			{'H', "Netherlands"},
			{'I', "Italy"},
			{'J', "Japan"},
			{'K', "Korea"},
			{'L', "USA (L)"},
			{'M', "Sweden"},
			{'N', "Norway"},
			{'O', "International"},
			{'P', "Europe"},
			{'Q', "Denmark"},
			{'R', "Russia"},
			{'S', "Spain"},
			{'T', "USA + Australia"},
			{'U', "Australia"},
			{'V', "Europe + Australia"},
			{'W', "Europe (W)"},
			{'X', "Europe (X)"},
			{'Y', "Europe (Y)"},
			{'Z', "Europe (Z)"},
		};

		public readonly static IDictionary<int, string> BANNER_VERSIONS = new Dictionary<int, string>() {
			{1, "Original"},
			{2, "With Chinese title"},
			{3, "With Chinese and Korean titles"},
			{0x103, "With Chinese and Korean titles and DSi animated icon"},
		};

		public readonly static IDictionary<int, string> CERO_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{12, "B (12)"},
			{15, "C (15)"},
			{17, "D (17)"},
			{18, "Z (17)"},
		};

		public readonly static IDictionary<int, string> ESRB_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{3, "EC"}, //The fuck is that?
			{6, "Everyone"},
			{10, "E10+"},
			{13, "Teen"},
			{17, "Mature"},
		};

		public readonly static IDictionary<int, string> USK_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{6, "6+"},
			{12, "12+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> PEGI_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{3, "3+"},
			{7, "7+"},
			{12, "12+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> PEGI_PORTUGAL_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{4, "4+"},
			{6, "6+"},
			{12, "12+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> PEGI_UK_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{3, "3+"},
			{4, "4+/U"},
			{7, "7+"},
			{8, "8+/PG"},
			{12, "12+"},
			{15, "15+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> AGCB_RATINGS = new Dictionary<int, string>() {
			{0, "G"},
			{7, "PG"},
			{14, "M"},
			{15, "MA"},
			{18, "R"},
		};

		public readonly static IDictionary<int, string> GRB_RATINGS = new Dictionary<int, string>() {
			{0, "No rating"},
			{12, "12+"},
			{15, "15+"},
			{18, "18+"},
		};

		public static Bitmap decodeDSIcon(byte[] bitmap, byte[] palette) {
			//Convert the palette data into 16-bit ints, because I forgot to
			//parse it like that when I read it, and maybe I should do that
			short[] palette16 = new short[16];
			for(int i = 0; i < 16; ++i) {
				palette16[i] = (short)(palette[i * 2] + (palette[i * 2 + 1] << 8));
			}

			Color[] actualPalette = new Color[16];
			for(int i = 0; i < 16; ++i) {
				int r = (palette16[i] & 0b000000000011111) << 3;
				int g = (palette16[i] & 0b000001111100000) >> 2;
				int b = (palette16[i] & 0b111110000000000) >> 7;

				int alpha = 0xff;
				if(i == 0) {
					alpha = 0;
				}
				actualPalette[i] = Color.FromArgb(alpha, r, g, b);
			}

			Bitmap icon = new Bitmap(32, 32); 
			int pos = 0;
			for(int tile_y = 0; tile_y < 4; ++tile_y) {
				for(int tile_x = 0; tile_x < 4; ++tile_x) {
					for(int y = 0; y < 8; ++y) {
						for(int x = 0; x < 4; ++x) {
							icon.SetPixel((x * 2) + (8 * tile_x), y + 8 * tile_y, actualPalette[bitmap[pos] & 0x0f]);
							icon.SetPixel((x * 2 + 1) + (8 * tile_x), y + 8 * tile_y, actualPalette[(bitmap[pos] & 0xf0) >> 4]);
							//icon.SetPixel((x * 2) + (8 * tile_x), y + 8 * tile_y, actualPalette[(bitmap[pos] & 0xf0) >> 4]);
							//icon.SetPixel((x * 2 + 1) + (8 * tile_x), y + 8 * tile_y, actualPalette[bitmap[pos] & 0x0f]);
							pos++;
						}
					}
				}
			}
			return icon;
		}

		//TODO DSi animated icon (just need to know how to store animated icons)

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			InputStream s = file.stream;
			long origPos = s.Position;
			try {
				string title = s.read(12, Encoding.ASCII).Trim('\0');
				info.addInfo("Internal name", title);

				string gameCode = s.read(4, Encoding.ASCII);
				info.addInfo("Product code", gameCode);
				char gameType = gameCode[0];
				info.addInfo("Type", gameType, GAME_TYPES);
				string shortTitle = gameCode.Substring(1, 2);
				info.addInfo("Short title", shortTitle);
				char region = gameCode[3];
				info.addInfo("Region", region, REGIONS);

				string makerCode = s.read(2, Encoding.ASCII);
				info.addInfo("Manufacturer", makerCode, NintendoHandheldCommon.LICENSEE_CODES);
				int unitCode = s.read();
				info.addInfo("Unit code", unitCode, UNIT_CODES);
				info.addInfo("Platform", unitCode == 3 ? "DSi" : "DS");
				int encryption_seed = s.read(); //From 0 to 7, usually 0
				info.addExtraInfo("Encryption seed", encryption_seed);
				long romSize = (128 * 1024) << s.read();
				info.addInfo("ROM size", romSize, ROMInfo.FormatMode.SIZE);

				//Should be 0 filled (Pokemon Black 2 doesn't 0 fill it, so maybe it doesn't have to be)
				byte[] reserved = s.read(7);
				info.addExtraInfo("Reserved", reserved);
				//Should be 0 normally, but used somehow on DSi
				int reserved2 = s.read();
				info.addExtraInfo("Reserved 2", reserved2);

				int regionCode = s.read();
				info.addInfo("Region code", regionCode, REGION_CODES);
				int version = s.read();
				info.addInfo("Version", version);
				int autostart = s.read(); //Bit 2 skips health and safety screen when autostarting the game
				info.addInfo("Autostart param", autostart);

				int arm9Offset = s.readIntLE();
				info.addExtraInfo("ARM9 offset", arm9Offset);
				int arm9Entry = s.readIntLE();
				info.addExtraInfo("ARM9 entry point", arm9Entry);
				int arm9RAMAddress = s.readIntLE();
				info.addExtraInfo("ARM9 RAM address", arm9RAMAddress);
				int arm9Size = s.readIntLE();
				info.addExtraInfo("ARM9 size", arm9Size, ROMInfo.FormatMode.SIZE);

				int arm7Offset = s.readIntLE();
				info.addExtraInfo("ARM7 offset", arm7Offset);
				int arm7Entry = s.readIntLE();
				info.addExtraInfo("ARM7 entry point", arm7Entry);
				int arm7RAMAddress = s.readIntLE();
				info.addExtraInfo("ARM7 RAM address", arm7RAMAddress);
				int arm7Size = s.readIntLE();
				info.addExtraInfo("ARM7 size", arm7Size, ROMInfo.FormatMode.SIZE);

				int filenameTableOffset = s.readIntLE();
				info.addExtraInfo("Filename table offset", filenameTableOffset);
				int filenameTableSize = s.readIntLE();
				info.addExtraInfo("Filename table size", filenameTableSize, ROMInfo.FormatMode.SIZE);
				int fatOffset = s.readIntLE();
				info.addExtraInfo("File allocation table offset", fatOffset);
				int fatSize = s.readIntLE();
				info.addExtraInfo("File allocation table size", fatSize, ROMInfo.FormatMode.SIZE);
				int fileARM9OverlayOffset = s.readIntLE();
				info.addExtraInfo("File ARM9 overlay offset", fileARM9OverlayOffset);
				int fileARM9OverlaySize = s.readIntLE();
				info.addExtraInfo("File ARM9 overlay size", fileARM9OverlaySize, ROMInfo.FormatMode.SIZE);
				int fileARM7OverlayOffset = s.readIntLE();
				info.addExtraInfo("File ARM7 overlay offset", fileARM7OverlayOffset);
				int fileARM7OverlaySize = s.readIntLE();
				info.addExtraInfo("File ARM7 overlay size", fileARM7OverlaySize, ROMInfo.FormatMode.SIZE);

				byte[] normalCommandSetting = s.read(4); //For port 0x40001A4 (ROMCTRL), usually 0x00586000
				info.addExtraInfo("Normal command setting", normalCommandSetting);
				byte[] key1CommandSetting = s.read(4); //For port 0x40001A4 (ROMCTRL), usually 0x001808f8
				info.addExtraInfo("KEY1 command cetting", key1CommandSetting);

				int bannerOffset = s.readIntLE();
				info.addExtraInfo("Banner offset", bannerOffset);

				byte[] secureAreaChecksum = s.read(2);
				info.addExtraInfo("Secure area checksum", secureAreaChecksum);
				//TODO Calculate (CRC16 of 0x20 to 0x7fff)
				int secureAreaDelay = s.readShortLE(); //131kHz units, 0x051e = 10ms, 0x0d7e = 26ms
				info.addExtraInfo("Secure area delay (ms)", secureAreaDelay / 131);

				int arm9AutoLoadRAMAddress = s.readIntLE();
				info.addExtraInfo("ARM9 auto load RAM address", arm9AutoLoadRAMAddress);
				int arm7AutoLoadRAMAddress = s.readIntLE();
				info.addExtraInfo("ARM7 auto load RAM address", arm7AutoLoadRAMAddress);

				byte[] secureAreaDisable = s.read(8); //Usually 0 filled
				info.addExtraInfo("Secure area disable", secureAreaDisable);

				int usedROMSize = s.readIntLE(); //Excludes DSi area
				info.addInfo("Used ROM size", usedROMSize, ROMInfo.FormatMode.SIZE);
				int romHeaderSize = s.readIntLE();
				info.addInfo("Header size", romHeaderSize, ROMInfo.FormatMode.SIZE);

				byte[] reserved3 = s.read(0x38); //0 filled except on DSi which uses first 12 bytes for some purpose
				info.addExtraInfo("Reserved 3", reserved3);
				byte[] nintendoLogo = s.read(0x9c); //Same as on GBA
				info.addExtraInfo("Nintendo logo", nintendoLogo);
				byte[] nintendoLogoChecksum = s.read(2); //CRC16 of nintendoLogo, should be 0xcf56? TODO calculate
				info.addExtraInfo("Nintendo logo checksum", nintendoLogoChecksum);
				byte[] headerChecksum = s.read(2); //CRC16 of header up until here (first 0x15d bytes) TODO calc
				info.addExtraInfo("Header checksum", headerChecksum);

				int debugROMOffset = s.readIntLE();
				info.addExtraInfo("Debug ROM offset", debugROMOffset);
				int debugSize = s.readIntLE();
				info.addExtraInfo("Debug ROM size", debugSize, ROMInfo.FormatMode.SIZE);
				int debugRAMAddress = s.readIntLE();
				info.addExtraInfo("Debug RAM address", debugRAMAddress);

				//Both zero filled, who cares
				byte[] reserved4 = s.read(4);
				info.addExtraInfo("Reserved 4", reserved4);
				byte[] reserved5 = s.read(0x90);
				info.addExtraInfo("Reserved 5", reserved5);

				if(unitCode >= 2) {
					s.Seek(0x210, System.IO.SeekOrigin.Begin);
					usedROMSize = s.readIntLE();
					info.addInfo("Used ROM size including DSi area", usedROMSize, ROMInfo.FormatMode.SIZE);

					info.addExtraInfo("DSi reserved", s.read(4));
					info.addExtraInfo("DSi reserved 2", s.read(4));
					info.addExtraInfo("DSi reserved 3", s.read(4));

					int modcryptOffset = s.readIntLE();
					info.addExtraInfo("Modcrypt area 1 offset", modcryptOffset);
					int modcryptSize = s.readIntLE();
					info.addExtraInfo("Modcrypt area 1 size", modcryptSize, ROMInfo.FormatMode.SIZE);
					int modcryptOffset2 = s.readIntLE();
					info.addExtraInfo("Modcrypt area 2 offset", modcryptOffset2);
					int modcryptSize2 = s.readIntLE();
					info.addExtraInfo("Modcrypt area 2 size", modcryptSize2, ROMInfo.FormatMode.SIZE);

					string emagCode = s.read(4, Encoding.ASCII);
					info.addInfo("DSi product code", emagCode);
					int dsiType = s.read();
					info.addInfo("Filetype", dsiType);
					byte[] titleIDReserved = s.read(3);
					info.addExtraInfo("DSi title ID reserved", titleIDReserved);

					int publicSaveSize = s.readIntLE();
					info.addInfo("DSiWare public.sav filesize", publicSaveSize, ROMInfo.FormatMode.SIZE);
					int privateSaveSize = s.readIntLE();
					info.addInfo("DSiWare private.sav filesize", publicSaveSize, ROMInfo.FormatMode.SIZE);

					info.addExtraInfo("DSi reserved 4", s.read(176));

					int ceroByte = s.read();
					if((ceroByte & 128) > 0) {
						info.addInfo("Banned in Japan", (ceroByte & 64) > 0);
						info.addInfo("CERO rating", ceroByte & 0x1f, CERO_RATINGS);
					}

					int esrbByte = s.read();
					if((esrbByte & 128) > 0) {
						info.addInfo("Banned in USA", (esrbByte & 64) > 0);
						info.addInfo("ESRB rating", esrbByte & 0x1f, ESRB_RATINGS);
					}

					int reservedRatingByte = s.read();
					if((reservedRatingByte & 128) > 0) {
						//Who knows maybe it's not so reserved after all, it does seem out of place in the middle here
						info.addInfo("Banned in <reserved>", (reservedRatingByte & 64) > 0);
						info.addInfo("<reserved> rating", reservedRatingByte & 0x1f);
					}

					int uskByte = s.read();
					if((uskByte & 128) > 0) {
						info.addInfo("Banned in Germany", (uskByte & 64) > 0);
						info.addInfo("USK rating", uskByte & 0x1f, USK_RATINGS);
					}

					int pegiByte = s.read();
					if((pegiByte & 128) > 0) {
						info.addInfo("Banned in Europe", (pegiByte & 64) > 0);
						info.addInfo("PEGI (Europe) rating", pegiByte & 0x1f, PEGI_RATINGS);
					}

					int reservedRating2Byte = s.read();
					if((reservedRating2Byte & 128) > 0) {
						info.addInfo("Banned in <reserved 2>", (reservedRating2Byte & 64) > 0);
						info.addInfo("<reserved 2> rating", reservedRating2Byte & 0x1f);
					}

					int pegiPortugalByte = s.read();
					if((pegiPortugalByte & 128) > 0) {
						info.addInfo("Banned in Portgual", (pegiPortugalByte & 64) > 0);
						info.addInfo("PEGI (Portgual) rating", pegiPortugalByte & 0x1f, PEGI_PORTUGAL_RATINGS);
					}

					int pegiUKByte = s.read();
					if((pegiUKByte & 128) > 0) {
						info.addInfo("Banned in the UK", (pegiUKByte & 64) > 0);
						info.addInfo("PEGI rating", pegiUKByte & 0x1f, PEGI_UK_RATINGS);
					}

					int agcbByte = s.read();
					if((agcbByte & 128) > 0) {
						info.addInfo("Banned in Australia", (agcbByte & 64) > 0);
						info.addInfo("AGCB rating", agcbByte & 0x1f, AGCB_RATINGS);
					}

					int grbByte = s.read();
					if((grbByte & 128) > 0) {
						info.addInfo("Banned in South Korea", (grbByte & 64) > 0);
						info.addInfo("GRB rating", grbByte & 0x1f, GRB_RATINGS);
					}
					//The next 6 bytes are reserved, and then there's apparently
					//something involving DEJUS (Brazil), GSRMR (Taiwan) and
					//PEGI (Finland) in there*
				}
				//TODO Secure area at 0x400
				//TODO Read FNT/FAT maybe?

				s.Seek(bannerOffset, System.IO.SeekOrigin.Begin);
				int bannerVersion = s.readShortLE();
				info.addInfo("Banner version", bannerVersion, BANNER_VERSIONS);
				if(BANNER_VERSIONS.ContainsKey(bannerVersion)) {
					byte[] bannerChecksum = s.read(2); //CRC16 of 0x20 to 0x83
					info.addExtraInfo("Banner checksum", bannerChecksum);
					byte[] bannerChecksum2 = s.read(2); //CRC16 of 0x20 to 0x93
					info.addExtraInfo("Banner checksum 2", bannerChecksum2);
					byte[] bannerChecksum3 = s.read(2); //CRC16 of 0x20 to 0xa3
					info.addExtraInfo("Banner checksum 3", bannerChecksum3);
					byte[] bannerChecksum4 = s.read(2); //CRC16 of 0x1240 to 0x23bf
					info.addExtraInfo("Banner checksum 4", bannerChecksum4);
					byte[] bannerReserved = s.read(0x16); //Should be zero filled
					info.addExtraInfo("Banner reserved", bannerReserved);

					byte[] iconBitmap = s.read(0x200);
					byte[] iconPalette = s.read(0x20);
					info.addInfo("Icon", decodeDSIcon(iconBitmap, iconPalette));

					string japaneseTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
					info.addInfo("Japanese title", japaneseTitle);
					string englishTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
					info.addInfo("English title", englishTitle);
					string frenchTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
					info.addInfo("French title", frenchTitle);
					string germanTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
					info.addInfo("German title", germanTitle);
					string italianTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
					info.addInfo("Italian title", italianTitle);
					string spanishTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
					info.addInfo("Spanish title", spanishTitle);
					if(bannerVersion >= 2) {
						string chineseTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
						info.addInfo("Chinese title", chineseTitle);
					}
					if(bannerVersion >= 3) {
						string koreanTitle = s.read(256, Encoding.Unicode).Trim('\0').Replace("\n", "\r\n");
						info.addInfo("Korean title", koreanTitle);
					}

					//Should be zero filled, but it's not
					byte[] titleReserved = s.read(0x800);

					if(bannerVersion >= 0x103) {
						//Same format as DS icon, but animated
						IList<byte[]> dsiIconBitmaps = new List<byte[]>();
						for(int i = 0; i < 8; ++i) {
							dsiIconBitmaps.Add(s.read(0x200));
						}
						IList<byte[]> dsiIconPalettes = new List<byte[]>();
						for(int i = 0; i < 8; ++i) {
							dsiIconPalettes.Add(s.read(0x20));
						}
						byte[] dsiIconSequence = s.read(0x80);
					}
				}
			} finally {
				s.Seek(origPos, System.IO.SeekOrigin.Begin);
			}
		}
	}
}
