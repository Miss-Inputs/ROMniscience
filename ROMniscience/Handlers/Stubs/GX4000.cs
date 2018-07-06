using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class GX4000 : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"bin", "Amstrad GX4000 ROM"},
			{"cpr", "Amstrad GX4000 cartridge RIFF file"}, //www.cpcwiki.eu/index.php/Format:CPR_CPC_Plus_cartridge_file_format
			//tl;dr there's not really much to this format, especially as it seems rather useless when straight .bin dumps just work. I guess you could do some autodetection thing with the "Ams!" thing, if I ever decide to do autodetection
		};

		public override string name => "Amstrad GX4000";
	}
}
