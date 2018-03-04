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
	class ROMFile: IDisposable {
		//TODO Should probably refactor this into BaseROMFile/ROMFile/CompressedROMFile classes I guess
		private IArchiveEntry entry;

		protected ROMFile() {

		}

		public ROMFile(FileInfo f) {
			path = f;
			stream = new WrappedInputStream(File.OpenRead(f.FullName));
		}

		public ROMFile(IArchiveEntry entry, FileInfo path) {
			this.entry = entry;
			this.path = path;
			stream = new WrappedInputStream(entry.OpenEntryStream());
		}


		public virtual FileInfo path {
			get;
		}

		public virtual string name {
			get {
				if(compressed) {
					return entry.Key;
				} else {
					return path.Name;
				}
			}
		}

		public virtual InputStream stream {
			get;
		}

		public virtual bool compressed => entry != null;

		public virtual long length {
			get {
				if(compressed) {
					return entry.Size;
				} else {
					return stream.Length;					
				}
			}
		}

		public virtual long compressedLength {
			get {
				if(compressed) {
					return entry.CompressedSize;
				} else {
					throw new NotImplementedException();
				}
			}
		}

		public virtual string extension => Path.GetExtension(name);
		
		//ACKSHUALLY it is being disposed, but because it's an auto property, it complains about the backing store being not disposed directly because bug
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
		public void Dispose() {
			((IDisposable)stream).Dispose();
		}
	}
}
