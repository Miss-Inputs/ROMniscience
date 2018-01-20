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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ROMniscience {
	static class CSVWriter {
		public static void writeCSV(System.Windows.Forms.DataGridView table, FileInfo filename) {
			string[] headers = new string[table.Columns.Count];
			for(var i = 0; i < headers.Length; ++i) {
				headers[i] = table.Columns[i].Name;
			}

			string[,] data = new string[table.Rows.Count, headers.Length];
			for(int i = 0; i < table.Rows.Count; ++i) {
				for(int j = 0; j < headers.Length; ++j) {
					object value = table[j, i].Value;
					if(value is byte[] bytes) {
						data[i, j] = BitConverter.ToString(bytes);
					} else if(value is string[] strings) {
						data[i, j] = String.Join(", ", strings);
					} else {
						data[i, j] = value?.ToString();
					}
				}
			}

			writeTable(filename, headers, data);
		}

		public static void writeTable(FileInfo path, string[] headers, string[,] data) {
			using(StreamWriter sw = new StreamWriter(path.FullName, false, Encoding.UTF8)) {
				writeTable(sw, headers, data);
			}
		}

		public static void writeTable(StreamWriter sw, string[] headers, string[,] data) {
			foreach(string header in headers.Take(headers.Length - 1)) {
				writeValue(sw, header, false);
			}
			writeValue(sw, headers.Last(), true);

			for(int i = 0; i < data.GetLength(0) - 1; ++i) {
				for(int j = 0; j < data.GetLength(1) - 1; ++j) {
					writeValue(sw, data[i, j], false);
				}
				writeValue(sw, data[i, data.GetUpperBound(1)], true);
			}
		}

		static readonly Regex CONTROL_CHARS = new Regex(@"[\x00-\x09\x0b\x0c\x0e-\x1f]");
		private static void writeValue(StreamWriter sw, string value, bool final) {
			if(value != null) {
				value = value.Replace("\"", "\"\"");
				value = CONTROL_CHARS.Replace(value, String.Empty); //LibreOffice does not like the output otherwise
				sw.Write('"' + value + '"');
			}
			if(final) {
				sw.WriteLine();
			} else {
				sw.Write(",");
			}
		}
	}
}
