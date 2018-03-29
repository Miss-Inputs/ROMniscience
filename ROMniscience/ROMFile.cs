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
using ROMniscience.IO;
using SharpCompress.Archives;

namespace ROMniscience {
	abstract class ROMFile : IDisposable {

		public abstract FileInfo path {
			get;
		}

		public abstract string name {
			//If compressed = true, this should be the original uncompressed filename
			get;
		}

		public abstract WrappedInputStream stream {
			get;
		}

		public abstract bool compressed {
			get;
		}

		public abstract long length {
			get;
		}

		public abstract long compressedLength {
			//If compressed = false, and someone calls this anyway without checking that first, this should probably just return length instead of throwing an exception or anything
			get;
		}

		public virtual string extension {
			get {
				string path = Path.GetExtension(name);
				if(path == null) {
					return null;
				}

				if(path[0] == '.') {
					path = path.Substring(1);
				}

				return path.ToLowerInvariant();
			}
		}

		public abstract void Dispose();
	}
}
