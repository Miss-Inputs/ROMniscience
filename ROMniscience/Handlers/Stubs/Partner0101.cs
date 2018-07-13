using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class Partner0101 : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"rkp", "SAM SKB VM Partner-01.01 tape image"},
			{"dsk", "SAM SKB VM Partner-01.01 disk image"},
		};

		public override string name => "Partner-01.01";
	}
}
