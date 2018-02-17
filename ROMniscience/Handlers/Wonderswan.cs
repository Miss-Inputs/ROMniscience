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
using ROMniscience.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience.Handlers {
    class Wonderswan : Handler {
        //https://www.zophar.net/fileuploads/2/10805ixdtg/wstech23.txt
        //http://daifukkat.su/docs/wsman/#cart_over

        public override IDictionary<string, string> filetypeMap => new Dictionary<string, string>() {
            {"ws" , "Bandai WonderSwan ROM"},
            {"wsc", "Bandai WonderSwan Color ROM"}
        };

        public override string name => "Bandai WonderSwan";

        public static readonly IDictionary<int, long> ROM_SIZES = new Dictionary<int, long> {
            {0, 128 * 1024},
            {1, 256 * 1024},
            {2, 512 * 1024},
            {3, 1024 * 1024},
            {4, 2 * 1024 * 1024},
            {5, 3 * 1024 * 1024},
            {6, 4 * 1024 * 1024},
            {7, 6 * 1024 * 1024},
            {8, 8 * 1024 * 1024},
            {9, 16 * 1024 * 1024},
        };

        public static readonly IDictionary<int, long> RAM_SIZES = new Dictionary<int, long> {
            {0, 0},
            {1, 8 * 1024},
            {2, 32 * 1024 },
            {3, 128 * 1024},
            {4, 256 * 1024},
            {5, 512 * 1024},
            {0x10, 128},
            {0x20, 2 * 1024},
            {0x50, 1024},
        };

        public static readonly IDictionary<int, string> PUBLISHERS = new Dictionary<int, string> {
            {0, "Nobody"},
            {1, "Bandai"},
            {2, "Taito"},
            {3, "Tomy"},
            {4, "Koei"},
            {5, "Data East"},
            {6, "Asmik Ace"},
            {7, "Media Entertainment"},
            {8, "Nichibutsu"},
            {0xa, "Coconuts Japan"},
            {0xb, "Sammy"},
            {0xc, "Sunsoft"},
            {0xd, "Mebius"},
            {0xe, "Banpresto"},
            {0x10, "Jaleco"},
            {0x11, "Imagineer"},
            {0x12, "Konami"},
            {0x16, "Kobunsha"},
            {0x17, "Bottom Up"},
            {0x18, "Kaga Tech"}, //Or Naxat or Mechanic Arms or Media Entertainment or what the damn heck
            {0x19, "Sunrise"},
            {0x1a, "Cyber Front"},
            {0x1b, "Mega House"},
            {0x1d, "Interbec"},
            {0x1e, "Nihon Application"},
            {0x1f, "Bandai Visual"}, //The heck is that (could also be "Emotion", apparently)
            {0x20, "Athena"},
            {0x21, "KID"},
            {0x22, "HAL"},
            {0x23, "Yuki Enterprise"},
            {0x24, "Omega Micott"},
            {0x25, "Layup"}, //Or "Upstar"
            {0x26, "Kadokawa Shoten"},
            {0x27, "Shall Luck"}, //Or "Cocktail Soft"
            {0x28, "Squaresoft"},
            {0x2a, "NTT DoCoMo"},
            {0x2b, "Tom Create"},
            {0x2d, "Namco"},
            {0x2e, "Movic"},
            {0x2f, "E3 Staff"}, //wat (could also be "Gust")
            {0x31, "Vanguard"}, //or Elorg
            {0x32, "Megatron"},
            {0x33, "Wiz"},
            {0x36, "Capcom"},
        };

        public static int calcChecksum(InputStream s) {
            long pos = s.Position;
            try {
                s.Seek(0, System.IO.SeekOrigin.Begin);
                int checksum = 0;
                while (s.Position < s.Length - 2) {
                    checksum = (checksum + s.read()) & 0xffff;
                }
                return checksum;
            } finally {
                s.Seek(pos, System.IO.SeekOrigin.Begin);
            }
        }

        public static void readWonderswanROM(ROMInfo info, ROMFile file) {
            InputStream s = file.stream;
            s.Seek(-10, System.IO.SeekOrigin.End);

            int publisher = s.read();
            info.addInfo("Manufacturer", publisher, PUBLISHERS);

            int deviceFlag = s.read();
            bool isColor = deviceFlag == 1;
            info.addInfo("Is colour", isColor);
            info.addInfo("Device flag", deviceFlag, true); //This might have more to it, but probably not

            int cartID = s.read(); //Last 2 digits of SKU
            info.addInfo("Product code", cartID);

            int version = s.read();
            info.addInfo("Version", version);

            int romSize = s.read();
            info.addInfo("ROM size", romSize, ROM_SIZES, ROMInfo.FormatMode.SIZE);

            int ramSize = s.read();
            info.addInfo("Save size", ramSize, RAM_SIZES, ROMInfo.FormatMode.SIZE);
            info.addInfo("Save type", ramSize >= 10 ? "EEPROM" : ramSize == 0 ? "None" : "SRAM");

            int flags = s.read();
            info.addInfo("Flags", flags, true); //Maybe there are more flags than these three, probably not
            info.addInfo("ROM speed", (flags & 4) > 0 ? "3 speed" : "1 speed");
            info.addInfo("Bus width (bits)", (flags & 2) > 0 ? 16 : 8);
            info.addInfo("Screen orientation", (flags & 1) == 1 ? "Vertical" : "Horizontal");

            bool hasRTC = s.read() == 1;
            info.addInfo("Has RTC", hasRTC);

            ushort checksum = (ushort)s.readShortLE();
            info.addInfo("Checksum", checksum, true);
            info.addInfo("Checksum valid?", checksum == calcChecksum(s));
        }

        public override void addROMInfo(ROMInfo info, ROMFile file) {
            info.addInfo("Platform", name);
            readWonderswanROM(info, file);
        }
    }
}
