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

namespace ROMniscience.IO {
	//Wraps a raw CD input stream (2352 byte sectors) and acts like a "cooked" CD (2048 byte sectors)
	//TODO: Work with sectors of other sizes (mode 2, XA, etc)
	class CDInputStream : WrappedInputStream {

		const int MODE1_SECTOR_SIZE = 2048; //Also the size for XA Mode 2 Form 1
		const int MODE2_SECTOR_SIZE = 2336; //PS1 discs (and PS2 CDs) use this
		const int XA_FORM2_SECTOR_SIZE = 2324;

		const int MODE1_HEADER_SIZE = 12 + 3 + 1; //Sync pattern + address + mode; Mode 2 uses this with no error correction data
		const int MODE1_FOOTER_SIZE = 4 + 8 + 276; //Error detection + reserved + error correction
		const int MODE1_RAW_SIZE = 2048 + MODE1_HEADER_SIZE + MODE1_FOOTER_SIZE;

		const int XA_FORM1_HEADER_SIZE = 12 + 3 + 1 + 8;
		const int XA_FORM1_FOOTER_SIZE = 4 + 276;
		const int XA_FORM2_HEADER_SIZE = XA_FORM1_HEADER_SIZE;
		const int XA_FORM2_FOOTER_SIZE = 4;

		long virtualPosition; //What position are we pretending to be at? Not innerStream's position
		long virtualLength; //What length would this stream be if it really did only have 2048 bytes per sector?
		
		public CDInputStream(Stream s) : base(s) {
			virtualPosition = 0;
			virtualLength = rawPositionToCookedPosition(innerStream.Length);
		}

		public override long Seek(long offset, SeekOrigin origin) {
			if (origin == SeekOrigin.Begin) {
				virtualPosition = offset;
			} else if (origin == SeekOrigin.Current) {
				virtualPosition += offset;
			} else if (origin == SeekOrigin.End) {
				virtualPosition = rawPositionToCookedPosition(Length) + offset;
			}
			return virtualPosition;
		}

		public override long Position {
			get => virtualPosition;
			set => virtualPosition = value;
		}

		public override int ReadByte() {
			innerStream.Position = cookedPositionToRawPosition(virtualPosition);
			int b = innerStream.ReadByte();
			virtualPosition += 1;
			return b;
		}

		public override int Read(byte[] buf, int offset, int count) {
			if (virtualPosition >= virtualLength) {
				return 0;
			}
			
			long end = (virtualPosition + count) - 1;
			long rawPos = cookedPositionToRawPosition(virtualPosition);
			long rawEnd = cookedPositionToRawPosition(end);
			long rawCount = (rawEnd - rawPos) + 1;

			int numberOfSectors = (int)((rawCount - count) / (MODE1_HEADER_SIZE + MODE1_FOOTER_SIZE) + 1); //It won't be bigger than an int, don't worry
			int bytesRead;

			//If we're reading through one sector and into another, things will get tricky
			//If we're just reading this one sector, it'll be fine
			if (numberOfSectors == 1) {
				innerStream.Position = rawPos;
				bytesRead = innerStream.Read(buf, offset, count);
				virtualPosition += bytesRead;
				return bytesRead;
			}

			int startSector = (int)(virtualPosition / MODE1_SECTOR_SIZE);
			int startOffsetInSector = (int)virtualPosition % MODE1_SECTOR_SIZE;
			int endSector = (int)(end / MODE1_SECTOR_SIZE);
			int endOffsetInSector = (int)end % MODE1_SECTOR_SIZE;

			//Read remainder of the start sector first
			innerStream.Position = rawPos;
			bytesRead = innerStream.Read(buf, offset, MODE1_SECTOR_SIZE - startOffsetInSector);

			//Read any sectors that might be between start and end sectors
			for(int i = 0; i < numberOfSectors - 2; ++i) {
				innerStream.Position = cookedPositionToRawPosition(MODE1_SECTOR_SIZE * (startSector + i + 1));
				bytesRead += innerStream.Read(buf, offset + bytesRead, MODE1_SECTOR_SIZE);
			}

			//Read as much of the end sector as requested
			innerStream.Position = cookedPositionToRawPosition(MODE1_SECTOR_SIZE * endSector);
			bytesRead += innerStream.Read(buf, offset + bytesRead, endOffsetInSector + 1);

			return bytesRead;
		}

		static long rawPositionToCookedPosition(long rawPos) {
			long sectorCount = rawPos / MODE1_RAW_SIZE;
			long headerBytes = MODE1_HEADER_SIZE * (sectorCount + 1);
			long footerBytes = MODE1_FOOTER_SIZE * sectorCount;
			return rawPos - headerBytes - footerBytes;
		}

		static long cookedPositionToRawPosition(long cookedPos) {
			long sectorCount = cookedPos / MODE1_SECTOR_SIZE;
			long headerBytes = MODE1_HEADER_SIZE * (sectorCount + 1);
			long footerBytes = MODE1_FOOTER_SIZE * sectorCount;
			return cookedPos + headerBytes + footerBytes;
		}
	}
}
