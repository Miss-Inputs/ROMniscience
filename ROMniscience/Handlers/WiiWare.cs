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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	//http://wiibrew.org/wiki/WAD_files
	class WiiWare : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
			{"wad", "WiiWare/Wii System Menu WAD"}
		};

		public override string name => "WiiWare";

		static int roundUpToMultiple(int n, int f) {
			int m = n % f;
			int r = n - m;
			if(m > (f / 2)) {
				r += f;
			}
			return r;
		}

		public static void parseTMD(ROMInfo info, byte[] tmd) {
			InputStream s = new WrappedInputStream(new System.IO.MemoryStream(tmd));

			s.Position = 0x184;
			byte[] iosVersion = s.read(8);
			info.addInfo("IOS version", iosVersion, true);

			byte[] titleID = s.read(4);
			//The one and only documentation says this is 8 bytes, but that doesn't seem to work as there's 4 weird bytes at the front
			//This might be the type actually (game = 0x10000, channel = 0x10001, etc)
			info.addInfo("Title ID unknown", titleID, true);

			string productCode = s.read(4, Encoding.ASCII);
			info.addInfo("Product code", productCode);
			char gameType = productCode[0];
			info.addInfo("Type", gameType, NintendoCommon.DISC_TYPES);
			string shortTitle = productCode.Substring(1, 2);
			info.addInfo("Short title", shortTitle);
			char region = productCode[3];
			info.addInfo("Region", region, NintendoCommon.REGIONS);

			int titleFlags = s.readIntBE();
			info.addInfo("Title flags", titleFlags, true);
			info.addInfo("Is official", (titleFlags & 1) == 1); //Hmmmmmm

			string maker = s.read(2, Encoding.ASCII); //Documentation calls this "Group ID" for some reason
			info.addInfo("Manufacturer", maker, NintendoCommon.LICENSEE_CODES);

			byte[] unused = s.read(2);
			info.addInfo("Unused", unused, true);

			int regionCode = s.readShortBE();
			info.addInfo("Region code", regionCode, NintendoCommon.DISC_REGIONS);

			byte[] ratings = s.read(16);
			Wii.parseRatings(info, ratings);

			byte[] reserved = s.read(12);
			info.addInfo("TMD reserved", reserved, true);

			byte[] ipcMask = s.read(12); //The heck is that
			info.addInfo("IPC mask", ipcMask, true);

			byte[] reserved2 = s.read(18);
			info.addInfo("TMD reserved 2", reserved2, true);

			byte[] accessRights = s.read(4);
			info.addInfo("Access rights", accessRights, true); //How do I interpret these?

			int version = s.readShortBE();
			info.addInfo("Version", version);
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", name);

			InputStream s = file.stream;

			int headerSize = s.readIntBE();
			info.addInfo("Header size", headerSize, ROMInfo.FormatMode.SIZE, true);

			byte[] wadType = s.read(4);
			info.addInfo("WAD type", wadType, true); //Can be either Is, ib or Bk with by two nulls, but what does it mean?
			info.addInfo("WAD type ASCII", Encoding.ASCII.GetString(wadType), true);

			int certChainSize = s.readIntBE();
			info.addInfo("Certificate chain size", certChainSize, true);

			byte[] reserved = s.read(4);
			info.addInfo("Reserved", reserved);

			int ticketSize = s.readIntBE();
			info.addInfo("Ticket size", ticketSize, ROMInfo.FormatMode.SIZE, true);

			int tmdSize = s.readIntBE();
			info.addInfo("TMD size", tmdSize, ROMInfo.FormatMode.SIZE, true);

			int dataSize = s.readIntBE();
			info.addInfo("Data size", dataSize, ROMInfo.FormatMode.SIZE, true);

			int footerSize = s.readIntBE();
			info.addInfo("Footer size", footerSize, ROMInfo.FormatMode.SIZE, true);

			s.Position = 0x40; //All blocks are stored in the order of their sizes in the header, and aligned to 0x40 bytes, we've just read the header of course

			//TODO: Read certificate chain and ticket
			s.Seek(roundUpToMultiple(certChainSize, 0x40), System.IO.SeekOrigin.Current);
			s.Seek(roundUpToMultiple(ticketSize, 0x40), System.IO.SeekOrigin.Current);

			byte[] tmd = s.read(roundUpToMultiple(tmdSize, 0x40));
			parseTMD(info, tmd);
		}
	}
}
