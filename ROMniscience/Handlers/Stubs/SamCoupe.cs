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

namespace ROMniscience.Handlers.Stubs {
	class SamCoupe : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"dsk", "SAM Coupé raw disk image"},
			{"mgt", "SAM Coupé raw disk image"},
			{"sbt", "SAM BooTable disk"},
			{"sad", "SAM Coupé SAD disk image"}, //World of SAM says this is "Aley Keprt's Disk format"
			{"td0", "SAM Coupé Teledisk disk image"},
			{"tap", "SAM Coupé tape image"},
			{"sdf", "SAM Coupé SDF disk image"}, //World of SAM says this is "Si Owen's custom disk format"
			{"edsk", "SAM Coupé extended disk image"},
			{"ds2", "SAM Coupé Velesoft split side format"},
			{"cpm", "SAM Coupé Pro-DOS CP/M image" }
		};

		public override string name => "SAM Coupé";
	}
}
