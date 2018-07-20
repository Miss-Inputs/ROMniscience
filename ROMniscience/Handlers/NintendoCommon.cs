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
 * 
 * TODO: Does that restrict anyone from using the various dictionaries as documentation (I'd rather it not), and not just the actual code? I hate that I'm not a lawyer
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class NintendoCommon {

		public static readonly IDictionary<char, string> COUNTRIES = new Dictionary<char, string> {
			//Used by GBC, GBA, Gamecube/Wii, Pokemon Mini, SNES (with product codes, not the other one in the ROM header), Virtual Boy, and WiiWare
			//N64 and DS use the same XYYZ product code format where Z is region, but they seem to have their own set of country codes or do they?
			{'A', "Worldwide"}, //Or perhaps this is just Japan + USA (found in Wario Land 3 GBC)
			{'B', "Brazil"},
			{'C', "China"},
			{'D', "Germany"},
			{'E', "USA"},
			{'F', "France"},
			{'H', "Netherlands"},
			{'I', "Italy"},
			{'J', "Japan"},
			{'K', "Korea"},
			{'N', "Canada"}, //Or is it?
			{'P', "Europe"},
			{'Q', "Denmark"},
			{'R', "Russia"}, //Maybe?
			{'S', "Spain"},
			{'T', "Taiwan (T)"}, //Questionable, only shows up in a bunch of Gamecube multiboot ROMs where the product code is TEST; on DS this is USA + Australia
			{'U', "Australia"}, //Usually P or X is used to mean Europe + Australia, but there are a few exclusives
			{'W', "Taiwan"},
			{'X', "Europe (X)"},
			{'Y', "Europe (Y)"}, //Not seen very often...
		};

		public static IDictionary<char, string> DISC_TYPES => new Dictionary<char, string> {
			//https://wiki.dolphin-emu.org/index.php?title=GameIDs
			{'G', "GameCube game"},
			{'C', "Commodore 64 Virtual Console"},
			{'D', "GameCube demo disc"}, //Also OoT: Master Quest
			{'E', "Arcade/Neo Geo Virtual Console"},
			{'F', "NES Virtual Console"},
			{'H', "Wii channel"},
			{'J', "SNES Virtual Console"},
			{'L', "Sega Master System Virtual Console"},
			{'M', "Megadrive Virtual Console"},
			{'N', "N64 Virtual Console"},
			{'P', "GameCube promo or PC Engine Virtual Console"},
			{'Q', "TurboGrafx-CD Virtual Console"},
			{'R', "Wii game"},
			{'S', "Wii game (newer)"},
			{'U', "GameCube utility"}, //Used for GBA Player startup disc
			{'W', "WiiWare game"},
			{'X', "WiiWare demo or MSX Virtual Console"},
		};

		public static readonly IDictionary<int, string> REGIONS = new Dictionary<int, string>() {
			{0, "NTSC-J"},
			{1, "NTSC"},
			{2, "PAL"},
			{3, "Region free"},
			{4, "NTSC-K"},
		};

		public static readonly IDictionary<String, String> LICENSEE_CODES = new Dictionary<String, String>() {
			//Big list! Used in every Nintendo system except: NES (but FDS does use it), N64 (but 64DD does use it), e-Reader, and depending on who you ask Pokemon Mini either has 2P for every game (i.e. if you ask me) or it doesn't use this. Well, I presume Switch does in some way; 3DS seems to be a bit weird, and Wii U uses 4 characters so I guess they ran out of codes with all the indie publishers (most just use this list prepended with 00)
			//Preferred format that I probably don't always use consistently:
			//Company without Co, Inc, Ltd, etc
			//Company (Brand name used by company)
			//Company A / Company B which purchased A or A otherwise got absorbed into
			//Company A / New name of Company A
			//[Company B that merged with company A but kept A's licensee code] Company A e.g. [Bandai] Namco (Namco used the code AF before the merger)

			{"00", "Nobody"}, //Probably homebrew; although... some 3DS games use this? What the heck, I don't think Smash 4 was published by nobody
			{"  ", "Nobody"},
			{"\0\0", "Nobody"},
			{"##", "Nobody"},
			{"??", "Nobody"},

			{"01", "Nintendo"},
			{"08", "Capcom"},
			{"09", "Hot-B"},
			{"0A", "Jaleco"},
			{"0B", "Coconuts"},
			{"0C", "Elite Systems"},
			{"13", "EA Japan"}, //This only appears in Japanese games by EA, like Sommelier DS, or the Japanese version of Harry Potter and the Philosopher's Stone for GBC; every other EA game uses the sex number; anyway I don't know what EA actually calls their Japanese branch but that'll do
			{"18", "Hudson Soft"},
			{"19", "Bandai (B-AI)"},
			{"1A", "Yanoman"},
			{"1D", "Clary"},
			{"1M", "Micro Cabin"},
			{"1Q", "TDK (Japan)"},
			{"20", "Zoo"}, //Are they called Zoo Publishing? Zoo Games? Zoo Entertainment? Zoo Digital? I honestly have no fucking clue it's like all four of them at once
			{"24", "PCM Complete"},
			{"29", "Seta"},
			{"2L", "Tamsoft"},
			{"2N", "Smilesoft"},
			{"2P", "The Pokémon Company"},
			{"35", "Hector"},
			{"36", "Codemasters"},
			{"3E", "Gremlin"},
			{"41", "Ubisoft"},
			{"44", "Malibu"},
			{"47", "Bullet-Proof Software/Spectrum Holobyte"},
			{"49", "Irem"},
			{"4A", "Gakken"},
			{"4B", "Raya Systems"},
			{"4F", "Eidos"}, //The other documentation on the internets seems to think this is U.S. Gold, which I don't think is correct from ROMs I've seen use the code; the other thing is that this appears in the Gauntlet DS proto, but that wasn't going to be published by Eidos nor did they develop it
			{"4Q", "Disney"}, //What's maximum spooky is that this also shows up in the Spiderman: Friend or Foe trailer, but that was 2007 and Disney hadn't purchased Marvel yet
			{"4S", "Black Pearl Software"},
			{"4Y", "Rare"},
			{"4Z", "Crave Entertainment"}, //Resident Evil GBC prototype also has this, but it was developed by HotGen and would have been published by Capcom
			{"50", "Absolute"},
			{"51", "Acclaim"},
			{"52", "Activision"},
			{"53", "Sammy (America)"}, //Sammy's USA division, which may or may not be called American Sammy
			{"54", "Take-Two Interactive"}, //Oof this one's a really fun one. Some documentation says "Konami/GameTek" which is just garbage because Konami has nothing to do with this, but some older games using this code like Wheel of Fortune on SNES and the InfoGenius Productivity Pak on GB are indeed published under GameTek. Where it gets fun is that GameTek became Take-Two Interactive at some point, and Duke Nukem Advance uses this code too and was published under the Take-Two Interactive name.. but then Dora the Explorer: Dora's World Adventures also uses this code but was published under Global Star Software, which was a company that became 2K Play which is then a branch of 2K Games and if you've lost track of everything I don't blame you and I just spent several tens of minutes googling around for info on Dora the Explorer why do I do this to myself
			//Anyway this one just seems to cover everything that's owned by Take-Two Interactive
			{"55", "Hi-Tech Expressions"},
			{"56", "LJN"},
			{"57", "Matchbox"},
			{"58", "Mattel"},
			{"59", "Milton Bradley"},
			{"5A", "Mindscape"},
			{"5B", "Romstar"},
			{"5D", "Midway/Tradewest/Williams"}, //They're all the same thing really, everyone's buying out everyone
			{"5F", "American Softworks"},
			{"5G", "Majesco"},
			{"5H", "The 3DO Company"},
			{"5K", "Hasbro"},
			{"5L", "NewKidsCo"},
			{"5Q", "Lego"},
			{"5T", "Cyro Interactive"},
			{"5X", "Microids"},
			{"5Z", "Classified Games"}, //Also appears in the Card Shark (SNES) proto, which as far as I can tell was developed by someone named Bonsai and involved someone named Bicycle as well
			{"60", "Titus"},
			{"61", "Virgin"},
			{"64", "LucasArts"},
			{"67", "Ocean"},
			{"69", "Electronic Arts"}, //nice
			{"6B", "Beam Software/Melbourne House"},
			{"6F", "Electro Brain"},
			{"6H", "BBC Multimedia"},
			{"6J", "Software 2000"},
			{"6L", "BAM! Entertainment"},
			{"6S", "TDK"},
			{"6V", "JoWooD Entertainment"},
			{"70", "Infogrames/Atari, SA"}, //The modern Atari these days _is_ Infogrames basically and it's all very confusing, but anyway yeah there's DS games like Point Blank DS which use this and they're published under the Atari name, and then there's stuff like Legacy of Goku which is published under Infogrames but they're basically just the same company, and don't ask me about the original Atari because I'll die of confusion
			{"71", "Interplay"},
			{"72", "JVC"},
			{"78", "THQ"},
			{"79", "Accolade"},
			{"7A", "Triffix Entertainment"},
			{"7C", "Microprose"},
			{"7D", "Vivendi"},
			{"7F", "Kemco"},
			{"7G", "Rage Software"},
			{"7L", "Simon & Schuster"},
			{"80", "Misawa Entertainment"},
			{"82", "Namcot"}, //Some division of Namco that was apparently necessary to create as a separate thing.... I don't know
			{"83", "LOZC"},
			{"86", "Tokuma Shoten"},
			{"87", "Tsukuda Ori"},
			{"8B", "Bullet-Proof Software"},
			{"8C", "Vic Tokai"},
			{"8E", "Character Soft"},
			{"8F", "I'Max"},
			{"8N", "Success"},
			{"8P", "Sega"},
			{"91", "Chunsoft"},
			{"92", "Video System"},
			{"95", "Varie"},
			{"97", "Kaneko"},
			{"99", "Pack-in Video/Victor Interactive/Marvelous Interactive"}, //Merged with Victor in 1997 and then with Marvelous in 2003, so Victor and Marvelous probably used different codes before then
			{"9A", "Nihon Bussan (Nichibutsu)"},
			{"9B", "Tecmo"},
			{"9C", "Imagineer"},
			{"9F", "Nova"},
			{"9H", "Bottom Up"},
			{"9M", "Jaguar"}, //The sewing machine company
			{"9N", "Marvelous"}, //Before merger in 2003
			{"A0", "Telenet"}, //Sometimes known as Telenet Japan, or Nippon Telenet, but I think it's fair to just call it "Telenet"
			{"A1", "Hori Electric"},
			{"A2", "Scorpion Soft"},
			{"A4", "Konami"},
			{"A5", "K Amusement Leasing"},
			{"A6", "Kawada"},
			{"A7", "Takara"}, //Vast Fame also uses this for most of their GBC bootlegs, interestingly
			{"A8", "Royal Industries"}, //More damn sewing machine companies!
			{"A9", "Technos Japan"},
			{"AA", "Broderbund"},
			{"AC", "Toei Animation"}, //Used in some EXTREMELY obscure "Waiwai Check" games for Satellaview, which according to the title screen of one of them, are made by Hori Electric? Well I'm confused; there's an FDS game I already forgot the name of that's actually published by Toei though
			{"AD", "Toho"},
			{"AF", "[Bandai] Namco"}, //Namco games have always used this, but when they merged with Bandai, they kept this code (so newer games like that Code Geass DS one use this code as well), which is interesting because Square Enix doesn't reuse Squaresoft or Enix's licensee codes and gets a new one
			{"AH", "J-Wing"},
			{"AL", "Media Factory"},
			{"B1", "ASCII/Nexoft"},
			{"B2", "Bandai"},
			{"B3", "Soft Pro"},
			{"B4", "Enix"},
			{"B6", "HAL"},
			{"B7", "SNK"},
			{"B9", "Pony Canyon"},
			{"BA", "Culture Brain"},
			{"BB", "Sunsoft"},
			{"BC", "Toshiba EMI"},
			{"BD", "Sony Imagesoft"},
			{"BF", "Sammy"},
			{"BJ", "Compile"},
			{"BL", "MTO"},
			{"C0", "Taito"},
			{"C1", "Sunsoft (Chinou Game Series)"}, //I guess it's an educational branch of Sunsoft? Or it might all be published by Ask Odansha
			{"C3", "Squaresoft"},
			{"C5", "Data East"},
			{"C6", "Tokyo Shoseki (Tonkin House)"},
			{"C8", "Koei"},
			{"C9", "UFL"},
			{"CA", "Konami (Ultra Games)"},
			{"CB", "Vap"},
			{"CC", "Use"},
			{"CD", "Meldac"},
			{"CE", "Pony Canyon"}, //Might also be Fujisankei Communications International (which is owned by Fujisankei Communications Group which also owns Pony Canyon) as seen in SimEarth (SNES)
			{"CP", "Enterbrain"},
			{"CF", "Angel"},
			{"D1", "SOFEL"},
			{"D2", "Bothtec / Quest"},
			{"D3", "Sigma Enterprises"},
			{"D4", "Ask Kodansha"},
			{"D6", "Naxat Soft"},
			{"D7", "Copya Systems"},
			{"D9", "Banpresto"},
			{"DA", "Tomy"},
			{"DB", "Hiro"},
			{"DD", "Masaka"}, //Brand that Nippon Computer Systems uses to distribute games, but the rest of the company has no involvement so I might as well just put Masaka here and call it a day
			{"DE", "Human"},
			{"DF", "Altron"},
			{"E1", "Towachiki"},
			{"E2", "Yuutaka"},
			{"E4", "T&E Soft"},
			{"E5", "Epoch"},
			{"E7", "Athena"},
			{"E8", "Asmik"},
			{"E9", "Natsume"},
			{"EA", "King Records"},
			{"EB", "Atlus"},
			{"EC", "Epic/Sony Records"},
			{"EE", "Information Global Services"},
			{"F0", "A Wave"},
			{"F3", "Extreme Entertainment"},
			{"FJ", "Virtual Toys"},
			{"FQ", "iQue"}, //Sort of. Only WarioWare: Touched, Polarium, and Yoshi: Touch & Go use this, the other three iQue games: New Super Mario Bros, Super Mario 64, and Nintendogs (kiosk demo) use Nintendo instead. Those were all 2007 or later, the latter being on the iQue DSi, so it may be something corporate related going on there
			{"FR", "Digital Tainment Pool"},
			{"FT", "Daiwon C&A Holdings"},
			{"GD", "Square Enix"},
			{"GL", "Gameloft"},
			{"HF", "Level5"},
			{"JS", "Digital Leisure"},
			{"KM", "Deep Silver"},
			{"KR", "Krea Medie"},
			{"RW", "RealNetworks"},
			{"TL", "Telltale"},
			{"WY", "WayForward"},

			//Duplicates, may be dubious but some really are used with two different codes and what the heck?
			{"B0", "Acclaim (B0)"}, //Already uses 51
			{"46", "Angel (46)"}, //Already uses CF; seen in Super Putty (Europe) and Desert Fighter (Europe) which should be theoretically System 3 (Super Putty was published in Japan and US by Varie and US Gold respectively, Desert Fighter was published by Seta elsewhere)
			{"39", "Banpresto (39)"}, //Already uses D9
			{"9D", "Banpresto (9D)"},
			{"6E", "Elite Systems (6E)"}, //Already uses 0C; this one is used in the Power Slide SNES prototype; this may have something to do with their in-house development studio MotiveTime
			{"E0", "Jaleco (E0)"}, //Already uses 0A
			{"C2", "Kemco (C2)"}, //Already uses 7F; not sure what's going on here. This is used by Electrician and Roger Rabbit for FDS, 7F is used by Top Gear 3000, Daikatana GBC and Virtual League Baseball, could be C2 is Kemco Japan and 7F is Kemco Not-Japan? Also used for The Sword of Hope for GBC which was published by Seika
			{"28", "Kemco (28)"}, //Used in Virtual Pro Yakyuu 98 (VB)
			{"34", "Konami (34)"}, //Already uses A4
			{"4D", "Malibu (4D)"}, //Already uses 44
			{"5C", "Naxat Soft (5C)"},//Already uses D6
			{"37", "Taito (37)"}, //Already uses C0
			{"D0", "Taito (D0)"},
			{"C4", "Tokuma Shoten Intermedia"}, //Already uses 86 except without the word "Intermedia"; this is used for the FDS games and 86 is used for Madou Monogatari SNES
			{"E3", "Varie (E3)"}, //Already uses 95
			{"1F", "Virgin (1F)"}, //Already uses 61, this is found in the Muhammad Ali Heavyweight Boxing prototype (SNES)
			
			//Pan Docs (or whatever GB documentation I got them from) probably cut these off, and
			//if not I want to verify the names are the correct formatting
			{"22", "pow"},
			{"3C", "Entertainment I"}, //ndustries?
			{"25", "san-x"},
			{"75", "sci/The Sales Curve"}, //Seen in Super SWIV, which is _developed_ by those people, but published by Storm...
			{"96", "Yonezawa/s''pal"},

			//Questionable
			{"FF", "LJN (maybe junk)"}, //There is a good chance that this one is just completely invalid: Any time it shows up it's because it's some
						   //unlicensed software that doesn't care to fill in the headers correctly (looking at you Pro Action Replay), or it's something like a
						   //SNES game where I'm not detecting the header location correctly and so I'm reading the wrong data; LJN's infamous NES games
						   //were on a system with no manufacturer code in the header, and by the time they got to the SNES and GB they were bought out by
						   //Acclaim in 1990, and games like Spiderman and X-Men in Arcade's Revenge generally used Acclaim as the manufacturer 
						   //code anyway; and there is also an 0x56 up there for LJN for some SNES games that use it instead of Aclaim
			{"42", "Atlus (42)"}, //Only seen in Project S-11 which is by Paragon 5/Sunsoft and not Atlus (also some cheeky bugger homebrew games that want to use 42), all Atlus games so far use EB
			{"32", "Bandai (32)"}, //Only seen in the Picachu bootleg for SNES, other Bandai games use B2
			{"38", "Hudson/Capcom"},
			{"31", "Nintendo (31)"}, //Ehhh?? I've only seen this used in Heisei Gunjin Shougi for Satellaview, which was developed by some company called
			//Carrozzeria Japan apparently, which might be some brand used by Pioneer that's mostly used for car radios? I don't even know
			{"33", "Ocean/Acclaim"}, //This is an important one, because if this is the old licensee code it means it uses
			//the new licensee code instead (and the extended header in SNES), and also it needs to be 33 for Super Gameboy functions to work on GB
			//Is this even valid anyway? Ocean already has 67 and probably just uses Acclaim's licensee when it got purchased 
			{"93", "Ocean/Acclaim/Tsuburava"},
			{"73", "Sculptured Software"}, //Possibly wrong as I've only seen it in Monopoly SNES (only developed by them, published by Parker Bros), other games developed by them like Doom or The Simpsons: Bart's Nightmare use their publisher's code as you'd expect
			{"30", "Viacom/Infogrames"}, //Only seen in a GBC bootleg called Thunder Blast Man by Sachen. Viacom and Infogrames aren't related



			{"P8", "Pin Eight"}, //Well... I guess it's not really a licensee code if it's homebrew and therefore by definition unlicensed...
			{"AK", "Acekard"}, //Yeah, also not really a licensee either I guess; anyway we'll see if anything which isn't the Acecard firmware has this

			//I'm tempted to put ## and '  ' in here because of homebrew ROMs that
			//don't fill in the game code or put it as ####, as well as
			//Banjo-Pilot's beta version that has XXXX as the game code
		};

		public readonly static IDictionary<string, string> EXTENDED_LICENSEE_CODES = new Dictionary<string, string>() {
			//Seen on Wii U, which has four character licensee codes, but if they start with 00 they're just the old list of licensee codes (with the 00 at the front). So I guess they ran out of licensee codes at that point, huh? There's like, 1296 you can do with 2 alphanumeric characters. 1292 as 00, 33, FF, and ZZ would be probably invalid. So like... damn. I guess that's a thing that happened. Welp.
			{"010P", "13AM Games"},
			{"0167", "GalaxyTrail"},
		};

		//Is 0 = no rating correct for any of these, given that it's not no rating for the AGCB?
		public readonly static IDictionary<int, string> CERO_RATINGS = new Dictionary<int, string>() {
			//Used in Japan
			{0, "A"},
			{12, "B (12)"},
			{15, "C (15)"},
			{17, "D (17)"},
			{18, "Z (18)"},
			//There may be also some educational/utility/otherwise not-really-game software that has a
			//"Education/Database" rating
		};

		public readonly static IDictionary<int, string> ESRB_RATINGS = new Dictionary<int, string>() {
			//Used in USA, Canada, and Mexico
			{0, "No rating"}, //Shouldn't really exist, but it's used in Nintendo DSi Demo Video (which wasn't really sold) and Photo Channel and Wii Shop Channel (which were pre-installed)
			{3, "Early Childhood"},
			{6, "Everyone"},
			{10, "E10+"},
			{13, "Teen"},
			{17, "Mature"},
			//In theory there is 18 = Adults Only but I don't think any game like that would appear on these systems
		};

		public readonly static IDictionary<int, string> USK_RATINGS = new Dictionary<int, string>() {
			//Used in Germany
			{0, "No age restriction"},
			{6, "6+"},
			{12, "12+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> PEGI_RATINGS = new Dictionary<int, string>() {
			//Used in Europe, India, Pakistan, and Israel
			{0, "No rating"}, //In theory this doesn't exist (there isn't any PEGI rating label which corresponds to all ages including 2 and under), but Art Academy and Flipnote Studio use it?
			{3, "3+"},
			{7, "7+"},
			{12, "12+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> PEGI_PORTUGAL_RATINGS = new Dictionary<int, string>() {
			//Portugal decided to change 3 to 4 and 7 to 6 to align with the film rating system, so they end up being their own thing
			{0, "No rating"},
			{4, "4+"},
			{6, "6+"},
			{12, "12+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> PEGI_UK_RATINGS = new Dictionary<int, string>() {
			//No informational comment here, I'm just as confused as to what's going on as you are, or possibly more so
			{0, "No rating"},
			{3, "3+"},
			{4, "4+/U"},
			{7, "7+"},
			{8, "8+/PG"},
			{12, "12+"},
			{15, "15+"},
			{16, "16+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> AGCB_RATINGS = new Dictionary<int, string>() {
			//Used in Australia
			{0, "G"},
			{7, "PG"},
			{14, "M"},
			{15, "MA15+"},
			{18, "R"}, //Not introduced until 2013, all the Wii games released after that are not the kind of game which would earn such a rating
			//Not sure if there are games marked Exempt From Classification and how that would work
			//Back in my days there was a G8+ instead of PG
		};

		public readonly static IDictionary<int, string> GRB_RATINGS = new Dictionary<int, string>() {
			//Used in South Korea, apparently since 2006
			{0, "A"},
			{12, "12+"},
			{15, "15+"},
			{18, "18+"},
		};

		public readonly static IDictionary<int, string> FBFC_RATINGS = new Dictionary<int, string>() {
			//Used in Finland up until 2011, and then I think they started using PEGI
			//Definitely exists and used in Wii games, DSi might not be (GBATEK says the byte which would be used for FBFC is reserved)
			{0, "Not rated"},
			{3, "S"},
			{7, "K-7"},
			{11, "K-11"}, //May or may not exist, but Twilight Princess's FBFC rating is set to 11
			{12, "K-12"},
			{15, "K-15"}, //May or may not exist, but Red Steel's FBFC rating is set to 15
			{16, "K-16"},
			{18, "K-18"},
		};

		public readonly static IDictionary<int, string> CGSRR_RATINGS = new Dictionary<int, string>() {
			{0, "G"},
			{6, "Protected"},
			{12, "PG 12"},
			{15, "PG 15"},
			{18, "Restricted"},
		};

		//There are 6 unused bytes after these so those might be other countries
		//Some other countries and their rating boards in case they turn out to be used:
		//Brazil (ClassInd): L = General Audiences, 10+, 12+, 14+, 16+, 18+; "Especially recommended for children/teenagers" rating abandoned in 2009
		//Iran (ERSA): +3 (but they call it all ages), +7, +12, +15, +18
		//Argentina (INCAA): ATP = 0, 13+, 16+, 18+
		//New Zealand (OFLC, not at all like the OFLC Australia used to have): G = 0, PG = ?, M = 16+, RP13 = 13+ or with parental guidance, R13 = 13+, R15 = 15+, R16 = 16+, RP16 = 16+ or with parental guidance, R18 = 18+, RP18 (not created until April 2017) = 18+ or with parental guidance
		//There's a thing called the IARC which is an evil alliance of all the supervillains of the game rating world, and they have 3+/7+/12+/16+/18+ ratings for countries that don't have their own ratings thing, but that didn't exist since 2013

		readonly static Tuple<string, IDictionary<int, string>>[] RATING_NAMES = {
			new Tuple<string, IDictionary<int, string>>("CERO", CERO_RATINGS),
			new Tuple<string, IDictionary<int, string>>("ESRB", ESRB_RATINGS),
			new Tuple<string, IDictionary<int, string>>("<reserved>", null), //Probably BBFC, given position relative to Wii U XML stuff
			new Tuple<string, IDictionary<int, string>>("USK", USK_RATINGS),
			new Tuple<string, IDictionary<int, string>>("PEGI", PEGI_RATINGS),
			new Tuple<string, IDictionary<int, string>>("FBFC", FBFC_RATINGS), //Finland uses a different ratings board since 2011, but for Wii and DSi games it should be fine; I haven't seen this used anyway (actually, it might be Wii only and then became reserved with DSi/3DS)
			new Tuple<string, IDictionary<int, string>>("PEGI (Portgual)", PEGI_PORTUGAL_RATINGS),
			new Tuple<string, IDictionary<int, string>>("PEGI (UK)", PEGI_UK_RATINGS),
			new Tuple<string, IDictionary<int, string>>("AGCB", AGCB_RATINGS),
			new Tuple<string, IDictionary<int, string>>("GRB", GRB_RATINGS),
			new Tuple<string, IDictionary<int, string>>("CGSRR", CGSRR_RATINGS), //3DS only
		};

		public static void parseRating(ROMInfo info, int rating, string name, IDictionary<int, string> dict, bool isDSi) {
			//Bit 5 is set for ESRB on Super Smash Bros Brawl (USA v1.01), Bomberman Blast (USA), and
			//Mario Strikers Charged
			//Possibly indicates online interactivity, e.g. the specific label "Online Interactions Not Rated by the ESRB" / "Game Experience May Change During Online Play"

			//Bit 6 is set for USK in Madworld (PAL), so it possibly indicates something
			//like "banned in this country" or "refused classification"; otherwise Madworld is parsed as being all ages in Germany which is absolutely not the case
			//It's also set on Gnubox GX, VBA GX and USBLoaderCFG channel forwarders, but those are homebrew, so it might be just invalid (they also set bit 5, and without those two
			//bits the rating is read as 10, which isn't a rating category for USK)

			//On 3DS, bit 6 indicates "Rating Pending" and bit 5 indicates No Age Restriction but that can't be right for Wii and probably not DSi

			//Wii U seems to set bit 6 on every single BBFC and reserved rating so maybe it's like "not used"

			if ((rating & 0x40) > 0) {
				info.addInfo(name + " bit 6", true);
			}
			if ((rating & 0x20) > 0) {
				info.addInfo(name + " bit 5", true);
			}

			bool ratingExists;
			if (isDSi) {
				ratingExists = (rating & 0x80) != 0;
			} else {
				ratingExists = (rating & 0x80) == 0;
			}

			if (ratingExists) {
				//Actual rating is bits 0-4
				int ratingValue = rating & 0x1f;
				if (dict != null) {
					info.addInfo(name, ratingValue, dict);
				} else {
					info.addInfo(name, ratingValue);
				}
			}
		}

		public static void parseRatings(ROMInfo info, byte[] ratings, bool isDSi) {
			//Used with Wii/WiiWare and DSi, which both introduced parental controls features I guess
			//DSi seems to use bit 7 to indicate if a rating exists for a given country differently
			//To be precise: With DSi (and 3DS), bit 7 is set when a rating exists, on Wii, bit 7 is unset when a rating exists
			//Wii U has ratings too, but they're XML. Seemingly very similar, though

			
			for (int i = 0; i < 16; ++i) {
				int rating = ratings[i];
				string ratingName;
				if (i >= RATING_NAMES.Length) {
					ratingName = "Unknown rating " + (i - RATING_NAMES.Length);
				} else {
					ratingName = RATING_NAMES[i].Item1 + " rating";
				}

				IDictionary<int, string> ratingsDict = null;
				if (i < RATING_NAMES.Length && RATING_NAMES[i].Item2 != null) {
					ratingsDict = RATING_NAMES[i].Item2;
				}
				parseRating(info, rating, ratingName, ratingsDict, isDSi);

			}

		}
	}
}
