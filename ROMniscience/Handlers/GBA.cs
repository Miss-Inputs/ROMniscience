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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.IO;

namespace ROMniscience.Handlers {
	//Mostly adapted from http://problemkaputt.de/gbatek.htm#gbacartridgeheader
	class GBA : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"gba","Nintendo Game Boy Advance ROM" },
			{"bin","Nintendo Game Boy Advance ROM" },
			{"srl","Nintendo Game Boy Advance ROM" },
			{"mb","Nintendo Game Boy Advance multiboot ROM" },
		};
		public override string name => "Game Boy Advance";

		public static readonly IDictionary<char, string> GBA_GAME_TYPES = new Dictionary<char, string> {
			//Also used by GBC, seemingly
			{'A', "Game"},
			{'B', "Game (B)"}, //One day I wll find the rhyme or reason behind this, but newer games seem to use B
			{'F', "Famicom/Classic NES series"},
			{'K', "Acceleration sensor"}, //Yoshi's Universal Gravitation, Koro Koro Puzzle
			{'P', "e-Reader"},
			{'R', "Gyro sensor"}, //WarioWare: Twisted
			{'U', "Solar sensor"}, //Boktai: The Sun is in Your Hands
			{'V', "Rumble Pak"}, //Drill Dozer, various GBC games like the ol' Perfect Dark
			{'M', "GBA Video"}, //Also used by mb2gba and any multiboot roms converted by it
			{'Z', "DS expansion"}, //Daigassou! Band-Brothers - Request Selection (it's just a slot 2 device for a DS game, but it has a
			//GBA ROM header surprisingly), also Nintendo MP3 Player which was marketed as being for the DS so maybe "DS expansion" isn't quite
			//the right name but I dunno
			//Have also seen J for the Pokemon Aurora Ticket distribution cart, and G for GameCube multiboot images (they just use the product code of the GameCube disc they were from usually)
		};

		public static readonly IDictionary<int, string> GBA_MULTIBOOT_MODES = new Dictionary<int, string> {
			{0, "Not multiboot"},
			{1, "Joybus"},
			{2, "Normal"},
			{3, "Multiplay"},
		};

		int calculateChecksum(WrappedInputStream f) {
			long origPos = f.Position;
			try {
				int x = 0;
				f.Position = 0xa0;
				while (f.Position <= 0xbc) {
					x = (x - f.read()) & 0xff;
				}
				return (x - 0x19) & 0xff;
			} finally {
				f.Position = origPos;
			}
		}

		const int GBA_LOGO_CRC32 = -0x2F414AA2;

		static bool isNintendoLogoEqual(byte[] nintendoLogo) {
			return Datfiles.CRC32.crc32(nintendoLogo) == GBA_LOGO_CRC32;
		}

		//There's no official way to detect the save type, but Nintendo's SDK ends up
		//putting these strings in the ROM according to what it uses, apparently
		readonly static byte[] EEPROM = Encoding.ASCII.GetBytes("EEPROM_V");
		readonly static byte[] SRAM = Encoding.ASCII.GetBytes("SRAM_V");
		readonly static byte[] SRAM_F = Encoding.ASCII.GetBytes("SRAM_F_V");
		readonly static byte[] FLASH = Encoding.ASCII.GetBytes("FLASH_V");
		readonly static byte[] FLASH_512 = Encoding.ASCII.GetBytes("FLASH512_V");
		readonly static byte[] FLASH_1024 = Encoding.ASCII.GetBytes("FLASH1M_V");
		//It also puts this in for games that use the real time clock
		readonly static byte[] RTC = Encoding.ASCII.GetBytes("SIIRTC_V");

		static void detectSaveType(ROMInfo info, byte[] bytes) {
			if (ByteSearch.contains(bytes, EEPROM)) {
				info.addInfo("Save type", "EEPROM");
				//Can't tell the save size from this, it's either 512 or 8192 though
			} else if (ByteSearch.contains(bytes, SRAM) || ByteSearch.contains(bytes, SRAM_F)) {
				info.addInfo("Save type", "SRAM");
				info.addInfo("Save size", 32 * 1024, ROMInfo.FormatMode.SIZE);
			} else if (ByteSearch.contains(bytes, FLASH) || ByteSearch.contains(bytes, FLASH_512)) {
				info.addInfo("Save type", "Flash");
				info.addInfo("Save size", 64 * 1024, ROMInfo.FormatMode.SIZE);
			} else if (ByteSearch.contains(bytes, FLASH_1024)) {
				info.addInfo("Save type", "Flash");
				info.addInfo("Save size", 128 * 1024, ROMInfo.FormatMode.SIZE);
			}
		}

		//Thanks to GBAMusRipper and saptapper for documenting these and whatnot
		readonly static byte[] MP2K_SELECTSONG = {
			0x00, 0xB5, 0x00, 0x04, 0x07, 0x4A, 0x08, 0x49,
			0x40, 0x0B, 0x40, 0x18, 0x83, 0x88, 0x59, 0x00,
			0xC9, 0x18, 0x89, 0x00, 0x89, 0x18, 0x0A, 0x68,
			0x01, 0x68, 0x10, 0x1C, 0x00, 0xF0
		};
		readonly static byte[] MP2K_NEW_SELECTSONG = {
			0x00, 0xB5, 0x00, 0x04, 0x07, 0x4B, 0x08, 0x49,
			0x40, 0x0B, 0x40, 0x18, 0x82, 0x88, 0x51, 0x00,
			0x89, 0x18, 0x89, 0x00, 0xC9, 0x18, 0x0A, 0x68,
			0x01, 0x68, 0x10, 0x1C, 0x00, 0xF0
		};
		readonly static byte[] NATSUME_MAIN = {
			0x70, 0xb5, 0x20, 0x49, 0x20, 0x4a, 0x10, 0x1c,
			0x08, 0x80, 0x00, 0xf0, 0x8d, 0xf8, 0x01, 0xf0,
			0x97, 0xfc, 0x00, 0xf0, 0x4b, 0xf8, 0x80, 0x21,
			0xc9, 0x04, 0x60, 0x20, 0x08, 0x80, 0x1b, 0x49,
			0x01, 0x20, 0x08, 0x60, 0x1a, 0x48, 0x00, 0x21,
			0x01, 0x60, 0x1a, 0x48, 0x01, 0x60, 0x37, 0xf0,
			0x81, 0xfa, 0x19, 0x48, 0x00, 0xf0, 0xce, 0xf8
		};

		readonly static byte[] GAX2_INIT = { 0x47, 0x41, 0x58, 0x32, 0x5f, 0x49, 0x4e, 0x49, 0x54}; //Literally "GAX2_INIT" in ASCII
		//Taken from lib/mixer_func.s from the source of Krawall on Github, converted from assembly to hex. Seems to be good enough for identification
		readonly static byte[] KRAWALL_MIXCENTER = {
			0xf0, 0x0f, 0x2d, 0xe9, //stmdb	sp! {r4-r11}
			0x08, 0x50, 0x90, 0xe5, //ldr	r5, [r0, #8]
			0x14, 0x60, 0x90, 0xe5, //ldr	r6, [r0, #20]
			0xbc, 0x71, 0xd0, 0xe1, //ldrh	r7, [r0, #28]
			0x1e, 0x30, 0xd0, 0xe5, //ldrb	r3, [r0, #30]
			0x22, 0x21, 0xa0, 0xe1  //mov r2, r2, lsr #2
		};
		//....Yeah
		readonly static byte[] RARE_AUDIO_ERROR = Encoding.ASCII.GetBytes("AUDIO ERROR, too many notes on channel 0.increase polyphony RAM");
		//Discovered this accidentally myself, sometimes it's credited as GBAModPlay and sometimes as LS_Play; sometimes 2002 and sometimes 2003; and Google has no results at all for either of these other than the latter appearing in the TCRF page for Garfield and His Nine Lives but having no explanation other than it being a hidden credit and the URL after the year is now simply about a mobile game which the company made and seems to be their only online presence
		readonly static byte[] LOGIK_STATE_COPYRIGHT = Encoding.ASCII.GetBytes(" (C) Logik State ");

		static string detectSoundDriver(ROMInfo info, byte[] bytes) {
			if (ByteSearch.contains(bytes, MP2K_SELECTSONG)) {
				//The standard driver among most GBA games, seemingly included in the official GBA SDK (apparently). Otherwise
				//known as Sappy or M4A
				return "MP2000";
			} else if (ByteSearch.contains(bytes, MP2K_NEW_SELECTSONG)) {
				//Apparently it was also recompiled at some point and some games use it (Mother 3, Minish Cap, some others) but there doesn't seem to be any consistency in terms of new games using this and older games using the allegedly older driver
				return "MP2000 (new)";
			} else if (ByteSearch.contains(bytes, NATSUME_MAIN)) {
				//Not sure what uses this. Games developed by Natsume, I guess (which amounts to basically Medabots, Keitai Denju Telefang 2, Buffy the Vampire Slayer, Shaun Palmer's Pro Snowboarder, some Power Rangers and wrestling games)
				return "Natsume";
			} else if (ByteSearch.contains(bytes, KRAWALL_MIXCENTER)) {
				//A third party thing that plays converted s3m/xm files, used by a few games such as
				//Lord of the Rings and The Sims according to the author's website, and also
				//Dora the Explorer: Dora's World Adventure and Harry Potter and the Prisoner of Azkaban (but
				//not the other Dora or Harry Potter games), unless I'm actually detecting this all wrong
				//and they use something else. It's possible I am because it appears in a few homebrew
				//demos (excluding the obvious Krawall Demo), but then maybe they actually do use it since it's now LGPL'd
				//so I guess I should check the credits of those?
				return "Krawall";
			} else if (ByteSearch.contains(bytes, RARE_AUDIO_ERROR)) {
				//Games developed by Rare have this huge block of error text. This is probably the most wrong way to
				//possibly do this but it works and whatnot so maybe it isn't. But I feel dirty for doing this
				return "Rare";
			} else if (ByteSearch.contains(bytes, GAX2_INIT)) {
				//Used by various third-party games. All of them have a block of copyright text
				//specifying that the game uses the GAX engine, and also the version which is nice, that
				//the engine is developed by Shin'en Multimedia, and also some function names like
				//GAX2_INIT a bit after that block. Although I feel like this might result in
				//false positives.... should be fine, hopefully
				return "GAX";
			} else if(ByteSearch.contains(bytes, LOGIK_STATE_COPYRIGHT)) {
				//I don't know what to call this one; used in a few third party games (Asterisk & Obelisk XXL, Driv3r among others)
				//Gotta admit I don't really like this and should detect it better, but it is apparent that those two games use things by this
				//company at least
				return "GBAModPlay/LS_Play";
			} else {
				return "Unknown";
			}
			//Games with unknown sound drivers:
			//007: Nightfire (JV Games)
			//Barbie as the Island Princess (Human Soft)
			//Barbie Groovy Games (DICE)
			//Barbie Horse Adventures: Blue Ribbon Race (Mobius, Blitz)
			//Classic NES Series / Famicom Mini
			//Crazy Frog Racer (Independent Arts)
			//Crazy Taxi: Catch a Ride (Graphic State, music by Paragon 5)
			//Doom (David A. Palmer Productions)
			//Doom II (Torus, same Southpaw engine as used in Duke Nukem Advance)
			//Dora the Explorer: Super Spies (Cinegroupe)
			//Dora the Explorer: The Search for the Pirate Pig's Treasure (Cinegroupe)
			//Dragon Ball GT: Transformation (Webfoot)
			//Dragon Ball Z: Collectible Card Game (ImaginEngine)
			//Dragon Ball Z: Taiketsu (Webfoot)
			//Dragon Ball Z: The Legacy of Goku trilogy (Webfoot)
			//Duke Nukem Advance (Torus, same Southpaw engine as used in Doom II)
			//FIFA Soccer 07 (Exient)
			//Hamtaro: Ham-Ham Games (AlphaDream)
			//Hamtaro: Rainbow Rescue (AlphaDream)
			//Harry Potter and the Philosopher's Stone, Chamber of Secrets (Griptonite, says something about MusyX in the intro)
			//Hello Kitty: Happy Party Pals (Webfoot)
			//Lego Island 2 (Silicon Dreams)
			//Mario vs. Donkey Kong (Nintendo Software Technology)
			//Mary Kate & Ashley: Girls Night Out, Sweet 16: Licensed to Drive (Powerhead)
			//Max Payne (Mobius)
			//Meine Tiearztpraxis / Meine Tierpension (Independent Arts)
			//Metroid Fusion (Uses the MP2000 sequence format but not the MP2000 playback. Metroid: Zero Mission and Wario Land 4 are probably the same?)
			//My Little Pony: Crystal Princess: The Runaway Rainbow (Webfoot)
			//Need For Speed: Underground (Pocketeers)
			//Pinball Challenge Deluxe (Binary9, uses Logik State's music playback according to the credits but doesn't have the usual copyright string, so I may be doing something wrong)
			//SimCity 2000 (Full Fat)
			//Super Mario Advance 2/3/4 (Nintendo R&D2, Super Mario Advance 1 uses MP2000)
			//V-Rally 3 (Velez & Dubail)
			//WarioWare Inc, WarioWare: Twisted (Nintendo SPD 1)
			//Who Wants to be a Millionaire? (Houthouse)
			//GBA Video (4Kidz apparently?)
			//Rhythm Tengoku (Nintendo SPD 1)

			//Pokemon Liquid Crystal and Pokemon Shiny Gold are Pokemon ROM hacks so they should use MP2000, but apparently they don't somehow or they broke something to make them not detect as using it
			//Also, apparently Mario & Luigi: Superstar Saga only uses MP2000 for the Mario Bros part and not for the main game, so that's weird
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);
			WrappedInputStream f = file.stream;

			byte[] entryPoint = f.read(4);
			info.addInfo("Entry point", entryPoint, true);
			byte[] nintendoLogo = f.read(156);
			info.addInfo("Nintendo logo", nintendoLogo, true);
			info.addInfo("Nintendo logo valid?", isNintendoLogoEqual(nintendoLogo));
			//TODO: Bits 2 and 7 of nintendoLogo[0x99] enable debugging functions when set (undefined instruction exceptions are sent
			//to a user handler identified using the device type)
			//0x9b bits 0 and 1 also have some crap in them but I don't even know

			string title = f.read(12, Encoding.ASCII).TrimEnd('\0');
			info.addInfo("Internal name", title);
			string gameCode = f.read(4, Encoding.ASCII);
			info.addInfo("Product code", gameCode);
			char gameType = gameCode[0];
			info.addInfo("Type", gameType, GBA_GAME_TYPES);
			string shortTitle = gameCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char country = gameCode[3];
			info.addInfo("Country", country, NintendoCommon.COUNTRIES);

			string makerCode = f.read(2, Encoding.ASCII);
			info.addInfo("Manufacturer", makerCode, NintendoCommon.LICENSEE_CODES);
			int fixedValue = f.read();
			info.addInfo("Fixed value", fixedValue, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Fixed value valid?", fixedValue == 0x96);

			//This indicates the required hardware, should be 0 but it's possible that
			//some prototype/beta/multiboot/other weird ROMs have something else
			int mainUnitCode = f.read();
			info.addInfo("Main unit code", mainUnitCode, true);

			//If bit 7 is set, the debugging handler entry point is at 0x9fe2000 and not 0x9ffc000, normally this will be 0
			int deviceType = f.read();
			info.addInfo("Device type", deviceType, true);

			byte[] reserved = f.read(7); //Should be all 0
			info.addInfo("Reserved", reserved, true);
			int version = f.read();
			info.addInfo("Version", version);
			int checksum = f.read();
			info.addInfo("Checksum", checksum, ROMInfo.FormatMode.HEX, true);
			int calculatedChecksum = calculateChecksum(f);
			info.addInfo("Calculated checksum", calculatedChecksum, ROMInfo.FormatMode.HEX, true);
			info.addInfo("Checksum valid?", checksum == calculatedChecksum);
			byte[] reserved2 = f.read(2);
			info.addInfo("Reserved 2", reserved2, true);

			byte[] restOfCart = f.read((int)f.Length);
			info.addInfo("Has RTC", ByteSearch.contains(restOfCart, RTC));
			detectSaveType(info, restOfCart);
			info.addInfo("Sound driver", detectSoundDriver(info, restOfCart));
		}
	}
}
