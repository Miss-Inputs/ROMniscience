using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	class Switch : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string> {
			{"nro", "Nintendo Switch NRO"},
			{"nso", "Nintendo Switch NSO"},
		};

		public override string name => "Nintendo Switch";

		private void addNROInfo(ROMInfo info, ROMFile file) {
			var s = file.stream;
			s.Seek(16, SeekOrigin.Begin);
			var magic = s.read(4, Encoding.ASCII);
			info.addInfo("Magic", magic);
			if (!("NRO0".Equals(magic))) {
				return;
			}
			int nroFormatVersion = s.readIntLE(); //Always 0
			info.addInfo("NRO format version", nroFormatVersion, true);
			int totalSize = s.readIntLE();
			info.addInfo("ROM size", totalSize, ROMInfo.FormatMode.SIZE);
			//Skip over flags (unused), segmentHeader[3] (text, ro, data), bssSize, reserved (unused)
			s.Seek(4 + (8 * 3) + 4 + 4, SeekOrigin.Current);
			var buildID = s.read(32);
			info.addInfo("Build ID", buildID);
			info.addInfo("Build ID as ASCII", Encoding.ASCII.GetString(buildID));
			//Skip over reserved 2 and segmentHeader2[3] (apiInfo, dynstr, dynsym)
			//s.Seek(8 + (8 * 3), SeekOrigin.Current);

			s.Seek(totalSize, SeekOrigin.Begin);
			var assetMagic = s.read(4, Encoding.ASCII);
			if ("ASET".Equals(assetMagic)) {
				long assetSectionStart = s.Position - 4;
				info.addInfo("Asset section offset", assetSectionStart, ROMInfo.FormatMode.HEX);
				int assetFormatVersion = s.readIntLE();
				info.addInfo("Asset format version", assetFormatVersion, true);
				ulong iconOffset = (ulong)(s.readIntLE() | (s.readIntLE() << 4));
				info.addInfo("Icon offset", iconOffset, ROMInfo.FormatMode.HEX);
				ulong iconSize = (ulong)(s.readIntLE() | (s.readIntLE() << 4));
				info.addInfo("Icon size", iconSize, ROMInfo.FormatMode.SIZE);
				ulong nacpOffset = (ulong)(s.readIntLE() | (s.readIntLE() << 4));
				info.addInfo(".nacp offset", nacpOffset, ROMInfo.FormatMode.HEX);
				ulong nacpSize = (ulong)(s.readIntLE() | (s.readIntLE() << 4));
				info.addInfo(".nacp size", nacpSize, ROMInfo.FormatMode.SIZE);
				ulong romfsOffset = (ulong)(s.readIntLE() | (s.readIntLE() << 4));
				info.addInfo("RomFS offset", romfsOffset, ROMInfo.FormatMode.HEX);
				ulong romfsSize = (ulong)(s.readIntLE() | (s.readIntLE() << 4));
				info.addInfo("RomFS size", romfsSize, ROMInfo.FormatMode.SIZE);

				if(iconSize > 0) {
					s.Seek((long)((ulong)assetSectionStart + iconOffset), SeekOrigin.Begin);
					byte[] icon = s.read((int)iconSize);
					using (MemoryStream mem = new MemoryStream(icon)) {
						info.addInfo("Icon", System.Drawing.Image.FromStream(mem));
					}
				}
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			if ("nro".Equals(file.extension)) {
				addNROInfo(info, file);
			}
		}
	}
}
