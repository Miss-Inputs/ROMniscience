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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Datfiles {
	class DatfileCollection: IEnumerable<XMLDatfile> {

		private IList<XMLDatfile> datfiles {
			get;
		}

		public static DatfileCollection loadFromFolder(DirectoryInfo folder) {
			IList<XMLDatfile> datfiles = new List<XMLDatfile>();
			foreach(var f in folder.EnumerateFiles("*.dat")) {
				try {
					XMLDatfile xf = new XMLDatfile(f.FullName);
					datfiles.Add(xf);
				} catch(System.Xml.XmlException) {
					//Well, that wasn't a valid XML file, moving on
					//TODO: This will still print crap to stdout, so we want to detect if it's a valid XML file some other way
				}
			}

			return new DatfileCollection(datfiles);
		}

		public DatfileCollection(IList<XMLDatfile> datfiles) {
			this.datfiles = datfiles;
		}

		public XMLDatfile.IdentifyResult identify(int crc32, byte[] md5, byte[] sha1) {
			foreach(XMLDatfile datfile in datfiles) {
				XMLDatfile.IdentifyResult result = datfile.identify(crc32, md5, sha1);
				if(result != null) {
					return result;
				}
			}
			return null;
		}

		public static Tuple<int, byte[], byte[]> hash(WrappedInputStream s, long offset) {
			long originalPos = s.Position;
			try {
				MD5 md5 = MD5.Create();
				SHA1 sha1 = SHA1.Create();
				int crc32 = 0;

				s.Position = offset;

				byte[] buf;
				while ((buf = s.read(1024 * 1024 * 10)).Length > 0) {
					md5.TransformBlock(buf, 0, buf.Length, buf, 0);
					sha1.TransformBlock(buf, 0, buf.Length, buf, 0);
					crc32 = CRC32.crc32(buf, crc32);
				}
				md5.TransformFinalBlock(new byte[0], 0, 0);
				sha1.TransformFinalBlock(new byte[0], 0, 0);

				return new Tuple<int, byte[], byte[]>(crc32, md5.Hash, sha1.Hash);
			} finally {
				s.Position = originalPos;
			}
		}

		public XMLDatfile.IdentifyResult identify(WrappedInputStream s, long offset) {
			var hashes = hash(s, offset);
			return identify(hashes.Item1, hashes.Item2, hashes.Item3);
		}

		public IEnumerator<XMLDatfile> GetEnumerator() {
			return datfiles.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return datfiles.GetEnumerator();
		}
	}
}
