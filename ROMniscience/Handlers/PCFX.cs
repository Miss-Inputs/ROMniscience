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
	class PCFX : CDBasedSystem {
		//https://bitbucket.org/trap15/pcfxtools/src/26e21d1209d79d73a58c2c362443c8f2baa53cb9/pcfx-cdlink.c?at=master&fileviewer=file-view-default
		public override string name => "NEC PC-FX";

		public override void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream) {
			//Note! This is track 2! Right now this fuckiness just lets you choose whatever track, or the first data track, but like... the thing is on track 2, okay? God damn I need to rewrite this whole entire damn thing

			info.addInfo("Platform", "PC-FX"); //TODO: Can we detect PC-FXGA, which isn't forwards compatible? It doesn't seem to be different in any obvious way so far

			//Don't like this hack, it looks like really I should be taking into account this INDEX 01 00:03:00 business in the cue sheet, but I don't
			//What the cool kids (i.e. Mednafen) seem to do, who probably know what they are doing, is to look through every single track and then look through every single sector looking for the thing
			//So I guess I'll have to do that, except how this works is that it's done on each individual track, so yeah
			//The thing

			long position = 0;
			while (true) {
				stream.Seek(position, System.IO.SeekOrigin.Begin);
				var magic = stream.read(16, Encoding.ASCII);
				if(magic.Length < 16) {
					return;
				}
				if("PC-FX:Hu_CD-ROM".Equals(magic.Substring(0, 15))) {
					break;
				}
				position += 2048;
				if(position > stream.Length) {
					return;
				}
			}

			var bootCode = stream.read(0x7f0);
			info.addInfo("Boot code", bootCode, true);

			var title = stream.read(32, MainProgram.shiftJIS).TrimEnd('\0');
			info.addInfo("Internal name", title);

			//Skip over sect_off, sect_count, prog_off, prog_point (all uint32) because I dunno what those do, and I'm not sure if I need to
			stream.Seek(4 + 4 + 4 + 4, System.IO.SeekOrigin.Current);

			var makerID = stream.read(4);
			info.addInfo("Maker ID", makerID);
			info.addInfo("Maker ID as ASCII", Encoding.ASCII.GetString(makerID).TrimEnd('\0')); //Usually ASCII but I've also seen 05-00-00-00 which is not. Anyway, these are kinda interesting, and I should build a datamabase of them

			var makerName = stream.read(60, MainProgram.shiftJIS).TrimEnd('\0');
			info.addInfo("Publisher", makerName);

			var volumeNumber = stream.readIntLE(); //Disc number? Is 1 on official titles
			info.addInfo("Volume number", volumeNumber);

			var version = stream.readShortLE();
			info.addInfo("Version", version); //Seems to be 256, except for Battle Heat where it's 257. Could be 0x0101 = v1.01? That's like... BCD I guess

			var country = stream.readShortLE();
			info.addInfo("Country", country); //pcfx-cdlink defaults to 1. Anyway, since it was only released in Japan, official games only have the 1 country of... 1, which I guess means Japan

			var date = stream.read(8, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Date", date); //TODO decode; YYYYMMDD format (D is single digit in a homebrew test program, so I guess I could either not care entirely, or just get the year and month if length != 8 after strip)

			//Then there's 0x380 bytes of "pad" and 0x400 bytes of "udata"
		}
	}
}
