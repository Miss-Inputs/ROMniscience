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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.IO {
	static class StreamExtensions {
		public static int read(this Stream s) => s.ReadByte();

		public static byte[] read(this Stream s, int bytes) {
			byte[] buf = new byte[bytes];
			int bytesRead = s.Read(buf, 0, bytes);
			if (bytesRead == 0) {
				return new byte[] { };
			} else if (bytesRead == bytes) {
				return buf;
			} else {
				byte[] buf2 = new byte[bytesRead];
				Array.Copy(buf, buf2, bytesRead);
				return buf2;
			}
		}

		public static String read(this Stream s, int length, Encoding encoding) {
			return encoding.GetString(s.read(length));
		}

		public static int readIntBE(this Stream s) {
			return (s.read() << 24) | (s.read() << 16) | (s.read() << 8) | s.read();
		}

		public static int readIntLE(this Stream s) {
			return s.read() | (s.read() << 8) | (s.read() << 16) | (s.read() << 24);
		}

		public static int readShortBE(this Stream s) {
			return (s.read() << 8) | s.read();
		}

		public static int readShortLE(this Stream s) {
			return s.read() | (s.read() << 8);
		}

		public static Stream cloneInputStream(Stream s) {
			return new MemoryStream(s.read((int)s.Length));
		}
	}
}
