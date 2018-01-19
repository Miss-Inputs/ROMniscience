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
