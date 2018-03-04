using ROMniscience.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class GCZROMFile: ROMFile {

		FileInfo fi;
		GCZInputStream gcz;

		public GCZROMFile(FileInfo path) {
			fi = path;
			gcz = new GCZInputStream(File.OpenRead(path.FullName));
		}

		public override bool compressed => true;

		public override long compressedLength => (long)gcz.compressedSize;

		public override FileInfo path => fi;

		public override long length => (long)gcz.uncompressedSize;

		public override InputStream stream => gcz;

		//This kinda sucks but it's better than pretending the uncompressed file is called .gcz
		public override string name => fi.Name.Replace(".gcz", ".gcm");
	}
}
