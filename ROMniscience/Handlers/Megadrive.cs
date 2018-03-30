/*
 * The MIT License
 *
 * Copyright 2017 Megan Leet (Zowayix).
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ROMniscience.IO;
using System.IO;

namespace ROMniscience.Handlers {
	class Megadrive: Handler {
		//Some stuff adapted from https://www.zophar.net/fileuploads/2/10614uauyw/Genesis_ROM_Format.txt
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"gen", "Sega Genesis/Megadrive ROM"},
			{"bin", "Sega Genesis/Megadrive ROM"},
			{"sgd", "Sega Genesis/Megadrive ROM"},
			{"smd", "Sega Genesis/Megadrive interleaved ROM"},
			{"md", "Sega Genesis/Megadrive ROM"},
		};

		public override string name => "Megadrive/Genesis";

		public readonly static IDictionary<string, string> PRODUCT_TYPES = new Dictionary<string, string> {
			{"AI", "Education"}, //Wonder Library, Time Trax (although that's not educational), Miracle Piano Teaching System... maybe this is actually wrong, especially as no Pico games use it
			{"BR", "Boot ROM"}, //Mega CD BIOS, LaserActive BIOS etc
			{"GM", "Game"},
			{"OS", "Operating system"}, //Genesis OS ROM uses this
			{"PX", "Pictures"}, //Hentai Collection homebrew (lol) etc
			{"SF", "Super Fighter Team game"}, //Beggar Price, Legend of Wukong, Star Odyssey, etc
			{"83", "Samsung Pico"}, //Some Samsung Pico titles also use T-, but 83 is only used by Samsung Pico
			{"MP", "Brazilian Pico game"},
			
			//Also seen:
			//G-: Mega Anser BIOS
			//HP: Many Pico games
			//MK: Many Pico games but also some Megadrive betas, for what it's worth (Dynamite Heddy, Virtua Fighter 2, etc)
			//RO: The 32X SDK programs, but also FIFA Soccer 96 for 32X... did they leave some SDK stuff in there?
			//T-: Many Pico games, but also some Megadrive/32X betas like Wacky Races and Soulstar X

			//I have no idea what the zillions of different types for Pico games are all about, but maybe if I actually knew more about the Pico it would start making sense
			//TE (Angry Birds demo hack)
			//CE (Censor Movie Trailer Demo, probably just stands for "Censor")
			//X- (Magical Christmas Greetings demo)
			//DE (Overdrive 2 demo, possibly stands for "demo")
			//TH (Overdrive demo by Titan)
	
			//Pico doesn't seem to use AI, surprisingly. Instead it has:
			//HP: e.g. A Bug's Life, Cooking Pico
			//MK: e.g. A Year at Pooh Corner, Crayola Crayons: Create a World
			//MP: Cores Magicas, Ecco Jr. No fundo do mar!, Hello Kitty no Castelo (these are all Brazilian)
			//83: Dorehmipa Dongmooleumakhwoe, 
				//Ehko Junieoeui Shinbiroun Badayeohaeng (Ecco Jr. and the Great Ocean Treasure Hunt but Korean), both Korean
			//61: Drive Pico: Saa Shuppatsu Da! Ken-chan to Pepe no Wanpaku Drive
			//A lot of T- as above
			//I can only guess what correlation there even is, or what they mean
	
			//Misused completely in 16 Zhang Majiang, which is a bootleg
		};

		public readonly static IDictionary<char, string> IO_SUPPORT = new Dictionary<char, string> {
			{'0', "Master System joypad"},
			{'4', "Team Play"},
			{'6', "6-button joypad"},
			{'A', "Analog joystick"}, //After Burner (is this the XE-1 AP?)
			{'B', "Control ball"},
			{'C', "CD-ROM"},
			{'F', "Floppy drive"},
			{'G', "Menacer"},
			{'J', "Joypad"},
			{'K', "Keyboard"},
			{'L', "Activator"},
			{'M', "Mouse"},
			{'O', "J-Cart"},
			{'P', "Printer"},
			{'R', "Serial RS232C"},
			{'T', "Tablet"},
			{'V', "Paddle"},
			
			//I would think the Ten Key Pad would have its own entry here but who knows
			//Others I've seen but I don't know:
			//D (pretty much every homebrew, could it mean "demo" or "development"? SGDK inserts this with no explanation)
			//Roadwar 2000 seems to corrupt and misuse this field entirely
			//Outline 2017 demo seems to misuse both this field and the
			//product type field
			//MDEM says "Joypad only!" in ASCII in this field, as well as putting 
			//"The best" as the product type and code
		};

		public readonly static IDictionary<char, string> REGIONS = new Dictionary<char, string> {
			{'4', "Brazil / USA"},
			{'8', "Hong Kong"}, //Questionable... only seen in a few European Pico games
			{'A', "Asia"}, //Is this actually Europe, or is No-Intro wrong? A handful of European betas and 32X games use this
			{'B', "Brazil (B)"}, //Doesn't seem to be used, all the Brazilian stuff uses 4 (which is also used by some USA games)
			{'C', "USA + Europe"}, //Usually not used in favour of just using U and E together, but Garfield: Caught in the Act uses it
			{'E', "Europe"}, //Some Sega Pico games have this twice for some reason, or this plus another more specific European country
			{'F', "France"}, //But then I've heard this can also be used for region-free
			{'G', "Germany"},

			{'I', "Italy"},
			{'J', "Japan"},
			{'S', "Spain"},
			{'U', "USA"},
			{'e', "Europe (e)"},
	
			{'1', "Japan"},
			{'5', "Japan + USA"}, //This one's not really seen that often because most games would just use Japan and USA together, but the odd 32X game uses this combination... but then just to be confusing, this is also used in Samsung Pico games and some Taiwanese game, and Magic School Bus which wasn't even released in Japan
	
			//There's a 2 in the Multi-Mega BIOS, not sure what it means, as far as I can tell
			//that BIOS is just for Europe which it also has as a country code
			//Puggsy protoype has "NOV" in this field which seems to be misused
		};

		public readonly static IDictionary<string, string> MANUFACTURERS = new Dictionary<string, string> {
			{"SEGA", "Sega"}, //Interestingly enough
			{"ACLD", "Ballistic"},
			{"ASCI", "Asciiware"},
			{"RSI", "Razorsoft"},
			{"TREC", "Treco"},
			{"VRGN", "Virgin"},
			{"WSTN", "Westone"},
			//https://segaretro.org/Third-party_T-series_codes
			{"T-10", "Takara"},
			{"T-11", "Taito"},
			{"T-12", "Capcom"},
			{"T-13", "Data East"},
			{"T-14", "Namco"},
			{"T-15", "Sunsoft"},
			{"T-16", "Ma-Ba (Mattel + Bandai)"},
			{"T-17", "Dempa"},
			{"T-18", "Technosoft"},
			{"T-19", "Technosoft (19)"},
			{"T-20", "Asmik"},
			{"T-21", "ASCII"},
			{"T-22", "Micronet"},
			{"T-23", "VIC Tokai"},
			{"T-24", "Treco or Sammy"},
			{"T-25", "Nippon Computer Systems (Masaya)"},
			{"T-26", "Sigma"},
			{"T-27", "Toho"},
			{"T-28", "Hot-B"},
			{"T-29", "Kyugo"},
			{"T-30", "Video System"},
			{"T-31", "SNK"},
			{"T-32", "Wolf Team"},
			{"T-33", "Kaneko"},
			{"T-34", "Dreamworks"},
			{"T-35", "Seismic Software/Compile"},
			{"T-36", "Tecmo"},
			{"T-40", "Toaplan"},
			{"T-41", "UNIPACC"},
			{"T-42", "UFL"},
			{"T-43", "Human"},
			{"T-44", "Sanritsu"},
			{"T-45", "Game Arts"},
			{"T-46", "Kodansha"},
			{"T-47", "Sage's Creation"},
			{"T-48", "Tengen"},
			{"T-49", "Telenet"},
			{"T-50", "Electronic Arts"},
			{"T-51", "Microcabin"},
			{"T-52", "Systemsoft"},
			{"T-53", "Riverhillsoft"},
			{"T-54", "Face"},
			{"T-55", "Nuvision Entertainment"},
			{"T-56", "Razorsoft"},
			{"T-57", "Jaleco"},
			{"T-58", "Visco"},
			{"T-60", "Victor"},
			{"T-61", "Wonder Amusement Studio"},
			{"T-62", "Sony Imagesoft"},
			{"T-63", "Toshiba EMI"},
			{"T-64", "Information Global Service"},
			{"T-65", "Tsukuda Ideal"},
			{"T-66", "Compile"},
			{"T-67", "Home Data/Magical"},
			{"T-68", "CSK Research Institute"},
			{"T-69", "Arena"}, //nice
			{"T-70", "Virgin"},
			{"T-71", "Nichibutsu"},
			{"T-72", "Varie"},
			{"T-73", "Coconuts Japan or Soft Vision"},
			{"T-74", "Palsoft"},
			{"T-75", "Pony Canyon"},
			{"T-76", "Koei"},
			{"T-77", "Takeru/Sur De Wave"},
			{"T-79", "U.S. Gold"},
			{"T-81", "Acclaim"},
			{"T-83", "Gametek"},
			{"T-84", "Datawest"},
			{"T-85", "PCM Complete"},
			{"T-86", "Absolute"},
			{"T-87", "Mindscape"},
			{"T-88", "Domark"},
			{"T-89", "Parker Bros"},
			{"T-91", "Pack-in-Soft"},
			{"T-92", "Polydor"},
			{"T-93", "Sony"},
			{"T-95", "Konami"},
			{"T-97", "Tradewest/Williams/Midway"},
			{"T-99", "Success"},
			{"T-100", "THQ"},
			{"T-101", "Tecmagik"},
			{"T-102", "Samsung"}, //Used for Pico titles
			{"T-103", "Takara"},
			{"T-105", "Shogakukan"},
			{"T-106", "Electronic Arts Victor"},
			{"T-107", "Electro Brain"},
			{"T-109", "Saddleback Graphics"},
			{"T-110", "Dynamix"},
			{"T-111", "American Laser Games"},
			{"T-112", "Hi-Tech Expressions"},
			{"T-113", "Psygnosis"},
			{"T-114", "T&E Soft"},
			{"T-115", "Core Design"},
			{"T-118", "The Learning Company"},
			{"T-119", "Accolade"},
			{"T-120", "Codemasters"},
			{"T-121", "ReadySoft"},
			{"T-123", "Gremlin"},
			{"T-124", "Spectrum Holobyte"},
			{"T-125", "Interplay"},
			{"T-126", "Maxis"},
			{"T-127", "Working Designs"},
			{"T-130", "Activision"},
			{"T-132", "Playmates"},
			{"T-133", "Bandai"},
			{"T-135", "CapDisc"},
			{"T-137", "ASC Games"},
			{"T-139", "Viacom"},
			{"T-141", "Toei"},
			{"T-143", "Hudson"},
			{"T-144", "Atlus"},
			{"T-145", "Sony"},
			{"T-146", "Takara"},
			{"T-147", "Sansan"},
			{"T-149", "Nisshouiwai Infocom"},
			{"T-150", "Imagineer"},
			{"T-151", "Infogrames"},
			{"T-152", "Davidson & Associates"},
			{"T-153", "Rocket Science Games"},
			{"T-154", "Technos Japan"},
			{"T-157", "Angel"},
			{"T-158", "Mindscape"},
			{"T-159", "Crystal Dynamics"},
			{"T-160", "Sales Curve"},
			{"T-161", "Fox"},
			{"T-162", "Digital Pictures"},
			{"T-164", "Ocean Software"},
			{"T-165", "Seta"},
			{"T-166", "Altron"},
			{"T-167", "ASK Kodansha"},
			{"T-168", "Athena"},
			{"T-169", "Gakken"},
			{"T-170", "General Entertainment"},
			{"T-174", "Glams"},
			{"T-176", "ASCII Something Good"},
			{"T-177", "Ubisoft"},
			{"T-178", "Hitachi"},
			{"T-180", "BMG"},
			{"T-181", "Obunsha"},
			{"T-182", "Thinking Cap"},
			{"T-185", "Gaga Communications"},
			{"T-186", "SoftBank"},
			{"T-187", "Naxat Soft"},
			{"T-188", "Mizuki"},
			{"T-189", "KAZe"},
			{"T-193", "Sega Yonezawa"},
			{"T-194", "We Net"},
			{"T-195", "Datam Polystar"},
			{"T-197", "KID"},
			{"T-198", "Epoch"},
			{"T-199", "Ving"},
			{"T-200", "Yoshimoto Kogyo"},
			{"T-201", "NEC Interchannel"},
			{"T-202", "Sonnet Computer Entertainment"},
			{"T-203", "Game Studio"},
			{"T-204", "Psikyo"},
			{"T-205", "Media Entertainment"},
			{"T-206", "Banpresto"},
			{"T-207", "Ecseco Development"},
			{"T-208", "Bullet-Proof Software"},
			{"T-209", "Sieg"},
			{"T-210", "Yanoman"},
			{"T-212", "Oz Club"},
			{"T-213", "Nihon Create"},
			{"T-214", "Media Rings Corporation"},
			{"T-215", "Shoeisha"},
			{"T-216", "OPeNBooK"},
			{"T-217", "Hakuhodo"},
			{"T-218", "Aroma"},
			{"T-219", "Societa Daikanyama"},
			{"T-220", "Arc System Works"},
			{"T-221", "Climax Entertainment"},
			{"T-222", "Pioneer LDC"},
			{"T-223", "Tokuma Shoten"},
			{"T-224", "I'MAX"},
			{"T-226", "Shogakukan"},
			{"T-227", "Vantan International"},
			{"T-229", "Titus"},
			{"T-230", "LucasArts"},
			{"T-231", "Pai"},
			{"T-232", "Ecole"},
			{"T-233", "Nayuta"},
			{"T-234", "Bandai Visual"},
			{"T-235", "Quintet"},
			{"T-239", "Disney"},
			{"T-240", "OpenBook9003"},
			{"T-241", "Multisoft"},
			{"T-242", "Sky Think System"},
			{"T-243", "OCC"},
			{"T-246", "Increment P"},
			{"T-249", "King Records"},
			{"T-250", "Fun House"},
			{"T-251", "Patra"},
			{"T-252", "Inner Brain"},
			{"T-253", "Make Software"},
			{"T-254", "GT Interactive"},
			{"T-255", "Kodansha"},
			{"T-257", "Clef"},
			{"T-259", "C-Seven"},
			{"T-260", "Fujitsu Parex"},
			{"T-261", "Xing Entertainment"},
			{"T-264", "Media Quest"},
			{"T-268", "Wooyoung System"},
			{"T-270", "Nihon System"},
			{"T-271", "Scholar"},
			{"T-273", "Datt Japan"},
			{"T-278", "MediaWorks"},
			{"T-279", "Kadokawa Shoten"},
			{"T-280", "Elf"},
			{"T-282", "Tomy"},
			{"T-289", "KSS"},
			{"T-290", "Mainichi Communications"},
			{"T-291", "Warashi"},
			{"T-292", "Metro"},
			{"T-293", "Sai-Mate"},
			{"T-294", "Kokopeli Digital Studios"},
			{"T-296", "Planning Office Wada (POW)"},
			{"T-297", "Telstar"},
			{"T-300", "Warp or Kumon Publishing"},
			{"T-303", "Masudaya"},
			{"T-306", "Soft Office"},
			{"T-307", "Empire Interactive"},
			{"T-308", "Genki"},
			{"T-309", "Neverland"},
			{"T-310", "Shar Rock"},
			{"T-311", "Natsume"},
			{"T-312", "Nexus Interact"},
			{"T-313", "Aplix Corporation"},
			{"T-314", "Omiya Soft"},
			{"T-315", "JVC"},
			{"T-316", "Zoom"},
			{"T-321", "TEN Institute"},
			{"T-322", "Fujitsu"},
			{"T-325", "TGL"},
			{"T-326", "Red Entertainment"},
			{"T-328", "Waka Manufacturing"},
			{"T-329", "Treasure"},
			{"T-330", "Tokuma Shoten Intermedia"},
			{"T-331", "Camelot"},
			{"T-339", "Sting"},
			{"T-340", "Chunsoft"},
			{"T-341", "Aki"},
			{"T-342", "From Software"},
			{"T-346", "Daiki"},
			{"T-348", "Aspect"},
			{"T-350", "Micro Vision"},
			{"T-351", "Gainax"},
			{"T-354", "FortyFive (45XLV)"},
			{"T-355", "Enix"},
			{"T-356", "Ray Corporation"},
			{"T-357", "Tonkin House"},
			{"T-360", "Outrigger"},
			{"T-361", "B-Factory"},
			{"T-362", "LayUp"},
			{"T-363", "Axela"},
			{"T-364", "WorkJam"},
			{"T-365", "Nihon Syscom"},
			{"T-367", "Full On Games"},
			{"T-368", "Eidos"},
			{"T-369", "UEP Systems"},
			{"T-370", "Shouei System"},
			{"T-371", "GMF"},
			{"T-373", "ADK"},
			{"T-374", "Softstar Entertainment"},
			{"T-375", "Nexton"},
			{"T-376", "Denshi Media Services"},
			{"T-379", "Takuyo"},
			{"T-380", "Starlight Marry"},
			{"T-381", "Crystal Vision"},
			{"T-382", "Kamata and Partners"},
			{"T-383", "AquaPlus"},
			{"T-384", "Media Gallop"},
			{"T-385", "Culture Brain"},
			{"T-386", "Locus"},
			{"T-387", "Entertainment Software Publishing"},
			{"T-388", "NEC"},
			{"T-390", "Pulse Interactive"},
			{"T-391", "Random House"},
			{"T-394", "Vivarium"},
			{"T-395", "Mebius"},
			{"T-396", "Panther Software"},
			{"T-397", "TBS"},
			{"T-398", "NetVillage"},
			{"T-400", "Vision"},
			{"T-401", "Shangri-La"},
			{"T-402", "Crave Entertainment"},
			{"T-403", "Metro3D"},
			{"T-404", "Majesco"},
			{"T-405", "Take-Two Interactive"},
			{"T-406", "Hasbro"},
			{"T-407", "Rage Games"},
			{"T-408", "Marvelous"},
			{"T-409", "Bottom Up"},
			{"T-410", "Daikoku Denki"},
			{"T-411", "Sunrise Interactive"},
			{"T-412", "Bimboosoft"},
			{"T-413", "UFO"},
			{"T-414", "Mattel"},
			{"T-415", "CaramelPot"},
			{"T-416", "Vatical Entertainment"},
			{"T-417", "Ripcord Games"},
			{"T-418", "Sega Toys"},
			{"T-419", "Gathering of Developers"},
			{"T-421", "Rockstar"},
			{"T-422", "Winkysoft"},
			{"T-423", "Cyberfront"},
			{"T-424", "DaZZ"},
			{"T-428", "Kobi"},
			{"T-429", "Fujicom"},
			{"T-433", "Real Vision"},
			{"T-434", "Visit"},
			{"T-435", "Global A Entertainment"},
			{"T-438", "Studio Wonder Effect"},
			{"T-439", "Media Factory"},
			{"T-441", "Red?"},
			{"T-443", "Agetec"},
			{"T-444", "Abel"},
			{"T-445", "Softmax"},
			{"T-446", "Isao"},
			{"T-447", "Kool Kizz"},
			{"T-448", "GeneX"},
			{"T-449", "Xicat Interactive"},
			{"T-450", "Swing! Entertainment"},
			{"T-451", "Yuke's"},
			{"T-454", "AAA Game"},
			{"T-455", "TV Asahi"},
			{"T-456", "Crazy Games"},
			{"T-457", "Atmark"},
			{"T-458", "Hackberry"},
			{"T-460", "AIA"},
			{"T-461", "Starfish-SD"},
			{"T-462", "Idea Factory"},
			{"T-463", "Broccoli"},
			{"T-465", "Oaks (Princess Soft)"},
			{"T-466", "Bigben Interactive"},
			{"T-467", "G.rev"},
			{"T-469", "Symbio Planning"},
			{"T-471", "Alchemist"},
			{"T-473", "SNK Playmore"},
			{"T-474", "D3Publisher"},
			{"T-475", "Rain Software"},
			{"T-476", "Good Navigate"},
			{"T-477", "Alfa System"},
			{"T-478", "Milestone"},
			{"T-479", "Triangle Service"},
		};

		public readonly static IDictionary<string, int> MONTH_ABBREVIATIONS = new Dictionary<string, int> {
			{"JAN", 1},
			{"FEB", 2},
			{"MAR", 3},
			{"APR", 4},
			{"APL", 4},
			{"MAY", 5},
			{"JUN", 6},
			{"JUL", 7},
			{"AUG", 8},
			{"08", 8},
			{"SEP", 9},
			{"SEPT", 9},
			{"OCT", 10},
			{"NOV", 11},
			{"DEC", 12},
		};

		public static WrappedInputStream decodeSMD(WrappedInputStream s) {
			s.Seek(512, SeekOrigin.Current);
			//Should only need this much to read the header. If I was actually converting
			//the ROM I'd need to use the SMD header to know how many blocks there are
			//and read multiple blocks and whatnot
			byte[] block = s.read(16384);

			byte[] buf = new byte[16386];
			//Yes, the below starting points are correct. It makes no goddamned
			//sense but if I use even = 0 and odd = 1 as the starting points
			//it goes off by one so I don't know anymore
			int buf_even = 1;
			int buf_odd = 2;

			int midpoint = 8192;
			for(int i = 0; i < block.Length; ++i) {
				if(i <= midpoint) {
					buf[buf_even] = block[i];
					buf_even += 2;
				} else {
					buf[buf_odd] = block[i];
					buf_odd += 2;
				}
			}

			byte[] buf2 = new byte[buf_odd];
			Array.Copy(buf, buf2, buf_odd);
			return new WrappedInputStream(new MemoryStream(buf2));
		}

		public static bool isSMD(WrappedInputStream s) {
			long origPos = s.Position;
			try {
				s.Position = 8;
				int b8 = s.read();
				int b9 = s.read();

				s.Position = 0x280;
				string str = s.read(4, Encoding.ASCII);

				return b8 == 0xaa && b9 == 0xbb && (String.Equals(str, "EAMG") || String.Equals(str, "EAGN"));
			} finally {
				s.Position = origPos;
			}
		}

		public static int calcChecksum(WrappedInputStream s) {
			long pos = s.Position;
			long len = s.Length;
			try {
				s.Position = 0x200;
				int checksum = 0;
				while (s.Position < len) {
					checksum = (checksum + s.readShortBE()) & 0xffff;
				}
				return checksum;
			} finally {
				s.Position = pos;
			}
		}

		private static readonly Regex copyrightRegex = new Regex(@"\(C\)(\S{4}.)(\d{4})\.(.{3})");
		public static void parseMegadriveROM(ROMInfo info, WrappedInputStream s) {
			s.Position = 0x100;

			string consoleName = s.read(16, Encoding.ASCII).TrimEnd('\0', ' ');
			info.addInfo("Console name", consoleName);
			if(consoleName.StartsWith("SEGA 32X")) {
				// There are a few homebrew apps (32xfire, Shymmer) and also Doom
				// that misuse this field and say something else, so I've used
				// startswith instead, which should be safe, and picks up those three
				// except for 32xfire which claims to be a Megadrive game (it has
				// "32X GAME" as the domestic and overseas name)
				// Some cheeky buggers just use SEGA MEGADRIVE or SEGA GENESIS anyway even when they're 32X
				// Note that whatever the case may be, a Genesis/Megadrive game
				// better have "SEGA" at the start of the header or a real
				// console won't boot it, which means there is inevitably a
				// bootleg game that doesn't have it there
				info.addInfo("Platform", "Sega 32X");
			} else {
				info.addInfo("Platform", "Sega Megadrive/Genesis");
			}

			string copyright = s.read(16, Encoding.ASCII).TrimEnd('\0', ' ');
			info.addInfo("Copyright", copyright);
			var matches = copyrightRegex.Match(copyright);
			if(matches.Success) {
				//TODO Sometimes you have stuff like T-075 instead of T-75 or T112 instead of T-112 (but is that just the game's fault for being weird?)
				info.addInfo("Manufacturer", matches.Groups[1].Value?.Trim().TrimEnd(','), MANUFACTURERS);
				info.addInfo("Year", matches.Groups[2].Value);
				if(MONTH_ABBREVIATIONS.TryGetValue(matches.Groups[3].Value, out int month)) {
					info.addInfo("Month", System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(month));
				} else {
					info.addInfo("Month", String.Format("Unknown ({0})", matches.Groups[3].Value));
				}
			}

			string domesticName = s.read(48, MainProgram.shiftJIS).TrimEnd('\0', ' ');
			info.addInfo("Internal name", domesticName);
			string overseasName = s.read(48, MainProgram.shiftJIS).TrimEnd('\0', ' ');
			info.addInfo("Overseas name", overseasName);
			string productType = s.read(2, Encoding.ASCII);
			info.addInfo("Type", productType, PRODUCT_TYPES);

			s.read(); //Space for padding
			string serialNumber = s.read(8, Encoding.ASCII).TrimEnd('\0', ' ');
			info.addInfo("Product code", serialNumber);
			s.read(); //- for padding
			string version = s.read(2, Encoding.ASCII);
			info.addInfo("Version", version);

			ushort checksum = (ushort)s.readShortBE();
			info.addInfo("Checksum", checksum, ROMInfo.FormatMode.HEX, true);
			int calculatedChecksum = calcChecksum(s);
			info.addInfo("Calculated checksum", calculatedChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksum valid?", checksum == calculatedChecksum);

			char[] ioSupportList = s.read(16, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("IO support", ioSupportList, IO_SUPPORT);

			int romStart = s.readIntBE();
			info.addInfo("ROM start", romStart, ROMInfo.FormatMode.HEX, true);
			int romEnd = s.readIntBE();
			info.addInfo("ROM end", romEnd, ROMInfo.FormatMode.HEX, true);
			info.addInfo("ROM size", romEnd - romStart, ROMInfo.FormatMode.SIZE);
			int ramStart = s.readIntBE();
			info.addInfo("RAM start", ramStart, ROMInfo.FormatMode.HEX, true);
			int ramEnd = s.readIntBE();
			info.addInfo("RAM end", ramEnd, ROMInfo.FormatMode.HEX, true);
			info.addInfo("RAM size", ramEnd - ramStart, ROMInfo.FormatMode.SIZE);
			byte[] backupRamID = s.read(4);
			info.addInfo("Backup RAM ID", backupRamID);
			int backupRamStart = s.readIntBE();
			info.addInfo("Backup RAM start", backupRamStart, ROMInfo.FormatMode.HEX, true);
			int backupRamEnd = s.readIntBE();
			info.addInfo("Backup RAM end", backupRamEnd, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Save size", backupRamEnd - backupRamStart, ROMInfo.FormatMode.SIZE);

			byte[] modemData = s.read(12);
			info.addInfo("Modem data", modemData);
			//Technically this should be an ASCII string in the format MO<company><modem no#>.<version> or spaces if modem not supported but it isn't

			string memo = s.read(40, Encoding.ASCII).TrimEnd('\0', ' ');
			info.addInfo("Memo", memo);
			char[] regions = s.read(3, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("Region", regions, REGIONS);
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if(isSMD(file.stream)) {
				info.addInfo("Detected format", "Super Magic Drive interleaved");
				parseMegadriveROM(info, decodeSMD(file.stream));
			} else {
				info.addInfo("Detected format", "Plain");
				parseMegadriveROM(info, file.stream);
			}
		}
	}
}
