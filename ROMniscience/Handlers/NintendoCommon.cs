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

namespace ROMniscience.Handlers {
	class NintendoCommon {

		public static readonly IDictionary<char, string> REGIONS = new Dictionary<char, string> {
			//Used by GBC, GBA, and GameCube; DS might have a few differences (still unsure about A)
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
			{'N', "Canada"},
			{'P', "Europe"},
			{'R', "Russia"},
			{'S', "Spain"},
			{'T', "Taiwan"},
			{'U', "Australia"},
			{'W', "Sweden/Scandinavia"},
			{'X', "Europe (X)"},
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

		public static readonly IDictionary<int, string> DISC_REGIONS = new Dictionary<int, string>() {
			{0, "NTSC-J"},
			{1, "NTSC"},
			{2, "PAL"},
			{3, "Region free"}, //maybe?
			{4, "NTSC-K"},
		};

		public static readonly IDictionary<String, String> LICENSEE_CODES = new Dictionary<String, String>() {
			//These look like they're used by Gamecube, and Wii as well
			//Anyway, these are used by FDS, SNES, GB/GBC, GBA, DS, 64DD and Virtual Boy; seemingly Pokemon Mini
			//Preferred format that I probably don't always use consistently:
			//Company without Co, Inc, Ltd, etc
			//Company (Brand name used by company)
			//Company A / Company B which purchased A or A otherwise got absorbed into
			//Company A / New name of Company A
			//[Company B that merged with company A but kept A's licensee code] Company A e.g. [Bandai] Namco (Namco used the code AF before the merger)

			{"00", "Nobody"}, //Probably homebrew
			{"  ", "Nobody"},
			{"\0\0", "Nobody"},
			{"##", "Nobody"},
			{"??", "Nobody"},

			{"01", "Nintendo"},
			{"08", "Capcom"},
			{"0A", "Jaleco"},
			{"0B", "Coconuts"},
			{"0C", "Elite Systems"},
			{"13", "EA Japan"}, //This only appears in Japanese games by EA, like Sommelier DS, or the Japanese version of Harry Potter and the Philosopher's Stone for GBC; every other EA game uses the sex number; anyway I don't know what EA actually calls their Japanese branch but that'll do
			{"18", "Hudson Soft"},
			{"19", "Bandai (B-AI)"},
			{"1A", "Yanoman"},
			{"1D", "Clary"},
			{"20", "Zoo"}, //Are they called Zoo Publishing? Zoo Games? Zoo Entertainment? Zoo Digital? I honestly have no fucking clue it's like all four of them at once
			{"24", "PCM Complete"},
			{"29", "Seta"},
			{"2N", "Smilesoft"},
			{"2P", "The Pokémon Company"},
			{"30", "Viacom/Infogrames"},
			{"35", "Hector"},
			{"3E", "Gremlin"},
			{"41", "Ubisoft"},
			{"44", "Malibu"},
			{"47", "Bullet-Proof Software/Spectrum Holobyte"},
			{"49", "Irem"},
			{"4A", "Gakken"},
			{"4F", "Eidos"}, //The other documentation on the internets seems to think this is U.S. Gold, which I don't think is correct from ROMs I've seen use the code; the other thing is that this appears in the Gauntlet DS proto, but that wasn't going to be published by Eidos nor did they develop it
			{"4Q", "Disney"}, //What's maximum spooky is that this also shows up in the Spiderman: Friend or Foe trailer, but that was 2007 and Disney hadn't purchased Marvel yet
			{"4Y", "Rare"},
			{"4Z", "Crave Entertainment"}, //Resident Evil GBC prototype also has this, but it was developed by HotGen and published by Capcom
			{"50", "Absolute"},
			{"51", "Acclaim"},
			{"52", "Activision"},
			{"53", "American Sammy"},
			{"54", "Take-Two Interactive (2K Games, GameTek, Rockstar, etc)"}, //Oof this one's a really fun one. Some documentation says "Konami/GameTek" which is just garbage
																			   //because Konami has nothing to do with this, but some older games using this code like Wheel of Fortune
																			   //on SNES and the InfoGenius Productivity Pak on GB are indeed published under GameTek. Where it gets
																			   //fun is that GameTek became Take-Two Interactive at some point, and Duke Nukem Advance uses this
																			   //code too and was published under the Take-Two Interactive name.. but where it gets fun is that
																			   //Dora the Explorer: Dora's World Adventures also uses this code but was published
																			   //under Global Star Software, which was a company that became 2K Play which is then a branch of
																			   //2K Games and if you've lost track of everything I don't blame you and I just spent several
																			   //tens of minutes googling around for info on Dora the Explorer why do I do this to myself
																			   //Anyway this one just seems to cover everything that's owned by Take-Two Interactive
			{"55", "Hi-Tech Expressions"},
			{"56", "LJN"},
			{"57", "Matchbox"},
			{"58", "Mattel"},
			{"59", "Milton Bradley"},
			{"5A", "Mindscape"},
			{"5B", "Romstar"},
			{"5D", "Midway/Tradewest/Williams"}, //They're all the same thing really, everyone's buying out everyone
			{"5G", "Majesco"},
			{"5L", "NewKidsCo"},
			{"5Q", "Lego"},
			{"5T", "Cyro Interactive"},
			{"5Z", "Classified Games"}, //Also appears in the Card Shark (SNES) proto, which as far as I can tell was developed by someone named Bonsai and involved someone named Bicycle as well
			{"60", "Titus"},
			{"61", "Virgin"},
			{"64", "LucasArts"},
			{"67", "Ocean"},
			{"69", "Electronic Arts"}, //nice
			{"6B", "Beam Software/Melbourne House"},
			{"6F", "Electro Brain"},
			{"6S", "TDK"},
			{"70", "Infogrames/Atari, SA"}, //The modern Atari these days _is_ Infogrames basically and it's all very confusing, but anyway yeah there's DS games like Point Blank DS which use this and they're published under the Atari name, and then there's stuff like Legacy of Goku which is published under Infogrames but they're basically just the same company, and don't ask me about the original Atari because I'll die of confusion
			{"71", "Interplay"},
			{"78", "THQ"},
			{"79", "Accolade"},
			{"7A", "Triffix Entertainment"},
			{"7C", "Microprose"},
			{"7D", "Vivendi"},
			{"7F", "Kemco"},
			{"80", "Misawa Entertainment"},
			{"83", "LOZC"},
			{"86", "Tokuma Shoten"},
			{"87", "Tsukuda Ori"},
			{"8B", "Bullet-Proof Software"},
			{"8C", "Vic Tokai"},
			{"8P", "Sega"},
			{"91", "Chunsoft"},
			{"92", "Video System"},
			{"95", "Varie"},
			{"97", "Kaneko"},
			{"99", "Pack-in Video"},
			{"9A", "Nihon Bussan (Nichibutsu)"},
			{"9B", "Tecmo"},
			{"9C", "Imagineer"},
			{"9F", "Nova"},
			{"9M", "Jaguar"}, //The sewing machine company
			{"A1", "Hori Electric"},
			{"A2", "Scorpion Soft"},
			{"A4", "Konami"},
			{"A6", "Kawada"},
			{"A7", "Takara"}, //Vast Fame also uses this for most of their GBC bootlegs, interestingly
			{"A8", "Royal Industries"}, //More damn sewing machine companies!
			{"A9", "Technos Japan"},
			{"AA", "Broderbund"},
			{"AC", "Toei Animation"}, //Used in some EXTREMELY obscure "Waiwai Check" games for Satellaview, which according to the title screen of one of them, are made by Hori Electric? Well I'm confused; there's an FDS game I already forgot the name of that's actually published by Toei though
			{"AD", "Toho"},
			{"AF", "[Bandai] Namco"}, //Namco games have always used this, but when they merged with Bandai, they kept this code (so newer games like that Code Geass DS one use this code as well), which is interesting because Square Enix doesn't reuse Squaresoft or Enix's licensee codes and gets a new one
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
			{"CF", "Angel"},
			{"D1", "SOFEL"},
			{"D2", "Bothtec / Quest"},
			{"D3", "Sigma Enterprises"},
			{"D4", "Ask Kodansha"},
			{"D6", "Naxat Soft"},
			{"D7", "Copya Systems"},
			{"D9", "Banpresto"},
			{"DA", "Tomy"},
			{"DD", "Masaka"}, //Brand that Nippon Computer Systems uses to distribute games, but the rest of the company has no involvement so I might as well just put Masaka here and call it a day
			{"DE", "Human"},
			{"DF", "Altron"},
			{"E1", "Towachiki"},
			{"E2", "Yuutaka"},
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
			{"GD", "Square Enix"},

			//Duplicates
			{"B0", "Acclaim (B0)"}, //Already uses 51
			{"46", "angel"}, //Already uses CF
			{"39", "Banpresto (39)"}, //Already uses D9
			{"9D", "Banpresto (9D)"}, 
			{"6E", "Elite Systems (6E)"}, //Already uses 0C; this one isused in the Power Slide SNES prototype; this may have something to do with their in-house development studio MotiveTime
			{"E0", "Jaleco (E0)"}, //Already uses 0A
			{"C2", "Kemco (C2)"}, //Already uses 7F; not sure what's going on here. This is used by Electrician and Roger Rabbit for FDS, 7F is used by Top Gear 3000, Daikatana GBC and Virtual League Baseball, could be C2 is Kemco Japan and 7F is Kemco Not-Japan?
			{"28", "Kemco (28)"},
			{"34", "Konami (34)"}, //Already uses A4
			{"4D", "Malibu (4D)"}, //Already uses 44
			{"5C", "Naxat Soft (5C)"},//Already uses D6
			{"37", "Taito (37)"}, //Already uses C0
			{"D0", "Taito (D0)"},
			{"C4", "Tokuma Shoten Intermedia"}, //Already uses 86 except without the word "Intermedia"; this is used for the FDS games and 86 is used for Madou Monogatari SNES
			{"E3", "Varie (E3)"}, //Already uses 95
			{"1F", "Virgin (1F)"}, //Already uses 61, this is found in the Muhammad Ali Heavyweight Boxing prototype (SNES)
			
			//Pan Docs (or whatever GB documentation I got them from) probably cut these off, and
			//if not I want to verify the names
			{"22", "pow"},
			{"3C", "Entertainment I"}, //ndustries?
			{"09", "hot-b"},
			{"8F", "i'max"},
			{"25", "san-x"},
			{"75", "sci/The Sales Curve"},
			{"96", "Yonezawa/s''pal"},

			//Questionable
			{"FF", "LJN (maybe junk)"}, //There is a good chance that this one is just completely invalid: Any time it shows up it's because it's some
						   //unlicensed software that doesn't care to fill in the headers correctly (looking at you Pro Action Replay), or it's something like a
						   //SNES game where I'm not detecting the header location correctly and so I'm reading the wrong data; LJN's infamous NES games
						   //were on a system with no manufacturer code in the header, and by the time they got to the SNES and GB they were bought out by
						   //Acclaim in 1990, and games like Spiderman and X-Men in Arcade's Revenge use Acclaim as the manufacturer 
						   //code anyway; and there is also an 0x56 up there for LJN
			{"FR", "Digital Jesters"}, //Could also be Neko Entertainment, I'm just looking at Crazy Frog Racer
			{"42", "Atlus (42)"}, //Only seen in Project S-11 which is by Paragon 5/Sunsoft and not Atlus (also some cheeky bugger homebrew games that want to use 42), all Atlus games so far use EB
			{"32", "Bandai (32)"}, //Only seen in the Picachu bootleg for SNES, other Bandai games use B2
			{"72", "Broderbund (72)"}, //Might be wrong, as I've only seen it in Dungeon Master, which is published by JVC and not Broderbund
			{"DB", "LJN (DB)"}, //Actually might not be LJN, it is seen in Ishidou for FDS which is by Hiro
			{"38", "Hudson/Capcom"},
			{"31", "Nintendo (31)"}, //Ehhh?? I've only seen this used in Heisei Gunjin Shougi for Satellaview, which was developed by some company called
			//Carrozzeria Japan apparently, which might be some brand used by Pioneer that's mostly used for car radios? I don't even know
			{"33", "Ocean/Acclaim"}, //This is an important one, because if this is the old licensee code it means it uses
			//the new licensee code instead (and the extended header in SNES), and also it needs to be 33 for Super Gameboy functions to work on GB
			//Is this even valid anyway? Ocean already has 67 and probably just uses Acclaim's licensee when it got purchased 
			{"93", "Ocean/Acclaim/Tsuburava"},
			{"8E", "Ape"}, //Might be actually Character Soft
			{"73", "Sculptured Software"}, //Possibly wrong as I've only seen it in Monopoly SNES (only developed by them, published by Parker Bros), other games developed by them like Doom or The Simpsons: Bart's Nightmare use their publisher's code as you'd expect



			{"P8", "Pin Eight"}, //Well... I guess it's not really a licensee code if it's homebrew and therefore by definition unlicensed...
			{"AK", "Acekard"}, //Yeah, also not really a licensee either I guess; anyway we'll see if anything which isn't the Acecard firmware has this

			//I'm tempted to put ## and '  ' in here because of homebrew ROMs that
			//don't fill in the game code or put it as ####, as well as
			//Banjo-Pilot's beta version that has XXXX as the game code

			//Rambling and large amounts of comments and speculation on more maker codes below!

			//Official games, but I want to confirm by seeing if more titles use them:
			//1P (Chee-Chai Alien): Creatures
			//1Q: McDonalds Monogatari: Honobono Tenchou Ikusei Game (GBC) TDK Core (might be TDK's Japanese branch?)
			//2L: Barcode Taisen Bardigun (dev Graphic Research, pub Tamsoft)
			//36 (Cannon Fodder) Could be Codemasters (developer) or Activision (publisher)
			//4S: SimCity 2000 (SNES) (dev Maxis pub THQ for Europe version that this is from)
			//5H: Warriors of Might and Magic (3DO Company)
			//5K (Q-bert GBC) Dev: Pipe Dream / Pub: Hasbro
			//5V (Cookie & Cream demo) FromSoftware I guess?
			//62: SimAnt (SNES) Maxis, but this was the only game where they published their own game (albeit they didn't develop the SNES version, Imagineer did from what I can tell), so... hmm...
			//65 (X USA prototype) Dev: Nintendo & Argonaut / Pub: Nintendo for Japanese release
			//6K (Monster Rancher proto) Dev: Cing / Pub: would have been Tecmo or UFO Interactive
			//6Q: Microsoft Pinball Arcade (GBC) (dev: Saffire pub: Classified Games) but Classified Games are already 5Z? Ehhhh??
			//6V: The Nations: Land of Legends (dev: Neon / pub: JoWooD Entertainment)
			//7L: Sabrina the Teenage Witch: Spooked (dev: WayForward / pub: Simon & Schuster)
			//7G (Pocket Music GBC) Jester Interactive / Rage Software
			//82: Cosmo Gang the Video (SNES) (just Namco? wat although title screen says Namcot, which I guess is a brand of Namco for some reason)
			//8M (Densha de Go! 2 GBC) Dev: ITL / Pub: CyberFront
			//8N (Guruguru Nagetto demo) Dev: BeeWorks / Pub: Success (EU release was 505 Game Street), endrift also uses this for Scrum: A Game Very Vaguely About Programming
			//9G (Dora the Explorer: Super Spies (Dev: Cinegroupe / Pub: Gotham Games), Dora the Explorer: Super Star Adventures (Dev: ImaginEngine / Pub: Global Star Software)) could be just Nick Jr licensed games? But the other Dora games don't use this one
			//9H: Super Tsumeshougi 1000 (BS) Pub: Bottom Up
			//A0: Let's Pachinko Nante Gindama series (BS) dev: Daiichi / pub: Telenet, BS Parlor! Parlor!: Dai-2-shuu (BS) dev: Daiichi / pub: Telenet
			//BC: Pachicom (FDS) Dev: Bear's / Shouei System Pub: Toshiba EMI
			//BH: Super Drift Out: World Rally Championships proto (SNES) (dev: Dragnet / pub: was Visco in Japan, US was going to be Accolade)
			//C7: Reflect World (FDS) (East Cube)
			//CL: Oekaki Logic (SNES) (Sekaibunka Publishing)
			//DK (Initial D Gaiden) MTO / Kodansha?
			//DM: Doshin the Giant 64DD games, but also the Randnet Disk, and not sure what's in common there (Randnet themselves? Alps? Did they have any involvement with Doshin the Giant though?)
			//F9 Spectre (SNES) (Mac dev: Peninsula Gameworks / pub: Velocity Development) (SNES dev: Synergistic Software / pub: US Cybersoft EU GameTek)
			//FG Bomberman Selection (dev: Hudson Soft / pub: Jupiter or Hudson Soft themselves, depending on who you ask) This is tricky because Hudson Soft already has a licensee code of 0x18, and Jupiter never published anything else, maybe some kind of Korean branch of Hudson?
			//FQ (WarioWare: Touched iQue version aka Momo Waliou Zhizao) Alpha-Unit (according to the banner)? Intelligent Systems? iQue itself or Wei Yen?
			//G0 (Monster Finder (albeit a bad dump) aka Foto Showdown outside JP) Alpha Unit
			//GT (Picture Perfect Hair Salon): 505 Games
			//H4 (Doki Doki Majo Shinpan!): SNK
			//HC (Jam Sessions demo) Dev: Plato / Pub: Ubisoft
			//HF (Professor Layton and the Curious Village (Level 5 / Nintendo))
			//K0 (DSVision Starter Kit (am3 Inc, DSVision itself is just Nintendo))
			//MV (Contact beta): Dev: Grasshopper Manufacture / Pub: Marvelous (JP), Atlus (US), Rising Star Games (PAL); also Beer Belly Bill (GBA homebrew)
			//NJ (System Flaw Europe): Enjoy Gaming
			//NK (Cocoto: Kart Racer beta): Dev: Neko Entertainment / Pub: BigBen Interactive (EU), Conspiracy Games (US), Kemco (JP)
			//PQ (Peggle: Dual Shot): Popcap Games
			//QH (Intellivision Lives): Virtual Play Games
			//SZ (System Flaw USA): Storm City Entertainment
			//WR (Scribblenauts, Super Scribblenauts): Warner Bros

			//Darkrai Distribution cart and Surfing Pikachu Distribution cart have KX, but as they're
			//not supposed to be released, I think that doesn't count. Or it could mean
			//Game Freak themselves, but it's interesting because every other Pokemon DS distro cart has Nintendo as the maker
			//code, and those two in particular seem to be the odd ones out as No-Intro doesn't include them...
			//And then also to be confusing, Puyo Nexus's translation of Puyo Puyo 7 uses KX as well, but the
			//official Puyo Puyo 7 just uses 8P/Sega
			//Kirby's Amazing Mirror prototype has MA as the maker, but it also has
			//MAKO as the game code (and MAKOTOSAMPLE as the internal name)

			//The multiboot ROM used with Final Fantasy Crystal Chronicles on Gamecube uses
			//GC, which might be Square, but it also might be them setting the game code
			//to whatever they want because it's not meant to be used without the Gamecube
			//game, or it might be to indicate GameCube


			//Some I found in non-commercially released roms, just out of interest since unlicensed
			//games by definition have nothing to do with licensee codes:
			//I'm ignoring anything that has characters that aren't numbers or uppercase letters

			//02: Karate Joe (GBC, Datel)
			//0D: Little Short Demo (GBC homebrew)
			//27: Chip the Chick (GBC bootlegged hack of Peetan)
			//48: Jingorou (FDS bootleg)
			//7E: GB Basic (homebrew) (game code says JEFF)
			//CR: Super Fighter Demo (VB homebrew)
			//F1: Sonic 3D Blast 5 (bootleg): F1 (also has unknown cart type of 234)
			//GC: Shuma Baobei - Chao Mengmeng Fanji Zhan (Mewtwo Strikes Back),
			//  Shuma Baobei - Hai Zhi Shen (both GBC bootlegs, Li Cheng)
			//(Interestingly enough, Huojian Bingtuan is an alternate version of Chao Mengmeng Fanji Zhan or
			//	vice versa, but doesn't have the same manufacturer)
			//HY: Rocman X (GBC bootleg, Sachen)
			//MH: Advanced Pasta Cooking Simulator (Virtual Boy homebrew)
			//MK: Shuma Baobei - Huojian Bingtuan (Team Rocket, aka Pokemon Red), Sonic Adventure 7,
			//  Sonic Adventure 8, Pokemon Adventure (all GBC bootleg), but also
			//  HF Demo, VB Racing (Virtual Boy hommebrew)
			//NP: Krawall Demo (GBA homebrew)
			//OK: Bleep (GBC homebrew)
			//QC: SM Coukyoushi Hitomi (SNES unlicensed)
			//RX: Terrifying 9/11 (GBC bootleg)
			//SS: Magic Eye (GBC homebrew)
			//VB: Sample Soft for VUE Programming (Virtual Boy homebrew)
			//XX: 2048 (GBC homebrew), Hang Time Basketball (GBC bootleg, Datel), The Matrix (Virtual Boy homebrew), Lights Out (DS homebrew)
			//ZZ: mooneye-gb test ROMs

		};
	}
}
