using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers.Stubs {
	class IBMPCJr : StubHandler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"bin", "IBM PC Jr. ROM"},
			{"jrc", "IBM PC Jr. ROM"}, //There might be a difference here, actually... TOSEC just uses .jrc universally, and nobody else has bothered to make any kind of "authorative" datfile of PCjr carts
			//TODO Floppies
		};

		public override string name => "IBM PC Jr.";
	}
}
