using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class PocketStation : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"gme", "Sony PocketStation ROM"},
		};

		public override string name => "Sony PocketStation";
	}
}
