using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ROMniscience.Handlers {
	class WiiU : Handler {
		public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>{
			{"rpx", "Wii U executable"}
		};

		public override string name => "Wii U";

		static readonly Dictionary<string, string> imagePaths = new Dictionary<string, string> {
			{"../meta/IconText.tga", "Icon"},
			{"../meta/bootDrcTex.tga", "Gamepad boot screen"},
			{"../meta/bootLogoTex.tga", "Logo"},
			{"../meta/bootTvTex.tga", "TV boot screen"},

		};

		//CATEGORIES and GAME_TYPES are just what I know for now, don't think that it's all there is
		public static readonly IDictionary<char, string> CATEGORIES = new Dictionary<char, string> {
			{'T', "Trial"}, //eShop demo
		};

		public static readonly IDictionary<char, string> GAME_TYPES = new Dictionary<char, string> {
			{'D', "DS Virtual Console"}, 
			{'F', "NES Virtual Cosnole"},
			{'H', "Downloadable channel"},
			{'J', "SNES Virtual Console"},
			{'N', "N64 Virtual Console"}, 
			{'P', "GBA or PC Engine Virtual Console"},
		};

		[Flags]
		public enum RegionFlags : uint {
			Japan = 1 << 0,
			USA = 1 << 1,
			Europe = 1 << 2,
			Australia = 1 << 3, //Ends up going unused (PAL games have this region but it isn't checked), as the European 3DS ends up being sold here
			China = 1 << 4,
			Korea = 1 << 5,
			Taiwan = 1 << 6,
		}

		public static void addRPXInfo(ROMInfo info, ROMFile file) {
			//Mmmmm not sure I like this usage of .. but uhh I guess it works and it's what I have to do
			foreach (var kv in imagePaths) {
				if (file.hasSiblingFile(kv.Key)) {
					//var image = Image.FromStream(file.getSiblingFile(kv.Key));
					//info.addInfo(kv.Value, image);
					//TODO: Oh, I guess C# doesn't natively support TGA. Whoops. I thought it did. I guess I'll have to do that myself.
				}
			}
			//TODO: Perhaps add bootMovie.h264 and bootSound.btsnd... one day. I mean, they'd obviously be a bit complicated to decode. Unless there's some crossplatform H264 decoder for the former at least.
			if (file.hasSiblingFile("../meta/meta.xml")) {
				var doc = XDocument.Load(file.getSiblingFile("../meta/meta.xml")).Element("menu");
				var productCodeElement = doc.Element("product_code");
				if (productCodeElement != null) {
					//WUP-x-xxxx
					string productCode = productCodeElement.Value;
					info.addInfo("Product code", productCode);
					info.addInfo("Category", productCode[4], CATEGORIES);
					char gameType = productCode[6];
					info.addInfo("Type", gameType, GAME_TYPES);
					string shortTitle = productCode.Substring(7, 2);
					info.addInfo("Short title", shortTitle);
					char country = productCode[9];
					info.addInfo("Country", country, NintendoCommon.COUNTRIES);
				}
				//Seemingly always WUP
				info.addInfo("Content platform", doc.Element("content_platform")?.Value, true);
				//Alright, my favourite part
				var companyCodeElement = doc.Element("company_code");
				if (companyCodeElement != null) {
					string companyCode = companyCodeElement.Value;
					if (companyCode.StartsWith("00")) {
						info.addInfo("Publisher", companyCode.Substring(2, 2), NintendoCommon.LICENSEE_CODES);
					} else {
						info.addInfo("Publisher", companyCode, NintendoCommon.EXTENDED_LICENSEE_CODES);
					}
				}
				//... blank?
				info.addInfo("Mastering date", doc.Element("mastering_date")?.Value, true);
				//2 on third-party stuff (including Breath of Fire, which pretends to be published by Nintendo at least as far as the company code is concerned) and 0 on Nintendo's stuff. Hmm... that's interesting, especially as bootLogoText.tga seems to always be Nintendo even on third party stuff
				info.addInfo("Logo type", doc.Element("logo_type")?.Value, true);
				//1 on Wii U Chat, Parental Controls, Health and Safety Information and Daily Log; otherwise 0
				info.addInfo("App launch type", doc.Element("app_launch_type")?.Value, true);
				//6 on Wii U Chat, 0x10 on Parental Controls, and 0x16 on Health and Safety Information and Daily Log; otherwise 0. But what does it mean? Clearly some combination of several flags, but like.. invisible? I can see them on my Wii U menu just fine, thanks
				info.addInfo("Invisible flag", doc.Element("invisible_flag")?.Value, true);

				var regionElement = doc.Element("region");
				if(regionElement != null) {
					//Says type="hexBinary" so I dunno...
					int region = Convert.ToInt32(regionElement.Value, 16);
					if (region == -1) {
						//Only YouTube seems to use this (out of what I have). Netflix is apparently worldwide too, but its region flags are just set to Japan + USA + Europe
						info.addInfo("Region", "Region free");
					} else {
						info.addInfo("Region", Enum.ToObject(typeof(_3DS.RegionFlags), region).ToString());
					}
				} 
			}
		}

		public override void addROMInfo(ROMInfo info, ROMFile file) {
			info.addInfo("Platform", "Wii U");
			if ("rpx".Equals(file.extension)) {
				addRPXInfo(info, file);
			}
		}
	}
}
