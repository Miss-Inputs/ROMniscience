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

		private FileInfo archive;
		private IArchiveEntry entry;
		private WrappedInputStream archiveStream;

		public CompressedROMFile(IArchiveEntry entry, FileInfo path) {
			this.entry = entry;
			archive = path;
			archiveStream = new WrappedInputStream(entry.OpenEntryStream());
		}

		public override FileInfo path => archive;
		public override string name => entry.Key;
		public override long length => entry.Size;
		public override long compressedLength => entry.CompressedSize;
		public override bool compressed => true;

		public override InputStream stream => archiveStream;
		public override void Dispose() {
			((IDisposable)archiveStream).Dispose();
		}
	}
}
