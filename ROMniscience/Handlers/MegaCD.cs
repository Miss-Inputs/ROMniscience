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

namespace ROMniscience.Handlers  {
	class MegaCD : CDBasedSystem {
		public override string name => "Mega CD";

		public override IDictionary<string, string> filetypeMap {
			get {
				var map = base.filetypeMap;
				map.Add("md", "Mega CD BIOS");
				return map;
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream) {
			Megadrive.parseMegadriveROM(info, stream, true);
			//There's also a "MAIN SEGAOS" at 0x3000 followed by what appears to be some kind of title. Does that mean anything? I don't know
			//Some more info:
			//https://forums.sonicretro.org/index.php?showtopic=30588
			//https://segaretro.org/Sega_CD_programming_FAQ_(1998-12-06)
			//So 0x200 and beyond may have some kind of boot code which could then be checksummed to determine region, perhaps...
		}
	}
}
