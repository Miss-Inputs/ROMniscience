using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class PSP : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"iso", "PlayStation Portable UMD disc"}, //This is basically just a DVD with an ISO9660 filesystem
			{"pbp", "PlayStation Portable PBP file"}, //http://www.psdevwiki.com/ps3/Eboot.PBP (I still have no idea what the acronym means)
		};

		public override string name => "Sony PlayStation Portable";
	}
}
