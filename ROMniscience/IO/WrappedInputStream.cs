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
	//A convenience class to add some more methods to Stream to make it easier to work with, and ensure it's seekable (extension methods ended up not working out). You'll see it a lot
	class WrappedInputStream: Stream, IDisposable {

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;

		protected Stream innerStream;

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

		public override void SetLength(long value) {
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}

		//There's not really a reason to rename this, but it feels more consistent I guess
		public virtual int read() => ReadByte();

		public virtual byte[] read(int bytes) {
			byte[] buf = new byte[bytes];
			int bytesRead = Read(buf, 0, bytes);
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

		public String read(int length, Encoding encoding) {
			return encoding.GetString(read(length));
		}

		public int readIntBE() {
			return (read() << 24) | (read() << 16) | (read() << 8) | read();
		}

		public int readIntLE() {
			return read() | (read() << 8) | (read() << 16) | (read() << 24);
		}

		public short readShortBE() {
			return (short)((read() << 8) | read());
		}

		public short readShortLE() {
			return (short)(read() | (read() << 8));
		}


		public string readNullTerminatedString(Encoding encoding, int maxLength = -1) {
			List<Byte> buf = new List<byte>();

			while (true) {
				int b = read();
				if (b <= 0) {
					break;
				}
				buf.Add((byte)b);
				if (maxLength > 0 && buf.Count >= maxLength) {
					break;
				}
			}

			return encoding.GetString(buf.ToArray());
		}

	}
}
