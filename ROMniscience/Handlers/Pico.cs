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

namespace ROMniscience.Handlers {
	class Pico: Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			//It seems odd to me that Pico uses the .md file extension when that clearly
			//stands for Megadrive, but eh, I don't make the rules
			//Sure it's the same ROM format, but so is 32X and that gets its own extension
			{"md", "Sega Pico ROM"}
		};
		public override string name => "Sega Pico";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			Megadrive.parseMegadriveROM(info, file.stream);
		}

		public override bool shouldSeeInChooseView() {
			//Since it just passes over to the Megadrive handler, it'll be a bit annoying to have to choose it every time a .md file is encountered
			return false;
		}
	}
}
