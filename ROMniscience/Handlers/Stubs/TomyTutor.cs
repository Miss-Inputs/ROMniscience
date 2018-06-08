using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class TomyTutor : StubHandler {

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>{
			{"bin", "Tomy Tutor cartridge"},
			//It has casettes but they're only in .wav format? and like... nah, doesn't seem to have expansion devices either
		};

		public override string name => "Tomy Tutor / Pyuuta";
	}
}
