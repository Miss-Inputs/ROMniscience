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
using ROMniscience.Handlers;

namespace ROMniscience {
	class ROMInfo {
		public enum FormatMode {
			NONE,
			SIZE,
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

		//TODO Use .dat files (No-Intro, Redump, etc)
		public static ROMInfo getROMInfo(Handler handler, ROMFile rom) {
			//TODO Error handling
			ROMInfo info = new ROMInfo();
			try {
				info.addInfo("Filename", rom.path.Name);
				info.addInfo("Folder", rom.path.DirectoryName);
				info.addSizeInfo("Size", rom.length);
				//TODO Uncompressed filename, compressed size, compression ratio

				string extension = rom.extension;
				string fileType = handler.getFiletypeName(extension);
				info.addInfo("File type", fileType ?? "Unknown");

				//TODO Datfile stuff here

				handler.addROMInfo(info, extension, rom);
			} catch (Exception e) {
				info.addInfo("Exception", e);
			}
			return info;
		}

		public IDictionary<string, Tuple<object, FormatMode>> info => _info;
		public IDictionary<string, Tuple<object, FormatMode>> extraInfo => _extraInfo;

		//TODO Refactor to avoid duplication

		public void addInfo(string key, object value) {
			info.Add(key, new Tuple<object, FormatMode>(value, FormatMode.NONE));
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
				if(dict.TryGetValue(value[i], out string v)){
					stuff[i] = v;
				} else {
					stuff[i] = String.Format("Unknown ({0})", value[i]);
				}
			}
			addInfo(key, stuff);
		}

		public void addSizeInfo(string key, long value) {
			info.Add(key, new Tuple<object, FormatMode>(value, FormatMode.SIZE));
		}

		public void addSizeInfo<K>(string key, K value, IDictionary<K, long> dict) {
			if(dict.TryGetValue(value, out long v)) {
				addSizeInfo(key, v);
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
				if(dict.TryGetValue(value[i], out string v)){
					stuff[i] = v;
				} else {
					stuff[i] = String.Format("Unknown ({0})", value[i]);
				}
			}
			addExtraInfo(key, stuff);
		}

		public void addExtraSizeInfo(string key, long value) {
			extraInfo.Add(key, new Tuple<object, FormatMode>(value, FormatMode.SIZE));
		}

		public void addExtraSizeInfo<K>(string key, K value, IDictionary<K, long> dict) {
			if(dict.TryGetValue(value, out long v)) {
				addExtraSizeInfo(key, v);
			} else {
				addExtraInfo(key, String.Format("Unknown ({0})", value));
			}
		}
	}
}
