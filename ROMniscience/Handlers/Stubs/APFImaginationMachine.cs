using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class APFImaginationMachine : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"bin", "APF Imagination Machine cartridge"},
			{"cpf", "APF Imagination Machine casette"},
			{"cas", "APF Imagination Machine casette"},
		};

		//.bin files would be read by APF.cs which isn't a stub, I don't have any that are specific to Imagination Machine though... or do I
		public override string name => "APF Imagination Machine";
	}
}
