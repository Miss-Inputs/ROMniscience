using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class AppleLisa : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"dc42", "Apple Lisa Disk Copy 4.2 floppy image"},
			{"obj", "Apple Lisa executable"}, //Presumably? Can't find any info on it, but the two demoscene things for Lisa use it 
		};

		public override string name => "Apple Lisa";
	}
}
