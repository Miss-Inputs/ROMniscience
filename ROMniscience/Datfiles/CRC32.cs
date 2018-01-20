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
using ROMniscience.IO;
using System.Security.Cryptography;

namespace ROMniscience.Datfiles {
	static class CRC32 {

		private static readonly uint[] crc32Table = initTable();

		private static uint[] initTable() {

			uint[] a = new uint[256];
			for(uint i = 0; i < 256; ++i) {
				uint k = i;
				for(int j = 0; j < 8; ++j) {
					if((k & 1) != 0) {
						k >>= 1;
						k ^= 0xedb88320;
					} else {
						k >>= 1;
					}
				}
				a[i] = k;
			}
			return a;
		}

		public static int crc32(byte[] buf) {
			return crc32(buf, 0);
		}

		public static int crc32(byte[] buf, int existing) {
			uint crc = ~(uint)existing;
			foreach(byte b in buf) {
				crc = (crc >> 8) ^ crc32Table[(crc & 0xff) ^ b];
			}
			return ~(int)crc;
		}
	}
}
