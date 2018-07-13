using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class Mikrosha : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"rkm", "Mikrosha tape image"},
			{"bin", "Mikrosha cartridge"},
			{"rom", "Mikrosha cartridge"},
		};

		public override string name => "Mikrosha";
	}
}
