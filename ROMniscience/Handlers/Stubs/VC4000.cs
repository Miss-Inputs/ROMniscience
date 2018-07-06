using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class VC4000 : StubHandler {
		//There's like 9999 clones for this system, 1292 AVPS being one of them which maybe came first and maybe should be considered the "main" thing, or maybe it should be a separate thing, or... I don't know. It's really confusing. I'm not feeling that mentally sharp at the moment, if you are an Interton VC 4000 fanatic, go ahead and tell me which way around things should go. I'm not being sarcastic, I just don't know how to make things make sense for whatever end user would be involved here
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"bin", "Interton VC 4000 ROM"},
			{"rom", "Interton VC 4000 ROM"},
			//There's also .pgm and .tvc, but should they specifically be for that Elektron thingy?
		};

		public override string name => "Interton VC 4000";
	}
}
