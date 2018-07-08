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
using System.Windows.Forms;
using ROMniscience.GUI;

namespace ROMniscience {
	static class MainProgram {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] blargs) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if(blargs.Length > 0) {
				ViewIndividualFile.viewFile(blargs[0]);
			} else {
				Application.Run(new MainWindow());
			}
		}

		//Just here in case some things work on normal .NET but not Mono... which is the case, sadly
		private static Lazy<bool> _isMono => new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

		public static bool isMono => _isMono.Value;

		private static Encoding getShiftJIS() {
			try {
				//Just to be annoying, Shift-JIS isn't always available (it _probably_ is, but it's possible that
				//it isn't, since it's not listed as having ".NET Framework Support" in that one table on MSDN or
				//whatever). But we need it for a lot of stuff
				return Encoding.GetEncoding("shift_jis");
			} catch(ArgumentException ae) {
				//Bugger... well, I guess the worst that can happen is that there's question marks everywhere
				System.Diagnostics.Trace.TraceWarning(ae.Message);
				return Encoding.ASCII;
			}
		}
		public static readonly Encoding shiftJIS = getShiftJIS();
	}
}
