﻿/*
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
	class Radio86RK : StubHandler {
		//☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭☭
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"rk", "Radio 86-RK tape image"},
			{"rkr", "Radio 86-RK tape image"},
			{"gam", "Radio 86-RK tape image"},
			{"g16", "Radio 86-RK tape image"},
			{"pki", "Radio 86-RK tape image"},
			//These are clones of the Radio 86-RK or something
			{"rko", "Orion tape iage"},
			{"rkp", "SAM SKB VM Partner-01.01 tape image"},
			{"rka", "Zavod BRA Apogee BK-01 tape image"},
			{"rkm", "Mikroshka tape image"},
			{"rku", "UT-88 tape image"},
			{"rk8", "Mikro-80 tape image"},
			{"rks", "Specialist tape image"},
		};

		public override string name => "Radio 86-RK";
	}
}