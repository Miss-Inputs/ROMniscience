using ROMniscience.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class NormalROMFile: ROMFile {

		private WrappedInputStream fileStream;

		public NormalROMFile(FileInfo f) {
			path = f;
			fileStream = new WrappedInputStream(File.OpenRead(f.FullName));
		}

		public override FileInfo path {
			get;
		}

		public override string name => path.Name;
		public override bool compressed => false;
		public override long compressedLength => length;
		public override long length => stream.Length;

		public override InputStream stream => fileStream;

		public override void Dispose() {
			((IDisposable)fileStream).Dispose();
		}
	}
}
