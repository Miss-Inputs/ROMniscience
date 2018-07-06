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

namespace ROMniscience.Handlers.Stubs {
	//http://www.orphanedgames.com/APF/
	class APF : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"bin", "APF-MP1000 ROM"},
		};

		public override string name => "APF-MP1000"; //Or is it "APF Microcomputer System"? I don't fuckin know

		public static void parseAPFCart(ROMInfo info, WrappedInputStream s) {
			//First byte should always be 0xbb, and I should check that, but ehhhhh
			s.Position = 1;
			ushort menuString = (ushort)(((ushort)s.readShortBE()) - 0x8000);
			int numChoices = s.read();
			info.addInfo("Number of menu choices", numChoices);

			//Next byte must be 0, not very interesting I know
			s.Position = menuString;

			//Basically this just writes a giant string, but the screen is 32 chars wide (in text mode, which the game menus are; 16 lines long if you care) so it wraps there
			List<byte> bytes = new List<byte>();

			while (true) {
				int b = s.read();
				if(b == -1 || b == 255) {
					//255 indicates end of string, but end of file would imply end of string too (or would that just crash or do bad things on a real cart that didn't terminate its string? Oh well)
					break;
				}

				if(b < 0x80) {
					bytes.Add((byte)b);
				} else if(b > 0xc0) {
					//Heckin control characters
					//0x80 to 0xbf are ignored (and 0xc0 would write 0 characters)
					byte fillByte;
					if(b >= 0xe0) {
						fillByte = 0x20;
					} else {
						int nextByte = s.read();
						if(b == -1) {
							break;
						}

						fillByte = (byte)nextByte;
						if(fillByte > 0x7f) {
							//These are actually coloured drawing shapes, but we don't really have those in 2018, so let's just pretend they're spaces
							//Note that 0x60 will appear as a light green square and not `
							fillByte = 0x20;
						}
					}

					int num = b & 0x1f;

					for (int i = 0; i < num; ++i){
						bytes.Add(fillByte);
					}
				}
			}

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < bytes.Count; i += 32) {
				var line = bytes.GetRange(i, 32).ToArray();
				sb.AppendLine(Encoding.ASCII.GetString(line)); //Well it's not quite ASCII but still... okay yeah I should fix that
			}
			//Actual character set:
			//0x00 - 0x3f: Light green text inside dark green squares
			//@ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \ ] (up) (left) (space) ! " # $ % & * ( ) <diamond> + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ?
			//0x40 - 0x7f: That but dark green text inside light green squares (hence 0x60, being a space, appears as a light green square)
			//0x80 - 0xff (for fill bytes): Drawing characters (coloured 4x4 grid on black background):
			//	High nibble: 8 = green, 9 = yellow, a = blue, b = red, c = white, d = cyan, e = magenta, f = orange
			//	Low nibble: 0 = not filled in (black square), 1 = lower right corner, 2 = lower left, 3 = lower, 4 = upper right, 5 = right, 6 = upper right and lower left, 7 = all except upper left, 8 = upper left, 9 = upper left and lower right, a = left, b = all except upper right, c = upper, d = all except lower left, e = all except lower right, f = filled in completely
			//
			//It's just lucky this means the dark-on-light alphabet and the light-on-dark numbers match up with ASCII, and the cartridges tend to do that
			//TODO Do that thing (can't be bothered implementing a character map thing right now)

			info.addInfo("Menu text", sb.ToString());
			//Hmmmmm might be cool to parse this to get the main names of the games inside the cartridge (if any), and the menu options? But that requires making some assumptions on how developers like to format their games, which is only _sort of_ consistent with the official carts
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if ("bin".Equals(file.extension)) {
				parseAPFCart(info, file.stream);
			}
		}
	}
}
