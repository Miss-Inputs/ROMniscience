using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
	static class MicrosoftCommon {
		public static readonly IDictionary<string, string> LICENSEE_CODES = new Dictionary<string, string>() {
			{"AC", "Acclaim"},
			{"AH", "ARUSH"},
			{"AQ", "Aqua System"},
			{"AS", "ASK"},
			{"AT", "Atlus"},
			{"AV", "Activision"},
			{"AY", "Aspyr"},
			{"BA", "Bandai"},
			{"BL", "Black Box"},
			{"BM", "BAM! Entertainment"},
			{"BR", "Broccoli"},
			{"BS", "Bethesda"},
			{"BU", "Bunkasha"},
			{"BV", "Buena Vista"},
			{"BW", "BBC"},
			{"BZ", "Blizzard"},
			{"CC", "Capcom"},
			{"CK", "Kemco"}, //The Xbox dev wiki puts a citation needed here
			{"CM", "Codemasters"},
			{"CV", "Crave Entertainment"},
			{"DC", "DreamCatcher Interactive"},
			{"DX", "Davilex"},
			{"EA", "Electronic Arts"},
			{"EC", "Encore"},
			{"EL", "Enlight"},
			{"EM", "Empire"},
			{"ES", "Eidos"},
			{"FI", "Fox"},
			{"FS", "FromSoftware"},
			{"GE", "Genki"},
			{"GV", "Groove Games"},
			{"HE", "Tru Blu Entertainment / HES"},
			{"HP", "Hip Games"},
			{"HU", "Hudson Soft"},
			{"HW", "HighwayStar"},
			{"IA", "Mad Catz"}, //What kind of abbreviation is that?
			{"IF", "Idea Factory"},
			{"IG", "Infogrames"},
			{"IL", "Interlex / Panther Software"},
			{"IM", "Imagine Media"},
			{"IO", "Ignition"},
			{"IP", "Interplay"},
			{"IX", "InXile"}, //Another citation needed
			{"JA", "Jaleco"},
			{"JW", "JoWooD"},
			{"KB", "Kemco"}, //Citation needed
			{"KI", "Kids Station"}, //Citation needed
			{"KN", "Konami"},
			{"KO", "Koei"},
			{"KU", "Kobi / Global A"},
			{"LA", "LucasArts"},
			{"LS", "Black Bean Games / Leader S.p.A"},
			{"MD", "Metro3D"},
			{"ME", "Medix"},
			{"MI", "Microïds"},
			{"MJ", "Majesco"},
			{"MM", "Myelin"},
			{"MP", "MediaQuest"}, //Citation needed
			{"MS", "Microsoft"},
			{"MW", "Midway"},
			{"MX", "Empire"}, //Citation needed
			{"NK", "NewKidsCo"},
			{"NL", "NovaLogic"},
			{"NM", "Namco"},
			{"OX", "Oxygen"},
			{"PC", "PlayLogic"},
			{"PL", "Phantagram"},
			{"RA", "Rage"},
			{"SA", "Sammy"},
			{"SC", "SCi"}, //Sony Computer Interactive? I don't even know
			{"SE", "Sega"},
			{"SN", "SNK"},
			{"SS", "Simon & Schuster"},
			{"SU", "Success Corporation"},
			{"SW", "Swing! Deutschland"},
			{"TA", "Takara"},
			{"TC", "Tecmo"},
			{"TD", "The 3DO Company"},
			{"TK", "Takuyo"},
			{"TM", "TDK"},
			{"TQ", "THQ"},
			{"TS", "Titus"},
			{"TT", "Take-Two Interactive"},
			{"US", "Ubisoft"},
			{"VC", "Victor"},
			{"VN", "Vivendi (VN) / Interplay"}, //Citation needed
			{"VU", "Vivendi"},
			{"VV", "Vivendi (VV)"}, //Citation needed
			{"WE", "Wanadoo Edition"},
			{"WR", "Warner Bros"}, //Citation needed,
			{"XI", "XPEC Entertainment / Idea Factory"},
			{"XK", "Xbox kiosk disc"}, //Citation needed
			{"XL", "Xbox live demo disc"}, //Citation needed
			{"XM", "Evolved Games"}, //Citation needed
			{"XP", "XPEC"},
			{"XR", "Panorama"},
			{"YB", "YBM Sisa"},
			{"ZD", "Zushi / Zoo"},
		};
	}
}
