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
using SharpCompress.Compressors.Deflate;

namespace ROMniscience.IO {
	class GCZInputStream : WrappedInputStream {
		//Header:
		//00 - 03: Magic (actually 01 c0 0b b1, not b1 0b b1 0b)
		//04 - 07: Type (big endian int), 0 = GC, 1 = Wii; or maybe it's little endian int 0x1000 = Wii
		//08 - 15: Compressed size (little endian ulong); doesn't seem to include this header or anything like that
		//16 - 23: Uncompressed size (little endian ulong) (note with this and above, you should be fine to cast it to signed long if you need to, unless you have a Gamecube/Wii disc bigger than 16 exabytes)
		//24 - 27: Block size (little endian uint)
		//28 - 31: Number of blocks (little endian uint)
		//32 - (numBlocks * 64): Block pointers (little endian ulong, high bit indicates if compressed)
		//(32 + (numBlocks * 64)) - (32 + (numBlocks * 64) + (numBlocks * 32)): Hashes (little endian uint)

		public override long Position {
			get => position;
			set => position = value;
		}

		public override int Read(byte[] buffer, int offset, int count) {

			if((ulong)position >= uncompressedSize) {
				return 0;
			}

			int firstBlock = (int)(position / blockSize);
			long end = position + count;
			int blocksToRead = (int)(((end - 1) / blockSize) + 1) - firstBlock;
			int bytesRead = 0;
			int remaining = count;

			for (int i = firstBlock; i < firstBlock + blocksToRead; ++i) {
				int positionInBlock = (int)(position - (i * blockSize));
				int bytesToRead = (int)(blockSize - positionInBlock);
				if (bytesToRead > remaining) {
					bytesToRead = remaining;
				}

				byte[] block = getBlock(i);
				Array.Copy(block, positionInBlock, buffer, bytesRead, bytesToRead);

				position += bytesToRead;
				bytesRead += bytesToRead;
				remaining -= bytesToRead;
				if ((ulong)position >= uncompressedSize) {
					break;
				}
			}

			return bytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin) {
			if(origin == SeekOrigin.Begin) {
				position = offset;
			} else if(origin == SeekOrigin.Current) {
				position += offset;
			} else if(origin == SeekOrigin.End) {
				position = (long)uncompressedSize + offset;
			}
			return position;
		}

		uint blockSize;
		uint numBlocks;
		ulong[] blockPointers;
		uint[] hashes;
		ulong dataOffset;
		long position;

		public GCZInputStream(Stream s) : base(s) {
			blockSize = getBlockSize();
			numBlocks = getNumberOfBlocks();

			compressedSize = getCompressedSize();
			uncompressedSize = getUncompressedSize();

			blockPointers = new ulong[numBlocks];
			hashes = new uint[numBlocks];
			dataOffset = 32 + (8 * numBlocks) + (4 * numBlocks);

			readBlockPointers();
			position = 0;
		}

		ulong getCompressedBlockSize(int blockNum) {
			ulong start = blockPointers[blockNum];
			if (blockNum < numBlocks - 1) {
				return blockPointers[blockNum + 1] - start;
			} else {
				return compressedSize - start;
			}
		}

		static uint adlerCRC32(byte[] bytes) {
			//I thought .NET had this builtin, I guess not or maybe I'm dumb
			const uint MOD_ADLER = 65521;

			uint a = 1;
			uint b = 0;

			for(int i = 0; i < bytes.Length; ++i) {
				a = (a + bytes[i]) % MOD_ADLER;
				b = (b + a) % MOD_ADLER;
			};

			return (b << 16) | a;
		}

		byte[] getBlock(int blockNum) {
			bool compressed = true;
			//At this point Dolphin is confusing me, but I guess the block size is only 16K which is fine for an uint
			//I hope it's fine for an int too, because some methods in Stream only take signed ints...
			uint compressedBlockSize = (uint)getCompressedBlockSize(blockNum);
			ulong offset = dataOffset + blockPointers[blockNum];

			if((offset & (1UL << 63)) > 0){
				compressed = false;
				offset &= ~(1UL << 63);
			}

			innerStream.Position = (long)offset;

			byte[] buf = new byte[compressedBlockSize];
			int bytesRead = innerStream.Read(buf, 0, (int)compressedBlockSize);
			if(bytesRead == -1) {
				throw new Exception("Ah shit");
			}

			var expectedHash = hashes[blockNum];
			var actualHash = adlerCRC32(buf);
			if(expectedHash != actualHash) {
				Console.WriteLine("Oh no block {0} might be corrupted expected = {1} actual = {2}", blockNum, expectedHash, actualHash);
			}

			if (compressed) {
				var mem = new MemoryStream(buf);
				using (var inflator = new ZlibStream(mem, SharpCompress.Compressors.CompressionMode.Decompress)) {
					byte[] buf2 = new byte[blockSize];
					int bytesInflated = inflator.Read(buf2, 0, (int)blockSize);
					Array.Resize(ref buf2, bytesInflated);
					return buf2;
				}
			} else {
				return buf;
			}
		}

		void readBlockPointers() {
			long pos = innerStream.Position;
			try {
				innerStream.Position = 32;
				for(int i = 0; i < numBlocks; ++i) {
					blockPointers[i] = readInnerULongLE();
				}
				for(int i = 0; i < numBlocks; ++i) {
					hashes[i] = readInnerUIntLE();
				}
			} finally {
				innerStream.Position = pos;
			}
		}

		uint getNumberOfBlocks() {
			long pos = innerStream.Position;
			try {
				innerStream.Position = 28;
				return readInnerUIntLE();
			} finally {
				innerStream.Position = pos;
			}
		}

		uint getBlockSize() {
			long pos = innerStream.Position;
			try {
				innerStream.Position = 24;
				return readInnerUIntLE();
			} finally {
				innerStream.Position = pos;
			}
		}

		uint readInnerUIntLE() {
			byte[] b = new byte[4];
			innerStream.Read(b, 0, 4);
			return BitConverter.ToUInt32(b, 0);
		}

		ulong readInnerULongLE() {
			byte[] b = new byte[8];
			innerStream.Read(b, 0, 8);
			return BitConverter.ToUInt64(b, 0);
		}

		ulong getCompressedSize() {
			long pos = innerStream.Position;
			try {
				innerStream.Position = 8;
				return readInnerULongLE();
			} finally {
				innerStream.Position = pos;
			}
		}

		ulong getUncompressedSize() {
			long pos = innerStream.Position;
			try {
				innerStream.Position = 16;
				return readInnerULongLE();
			} finally {
				innerStream.Position = pos;
			}
		}

		public ulong compressedSize {
			get;
		}

		public ulong uncompressedSize {
			get;
		}

	}
}
