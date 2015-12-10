﻿// Gameboy32: Gameboy ROM reader and modifier
// Library written in C# and MS .NET FX 4.5
// Copyright (c) 2015 by Gamecube, Nintendo
//
// License (GPL 3.0)
// This library is open source. That means that
// you can modify, share and update the code to
// your wishes under the following conditions:
// 
// 1. You might implement new classes.
// 2. You might share and modify code.
// 3. You have to release every change.
// 4. You have to pull a request on github.
// 5. You might not delete the author's name.
// 6. You might not implement weird things.
// 7. You might not share the code on any
//    other website than board.romresources.net

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Gameboy32 {
    /// <summary>
    /// Provides methods for reading and writing
    /// non-ASCII pokémon genIII characters.
    /// </summary>
    public static class Text {
        #region variable declarations

        /// <summary>
        /// Defines the most common set of pokémon
        /// characters. Their size is exactly one byte.
        /// </summary>
        private static byte[] encoded = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 
            0x7, 0x8, 0x9, 0xB, 0xC, 0xD, 0xE, 0xF, 0x10, 0x11, 0x12, 0x13, 0x14,
            0x15, 0x16, 0x17, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x20, 0x21, 0x22,
            0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2D, 0x2E, 0x34,
            0x35, 0x36, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A,
            0x5B, 0x5C, 0x5D, 0x68, 0x6F, 0x79, 0x7A, 0x7B, 0x7C, 0x85, 0x86, 0xA1,
            0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD,
            0xAE, 0xAF, 0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9,
            0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5,
            0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1,
            0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD,
            0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9,
            0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5,
            0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE };

        /// <summary>
        /// Defines foreground, background and shadow
        /// colors. They are three bytes sized each.
        /// </summary>
        private static int[] sp_encoded = new int[] { 0x0001FC, 0x0201FC, 0x0301FC, 0x0401FC,
            0x0501FC, 0x0601FC, 0x0701FC, 0x0801FC, 0x0901FC, 0x0002FC, 0x0202FC,
            0x0302FC, 0x0402FC, 0x0502FC, 0x0602FC, 0x0702FC, 0x0802FC, 0x0902FC,
            0x0003FC, 0x0203FC, 0x0303FC, 0x0403FC, 0x0503FC, 0x0603FC, 0x0703FC,
            0x0803FC, 0x0903FC, 0x0006FC, 0x0106FC };

        /// <summary>
        /// Defines keys, symbols and predefined
        /// buffers. They are two bytes big each.
        /// </summary>
        private static int[] sk_encoded = new int[] { 0x00F8, 0x01F8, 0x02F8, 0x03F8, 0x04F8,
            0x05F8, 0x00F9, 0x01F9, 0x02F9, 0x03F9, 0x04F9, 0x05F9, 0x06F9, 0x07F9,
            0x08F9, 0x09F9, 0x0AF9, 0x0BF9, 0x0CF9, 0x0DF9, 0x0EF9, 0x0FF9, 0x10F9,
            0x11F9, 0x12F9, 0x13F9, 0x14F9, 0x15F9, 0x16F9, 0x17F9, 0x01FD, 0x02FD,
            0x03FD, 0x04FD, 0x05FD, 0x06FD, 0x07FD, 0x08FD, 0x09FD, 0x0AFD, 0x0BFD,
            0x0CFD };

        /// <summary>
        /// Defines the most common set of pokémon
        /// characters. Their size is exactly one byte.
        /// </summary>
        private static string[] decoded = new string[] { " ", "À", "Á", "Â", "Ç", "È", "É", 
            "Ê", "Ë", "Ì", "Î", "Ï", "Ò", "Ó", "Ô", "Œ", "Ù", "Ú", "Û", "Ñ", "ß",
            "à", "á", "ç", "è", "é", "ê", "ë", "ì", "î", "ï", "ò", "ó", "ô", "œ", 
            "ù", "ú", "û", "ñ", "º", "ª", "&", "+", "[lv]", "=", ";", "¿", "¡", "[pk]",
            "[mn]", "[po]", "[ké]", "[bl]", "[oc]", "[k]", "Í", "%", "(", ")", "â", "í",
            "[u]", "[d]", "[l]", "[r]", "<", ">", "0", "1", "2", "3", "4", "5", "6", "7",
            "8", "9", "!", "?", ".", "-", "·", "[.]", "«", "»", "'", "'", "[m]", "[f]", "$",
            ",", "*", "/", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c",
            "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s",
            "t", "u", "v", "w", "x", "y", "z", "[>]", ":", "Ä", "Ö", "Ü", "ä", "ö", "ü", "[f7]",
            "[f8]", "[f9]", @"\l", @"\p", "[fc]", "[fd]", @"\n" };

        /// <summary>
        /// Defines foreground, background and shadow
        /// colors. They are three bytes sized each.
        /// </summary>
        private static string[] sp_decoded = new string[] { "[c$white]", "[c$black]", "[c$grey]", "[c$red]",
            "[c$orange]", "[c$green]", "[c$cyan]", "[c$blue]", "[c$royal]", "[c$white$bg]", "[c$black$bg]",
            "[c$grey$bg]", "[c$red$bg]", "[c$orange$bg]", "[c$green$bg]", "[c$cyan$bg]", "[c$blue$bg]",
            "[c$royal$bg]", "[c$white$sw]", "[c$black$sw]", "[c$grey$sw]", "[c$red$sw]", "[c$orange$sw]",
            "[c$green$sw]", "[c$cyan$sw]", "[c$blue$sw]", "[c$royal$sw]", "[font$small]", "[font$big]" };

        /// <summary>
        /// Defines keys, symbols and predefined
        /// buffers. They are two bytes big each.
        /// </summary>
        private static string[] sk_decoded = new string[] { "[key$a]", "[key$b]", "[key$l]", "[key$r]",
            "[key$start]", "[key$select]", "[sym$arrow$up]", "[sym$arrow$down]", "[sym$arrow$left]",
            "[sym$arrow$right]", "[sym$plus]", "[sym$lvl]", "[sym$ap]", "[sym$id]", "[sym$nr]",
            "[sym$ul]", "[sym$1]", "[sym$2]", "[sym$3]", "[sym$4]", "[sym$5]", "[sym$6]", "[sym$7]",
            "[sym$8]", "[sym$9]", "[sym$br$open]", "[sym$br$close]", "[sym$circle]", "[sym$triangle]",
            "[sym$x]", "[bfr$player]", "[bfr$buff1]", "[bfr$buff2]", "[bfr$buff3]", "[bfr$rival]",
            "[bfr$ruby]", "[bfr$magma]", "[bfr$aqua]", "[bfr$max]", "[bfr$arch]", "[bfr$groud]", 
            "[bfr$kyog]" };

        #endregion
        #region static class methods

        /// <summary>
        /// Decodes a given byte array and
        /// returns its string representation.
        /// </summary>
        public static string Decode(byte[] source) {
            int length = source.Length;
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++) {
                byte entry = source[i];
                if (entry == 0xF8 || entry == 0xF9 || entry == 0xFD) {
                    byte hi = source[i++];
                    int s16 = ((hi << 8) | entry);
                    int index = sk_encoded.IndexOf(s16);
                    if (index != -1)
                        builder.Append(sk_decoded[index]);
                } else if (entry == 0xFC) {
                    byte lo = source[i++];
                    byte hi = source[i++];
                    int s24 = ((hi << 16) | (lo << 8) | entry);
                    int index = sp_encoded.IndexOf(s24);
                    if (index != -1)
                        builder.Append(sp_decoded[index]);
                } else {
                    int index = encoded.IndexOf(entry);
                    if (index != -1)
                        builder.Append(decoded[index]);
                }
            };

            return builder.ToString();
        }

        /// <summary>
        /// Encodes a given string and returns
        /// its byte array representation.
        /// </summary>
        public static byte[] Encode(string source) {
            int length = source.Length;
            var bytes = new List<byte>();

            for (int i = 0; i < length; i++) {
                char entry = source[i];
                if (entry == '\\') {
                    char hi = source[i++];
                    string s16 = string.Concat(entry, hi);
                    int index = decoded.IndexOf(s16);
                    if (index != -1)
                        bytes.Add(encoded[index]);
                } else if (entry == '[') {
                    int endindex = source.IndexOf(']', i);
                    string value = source.Substring(i, (endindex - i));
                    if (value.StartsWith("key$") || value.StartsWith("sym$") || value.StartsWith("bfr$")) {
                        int index = sk_decoded.IndexOf(value);
                        if (index != -1)
                            bytes.AddRange(GetBytes(sk_encoded[index]));
                    } else {
                        int index = sp_decoded.IndexOf(value);
                        if (index != -1)
                            bytes.AddRange(GetBytes(sp_encoded[index]));
                    } i += (endindex - i);
                }  else {
                    int index = decoded.IndexOf(entry.ToString());
                    if (index != -1)
                        bytes.Add(encoded[index]);
                }
            };

            return bytes.ToArray();
        }

        /// <summary>
        /// Splits a 24-bit integer into
        /// a byte array of three elements.
        /// </summary>
        private static byte[] GetBytes(int s24) {
            byte b1 = (byte)(s24 & 255);
            byte b2 = (byte)(s24 >> 16);
            byte b3 = (byte)(s24 >> 8);
            return new byte[] { b1, b2, b3 };
        }

        /// <summary>
        /// Splits a 16-bit short into
        /// a byte array of two elements.
        /// </summary>
        private static byte[] GetBytes(short s16) {
            byte b1 = (byte)(s16 & 255);
            byte b2 = (byte)(s16 >> 8);
            return new byte[] { b1, b2 };
        }

        /// <summary>
        /// Returns the index of an element
        /// in a signed integer array.
        /// </summary>
        public static int IndexOf(this int[] array, int value) {
            int length = array.Length;
            for (int i = 0; i < length; i++) {
                if (array[i] == value) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of a
        /// value in a string array.
        /// </summary>
        public static int IndexOf(this string[] array, string value) {
            int length = array.Length;
            for (int i = 0; i < length; i++) {
                if (array[i] == value)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of a
        /// value in a byte array.
        /// </summary>
        public static int IndexOf(this byte[] array, byte value) {
            int length = array.Length;
            for (int i = 0; i < length; i++) {
                if (array[i] == value)
                    return i;
            }

            return -1;
        }

        #endregion
    }
}