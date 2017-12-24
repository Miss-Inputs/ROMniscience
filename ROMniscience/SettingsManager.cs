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
using System.Text.RegularExpressions;

namespace ROMniscience {
	//Yeah what up I used the word "Manager" at the end of the name that means I'm enterprise and shit
	static class SettingsManager {
		public static string configPath {
			get {
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				return Path.Combine(appData, "ROMniscience", "config.ini");
			}
		}

		public static Regex LINE_MATCHER = new Regex("(.+)=(.+)");

		private static void ensureConfigExists() {
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			Directory.CreateDirectory(Path.Combine(appData, "ROMniscience"));
			if(!File.Exists(configPath)) {
				File.CreateText(configPath).Dispose();
			}
		}

		public static IDictionary<string, string> readAllSettings() {
			ensureConfigExists();

			IDictionary<string, string> dict = new Dictionary<string, string>();
			using(StreamReader f = File.OpenText(configPath)) {
				string line;
				while((line = f.ReadLine()) != null) {
					Match match = LINE_MATCHER.Match(line);
					if(match.Success) {
						dict.Add(match.Groups[1].Value, match.Groups[2].Value);
					}
				}
			}
			return dict;
		}

		public static bool doesKeyExist(string key) {
			return readAllSettings().ContainsKey(key);
		}

		public static string readSetting(string key) {
			return readSetting(key, null);
		}

		public static string readSetting(string key, string def) {
			var stuff = readAllSettings();
			return stuff.ContainsKey(key) ? stuff[key] : def;
		}

		public static void writeSettings(IDictionary<string, string> settings) {
			ensureConfigExists();

			string[] lines = File.ReadAllLines(configPath, Encoding.UTF8);
			IDictionary<string, string> existingSettings = new Dictionary<string, string>();
			foreach(string line in lines) {
				Match match = LINE_MATCHER.Match(line);
				if(match.Success) {
					existingSettings.Add(match.Groups[1].Value, match.Groups[2].Value);
				}
			}

			foreach(var setting in settings) {
				if(setting.Value == null){
					existingSettings.Remove(setting.Key);
				} else if(existingSettings.ContainsKey(setting.Key)) {
					existingSettings[setting.Key] = setting.Value;
				} else {
					existingSettings.Add(setting);
				}
			}

			using(StreamWriter sw = File.CreateText(configPath)) {
				foreach(var setting in existingSettings) {
					sw.WriteLine("{0}={1}", setting.Key, setting.Value);
				}
			}
		}

		public static void writeSetting(string key, string value) {
			writeSettings(new Dictionary<string, string>() { { key, value } });
		}
	}
}
