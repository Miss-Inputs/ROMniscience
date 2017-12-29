using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.IO {
	class WrappedInputStream: InputStream, IDisposable {
		private Stream innerStream;

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
	}
}
