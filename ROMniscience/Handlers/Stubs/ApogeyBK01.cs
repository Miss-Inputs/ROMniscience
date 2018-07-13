using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class ApogeyBK01 : StubHandler {
		//Can't seem to know for sure if it's spelled Apogee or Apogey. Maybe they're both Romanizations of Soviet Russian text and hence up to interpretation or something like that.
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"rka", "Zavod BRA Apogee BK-01 tape image"},
		};

		public override string name => "Apogey BK01";
	}
}
