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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class VC4000 : StubHandler {
		//There's like 9999 clones for this system, 1292 AVPS being one of them which maybe came first and maybe should be considered the "main" thing, or maybe it should be a separate thing, or... I don't know. It's really confusing. I'm not feeling that mentally sharp at the moment, if you are an Interton VC 4000 fanatic, go ahead and tell me which way around things should go. I'm not being sarcastic, I just don't know how to make things make sense for whatever end user would be involved here
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"bin", "Interton VC 4000 ROM"},
			{"rom", "Interton VC 4000 ROM"},
			//There's also .pgm and .tvc, but should they specifically be for that Elektron thingy?
		};

		public override string name => "Interton VC 4000";
	}
}
