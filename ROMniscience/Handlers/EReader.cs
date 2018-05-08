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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class EReader : Handler {
		//http://problemkaputt.de/gbatek.htm#gbacartereader
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"bin", "e-Reader card binary"},
			{"raw", "e-Reader card raw dump"},
		};

		public override string name => "e-Reader";

		public static readonly IDictionary<byte, string> DOTCODE_TYPES = new Dictionary<byte, string> {
			{2, "Short strip"},
			{3, "Long strip"},
		};

		public static readonly IDictionary<int, string> CARD_TYPES = new Dictionary<int, string> {
			{2, "Pokemon card application with music A"},
			{3, "Pokemon card application with music A (2)"},
			{4, "Pokemon card application with music B"},
			{5, "Pokemon card application with music B (2)"},
			{6, "P-Letter Attacks"}, //The what now? GBATEK, please make more sense
			{7, "P-Letter Attacks (2)"},
			{8, "Construction Escape"},
			{9, "Construction Escape (2)"},
			{10, "Construction Action"},
			{11, "Construction Action (2)"},
			{12, "Construction Melody Box"},
			{13, "Construction Melody Box (2)"},
			{14, "Application"},
			{30, "Application (2)"},
			{15, "Game specific application"}, //Oddly enough, the Pokemon Battle-e cards seem to be 14 and not this, it's used for Super Mario Advance 4 cards though
			{31, "Game specific application (2)"},

			{16, "P-Letter viewer"}, //What the dicks is that? Why are there 14 of them? Like I have a feeling this has to do with Pokemon TCG cards, but in what way? 14... could be 10 types (since Fairy wasn't invented yet) + energy + 3 different types of trainer cards? Probably not
			{17, "P-Letter viewer (2)"},
			{18, "P-Letter viewer (3)"},
			{19, "P-Letter viewer (4)"},
			{20, "P-Letter viewer (5)"},
			{21, "P-Letter viewer (6)"},
			{22, "P-Letter viewer (7)"},
			{23, "P-Letter viewer (8)"},
			{24, "P-Letter viewer (9)"},
			{25, "P-Letter viewer (10)"},
			{26, "P-Letter viewer (11)"},
			{27, "P-Letter viewer (12)"},
			{28, "P-Letter viewer (13)"},
			{29, "P-Letter viewer (14)"},
		};

		public static readonly IDictionary<int, string> REGIONS = new Dictionary<int, string>() {
			{0, "Japan"}, //No link port
			{1, "USA/Australia"},
			{2, "Japan+"}, //Updated version which was then released internationally
		};

		struct Fragment {
			public byte[] data;
			public byte[] errorInfo;
			public Fragment(byte[] data, byte[] errorInfo) {
				this.data = data;
				this.errorInfo = errorInfo;
			}
		}

		static byte[] shortToBytesLE(short s) {
			byte[] b = new byte[2];
			b[1] = (byte)((s >> 8) & 0xff);
			b[0] = (byte)(s & 0xff);
			return b;
		}

		static byte[] intToBytesLE(int i) {
			byte[] b = new byte[4];
			b[3] = (byte)((i >> 24) & 0xff);
			b[2] = (byte)((i >> 16) & 0xff);
			b[1] = (byte)((i >> 8) & 0xff);
			b[0] = (byte)(i & 0xff);
			return b;
		}

		static byte xorBytes(byte[] b) {
			byte x = b[0];
			for(int i = 1; i < b.Length; ++i) {
				x ^= b[i];
			}
			return x;
		}

		public static void parseUninterleavedData(ROMInfo info, byte[] data) {
			var s = new WrappedInputStream(new MemoryStream(data));

			byte[] fixed1 = s.read(3); //00 30 01, supposedly the 01 means don't calculate global checksum (okay I won't)
			info.addInfo("Fixed value 1", fixed1, true);

			int primaryType = s.read();
			int cardTypeUpperBit = primaryType & 1; //Rest of primaryType is unknown

			byte[] fixed2 = s.read(2); //00 01
			info.addInfo("Fixed value 2", fixed2, true);

			short stripSize = s.readShortBE(); //FIXME Not sure if this is BE or LE because it seems wrong either way... but BE is closest to not being wrong (LE results in everything being 4KB which is definitely not the case... or is it? No, that can't be right. I'm confused...)
			info.addInfo("Strip size", stripSize, ROMInfo.FormatMode.SIZE);

			byte[] fixed3 = s.read(4); //00 00 10 12
			info.addInfo("Fixed value 3", fixed3, true);

			short regionAndType = s.readShortLE();
			int cardTypeLowerBits = (regionAndType & 0b0000_0000_1111_0000) >> 4;
			int cardType = (cardTypeUpperBit << 4) | cardTypeLowerBits;
			info.addInfo("Type", cardType, CARD_TYPES);
			int region = (regionAndType & 0b0000_1111_0000_0000) >> 8;
			info.addInfo("Region", region, REGIONS);

			int stripType = s.read();
			//2 = short and 1 = long, but we might have already gotten that from the .raw block header. Or have we?

			int fixed4 = s.read(); //0
			byte[] unknown = s.read(2);
			int fixed5 = s.read(); //0x10
			info.addInfo("Fixed value 4", fixed4, true);
			info.addInfo("Unknown 4", unknown, true);
			info.addInfo("Fixed value 5", fixed5, true);

			//"Data Checksum [13h-14h] is the complement (NOT) of the sum of all halfwords in all Data Fragments, however, it's all done in reversed byte order: checksum is calculated with halfwords that are read in MSB,LSB order, and the resulting checksum is stored in MSB,LSB order in the Header Fragment."
			//Needless to say I will not bother
			short dataChecksum = s.readShortLE();
			info.addInfo("Data checksum", dataChecksum, ROMInfo.FormatMode.HEX, true);

			byte[] fixed6 = s.read(5); //19 00 00 00 08
			info.addInfo("Fixed value 6", fixed6, true);

			string copyright = s.read(8, Encoding.ASCII); //Should be NINTENDO
			info.addInfo("Copyright", copyright, true);

			byte[] fixed7 = s.read(4); //00 22 00 09
			info.addInfo("Fixed value 7", fixed7, true);

			int sizeInfo = s.readIntLE();
			//Bit 0 is unknown, but it seems to be only set for Pokemon TCG cards. Not all of them though, so there's no workarounds for them being "funny"
			int stripNumber = (sizeInfo & 0b0000_0000_0000_0000_0000_0000_0001_1110) >> 1;
			int numberOfStrips = (sizeInfo & 0b0000_0000_0000_0000_0000_0001_1110_0000) >> 5; //FIXME This ain't right sometimes... what's going on? Anything that's a normal e-Reader card is fine, Pokemon TCG cards are often not. I blame Wizards of the Coast for everything, like sometimes you'll have stripNumber > numberOfStrips, or stripNumber == 0, or numberOfStrips == 0, and that's obviously wrong
			int sizeOfAllStrips = (sizeInfo & 0b0000_0000_1111_1111_1111_1110_0000_0000) >> 9; //This ain't right either... 
			info.addInfo("Strip number", stripNumber);
			info.addInfo("Number of strips", numberOfStrips);
			info.addInfo("Size of all strips", sizeOfAllStrips, ROMInfo.FormatMode.SIZE);

			int flags = s.readIntLE();
			info.addInfo("Permission to save", (flags & 1) > 0 ? "Prompt for save" : "Start immediately");
			bool hasSubTitle = (flags & 2) == 0;
			info.addInfo("Has sub-title", hasSubTitle);
			bool isNES = (flags & 4) > 0;

			int headerChecksum = s.read(); //regionAndType, unknown, sizeInfo, flags xored together
			int globalChecksum = s.read(); //"Global Checksum [2Fh] is the complement (NOT) of the sum of the first 2Fh bytes in the Data Header plus the sum of all Data Fragment checksums; the Data Fragment checksums are all 30h bytes in a fragment XORed with each other." what
			info.addInfo("Checksum", headerChecksum, ROMInfo.FormatMode.HEX, true);
			int calculatedHeaderChecksum = xorBytes(shortToBytesLE(regionAndType)) ^ xorBytes(unknown) ^ xorBytes(intToBytesLE(sizeInfo)) ^ xorBytes(intToBytesLE(flags));
			info.addInfo("Calculated checksum", calculatedHeaderChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksum valid?", headerChecksum == calculatedHeaderChecksum);
			info.addInfo("Global checksum", globalChecksum, ROMInfo.FormatMode.HEX, true);

			bool isPokemon = cardType >= 2 && cardType <= 5; //It warms my heart immensely to write this line
															 //Well, what it really means is "has stats"

			int mainTitleLength = isPokemon ? 17 : 33;

			Encoding encoding = region == 1 ? Encoding.ASCII : MainProgram.shiftJIS;

			//FIXME This is broken for "Construction" things and "P-Letter" things, do they not have a title?
			byte[] mainTitle = s.read(mainTitleLength);
			mainTitle = mainTitle.TakeWhile(b => b != 0).ToArray();
			//This seems to be blank for most of those applications with "Permission to save" == "Start immediately", but could also just be things being broken
			info.addInfo("Internal name", encoding.GetString(mainTitle));

			if (hasSubTitle) {
				int subTitleLength = isPokemon ? 21 : 33;

				for (int i = 0; i < numberOfStrips; ++i) {
					string prefix = "Sub-title " + (i + 1);
					byte[] subTitleBytes = s.read(subTitleLength);

					string subTitle;
					if (isPokemon) {
						int stats = (subTitleBytes[2] << 16) | (subTitleBytes[1] << 8) | subTitleBytes[0];

						int hp = (stats & 0xf) * 10;
						info.addInfo(prefix + " Pokemon HP", hp);

						int id3 = (stats & 0b0000_0000_0000_0000_0111_0000) >> 4;
						int id2 = (stats & 0b0000_0000_0011_1111_1000_0000) >> 7;
						int id1 = (stats & 0b0000_0111_1100_0000_0000_0000) >> 14;

						info.addInfo(prefix + " ID", String.Format("{0}-{1:D2}-{2}", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[id1], id2 + 1, "ABCDEFG#"[id3]));

						subTitle = encoding.GetString(subTitleBytes.Skip(3).TakeWhile(b => b != 0).ToArray());
					} else {
						subTitle = encoding.GetString(subTitleBytes.TakeWhile(b => b != 0).ToArray());
					}
					info.addInfo(prefix, subTitle);
				}
			}


			if (stripNumber == 1) {
				short vpkSize = s.readShortLE();
				info.addInfo("VPK size", vpkSize, ROMInfo.FormatMode.SIZE);

				byte[] check = s.read(4);
				if (check[0] == 0 && check[1] == 0 && check[2] == 0 && check[3] == 0) {
					//GBA type cards have a 4 byte value here for some reason
					info.addInfo("Application type", "GBA");
				} else {
					info.addInfo("Application type", isNES ? "NES" : "Z80");
					s.Seek(-4, SeekOrigin.Current);
				}
			} else {
				info.addInfo("Application type", isNES ? "NES" : "GBA/Z80");
			}

			//The rest of this is VPK compressed data (should be vpkSize)
		}

		public static byte[] uninterleaveRawFile(ROMInfo info, ROMFile file) {
			const int BLOCK_SIZE = 102;

			IList<byte> blockHeader = new List<byte>();
			IList<byte[]> blocks = new List<byte[]>();

			var s = file.stream;
			while (true) {
				byte[] blockHeaderPart = s.read(2);
				if (blockHeaderPart.Length < 2) {
					break;
				}
				byte[] block = s.read(BLOCK_SIZE);

				blockHeader.Add(blockHeaderPart[0]);
				blockHeader.Add(blockHeaderPart[1]);

				blocks.Add(block);
			}

			byte unknown1 = blockHeader[0];
			info.addInfo("Unknown 1", unknown1, true);

			byte dotcodeType = blockHeader[1];
			info.addInfo("Dotcode type", dotcodeType, DOTCODE_TYPES);

			byte unknown2 = blockHeader[2];
			info.addInfo("Unknown 2", unknown2, true);

			byte addressOfFirstBlock = blockHeader[3]; //We don't need this... I think
			info.addInfo("Address of first block", addressOfFirstBlock, ROMInfo.FormatMode.HEX, true);

			byte totalFragmentSize = blockHeader[4];
			byte errorInfoSize = blockHeader[5]; //How much of totalFragmentSize is error info
			int actualFragmentSize = totalFragmentSize - errorInfoSize;
			info.addInfo("Total fragment size", totalFragmentSize, ROMInfo.FormatMode.SIZE);
			info.addInfo("Error info size", errorInfoSize, ROMInfo.FormatMode.SIZE);
			info.addInfo("Actual fragment size", actualFragmentSize, ROMInfo.FormatMode.SIZE);

			byte unknown3 = blockHeader[6];
			info.addInfo("Unknown 3", unknown3, true);

			byte interleaveValue = blockHeader[7];
			info.addInfo("Interleave value", interleaveValue);

			byte[] reedSolomon = blockHeader.Skip(8).ToArray();
			info.addInfo("Reed-Solomon error correction info for block header", reedSolomon); //Not sure what use I have for that

			IList<Fragment> fragments = new List<Fragment>();

			for (int i = 0; i < interleaveValue; ++i) {
				IList<byte> fragment = new List<byte>();
				for (int j = 0, pos = i; j < totalFragmentSize; ++j) {
					
					int blockNum = pos / (BLOCK_SIZE);
					byte[] block = blocks[blockNum];
					fragment.Add(block[pos % (BLOCK_SIZE)]);
					pos += interleaveValue;

				}

				byte[] fragmentData = fragment.Take(actualFragmentSize).ToArray();
				byte[] fragmentErrorInfo = fragment.Skip(actualFragmentSize).ToArray();
				fragments.Add(new Fragment(fragmentData, fragmentErrorInfo));
			}

			IList<byte> actualData = new List<byte>();
			foreach (var fragment in fragments) {
				foreach (byte b in fragment.data) {
					actualData.Add(b);
				}
			}

			return actualData.ToArray();
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "e-Reader");
			if ("raw".Equals(file.extension)) {
				byte[] data = uninterleaveRawFile(info, file);
				parseUninterleavedData(info, data);
				return;
			}

			//Not sure how we'd handle .bin yet. However, most dumps are .raw instead, so it should be fine
		}
	}
}
