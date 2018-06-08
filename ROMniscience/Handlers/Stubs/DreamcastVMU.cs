using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class DreamcastVMU : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"vmu", "Dreamcast VMU ROM"},
			{"bin", "Dreamcast VMU ROM"},
		};

		public override string name => "Dreamcast Visual Memory Unit";
	}
}
