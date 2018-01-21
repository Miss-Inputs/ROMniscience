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
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class Vectrex: Handler {
		//Heck this I basically had to piece together info here from PDFs floating around the place that don't actually 
		//describe the header, just assembly code for homebrew programs that gets turned into a header, and basically I
		//just have to make educated guesses. Anyway, point being is that no URL sources for you

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"vec", "Vectrex ROM"},
			{"bin", "Vectrex ROM"}
		};

		public override string name => "Vectrex";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);
			InputStream s = file.stream;

			string copyright = s.read(6, Encoding.ASCII);
			info.addInfo("Copyright", copyright); //Seems to always just be "g GCE " and lowercase g is the copyright symbol on this thing so I'm told

			string yearString = s.read(4, Encoding.ASCII);
			if(int.TryParse(yearString, out int year)) {
				info.addInfo("Year", year);
			} else {
				info.addInfo("Year", yearString);
			}

			int unknown = s.read();
			info.addExtraInfo("Unknown", unknown); //It seems to just be 0x80

			//Are these big endian? Are these little endian? Are these not 16-bit addresses at all?
			byte[] musicAddress = s.read(2);
			info.addExtraInfo("Music address", musicAddress);
			byte[] unknown2 = s.read(2);
			info.addExtraInfo("Unknown 2", unknown2);
			byte[] unknown3 = s.read(2);
			info.addExtraInfo("Unknown 3", unknown3);

			//Fuck me
			//Well, I think it's safe to say that the title will never be over 255 characters, what with the 6809 being 8-bit
			byte[] titleBytes = s.read(255);
			//My documentation here is actually YouTube videos that show gameplay footage of Vectrex games... I wouldn't know what
			//it actually looks like with the title screen that gets displayed for every game. So from what I can gather by looking at
			//various carts, this is terminated by 0x80 0x00, and then there's a newline sequence for stuff like Art Master where it's
			//formatted on screen as "ART<newline>MASTER" and it's in the ROM as "ART<80 F8 40 04 E0>MASTER" and Melody Master has
			//a new line as well but it's in the ROM as "MELODY<80 F8 50 00 DC>MASTER" and what the frickin' hell even is this console?
			//Anyway I guess I'll just replace 80 F8 with a newline and then skip the next 3 bytes after any 80 F8 encountered and then it
			//turns out Pole Position uses 80 FA for a newline for no frickin reason
			IList<byte> temp = new List<byte>(255);
			//This is gonna suck
			int i = 0;
			while(i < 255) {
				if(titleBytes[i] == 0x80) {
					if(titleBytes[i + 1] == 0) {
						break;
					} else if(titleBytes[i + 1] == 0xf8 || titleBytes[i + 1] == 0xfa) {
						temp.Add(0x0a);
						i += 4;
					}
				} else {
					temp.Add(titleBytes[i]);
				}
				i += 1;
			}
			info.addInfo("Internal name", Encoding.ASCII.GetString(temp.ToArray()).Replace("\n", Environment.NewLine));
		}
	}
}
