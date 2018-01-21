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

using System.IO;

namespace ROMniscience.IO {
	/*This is to wrap the various IO classes rather than making a mess of figuring out how to use each different one, if that makes sense
	If that doesn't make sense, I'm just lazy
	It is my philosophy that the programmer should be enabled to be as lazy as possible while still achieving 
	the same functional end result, which I call "productivity"
	*/
	abstract class InputStream: Stream {
		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Position {
			set => Seek(value, SeekOrigin.Begin);
		}

		public override void SetLength(long value) {
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}

		public virtual int read() => ReadByte();

		public virtual byte[] read(int bytes) {
			byte[] buf = new byte[bytes];
			int bytesRead = Read(buf, 0, bytes);
			if(bytesRead == 0) {
				return new byte[] { };
			} else if(bytesRead == bytes) {
				return buf;
			} else {
				byte[] buf2 = new byte[bytesRead];
				Array.Copy(buf, buf2, bytesRead);
				return buf2;
			}
		}

		public String read(int length, Encoding encoding) {
			return encoding.GetString(read(length));
		}

		public int readIntBE() {
			return (read() << 24) | (read() << 16) | (read() << 8) | read();
		}

		public int readIntLE() {
			return read() | (read() << 8) | (read() << 16) | (read() << 24);
		}

		public int readShortBE() {
			return (read() << 8) | read();
		}

		public int readShortLE() {
			return read() | (read() << 8);
		}

		public static InputStream cloneInputStream(InputStream s) {
			return new MemoryInputStream(s.read((int)s.Length));
		}
	}
}
