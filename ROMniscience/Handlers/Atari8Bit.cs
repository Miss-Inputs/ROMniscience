/*
 * The MIT License
 *
 * Copyright 2018 Megan Leet(Zowayix).
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
	class Atari8Bit : Handler {
		//https://github.com/dmlloyd/atari800/blob/master/DOC/cart.txt
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>{
			{"bin", "Atari 8-bit cartridge"},
			{"rom", "Atari 8-bit cartridge"},
			{"car", "Atari 8-bit cartridge"},
			{"atr", "Atari 8-bit floppy"},
			{"dsk", "Atari 8-bit floppy"},
			{"xex", "Atari 8-bit executable"},
			//Casettes are just .wav, apparently
			//TODO: There are also some .bas files I have from somewhere... they're not plain text BASIC source, but presumably have something to do with BASIC. Are they media images? Raw files?
		};

		public override string name => "Atari 8-bit computers";

		public enum CartPlatform {
			AnyAtari8Bit,
			Atari800,
			AtariXL,
			AtariXE,
			XEGS,
			Atari5200, //While you probably wouldn't see a 5200 ROM with this CART header thingy, it's in the specification
		}

		public struct CartType {
			public string name;
			public long size;
			public CartPlatform platform;
			public CartType(string name, long size, CartPlatform platform) {
				this.name = name;
				this.size = size;
				this.platform = platform;
			}
		}

		public static readonly IDictionary<int, CartType> CART_TYPES = new Dictionary<int, CartType> {
			{1, new CartType("Standard 8KB", 8 * 1024, CartPlatform.AnyAtari8Bit)},
			{2, new CartType("Standard 16KB", 16 * 1024, CartPlatform.AnyAtari8Bit)},
			{3, new CartType("OSS two chip 16KB", 16 * 1024, CartPlatform.AnyAtari8Bit)},
			{4, new CartType("5200 32KB", 32 * 1024, CartPlatform.Atari5200)},
			{5, new CartType("DB 32KB", 32 * 1024, CartPlatform.AnyAtari8Bit)},
			{6, new CartType("5200 two chip 16KB", 16 * 1024, CartPlatform.Atari5200)},
			{7, new CartType("Bounty Bob Strikes Back 5200", 40 * 1024, CartPlatform.Atari5200)},
			{8, new CartType("Williams 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{9, new CartType("Express 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{10, new CartType("Diamond 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{11, new CartType("SpartaDOS X 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{12, new CartType("XEGS 32KB", 32 * 1024, CartPlatform.XEGS)},
			{13, new CartType("XEGS 64KB", 64 * 1024, CartPlatform.XEGS)},
			{14, new CartType("XEGS 128KB", 128 * 1024, CartPlatform.XEGS)},
			{15, new CartType("OSS one chip 16KB", 16 * 1024, CartPlatform.AnyAtari8Bit)},
			{16, new CartType("One chip 5200 16KB", 16 * 1024, CartPlatform.Atari5200)},
			{17, new CartType("Altrax 128KB", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{18, new CartType("Bounty Bob Strikes Back", 40 * 1024, CartPlatform.AnyAtari8Bit)},
			{19, new CartType("5200 8KB", 8 * 1024, CartPlatform.Atari5200)},
			{20, new CartType("5200 4KB", 4 * 1024, CartPlatform.Atari5200)},
			{21, new CartType("Right slot 8KB", 8 * 1024, CartPlatform.Atari800)},
			{22, new CartType("Williams 32KB", 32 * 1024, CartPlatform.AnyAtari8Bit)},
			{23, new CartType("XEGS 256KB", 256 * 1024, CartPlatform.XEGS)},
			{24, new CartType("XEGS 512KB", 512 * 1024, CartPlatform.XEGS)},
			{25, new CartType("XEGS 1MB", 1024 * 1024, CartPlatform.XEGS)},
			{26, new CartType("MegaCart 16KB", 16 * 1024, CartPlatform.AnyAtari8Bit)},
			{27, new CartType("MegaCart 32KB", 32 * 1024, CartPlatform.AnyAtari8Bit)},
			{28, new CartType("MegaCart 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{29, new CartType("MegaCart 128KB", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{30, new CartType("MegaCart 256KB", 256 * 1024, CartPlatform.AnyAtari8Bit)},
			{31, new CartType("MegaCart 512KB", 512 * 1024, CartPlatform.AnyAtari8Bit)},
			{32, new CartType("MegaCart 1MB", 1024 * 1024, CartPlatform.AnyAtari8Bit)},
			{33, new CartType("Switchable XEGS 32KB", 32 * 1024, CartPlatform.XEGS)},
			{34, new CartType("Switchable XEGS 64KB", 64 * 1024, CartPlatform.XEGS)},
			{35, new CartType("Switchable XEGS 128KB", 128 * 1024, CartPlatform.XEGS)},
			{36, new CartType("Switchable XEGS 256KB", 256 * 1024, CartPlatform.XEGS)},
			{37, new CartType("Switchable XEGS 512KB", 512 * 1024, CartPlatform.XEGS)},
			{38, new CartType("Switchable XEGS 1MB", 1024 * 1024, CartPlatform.XEGS)},
			{39, new CartType("Phoenix 8KB", 8 * 1024, CartPlatform.AnyAtari8Bit)},
			{40, new CartType("Blizzard 16KB", 16 * 1024, CartPlatform.AnyAtari8Bit)},
			{41, new CartType("Atarimax 128KB Flash", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{42, new CartType("Atarimax 1MB Flash", 1024 * 1024, CartPlatform.AnyAtari8Bit)},
			{43, new CartType("SpartaDOS X 128KB", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{44, new CartType("OSS 8KB", 8 * 1024, CartPlatform.AnyAtari8Bit)},
			{45, new CartType("OSS two chip 16KB", 16 * 1024, CartPlatform.AnyAtari8Bit)},
			{46, new CartType("Blizzard 4KB", 4 * 1024, CartPlatform.AnyAtari8Bit)},
			{47, new CartType("AST 32KB", 32 * 1024, CartPlatform.AnyAtari8Bit)},
			{48, new CartType("Atrax SDX 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{49, new CartType("Atrax SDX 128KB", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{50, new CartType("Turbosoft 64KB", 64 * 1024, CartPlatform.AnyAtari8Bit)},
			{51, new CartType("Turbosoft 128KB", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{52, new CartType("Ultracart 32KB", 32 * 1024, CartPlatform.AnyAtari8Bit)},
			{53, new CartType("Low bank 8KB", 8 * 1024, CartPlatform.AnyAtari8Bit)},
			{54, new CartType("SIC! 128KB", 128 * 1024, CartPlatform.AnyAtari8Bit)},
			{55, new CartType("SIC! 256KB", 256 * 1024, CartPlatform.AnyAtari8Bit)},
			{56, new CartType("SIC! 512KB", 512 * 1024, CartPlatform.AnyAtari8Bit)},
			{57, new CartType("Standard 2KB", 2 * 1024, CartPlatform.AnyAtari8Bit)},
			{58, new CartType("Standard 4KB", 4 * 1024, CartPlatform.AnyAtari8Bit)},
			{59, new CartType("Right slot 4KB", 4 * 1024, CartPlatform.Atari800)},
			{60, new CartType("Blizzard 32KB", 32 * 1024, CartPlatform.AnyAtari8Bit)},
		};

		static bool isCARTMagic(byte[] magic) {
			//CART in ASCII
			return magic[0] == 0x43 && magic[1] == 0x41 && magic[2] == 0x52 && magic[3] == 0x54;
		}

		public static void parseCART(ROMInfo info, WrappedInputStream s) {
			int cartTypeNumber = s.readIntBE();
			if (CART_TYPES.TryGetValue(cartTypeNumber, out CartType cartType)) {
				info.addInfo("Type", cartType.name);
				info.addInfo("ROM size", cartType.size, ROMInfo.FormatMode.SIZE);
				if (cartType.platform == CartPlatform.Atari5200) {
					info.addInfo("Platform", "Atari 5200");
				} else if (cartType.platform == CartPlatform.XEGS) {
					info.addInfo("Platform", "XEGS");
				} else if (cartType.platform == CartPlatform.Atari800) {
					info.addInfo("Platform", "Atari 800");
				} else {
					info.addInfo("Platform", "Atari 400/800/XL/XE");
				}
			} else {
				info.addInfo("Type", String.Format("Unknown ({0})", cartTypeNumber));
				info.addInfo("Platform", "Atari 8-bit");
			}
			int checksum = s.readIntBE(); //TODO: How is this calculated?
			info.addInfo("Checksum", checksum, ROMInfo.FormatMode.HEX);
			int unused = s.readIntBE();
			info.addInfo("Unused", unused, true);
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if("bin".Equals(file.extension) || "rom".Equals(file.extension) || "car".Equals(file.extension)) {
				byte[] magic = file.stream.read(4);
				if (isCARTMagic(magic)) {
					info.addInfo("Detected format", "CART header");
					parseCART(info, file.stream);
				} else {
					//Well, nothing we can really do here, then
					info.addInfo("Platform", "Atari 8-bit");
					info.addInfo("Detected format", "Unheadered");
				}
			} else {
				//TODO: Floppy images
				info.addInfo("Platform", "Atari 8-bit");
			}
		}
	}
}
