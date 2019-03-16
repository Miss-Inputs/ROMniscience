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
using ROMniscience.IO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class PSP : Handler {
		//http://www.psdevwiki.com/ps3/Eboot.PBP

		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"iso", "PlayStation Portable UMD disc"}, //This is basically just a DVD with an ISO9660 filesystem
			{"pbp", "PlayStation Portable PBP file"}, //http://www.psdevwiki.com/ps3/Eboot.PBP (I still have no idea what the acronym means)
		};

		public override string name => "Sony PlayStation Portable";

		public static bool isELFMagic(byte[] magic) {
			//<DEL>ELF in ASCII
			return magic[0] == 0x7f && magic[1] == 0x45 && magic[2] == 0x4c && magic[3] == 0x46;
		}

		public static bool isPBPMagic(byte[] magic) {
			//\0PBP in ASCII
			return magic[0] == 0x00 && magic[1] == 0x50 && magic[2] == 0x42 && magic[3] == 0x50;
		}

		public static Dictionary<string, object> convertParamSFO(WrappedInputStream s) {
			var d = new Dictionary<string, object>();

			s.Position = 0x08;
			int keyTableStart = s.readIntLE();
			int dataTableStart = s.readIntLE();
			int numberOfEntries = s.readIntLE();

			for (int i = 0; i < numberOfEntries; ++i) {
				short keyRelativeOffset = s.readShortLE();
				int keyOffset = keyTableStart + keyRelativeOffset;

				short dataFormat = s.readShortLE();
				int dataUsedLength = s.readIntLE();
				int dataTotalLength = s.readIntLE();

				int dataRelativeOffset = s.readIntLE();
				int dataOffset = dataTableStart + dataRelativeOffset;

				long originalPos = s.Position;

				s.Position = keyOffset;
				string key = s.readNullTerminatedString(Encoding.UTF8);

				s.Position = dataOffset;
				object value = null;
				switch (dataFormat) {
					case 0x0004: //utf8 special mode (not null terminated)
						value = s.read(dataUsedLength, Encoding.UTF8);
						break;
					case 0x0204: //utf8 (null terminated)
						value = s.readNullTerminatedString(Encoding.UTF8, dataUsedLength);
						break;
					case 0x0404: //int32
						value = s.readIntLE();
						break;
					default:
						value = String.Format("Unknown format!!! 0x{0:X2}", dataFormat);
						break;
				}
				if (!d.ContainsKey(key)) {
					d.Add(key, value);
				}
				//I guess I should report if there's a duplicate key but that shouldn't happen

				s.Position = originalPos;
			}

			return d;
		}

		public static bool isSFOMagic(byte[] magic) {
			//\0PSF in ASCII
			return magic[0] == 0x00 && magic[1] == 0x50 && magic[2] == 0x53 && magic[3] == 0x46;
		}

		public static readonly Dictionary<string, string> PSP_GAME_CATEGORIES = new Dictionary<string, string>() {
			{"EG", "PSP Remaster/minis"},
			{"MA", "Application"},
			{"ME", "PS1 Classic"},
			{"MG", "Memory Stick game"}, //Homebrew seems to always use this
			{"MS", "Memory Stick save"},
			{"UG", "UMD Disc game"},
			{"PG", "Update"},
		};

		public static void parseAttributeFlags(ROMInfo info, int flags) {
			//Looks like this is used for PS Vita and PS3 as well whoops
			//1 << 13 is unknown, has something to do with PS Now beta
			//1 << 14, 1 << 15 are unused
			//1 << 18 is unknown
			//1 << 24 and up have something to do with PC Engine but are unknown
			info.addInfo("Remote play v1", (flags & 1 << 0) > 0, true);
			info.addInfo("Copyable", (flags & 1 << 1) > 0, true);
			info.addInfo("Remote play v2", (flags & 1 << 2 ) > 0, true);
			info.addInfo("XMB In-game forced enabled", (flags & 1 << 3 ) > 0, true);
			info.addInfo("XMB In-game disabled", (flags & 1 << 4) > 0, true);
			info.addInfo("XMB In-game custom music enabled", (flags & 1 << 5) > 0, true);
			info.addInfo("Voice chat", (flags & 1 << 6) > 0, true);
			info.addInfo("PS Vita remote play", (flags & 1 << 7) > 0, true);

			//Are these actually used on PSP?
			info.addInfo("Move Controller warning", (flags & 1 << 8) > 0, true);
			info.addInfo("Navigation Controller warning", (flags & 1 << 9) > 0, true);
			info.addInfo("PlayStation Eye Cam warning", (flags & 1 << 10) > 0, true);
			info.addInfo("Move Controller needs calibration notification", (flags & 1 << 11) > 0, true);
			info.addInfo("Stereoscopic 3D warning", (flags & 1 << 12) > 0, true);

			//Yeah these don't sound right
			info.addInfo("Install disc", (flags & 1 << 16) > 0, true);
			info.addInfo("Install packages", (flags & 1 << 17) > 0, true);
			info.addInfo("Game purchase enabled", (flags & 1 << 19) > 0, true);
			info.addInfo("Disable About this Game", (flags & 1 << 20) > 0, true);
			info.addInfo("PC Engine", (flags & 1 << 21) > 0, true);
			info.addInfo("License disabled", (flags & 1 << 22) > 0, true);
			info.addInfo("Move controller enabled", (flags & 1 << 23) > 0, true);
		}

		public static void parseParamSFO(ROMInfo info, WrappedInputStream s) {
			byte[] magic = s.read(4);
			if (!isSFOMagic(magic)) {
				return;
			}

			var dict = convertParamSFO(s);
			foreach(var kv in dict) {
				switch (kv.Key) {
					case "ACCOUNT_ID":
						info.addInfo("Account ID", kv.Value);
						break;
					case "ANALOG_MODE":
						info.addInfo("Analog mode enabled", (int)kv.Value == 1);
						break;
					case "APP_VER":
						info.addInfo("App version", kv.Value);
						break;
					case "DISC_VERSION":
						info.addInfo("Version", kv.Value);
						break;
					case "ATTRIBUTE":
						parseAttributeFlags(info, (int)kv.Value);
						break;
					case "BOOTABLE":
						info.addInfo("Bootable", (int)kv.Value == 1);
						break;
					case "CATEGORY":
						info.addInfo("Type", (string)kv.Value, PSP_GAME_CATEGORIES);
						break;
					case "DISC_ID":
						info.addInfo("Product code", kv.Value);
						break;
					case "DISC_NUMBER":
						info.addInfo("Disc number", kv.Value);
						break;
					case "DISC_TOTAL":
						info.addInfo("Number of discs", kv.Value);
						break;
					case "MEMSIZE":
						info.addInfo("Use extra RAM", (int)kv.Value == 1);
						break;
					case "PARENTAL_LEVEL":
						info.addInfo("Parental controls level", kv.Value);
						break;
					case "PSP_SYSTEM_VER":
						info.addInfo("Required firmware version", kv.Value);
						break;
					case "REGION":
						//TODO Parse (it's a bitmask of allowed regions somehow)
						info.addInfo("Region info", kv.Value, true);
						break;
					case "TITLE":
						info.addInfo("Banner title", kv.Value);
						break;
					case "USE_USB":
						info.addInfo("Use USB", (int)kv.Value == 1);
						break;
					default:
						info.addInfo("PARAM.SFO: " + kv.Key, kv.Value);
						break;
					//TODO There are probably a lot more that are used in full .iso games, but I'm not reading those yet
				}
			}
		}

		public static void parsePBP(ROMInfo info, WrappedInputStream s) {
			byte[] magic = s.read(4);
			info.addInfo("Magic", magic, true); //Should be "\0PBP", or maybe "PBP\0" because endians confuse me
			if(isELFMagic(magic)) {
				info.addInfo("Detected format", "ELF");
				//There will not be anything to see here
				return;
			} else if (!isPBPMagic(magic)) {
				info.addInfo("Detected format", "Unknown");
				return;
			}
			info.addInfo("Detected format", "PBP");


			byte[] unknown = s.read(4); //This is speculated to be some kind of version number but I dunno
			info.addInfo("Unknown", unknown, true);

			//The files embedded here are supposedly always in this order, so you get the size by getting the difference between that file's offset and the next one (or the end of the file if it's the last one)

			int paramOffset = s.readIntLE(); //Apparently should always be 0x28
			info.addInfo("PARAM.SFO offset", paramOffset, ROMInfo.FormatMode.HEX, true);

			int icon0Offset = s.readIntLE();
			info.addInfo("ICON0.PNG offset", icon0Offset, ROMInfo.FormatMode.HEX, true);

			int icon1Offset = s.readIntLE();
			info.addInfo("ICON1.PNG offset", icon1Offset, ROMInfo.FormatMode.HEX, true);

			int pic0Offset = s.readIntLE();
			info.addInfo("PIC0.PNG offset", pic0Offset, ROMInfo.FormatMode.HEX, true);

			int pic1Offset = s.readIntLE();
			info.addInfo("PIC1.PNG offset", pic1Offset, ROMInfo.FormatMode.HEX, true);

			int sndOffset = s.readIntLE();
			info.addInfo("SND0.AT3 offset", sndOffset, ROMInfo.FormatMode.HEX, true);

			int dataPSPOffset = s.readIntLE();
			info.addInfo("DATA.PSP offset", dataPSPOffset, ROMInfo.FormatMode.HEX, true);

			int dataPSAROffset = s.readIntLE();
			info.addInfo("DATA.PSAR offset", dataPSAROffset, ROMInfo.FormatMode.HEX, true);

			if (paramOffset > 0x24) {
				int paramSize = icon0Offset - paramOffset;
				if (paramSize > 0) {
					s.Position = paramOffset;
					byte[] param = s.read(paramSize);

					using (WrappedInputStream mem = new WrappedInputStream(new MemoryStream(param))) {
						parseParamSFO(info, mem);
					}
				}
			}

			if (icon0Offset > paramOffset) {
				int icon0Size = icon1Offset - icon0Offset;
				if (icon0Size > 0) {
					s.Position = icon0Offset;
					byte[] icon0 = s.read(icon0Size);

					using (MemoryStream mem = new MemoryStream(icon0)) {
						info.addInfo("Icon", System.Drawing.Image.FromStream(mem));
					}
				}
			}

			if (icon1Offset > icon0Offset) {
				int icon1Size = pic0Offset - icon1Offset;
				if (icon1Size > 0) {
					s.Position = icon1Offset;
					byte[] icon0 = s.read(icon1Size);
					//TODO: Detect if PSMF which is some kind of animated icon (I think the magic number is either PSMF00 or PAMF00 at the beginning but not sure)

					using (MemoryStream mem = new MemoryStream(icon0)) {
						info.addInfo("Icon 2", System.Drawing.Image.FromStream(mem));
					}
				}
			}

			if (pic0Offset > icon1Offset) {
				int pic0Size = pic1Offset - pic0Offset;
				if (pic0Size > 0) {
					s.Position = pic0Offset;
					byte[] pic0 = s.read(pic0Size);

					using (MemoryStream mem = new MemoryStream(pic0)) {
						//Added as extra info because these can be up to 480x272, and that would be kind of shit to display in a table
						info.addInfo("Information image", System.Drawing.Image.FromStream(mem), true);
					}
				}
			}

			if (pic1Offset > pic0Offset) {
				int pic1Size = sndOffset - pic1Offset;
				if (pic1Size > 0) {
					s.Position = pic1Offset;
					byte[] pic1 = s.read(pic1Size);

					using (MemoryStream mem = new MemoryStream(pic1)) {
						info.addInfo("Background image", System.Drawing.Image.FromStream(mem), true);
					}
				}
			}

			//TODO Get the sound... maybe the individual file info can show a "Sounds" button which plays the thing

		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "PlayStation Portable");
			if("pbp".Equals(file.extension)) {
				parsePBP(info, file.stream);
			}
			//.iso will be done later
		}
	}
}
