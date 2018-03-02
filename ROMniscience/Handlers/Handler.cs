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
using System.IO;
using ROMniscience.Handlers.Stubs;

namespace ROMniscience.Handlers {
	abstract class Handler {
		public string getFiletypeName(string extension) {
			if(String.IsNullOrEmpty(extension)) {
				return "Unknown";
			}
			if(extension?[0] == '.') {
				return getFiletypeName(extension.Substring(1));
			}
			if (!filetypeMap.ContainsKey(extension.ToLowerInvariant())) {
				return null;
			}
			return filetypeMap[extension.ToLowerInvariant()];
		}

		public bool handlesExtension(string extension) {
			if(String.IsNullOrEmpty(extension)) {
				return false;
			}
			if(extension[0] == '.') {
				return filetypeMap.ContainsKey(extension.Substring(1).ToLowerInvariant());
			}
			return filetypeMap.ContainsKey(extension.ToLowerInvariant());
		}

		public abstract IDictionary<string, string> filetypeMap {
			get;
		}

		public abstract string name {
			get;
		}

		public DirectoryInfo folder {
			get {
				string setting = SettingsManager.readSetting(name);
				return setting == null ? null : new DirectoryInfo(setting);
			}
		}

		public bool configured => SettingsManager.doesKeyExist(name);

		public bool enabled {
			get {
				string enabledKey = name + "_enabled";
				if (SettingsManager.doesKeyExist(enabledKey)) {
					if(bool.TryParse(SettingsManager.readSetting(enabledKey), out bool result)) {
						return result;
					}
				}
				return true;
			}
		}

		public bool shouldCalculateHash {
			get {
				string enabledKey = name + "_hash";
				if (SettingsManager.doesKeyExist(enabledKey)) {
					if (bool.TryParse(SettingsManager.readSetting(enabledKey), out bool result)) {
						return result;
					}
				}
				return true;
			}
		}

		public abstract void addROMInfo(ROMInfo info, ROMFile file);

		public static ICollection<Handler> allHandlers {
			get {
				List<Handler> list = new List<Handler>();
				foreach(var type in System.Reflection.Assembly.GetCallingAssembly().GetTypes()) {
					if(type.IsSubclassOf(typeof(Handler)) && !type.IsAbstract) {
						Handler h = (Handler)Activator.CreateInstance(type);
						list.Add(h);
					}
				}
				return list;
			}
		}

		public virtual bool shouldSkipHeader(ROMFile rom) {
			return false;
		}

		public virtual int skipHeaderBytes() {
			//Override this with the amount of bytes this should skip
			return 0;
		}
	}
}
