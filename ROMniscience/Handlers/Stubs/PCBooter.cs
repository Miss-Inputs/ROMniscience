using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class PCBooter : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"img", "IBM PC boot disk image"},
		};

		public override string name => "IBM PC Booter";
	}
}
