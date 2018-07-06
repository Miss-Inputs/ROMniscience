/*
 * The MIT License
 *
 * Copyright 2018 Megan Leet (Zowayix).
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
			{"SQ", "Square Enix"}, //It's in the LR:FFXIII demo so I guess
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
			{"XA", "Xbox Live Arcade"}, //I guess? All XBLA titles have this manufacturer
			{"XH", "Xbox app"}, //Not sure how else to word this, all the non-game Xbox 360 apps have this (Facebook, Internet Exploder, etc)
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
