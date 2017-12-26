using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class _32X: Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"32x", "Sega 32X ROM"},
			{"bin", "Sega 32X ROM"},
		};
		public override string name => "32X";

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			Megadrive.parseMegadriveROM(info, file.stream);
		}
	}
}
