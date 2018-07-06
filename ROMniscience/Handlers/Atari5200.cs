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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class Atari5200: Handler {
		//https://atarihq.com/danb/files/5200BIOS.txt
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"a52", "Atari 5200 ROM"},
			{"bin", "Atari 5200 ROM"},
		};

		public override string name => "Atari 5200";

		public static IDictionary<byte, char> ATARI_5200_CHARSET = new Dictionary<byte, char>() {
			//This is all that's known from all known Atari 5200 ROMs (that are in No-Intro)... could be more
			//Lowercase here is used to represent rainbow characters, because how else am I gonna represent them? No really, I dunno
			{0x00, ' '}, //Maybe it's a rainbow space
			{0x01, '!'}, //Rainbow
			{0x07, '\''}, //Rainbow
			{0x0d, '-'}, //Rainbow
			{0x0e, '.'}, //Rainbow
			{0x10, '0'}, //Rainbow
			{0x11, '1'}, //Rainbow
			{0x12, '2'}, //Rainbow
			{0x13, '3'}, //Rainbow
			{0x14, '4'}, //Rainbow
			{0x15, '5'}, //Rainbow
			{0x16, '6'}, //Rainbow
			{0x17, '7'}, //Rainbow
			{0x18, '8'}, //Rainbow
			{0x19, '9'}, //Rainbow
			{0x20, '@'}, //Rainbow
			{0x21, 'a'},
			{0x22, 'b'},
			{0x23, 'c'},
			{0x24, 'd'},
			{0x25, 'e'},
			{0x26, 'f'},
			{0x27, 'g'},
			{0x28, 'h'},
			{0x29, 'i'},
			{0x2a, 'j'},
			{0x2b, 'k'},
			{0x2c, 'l'},
			{0x2d, 'm'},
			{0x2e, 'n'},
			{0x2f, 'o'},
			{0x30, 'p'},
			{0x31, 'q'},
			{0x32, 'r'},
			{0x33, 's'},
			{0x34, 't'},
			{0x35, 'u'},
			{0x36, 'v'},
			{0x37, 'w'},
			{0x38, 'x'},
			{0x39, 'y'},
			{0x3a, 'z'},
			{0x40, ' '},
			{0x41, '!'},
			{0x47, '\''},
			{0x4e, '.'},
			{0x50, '0'},
			{0x51, '1'},
			{0x52, '2'},
			{0x53, '3'},
			{0x54, '4'},
			{0x55, '5'},
			{0x56, '6'},
			{0x57, '7'},
			{0x58, '8'},
			{0x59, '9'},
			{0x5a, ':'},
			{0x61, 'A'},
			{0x62, 'B'},
			{0x63, 'C'},
			{0x64, 'D'},
			{0x65, 'E'},
			{0x66, 'F'},
			{0x67, 'G'},
			{0x68, 'H'},
			{0x69, 'I'},
			{0x6a, 'J'},
			{0x6b, 'K'},
			{0x6c, 'L'},
			{0x6d, 'M'},
			{0x6e, 'N'},
			{0x6f, 'O'},
			{0x70, 'P'},
			{0x71, 'Q'},
			{0x72, 'R'},
			{0x73, 'S'},
			{0x74, 'T'},
			{0x75, 'U'},
			{0x76, 'V'},
			{0x77, 'W'},
			{0x78, 'X'},
			{0x79, 'Y'},
			{0x7a, 'Z'},
			{0xe1, ' '}, //Not sure about this one, but it really does display as blank. Maybe all unknown characters just display as blank?
		};

		public static string decodeAtari5200String(byte[] bytes) {
			StringBuilder sb = new StringBuilder();
			foreach(byte b in bytes) {
				if (ATARI_5200_CHARSET.ContainsKey(b)) {
					sb.Append(ATARI_5200_CHARSET[b]);
				} else {
					sb.Append("<0x" + Convert.ToString(b, 16) + ">");
				}
			}
			return sb.ToString();
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Atari 5200");
			var s = file.stream;
			s.Position = file.length - 3;// 0x1ffd;

			bool skipLogo = s.read() == 0xff; //Frogger does this, maybe some others. I guess this is the second digit of the year being set to F?
			info.addInfo("Skip BIOS", skipLogo);
			short entryPoint = s.readShortLE();
			info.addInfo("Entry point", entryPoint, ROMInfo.FormatMode.HEX);

			if (!skipLogo) {
				s.Position = file.length - 24; // 0x1fe8;
				//Copyright info is usually displayed on the screen, the BIOS displays a graphic from its own ROM and something like "TITLE\nCOPYRIGHT YEAR ATARI"
				//This can be used to get the name for our purposes, though it will be padded with @ (displays as blank I guess?) or null char to make it centred on screen
				//string name = s.read(20, Encoding.ASCII).Replace("\x0", "<0!>").Replace(" ", "<sp>");
				string name = decodeAtari5200String(s.read(20));
				info.addInfo("Internal name", name);

				int yearFirstDigit = s.read() - 0x50;
				int yearSecondDigit = s.read() - 0x50;

				//Nice Y2K bug m8
				info.addInfo("Year", 1900 + (yearFirstDigit * 10) + yearSecondDigit);
			}

			//TODO: BIOS disassemby also mentions something about PAL carts? I wasn't aware of any PAL carts or even PAL 5200 systems, so that's interesting if that actually exists; it seems that PAL carts have something different at xFE7
		}
	}
}