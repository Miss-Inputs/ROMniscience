/*
 * The MIT License
 *
 * Copyright 2018 Megan Leet (Zowayix).
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	class PokemonMini: Handler {
		//Adapted mostly from http://www.pokemon-mini.net/documentation/cartridge/

		public static readonly IDictionary<char, string> GAME_TYPES = new Dictionary<char, string> {
			{'M', "Game cart"}, //That's all that's used for the few official games released
			{'K', "Prototype"}, //There's a prototype that's been seen that includes a cart that hasn't been dumped, but it has MIN-KCFO-01 as the serial
		};

		public static readonly IDictionary<char, string> REGIONS = new Dictionary<char, string> {
			//I know J, P, and E are used, I just took the GBA region list because I'm pretty sure there are European language ROMs and whatnot
			{'D', "Germany"},
			{'E', "USA"},
			{'F', "France"},
			{'I', "Italy"},
			{'J', "Japan"},
			{'O', "International"}
			{'P', "Europe"},
			{'S', "Spain"},
			{'X', "Europe (X)"},
		};

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"min", "Pokémon Mini ROM"}
		};

		public override string name => "Pokemon Mini";

		private static Encoding getTitleEncoding() {
			try {
				//All the Japanese exclusive games use some kind of JIS (maybe the Japanese versions of worldwide games do too)
				return Encoding.GetEncoding("shift_jis");
			} catch(ArgumentException ae) {
				//Bugger
				System.Diagnostics.Trace.TraceWarning(ae.Message);
				return Encoding.ASCII;
			}
		}
		private static readonly Encoding titleEncoding = getTitleEncoding();

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Pokemon Mini");

			InputStream s = file.stream;
			s.Seek(0x2100, SeekOrigin.Begin);

			string marker = s.read(2, Encoding.ASCII);
			info.addExtraInfo("Marker", marker);

			info.addExtraInfo("Entry point", s.read(6));
			//What the heck is all this
			info.addExtraInfo("PRC frame copy IRQ", s.read(6));
			info.addExtraInfo("PRC render IRQ", s.read(6));
			info.addExtraInfo("Timer 2 underflow upper IRQ", s.read(6));
			info.addExtraInfo("Timer 2 underflow lower IRQ", s.read(6));
			info.addExtraInfo("Timer 1 underflow upper IRQ", s.read(6));
			info.addExtraInfo("Timer 1 underflow lower IRQ", s.read(6));
			info.addExtraInfo("Timer 3 underflow upper IRQ", s.read(6));
			info.addExtraInfo("Timer 3 comparator IRQ", s.read(6));
			info.addExtraInfo("32Hz timer IRQ", s.read(6));
			info.addExtraInfo("8Hz timer IRQ", s.read(6));
			info.addExtraInfo("2Hz timer IRQ", s.read(6));
			info.addExtraInfo("1Hz timer IRQ", s.read(6));
			info.addExtraInfo("IR receiver IRQ", s.read(6));
			info.addExtraInfo("Shake sensor IRQ", s.read(6));
			info.addExtraInfo("Power key IRQ", s.read(6));
			info.addExtraInfo("Right key IRQ", s.read(6));
			info.addExtraInfo("Left key IRQ", s.read(6));
			info.addExtraInfo("Down key IRQ", s.read(6));
			info.addExtraInfo("Up key IRQ", s.read(6));
			info.addExtraInfo("C key IRQ", s.read(6));
			info.addExtraInfo("B key IRQ", s.read(6));
			info.addExtraInfo("A key IRQ", s.read(6));
			info.addExtraInfo("Unknown IRQ 1", s.read(6));
			info.addExtraInfo("Unknown IRQ 2", s.read(6));
			info.addExtraInfo("Unknown IRQ 3", s.read(6));
			info.addExtraInfo("Cartridge IRQ", s.read(6));

			string headerMagic = s.read("NINTENDO".Length, Encoding.ASCII);
			info.addExtraInfo("Header magic", headerMagic); //Should be "NINTENDO"

			string productCode = s.read(4, Encoding.ASCII);
			info.addInfo("Product code", productCode);

			char gameType = productCode[0];
			info.addInfo("Type", gameType, GAME_TYPES);
			string shortTitle = productCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char region = productCode[3];
			info.addInfo("Region", region, REGIONS);

			string title = s.read(12, titleEncoding).TrimEnd('\0').TrimEnd();
			info.addInfo("Internal name", title);

			string manufacturer = s.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", manufacturer, NintendoHandheldCommon.LICENSEE_CODES);

			byte[] reserved = s.read(18);
			info.addExtraInfo("Reserved", reserved); //Should be 0 filled
		}
	}
}
