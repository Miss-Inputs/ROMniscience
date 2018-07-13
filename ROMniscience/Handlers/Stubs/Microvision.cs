using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class Microvision : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"bin", "Microvision ROM"},
		};

		public override string name => "Microvision";
	}
}
