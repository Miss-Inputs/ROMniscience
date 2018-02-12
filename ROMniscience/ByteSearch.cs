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

namespace ROMniscience {
    static class ByteSearch {
        public static bool contains(byte[] searchWithin, byte[] searchFor) {
            return indexOf(searchWithin, searchFor) > -1;
        }

        public static int indexOf(byte[] searchWithin, byte[] searchFor) {
            int[] charTable = makeCharTable(searchFor);
            int[] offsetTable = makeOffsetTable(searchFor);

            for (int i = searchFor.Length - 1; i < searchWithin.Length;) {
                int j;
                for (j = searchFor.Length - 1; searchFor[j] == searchWithin[i]; --i, --j) {
                    if (j == 0) {
                        return i;
                    }
                }
                i += Math.Max(offsetTable[searchFor.Length - 1 - j], charTable[searchWithin[i]]);
            }
            return -1;

        }

        private static int[] makeCharTable(byte[] bytes) {
            int[] table = new int[256];

            for (int i = 0; i < 256; ++i) {
                table[i] = bytes.Length;
            }
            for (int i = 0; i < bytes.Length - 1; ++i) {
                table[bytes[i]] = bytes.Length - 1 - i;
            }

            return table;
        }

        private static int[] makeOffsetTable(byte[] bytes) {
            int[] table = new int[bytes.Length];
            int lastPrefixPosition = bytes.Length;

            for (int i = bytes.Length; i > 0; --i) {
                if (isPrefix(bytes, i)) {
                    lastPrefixPosition = i;
                }
                table[bytes.Length - i] = lastPrefixPosition - i + bytes.Length;
            }

            for (int i = 0; i < bytes.Length - 1; ++i) {
                int suffix = suffixLength(bytes, i);
                table[suffix] = bytes.Length - 1 - i + suffix;
            }

            return table;
        }

        private static bool isPrefix(byte[] bytes, int offset) {
            for (int i = offset, j = 0; i < bytes.Length; ++i, ++j) {
                if (bytes[i] != bytes[j]) {
                    return false;
                }
            }
            return true;
        }

        private static int suffixLength(byte[] bytes, int offset) {
            int length = 0;
            for (int i = offset, j = bytes.Length - 1; i >= 0 && bytes[i] == bytes[j]; --i, --j) {
                length++;
            }
            return length;
        }
    }
}
