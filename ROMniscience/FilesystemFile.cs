using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class FilesystemFile: FilesystemNode {
		public long offset {
			get;
			set;
		}

		public long size {
			get;
			set;
		}
	}
}
