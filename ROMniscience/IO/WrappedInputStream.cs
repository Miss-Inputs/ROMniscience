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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.IO {
	class WrappedInputStream: InputStream, IDisposable {
		private Stream innerStream;

		public WrappedInputStream(Stream s) {
			if(!s.CanSeek) {
				//You little fucker
				MemoryStream mem = new MemoryStream();
				s.CopyTo(mem);
				mem.Position = 0;
				innerStream = mem;
			} else {
				innerStream = s;
			}
			
		}

		public override long Length => innerStream.Length;
		public override long Position {
			get => innerStream.Position;
			set => innerStream.Position = value;
		}

		public override void Flush() {
			innerStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			return innerStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin) {
			return innerStream.Seek(offset, origin);
		}

		protected override void Dispose(bool disposing) {
			innerStream.Dispose();
		}
	}
}
