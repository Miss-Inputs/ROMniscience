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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ROMniscience.Datfiles;
using ROMniscience.Handlers;

namespace ROMniscience {
	class ROMInfo {
		public enum FormatMode {
			NONE,
			SIZE,
			PERCENT,
			HEX,
		}

		public static string formatByteSize(long bytes) {
			if (bytes < 1000) {
				return String.Format("{0} bytes", bytes);
			}
			return String.Format("{0} / {1}", formatByteSize(bytes, true), formatByteSize(bytes, false));
		}

		public static string formatByteSize(long bytes, bool isMetric) {
			int baseUnit = isMetric ? 1000 : 1024;
			//FIXME 4GB bytes is formatted as 0 bytes
			if (bytes < baseUnit) {
				return String.Format("{0} bytes", bytes);
			}
			
			int exp = (int)(Math.Log(bytes) / Math.Log(baseUnit));
			char suffix = "KMGTPE"[exp - 1];

			string unit = isMetric ? "B" : "iB";
			return String.Format("{0:0.##} {1}{2}", bytes / Math.Pow(baseUnit, exp), suffix, unit);
		}


		public static ROMInfo getROMInfo(Handler handler, ROMFile rom) {
			return getROMInfo(handler, rom, null);
		}

		static void addDatfileResults(IList<XMLDatfile.IdentifyResult> results, ROMInfo info) {
			int i = 1;
			foreach (var result in results) {
				string prefix = i == 1 ? "Datfile" : String.Format("Datfile {0}", i);
				info.addInfo(prefix, result?.datfile.name);
				info.addInfo(prefix + " game name", result?.game.name);
				info.addInfo(prefix + " game category", result?.game.category);
				info.addInfo(prefix + " game description", result?.game.description, true); //Generally the same as the name
				info.addInfo(prefix + " ROM name", result?.rom.name);
				info.addInfo(prefix + " ROM status", result?.rom.status);
				if (result != null) {
					//Lowkey hate that I can't just do result? here
					//Anyway, if there's a match, then this should just be equal to the file size anyway; if not, you know some hash collision shit occured
					info.addInfo(prefix + " ROM size", result.rom.size, FormatMode.SIZE, true);
				}
				++i;
			}
		}

		public static ROMInfo getROMInfo(Handler handler, ROMFile rom, DatfileCollection datfiles) {
			ROMInfo info = new ROMInfo();
			try {
				info.addInfo("Filename", rom.path.Name);
				info.addInfo("Folder", rom.path.DirectoryName);
				info.addInfo("Size", rom.length, FormatMode.SIZE);

				if (rom.compressed) {
					info.addInfo("Uncompressed filename", rom.name);
					info.addInfo("Compressed size", rom.compressedLength, FormatMode.SIZE);
					info.addInfo("Compression ratio", 1 - ((double)rom.compressedLength / rom.length), FormatMode.PERCENT);
				}

				string extension = rom.extension;
				string fileType = handler.getFiletypeName(extension);
				info.addInfo("File type", fileType ?? "Unknown");

				if (handler.shouldCalculateHash) {
					var hashes = DatfileCollection.hash(rom.stream, handler.shouldSkipHeader(rom) ? handler.skipHeaderBytes() : 0);
					info.addInfo("CRC32", hashes.Item1, FormatMode.HEX);
					info.addInfo("MD5", hashes.Item2);
					info.addInfo("SHA-1", hashes.Item3);

					if (datfiles != null) {
						var results = datfiles.identify(hashes.Item1, hashes.Item2, hashes.Item3);
						addDatfileResults(results, info);
					}
				}

				handler.addROMInfo(info, rom);
			} catch (Exception e) {
				info.addInfo("Exception", e);
			}
			return info;
		}

		public struct InfoItem {
			public object value;
			public FormatMode formatMode;
			public bool extra; //For stuff like reserved/unknown fields that just take up too much space in table views usually

			public InfoItem(object value) : this(value, FormatMode.NONE) { }

			public InfoItem(object value, FormatMode formatMode) : this(value, formatMode, false) { }

			public InfoItem(object value, FormatMode formatMode, bool extra) {
				this.value = value;
				this.formatMode = formatMode;
				this.extra = extra;
			}
		}

		private readonly IDictionary<string, InfoItem> _info = new Dictionary<string, InfoItem>();
		public IDictionary<string, InfoItem> info => _info;

		public bool hasInfoAlreadyBeenAdded(string key) {
			return info.ContainsKey(key);
		}

		public void addInfo(string key, object value) {
			addInfo(key, value, FormatMode.NONE);
		}

		public void addInfo<K, V>(string key, K value, IDictionary<K, V> dict) {
			addInfo(key, value, dict, false);
		}

		public void addInfo<K, V>(string key, K value, IDictionary<K, V> dict, bool extra) {
			addInfo(key, value, dict, FormatMode.NONE, extra);
		}

		public void addInfo<K>(string key, K[] value, IDictionary<K, string> dict) {
			if (value.Length == 1) {
				addInfo(key, value[0], dict);
				return;
			}

			string[] stuff = new string[value.Length];
			for (var i = 0; i < value.Length; ++i) {
				if (dict.TryGetValue(value[i], out string v)) {
					stuff[i] = v;
				} else {
					stuff[i] = String.Format("Unknown ({0})", value[i]);
				}
			}
			addInfo(key, stuff);
		}

		public void addInfo<K, V>(string key, K value, IDictionary<K, V> dict, FormatMode format, bool extra) {
			if (dict.TryGetValue(value, out V v)) {
				addInfo(key, v, format, extra);
			} else {
				addInfo(key, String.Format("Unknown ({0})", value), extra);
			}
		}

		public void addInfo<K, V>(string key, K value, IDictionary<K, V> dict, FormatMode format) {
			addInfo(key, value, dict, format, false);
		}

		public void addInfo(string key, object value, bool extra) {
			addInfo(key, value, FormatMode.NONE, extra);
		}

		public void addInfo(string key, object value, FormatMode format) {
			addInfo(key, value, format, false);
		}

		public void addInfo(string key, object value, FormatMode format, bool extra) {
			info.Add(key, new InfoItem(value, format, extra));
		}

	}
}
