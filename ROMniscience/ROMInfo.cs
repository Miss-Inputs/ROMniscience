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
using ROMniscience.Datfiles;
using ROMniscience.Handlers;

namespace ROMniscience {
	class ROMInfo {
		public enum FormatMode {
			NONE,
			SIZE,
			PERCENT,
		}

		public static string formatByteSize(long bytes) {
			if(bytes < 1000) {
				return String.Format("{0} bytes", bytes);
			}
			return String.Format("{0} / {1}", formatByteSize(bytes, true), formatByteSize(bytes, false));
		}

		public static string formatByteSize(long bytes, bool isMetric) {
			int baseUnit = isMetric ? 1000 : 1024;

			if(bytes < baseUnit) {
				return String.Format("{0} bytes", bytes);
			}

			int exp = (int)(Math.Log(bytes) / Math.Log(baseUnit));
			char suffix = "KMGTPE"[exp - 1];

			string unit = isMetric ? "B" : "iB";
			return String.Format("{0:0.##} {1}{2}", bytes / Math.Pow(baseUnit, exp), suffix, unit);
		}

		private readonly IDictionary<string, Tuple<object, FormatMode>> _info = new Dictionary<string, Tuple<object, FormatMode>>();
		private readonly IDictionary<string, Tuple<object, FormatMode>> _extraInfo = new Dictionary<string, Tuple<object, FormatMode>>();

		public static ROMInfo getROMInfo(Handler handler, ROMFile rom) {
			return getROMInfo(handler, rom, null);
		}

		public static ROMInfo getROMInfo(Handler handler, ROMFile rom, DatfileCollection datfiles) {
			ROMInfo info = new ROMInfo();
			try {
				info.addInfo("Filename", rom.path.Name);
				info.addInfo("Folder", rom.path.DirectoryName);
				info.addInfo("Size", rom.length, ROMInfo.FormatMode.SIZE);

				if(rom.compressed) {
					info.addInfo("Uncompressed filename", rom.name);
					info.addInfo("Compressed size", rom.compressedLength, ROMInfo.FormatMode.SIZE);
					info.addInfo("Compression ratio", 1 - ((double)rom.compressedLength / rom.length), ROMInfo.FormatMode.PERCENT);
				}

				string extension = rom.extension;
				string fileType = handler.getFiletypeName(extension);
				info.addInfo("File type", fileType ?? "Unknown");

				if(datfiles != null) {
					XMLDatfile.IdentifyResult result = datfiles.identify(rom.stream);
					info.addInfo("Datfile", result?.datfile.name);
					info.addInfo("Datfile game name", result?.game.name);
					info.addInfo("Datfile game category", result?.game.category);
					info.addExtraInfo("Datfile game description", result?.game.description); //So far I haven't seen this not be the same as the name
					info.addInfo("Datfile ROM name", result?.rom.name);
					info.addInfo("Datfile ROM status", result?.rom.status);
					if(result != null) {
						//Lowkey hate that I can't just do result? here
						info.addExtraInfo("Datfile ROM size", result.rom.size, ROMInfo.FormatMode.SIZE);
					}
				}

				handler.addROMInfo(info, rom);
			} catch(Exception e) {
				info.addInfo("Exception", e);
			}
			return info;
		}

		public IDictionary<string, Tuple<object, FormatMode>> info => _info;
		public IDictionary<string, Tuple<object, FormatMode>> extraInfo => _extraInfo;

		//TODO Refactor to avoid duplication between addInfo overloads and addExtraInfo overloads

		public void addInfo(string key, object value) {
			//info.Add(key, new Tuple<object, FormatMode>(value, FormatMode.NONE));
			addInfo(key, value, FormatMode.NONE);
		}

		public void addInfo<K, V>(string key, K value, IDictionary<K, V> dict) {
			if(dict.TryGetValue(value, out V v)) {
				addInfo(key, v);
			} else {
				addInfo(key, String.Format("Unknown ({0})", value));
			}
		}

		public void addInfo<K>(string key, K[] value, IDictionary<K, string> dict) {
			if(value.Length == 1) {
				addInfo(key, value[0], dict);
				return;
			}

			string[] stuff = new string[value.Length];
			for(var i = 0; i < value.Length; ++i) {
				if(dict.TryGetValue(value[i], out string v)) {
					stuff[i] = v;
				} else {
					stuff[i] = String.Format("Unknown ({0})", value[i]);
				}
			}
			addInfo(key, stuff);
		}

		public void addInfo(string key, object value, FormatMode format) {
			info.Add(key, new Tuple<object, FormatMode>(value, format));
		}

		public void addInfo<K, V>(string key, K value, IDictionary<K, V> dict, FormatMode format) {
			if(dict.TryGetValue(value, out V v)) {
				addInfo(key, v, format);
			} else {
				addInfo(key, String.Format("Unknown ({0})", value));
			}
		}

		public void addExtraInfo(string key, object value) {
			extraInfo.Add(key, new Tuple<object, FormatMode>(value, FormatMode.NONE));
		}

		public void addExtraInfo<K, V>(string key, K value, IDictionary<K, V> dict) {
			if(dict.TryGetValue(value, out V v)) {
				addExtraInfo(key, v);
			} else {
				addExtraInfo(key, String.Format("Unknown ({0})", value));
			}
		}

		public void addExtraInfo<K>(string key, K[] value, IDictionary<K, string> dict) {
			if(value.Length == 1) {
				addExtraInfo(key, value[0], dict);
				return;
			}

			string[] stuff = new string[value.Length];
			for(var i = 0; i < value.Length; ++i) {
				if(dict.TryGetValue(value[i], out string v)) {
					stuff[i] = v;
				} else {
					stuff[i] = String.Format("Unknown ({0})", value[i]);
				}
			}
			addExtraInfo(key, stuff);
		}

		public void addExtraInfo(string key, object value, FormatMode format) {
			extraInfo.Add(key, new Tuple<object, FormatMode>(value, format));
		}

		public void addExtraInfo<K, V>(string key, K value, IDictionary<K, V> dict, FormatMode format) {
			if(dict.TryGetValue(value, out V v)) {
				addExtraInfo(key, v, format);
			} else {
				addExtraInfo(key, String.Format("Unknown ({0})", value));
			}
		}
	}
}
