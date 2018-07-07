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
	class Picno : Handler {
		//Not to be confused with Sega Pico! I mention that because I keep confusing it with Sega Pico
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"bin", "Konami Picno ROM"},
		};

		public override string name => "Konami Picno";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Konami Picno");

			var s = file.stream;

			//This is different than what you'd see on the box art and hence what is considered the "serial" as far as the software list is concerned. But similar, though. These start with ZPJ and the actual serials start with RX, but other than that, if the number is less than 100 you add 100 to it to get the actual serial, e.g. ZPJ006 (Anime Enikki) -> RX106, but ZPJ116 (Kanji Club) -> RX116.
			//The weird exception is Montage, which has ZPJ001 internally, but RX102 externally (RX101 is the undumped save card)
			string productCode = s.read(6, Encoding.ASCII);
			info.addInfo("Product code", productCode);

			string name = s.read(10, Encoding.ASCII).TrimEnd('+'); //Yeah, plus signs. I don't make the rules
			info.addInfo("Internal name", name);
		}
	}
}
