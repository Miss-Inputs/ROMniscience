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
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class CompressedROMFile : ROMFile {

		private FileInfo archivePath;
		private IArchiveEntry entry;
		private WrappedInputStream archiveStream;

		public CompressedROMFile(IArchiveEntry entry, FileInfo path) {
			this.entry = entry;
			archivePath = path;
			archiveStream = new WrappedInputStream(entry.OpenEntryStream());
		}

		public override FileInfo path => archivePath;
		public override string name => entry.Key;
		public override long length => entry.Size;
		//This isn't entirely the right way to do it and when you have more than one file it'll have a negative compression ratio, but then what _is_ the right way to do it
		public override long compressedLength => entry.Archive.Type == SharpCompress.Common.ArchiveType.SevenZip ? archivePath.Length : entry.CompressedSize;
		public override bool compressed => true;

		public override WrappedInputStream stream => archiveStream;
		public override void Dispose() {
			((IDisposable)archiveStream).Dispose();
		}

		public override WrappedInputStream getSiblingFile(string filename) {
			foreach(var siblingEntry in entry.Archive.Entries) {
				if (siblingEntry.Key.Equals(filename)) {
					return new WrappedInputStream(siblingEntry.OpenEntryStream());
				}
			}
			throw new FileNotFoundException("Archive " + archivePath.FullName + " doesn't contain this file", filename);
		}

		public override bool hasSiblingFile(string filename) {
			foreach (var siblingEntry in entry.Archive.Entries) {
				if (siblingEntry.Key.Equals(filename)) {
					return true;
				}
			}
			return false;
		}
	}
}
