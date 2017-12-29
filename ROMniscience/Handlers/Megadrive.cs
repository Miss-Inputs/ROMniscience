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

namespace ROMniscience.Handlers {
	class Megadrive: Handler {
		//Some stuff adapted from https://www.zophar.net/fileuploads/2/10614uauyw/Genesis_ROM_Format.txt
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"gen", "Sega Genesis/Megadrive ROM"},
			{"bin", "Sega Genesis/Megadrive ROM"},
			{"sgd", "Sega Genesis/Megadrive ROM"},
			{"smd", "Sega Genesis/Megadrive interleaved ROM"},
			{"md", "Sega Genesis/Megadrive ROM"}, //Apparently there's another headered format that uses this extension, but I haven't seen it
		};

		public override string name => "Megadrive/Genesis";

		public readonly static IDictionary<string, string> MEGADRIVE_PRODUCT_TYPES = new Dictionary<string, string> {
			{"GM", "Game"},
			{"AI", "Education"}, //Or is it Al? Need to find something that actually uses this
			{"OS", "Operating system"}, //Genesis OS ROM uses this
			{"BR", "Boot ROM"}, //Mega CD BIOS etc
			{"SF", "Super Fighter Team game"}, //Beggar Price, Legend of Wukong, Star Odyssey, etc
			{"PX", "Pictures"}, //Hentai Collection homebrew (lol) etc

			//Also seen:
			//BL (32X Shymmer demo)
			//RO (32X SDK builds, X-Men 32X prototype, also FIFA Soccer '96 32X for whatever reason. Maybe
				//it means 32X SDK sample and the FIFA devs were lazy?)
			//T- (Soulstar X 32X prototype, Adventures in Letterland with Jack & Jill, 
				//some Samsung Pico games, B-Fighter Kabuto)
			//G- (Mega Anser, I forgot what that does)
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

		public readonly static IDictionary<char, string> MEGADRIVE_IO_SUPPORT = new Dictionary<char, string> {
			{'J', "Joypad"},
			{'6', "6-button joypad"},
			{'K', "Keyboard"},
			{'P', "Printer"},
			{'B', "Control ball"},
			{'F', "Floppy drive"},
			{'L', "Activator"},
			{'4', "Team Play"},
			{'0', "Master System joypad"},
			{'R', "Serial RS232C"},
			{'T', "Tablet"},
			{'V', "Paddle"},
			{'C', "CD-ROM"},
			{'M', "Mouse"},
			{'A', "Analog joystick"}, //After Burner (is this the XE-1 AP?)
			{'G', "Menacer"},
			
			//I would think the Ten Key Pad would have its own entry here but who knows
			//Others I've seen but I don't know:
			//D (pretty much every homebrew, could it mean "demo" or "development"?)
			//Roadwar 2000 seems to corrupt and misuse this field entirely
			//Outline 2017 demo seems to misuse both this field and the
			//product type field
			//MDEM says "Joypad only!" in ASCII in this field, as well as putting 
			//"The best" as the product type and code
		};

		public readonly static IDictionary<char, string> MEGADRIVE_REGIONS = new Dictionary<char, string> {
			{'E', "Europe"},
			{'J', "Japan"},
			{'U', "USA"},
			{'A', "Asia"},
			{'B', "Brazil"},
			{'4', "Brazil (4) or USA"},
			{'F', "France"}, //But then I've heard this can also be used for region-free
			{'8', "Hong Kong"},

			{'C', "USA + Europe"},
			{'G', "Germany"},
			{'S', "Spain"},
			{'I', "Italy"},
			{'e', "Europe"},
	
			//Not sure about these ones, I've only seen them in 32X stuff so far
			{'1', "Japan"},
			{'5', "Japan + USA"},
	
			//There's a 2 in the Multi-Mega BIOS, not sure what it means, as far as I can tell
			//that BIOS is just for Europe which it also has as a country code
			//Puggsy protoype has "NOV" in this field which seems to be misused
		};

		public static InputStream decodeSMD(InputStream s) {
			s.Seek(512, System.IO.SeekOrigin.Current);
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
			return new MemoryInputStream(buf2);
		}

		public static bool isSMD(InputStream s) {
			long origPos = s.Position;
			try {
				s.Seek(8, System.IO.SeekOrigin.Begin);
				int b8 = s.read();
				int b9 = s.read();

				s.Seek(0x280, System.IO.SeekOrigin.Begin);
				string str = s.read(4, Encoding.ASCII);

				return b8 == 0xaa && b9 == 0xbb && (String.Equals(str, "EAMG") || String.Equals(str, "EAGN"));
			} finally {
				s.Seek(origPos, System.IO.SeekOrigin.Begin);
			}
		}

		public static void parseMegadriveROM(ROMInfo info, InputStream s) {
			s.Seek(0x100, System.IO.SeekOrigin.Begin);

			string consoleName = s.read(16, Encoding.ASCII).Trim('\0').Trim();
			info.addInfo("Console name", consoleName);
			if(consoleName.StartsWith("SEGA 32X")) {
				// There are a few homebrew apps (32xfire, Shymmer) and also Doom
				// that misuse this field and say something else, so I've used
				// startswith instead, which should be safe, and picks up those three
				// except for 32xfire which claims to be a Megadrive game (it has
				//"32X GAME" as the domestic and overseas name)
				// Some cheeky buggers just use SEGA MEGADRIVE or SEGA GENESIS anyway even when they're 32X
				// Note that whatever the case may be, a Genesis/Megadrive game
				// better have "SEGA" at the start of the header or a real
				// console won't boot it, which means there is inevitably a
				// bootleg game that doesn't have it there
				// TODO: Detect Mega CD properly by seeking to start and looking
				// for 'SEGADISCSYSTEM'
				info.addInfo("Platform", "Sega 32X");
			} else {
				info.addInfo("Platform", "Sega Megadrive/Genesis");
			}

			string copyright = s.read(16, Encoding.ASCII).Trim('\0').Trim();
			info.addInfo("Copyright", copyright);
			Regex copyrightRegex = new Regex(@"\(C\)(\S{4}) (\d{4}\..{3})");
			var matches = copyrightRegex.Match(copyright);
			if(matches.Success) {
				info.addInfo("Manufacturer", matches.Groups[1].Value); //TODO This should be a map actually
				if(DateTime.TryParseExact(matches.Groups[2].Value, "yyyy.MMM", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out DateTime date)) {
					info.addInfo("Date", date);
				}
			}


			string domesticName = s.read(48, Encoding.ASCII).Trim('\0').Trim();
			info.addInfo("Internal name", domesticName);
			string overseasName = s.read(48, Encoding.ASCII).Trim('\0').Trim();
			info.addInfo("Overseas name", overseasName);
			string productType = s.read(2, Encoding.ASCII);
			info.addInfo("Type", productType, MEGADRIVE_PRODUCT_TYPES);

			s.read(); //Space for padding
			string serialNumber = s.read(8, Encoding.ASCII).Trim('\0').Trim();
			info.addInfo("Product code", serialNumber);
			s.read(); //- for padding
			string version = s.read(2, Encoding.ASCII);
			info.addInfo("Version", version);

			int checksum = s.readShortBE();
			//TODO calc checksum (add every byte in the ROM starting from 0x200 in 2-byte chunks (first byte multiplied by 256), use only first 16 bits of result)
			info.addExtraInfo("Checksum", checksum);

			char[] ioSupportList = s.read(16, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("IO support", ioSupportList, MEGADRIVE_IO_SUPPORT);
			int romStart = s.readIntBE();
			info.addExtraInfo("ROM start", romStart);
			int romEnd = s.readIntBE();
			info.addExtraInfo("ROM end", romStart);
			info.addInfo("ROM size", romEnd - romStart, ROMInfo.FormatMode.SIZE);
			int ramStart = s.readIntBE();
			info.addExtraInfo("RAM start", ramStart);
			int ramEnd = s.readIntBE();
			info.addExtraInfo("RAM end", ramEnd);
			info.addInfo("RAM size", ramEnd - ramStart, ROMInfo.FormatMode.SIZE);
			byte[] backupRamID = s.read(4);
			info.addExtraInfo("Backup RAM ID", backupRamID);
			int backupRamStart = s.readIntBE();
			info.addExtraInfo("Backup RAM start", backupRamStart);
			int backupRamEnd = s.readIntBE();
			info.addExtraInfo("Backup RAM end", backupRamEnd);
			info.addInfo("Save size", backupRamEnd - backupRamStart, ROMInfo.FormatMode.SIZE);
			byte[] modemData = s.read(12);
			info.addInfo("Modem data", modemData);
			//Technically this should be an ASCII string in the format MO<company><modem no#>.<version> or spaces if modem not supported

			string memo = s.read(40, Encoding.ASCII).Trim('\0').Trim();
			info.addInfo("Memo", memo);
			char[] regions = s.read(3, Encoding.ASCII).ToCharArray().Where((c) => c != ' ' && c != '\0').ToArray();
			info.addInfo("Region", regions, MEGADRIVE_REGIONS);
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
