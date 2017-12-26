using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class Pico: Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			//It seems odd to me that Pico uses the .md file extension when that clearly
			//stands for Megadrive, but eh, I don't make the rules
			//Sure it's the same ROM format, but so is 32X and that gets its own extension
			{"md", "Sega Pico ROM"}
		};
		public override string name => "Sega Pico";

		public override void addROMInfo(ROMInfo info, string extension, ROMFile file) {
			Megadrive.parseMegadriveROM(info, file.stream);
		}
	}
}
