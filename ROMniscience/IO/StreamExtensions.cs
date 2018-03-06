using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.IO {
	static class StreamExtensions {
		public static int read(this Stream s) => s.ReadByte();

		public static byte[] read(this Stream s, int bytes) {
			byte[] buf = new byte[bytes];
			int bytesRead = s.Read(buf, 0, bytes);
			if (bytesRead == 0) {
				return new byte[] { };
			} else if (bytesRead == bytes) {
				return buf;
			} else {
				byte[] buf2 = new byte[bytesRead];
				Array.Copy(buf, buf2, bytesRead);
				return buf2;
			}
		}

		public static String read(this Stream s, int length, Encoding encoding) {
			return encoding.GetString(s.read(length));
		}

		public static int readIntBE(this Stream s) {
			return (s.read() << 24) | (s.read() << 16) | (s.read() << 8) | s.read();
		}

		public static int readIntLE(this Stream s) {
			return s.read() | (s.read() << 8) | (s.read() << 16) | (s.read() << 24);
		}

		public static int readShortBE(this Stream s) {
			return (s.read() << 8) | s.read();
		}

		public static int readShortLE(this Stream s) {
			return s.read() | (s.read() << 8);
		}

		public static Stream cloneInputStream(Stream s) {
			return new MemoryStream(s.read((int)s.Length));
		}
	}
}
