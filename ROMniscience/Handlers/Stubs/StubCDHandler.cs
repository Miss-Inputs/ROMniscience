using ROMniscience.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	abstract class StubCDHandler: CDBasedSystem {

		public override void addROMInfo(ROMInfo info, ROMFile file, WrappedInputStream stream) {
			info.addInfo("Platform", name);
		}

		public override bool shouldSeeInChooseView() {
			return false;
		}
	}
}
