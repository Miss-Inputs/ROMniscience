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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class NormalROMFile: ROMFile {

		private WrappedInputStream fileStream;

		public NormalROMFile(FileInfo f) {
			path = f;
			fileStream = new WrappedInputStream(f.OpenRead());
		}

		public override FileInfo path {
			get;
		}

		public override string name => path.Name;
		public override bool compressed => false;
		public override long compressedLength => length;
		public override long length => stream.Length;

		public override WrappedInputStream stream => fileStream;

		public override void Dispose() {
			((IDisposable)fileStream).Dispose();
		}

		public override WrappedInputStream getSiblingFile(string filename) {
			string p = Path.Combine(path.DirectoryName, filename);
			return new WrappedInputStream(File.OpenRead(p));
		}

		public override bool hasSiblingFile(string filename) {
			return new FileInfo(Path.Combine(path.DirectoryName, filename)).Exists;
		}
	}
}
