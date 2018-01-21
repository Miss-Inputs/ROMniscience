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
	//Actually it looks like these are used by Gamecube discs as well
	//Anyway they're definitely used by SNES, GB, GBA, and DS, and sort of Pokemon Mini
	class NintendoCommon {
		public static readonly IDictionary<String, String> LICENSEE_CODES = new Dictionary<String, String>() {
			{"00", "Nobody"}, //Probably homebrew
			{"01", "Nintendo"},
			{"08", "Capcom"},
			{"09", "hot-b"},
			{"0A", "Jaleco"},
			{"0B", "Coconuts"},
			{"0C", "Elite Systems"},
			{"13", "Electronic Arts"},
			{"18", "Hudson Soft"},
			{"19", "b-ai/ITC Entertainment"},
			{"1A", "Yanoman"},
			{"1D", "Clary"},
			{"1F", "Virgin"},
			{"22", "pow"},
			{"24", "PCM Complete"},
			{"25", "san-x"},
			{"28", "Kemco Japan/Kotobuki Systems"},
			{"29", "Seta"},
			{"30", "Viacom/Infogrames"},
			{"31", "Nintendo"},
			{"32", "Bandai"},
			{"33", "Ocean/Acclaim"},

			//33 is an important one, because if this is the old licensee code it means it uses
			//the new licensee code instead, and also it needs to be 33 for Super Gameboy functions to work
			{"34", "Konami"},
			{"35", "Hector"},
			{"37", "Taito"},
			{"38", "Hudson/Capcom"},
			{"39", "Banpresto"},
			{"3C", "Entertainment I"}, //ndustries?
			{"3E", "Gremlin"},
			{"41", "Ubisoft"},
			{"42", "Atlus"},
			{"44", "Malibu"},
			{"46", "angel"},
			{"47", "Bullet-Proof Software/Spectrum Holobyte"},
			{"49", "Irem"},
			{"4A", "Virgin"},
			{"4D", "Malibu"},
			{"4F", "Eidos"}, //The other documentation on the internets seems to think this is U.S. Gold, which I don't think is correct from ROMs I've seen use the code
			{"50", "Absolute"},
			{"51", "Acclaim"},
			{"52", "Activision"},
			{"53", "American Sammy"},
			{"54", "Konami/Gametek"},
			{"55", "Hi-Tech Entertainment/Park Place"},
			{"56", "LJN"},
			{"57", "Matchbox"},
			{"58", "Mattel"},
			{"59", "Milton Bradley"},
			{"5A", "Mindscape"},
			{"5B", "Romstar"},
			{"5C", "Naxat Soft"},
			{"5D", "Midway/Tradewest"},
			{"60", "Titus"},
			{"61", "Virgin"},
			{"64", "LucasArts"},
			{"67", "Ocean"},
			{"69", "Electronic Arts"}, //nice
			{"6E", "Elite Systems"},
			{"6F", "Electro Brain"},
			{"70", "Infogrames"},
			{"71", "Interplay"},
			{"72", "Broderbund"},
			{"73", "Sculptured Soft"},
			{"75", "sci/The Sales Curve"},
			{"78", "THQ"},
			{"79", "Accolade"},
			{"7A", "Triffix Entertainment"},
			{"7C", "Microprose"},
			{"7F", "Kemco"},
			{"80", "Misawa Entertainment"},
			{"83", "LOZC"},
			{"86", "Tokuma Shoten Intermedia"},
			{"87", "Tsukuda Ori"},
			{"8B", "Bullet-Proof Software"},
			{"8C", "Vic Tokai"},
			{"8E", "Ape"},
			{"8F", "i'max"},
			{"91", "Chunsoft"},
			{"92", "Video System"},
			{"93", "Ocean/Acclaim/Tsuburava"},
			{"95", "Varie"},
			{"96", "Yonezawa/s''pal"},
			{"97", "Kaneko"},
			{"99", "Pack in soft/arc"},
			{"9A", "Nihon Bussan"},
			{"9B", "Tecmo"},
			{"9C", "Imagineer"},
			{"9D", "Banpresto"},
			{"9F", "Nova"},
			{"A1", "Hori Electric"},
			{"A2", "Bandai"},
			{"A4", "Konami"},
			{"A6", "Kawada"},
			{"A7", "Takara"},
			{"A9", "Technos Japan"},
			{"AA", "Broderbund"},
			{"AC", "Toei Animation"},
			{"AD", "Toho"},
			{"AF", "Namco"},
			{"B0", "Acclaim"},
			{"B1", "ASCII/Nexoft"},
			{"B2", "Bandai"},
			{"B3", "Enix"},
			{"B6", "HAL"},
			{"B7", "SNK"},
			{"B9", "Pony Canyon"},
			{"BA", "Culture Brain"},
			{"BB", "Sunsoft"},
			{"BD", "Sony Imagesoft"},
			{"BF", "Sammy"},
			{"C0", "Taito"},
			{"C2", "Kemco"},
			{"C3", "Squaresoft"},
			{"C4", "Tokuma Shoten Intermedia"},
			{"C5", "Data East"},
			{"C6", "Tonkin House"},
			{"C8", "Koei"},
			{"C9", "UFL"},
			{"CA", "Ultra"},
			{"CB", "Vap"},
			{"CC", "Use"},
			{"CD", "Meldac"},
			{"CE", "Pony Canyon"},
			{"CF", "Angel"},
			{"D0", "Taito"},
			{"D1", "SOFEL"},
			{"D2", "Quest"},
			{"D3", "Sigma Enterprises"},
			{"D4", "Ask Kodansha"},
			{"D6", "Naxat Soft"},
			{"D7", "Copya Systems"},
			{"D9", "Banpresto"},
			{"DA", "Tomy"},
			{"DB", "LJN"},
			{"DD", "NCS"},
			{"DE", "Human"},
			{"DF", "Altron"},
			{"E0", "Jaleco"},
			{"E1", "Towachiki"},
			{"E2", "Uutaka"},
			{"E3", "Varie"},
			{"E5", "Epoch"},
			{"E7", "Athena"},
			{"E8", "Asmik"},
			{"E9", "Natsume"},
			{"EA", "King Records"},
			{"EB", "Atlus"},
			{"EC", "Epic/Sony Records"},
			{"EE", "IGS"},
			{"F0", "A Wave"},
			{"F3", "Extreme Entertainment"},
			{"FF", "LJN"},

			//Found these myself and I'm confident enough they're correct
			{"5G", "Majesco"},
			{"8P", "Sega"},
			{"5Q", "Lego"},
			{"FR", "Digital Jesters"},
			{"P8", "Pin Eight"},
			{"6S", "TDK"},
			{"AK", "Acekard"},
			{"4Z", "Crave Entertainment"}, //Resident Evil GBC prototype also has this, but it was developed by HotGen and published by Capcom
			{"20", "Zoo"}, //Are they called Zoo Publishing? Zoo Games? Zoo Entertainment? Zoo Digital? I honestly have no fucking clue it's like all four of them at once
			{"4Q", "Disney"}, //What's maximum spooky is that this also shows up in the Spiderman: Friend or Foe trailer, but that was 2007 and Disney hadn't purchased Marvel yet
			{"7D", "Vivendi"},
			{"BJ", "Compile"},
			{"GD", "Square Enix"},
			{"BL", "MTO"},
			{"2P", "The Pokémon Company"},

			//I'm tempted to put ## and '  ' in here because of homebrew ROMs that
			//don't fill in the game code or put it as ####, as well as
			//Banjo-Pilot's beta version that has XXXX as the game code

			//Rambling and large amounts of comments and speculation on more maker codes below!

			//Some I found in non-commercially released GB roms:
			//GB Basic (homebrew): 7E (game code says JEFF)
			//Bleep (homebrew): OK
			//Harry Potter II (bootleg by Vast Fame): @7, probably corrupted
			//Sonic 3D Blast 5 (bootleg): F1 (also has unknown cart type of 234)
			//Little Short Demo (homebrew): 0D
			//Chip the Chick (bootleggy hack of Peetan): 27 (original Peetan has Kaneko as you would expect)
			//Shuma Baobei - Chao Mengmeng Fanji Zhan (Mewtwo Strikes Back),
			//	Shuma Baobei - Hai Zhi Shen (both bootlegs by Li Cheng): GC
			//Shuma Baobei - Huojian Bingtuan (Team Rocket, aka Pokemon Red), Sonic Adventure 7,
			//	Sonic Adventure 8, Pokemon Adventure (all bootleg): MK
			//(Interestingly enough, Huojian Bingtuan is an alternate version of Chao Mengmeng Fanji Zhan or
			//	vice versa, but doesn't have the same manufacturer)
			//Magic Eye (homebrew): SS
			//2048 (homebrew): XX (it uses XXXX for the manufacturer code as well)
			//Bitte 8 Bit (homebrew): \xfe\xaf
			//mooneye-gb test ROMs: ZZ

			//Official GB/GBA/DS roms, but I want to confirm by seeing if more titles use them:
			//1P (Chee-Chai Alien): Creatures
			//2N (Keitai Denjuu Telefang) Accordingly, Pokemon Diamond and presumably Jade have this too (Smilesoft/Natsume?)
			//36 (Cannon Fodder) Could be Codemasters (developer) or Activision (publisher)
			//5K (Q-bert GBC) Dev: Pipe Dream / Pub: Hasbro
			//5L (Hello Kitty's Cube Frenzy (Dev: Torus Games / Pub: Ubisoft), Dora the Explorer: The Search for the Pirate Pig's Treasure (Dev: Cinegroupe / Pub: NewKidsCo))
			//5V (Cookie & Cream demo) FromSoftware I guess?
			//65 (X USA prototype) Dev: Nintendo & Argonaut / Pub: Nintendo for Japanese release
			//6K (Monster Rancher proto) Dev: Cing / Pub: would have been Tecmo or UFO Interactive
			//7G (Pocket Music GBC) Jester Interactive / Rage Software
			//8M (Densha de Go! 2 GBC) Dev: ITL / Pub: CyberFront
			//8N (Guruguru Nagetto demo) Dev: BeeWorks / Pub: Success (EU release was 505 Game Street)
			//9G (Dora the Explorer: Super Spies (Dev: Cinegroupe / Pub: Gotham Games), Dora the Explorer: Super Star Adventures (Dev: ImaginEngine / Pub: Global Star Software)) could be just Nick Jr licensed games?
			//B4 (Dragon Warrior Monsters 2, Dragon Warrior I + II, Dragon Warrior III GBC): Dev: TOSE / Pub: Enix Dragon Warrior Monsters 1 uses Eidos
			//DK (Initial D Gaiden) MTO / Kodansha?
			//FQ (WarioWare: Touched iQue version aka Momo Waliou Zhizao) Alpha-Unit (according to the banner)? Intelligent Systems? iQue itself or Wei Yen?
			//G0 (Monster Finder (albeit a bad dump) aka Foto Showdown outside JP) Alpha Unit
			//GT (Picture Perfect Hair Salon): 505 Games
			//H4 (Doki Doki Majo Shinpan!): SNK
			//HC (Jam Sessions demo) Dev: Plato / Pub: Ubisoft
			//HF (Professor Layton and the Curious Village (Level 5 / Nintendo))
			//K0 (DSVision Starter Kit (am3 Inc, DSVision itself is just Nintendo))
			//MV (Contact beta): Dev: Grasshopper Manufacture / Pub: Marvelous (JP), Atlus (US), Rising Star Games (PAL)
			//NJ (System Flaw Europe): Enjoy Gaming
			//NK (Cocoto: Kart Racer beta): Dev: Neko Entertainment / Pub: BigBen Interactive (EU), Conspiracy Games (US), Kemco (JP)
			//PQ (Peggle: Dual Shot): Popcap Games
			//QH (Intellivision Lives): Virtual Play Games
			//SZ (System Flaw USA): Storm City Entertainment
			//WR (Scribblenauts, Super Scribblenauts): Warner Bros

			//Darkrai Distribution cart and Surfing Pikachu Distribution cart have KX, but as they're
			//not supposed to be released, I think that doesn't count. Or it could mean
			//Game Freak themselves, but it's interesting because every other Pokemon DS distro cart has Nintendo as the maker code
			//Kirby's Amazing Mirror prototype has MA as the maker, but it also has
			//MAKO as the game code (and MAKOTOSAMPLE as the internal name)
			//Beer Belly Bill uses MV, and Krawall Demo uses NP, but does that mean anything or
			//are they just random values?

			//The multiboot ROM used with Final Fantasy Crystal Chronicles on Gamecube uses
			//GC, which might be Square, but it also might be them setting the game code
			//to whatever they want because it's not meant to be used without the Gamecube
			//game, or it might be to indicate GameCube
		};
	}
}
