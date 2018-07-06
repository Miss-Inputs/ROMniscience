using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class AppleIIGS : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>{
			{"2mg", "Apple IIgs disk image"},
		};

		public override string name => "Apple IIgs";
	}
}
