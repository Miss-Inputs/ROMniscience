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
	class ByteSwappedInputStream: WrappedInputStream {
		public ByteSwappedInputStream(Stream s) : base(s) { }

		byte buffer;
		bool haveBuffer = false;

		public override int ReadByte() {
			if(haveBuffer) {
				haveBuffer = false;
				return buffer;
			} else {
				int b1 = base.ReadByte();
				int b2 = base.ReadByte();
				if(b1 == -1 || b2 == -1) {
					return -1;
				}

				buffer = (byte)(b1 & 0xff);
				haveBuffer = true;
				return b2;
			}
		}

		public override byte[] read(int count) {
			bool odd = false;
			if(count % 2 == 1) {
				odd = true;
				if(haveBuffer) {
					count--;
				} else {
					count++;
				}
			}


			byte[] bytes = base.read(count);

			for(var i = 0; i < bytes.Length - 1; i += 2) {
				byte temp = bytes[i];
				bytes[i] = bytes[i + 1];
				bytes[i + 1] = temp;
			}

			if(odd) {
				if(haveBuffer) {
					byte[] temp = new byte[bytes.Length + 1];
					Array.Copy(bytes, 0, temp, 1, bytes.Length);
					temp[0] = buffer;
					haveBuffer = false;
					return temp;
				} else {
					haveBuffer = true;
					buffer = bytes[bytes.Length - 1];
					Array.Resize(ref bytes, bytes.Length - 1);
					return bytes;
				}
			} else if(haveBuffer) {
				byte oldSwappedByte = buffer;
				buffer = bytes[bytes.Length - 1];
				byte[] temp = new byte[bytes.Length + 1]; 
				Array.Copy(bytes, 0, temp, 1, bytes.Length); 
				temp[0] = oldSwappedByte;
				return temp;
			}

			return bytes;
		}

		}
	}
