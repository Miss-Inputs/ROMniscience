using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class CommodorePET : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"tap", "Commodore PET tape"},
			{"prg", "Commodore PET program"},
			//TODO: Floppy formats (can't be stuffed right now)
		};

		public override string name => "Commodore PET / CBM";
	}
}
