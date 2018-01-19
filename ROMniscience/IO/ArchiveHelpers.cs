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
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using System.IO;

namespace ROMniscience.IO {
	static class ArchiveHelpers {

		public static readonly IList<string> ARCHIVE_EXTENSIONS = new List<string>{
			"7z",
			"zip",
			"gz",
			"tar",
			"rar", //bah
		};
		public static bool isArchiveExtension(String extension) {
			if(String.IsNullOrEmpty(extension)) {
				return false;
			}

			if(extension[0] == '.') {
				return ARCHIVE_EXTENSIONS.Contains(extension.Substring(1).ToLowerInvariant());
			}
			return ARCHIVE_EXTENSIONS.Contains(extension.ToLowerInvariant());
		}

		public static bool isArchive(FileInfo file) {
			//FIXME This is broken and detects .gba ROMs as stuff
			return GZipArchive.IsGZipFile(file) || RarArchive.IsRarFile(file) || SevenZipArchive.IsSevenZipFile(file) ||
				TarArchive.IsTarFile(file) || ZipArchive.IsZipFile(file);
		}
	}
}
