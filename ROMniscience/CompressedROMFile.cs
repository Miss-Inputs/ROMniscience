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
