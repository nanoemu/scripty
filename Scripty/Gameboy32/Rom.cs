// Gameboy32: Gameboy ROM reader and modifier
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
    /// Holds a byte-array which is
    /// read and modified directly.
    /// </summary>
    public sealed class Rom {
        #region variable declarations

        /// <summary>
        /// System file path of the ROM.
        /// </summary>
        private string path;
        /// <summary>
        /// The 16-digits long identifier.
        /// </summary>
        private string code;
        /// <summary>
        /// The raw 8-bit ROM data.
        /// </summary>
        private byte[] data;
        /// <summary>
        /// The total size of the ROM.
        /// </summary>
        private uint size;
        /// <summary>
        /// The current stream location.
        /// </summary>
        private uint off;

        /// <summary>
        /// Receives the size.
        /// </summary>
        public uint Size {
            get { return size; }
        }
        /// <summary>
        /// Receives the offset.
        /// </summary>
        public uint Offset {
            get { return off; }
        }
        /// <summary>
        /// Receives the direct file path.
        /// </summary>
        public string FilePath {
            get { return path; }
        }
        /// <summary>
        /// Receives the raw byte data.
        /// </summary>
        public byte[] RawData {
            get { return data; }
        }
        /// <summary>
        /// Receives the ROM title code.
        /// </summary>
        public string Title {
            get { return code.Substring(0,12); }
        }
        /// <summary>
        /// Receives the ROM identifier.
        /// </summary>
        public string Code {
            get { return code.Substring(12,4); }
        }
        public bool Success {
            get; set;
        }

        #endregion
        #region instance construction

        /// <summary>
        /// Reads the whole ROM from a file.
        /// Returns false if file doesn't exist.
        /// </summary>
        public Rom(string file) {
            if (!File.Exists(file)) {
                Success = false;
            } else {
                data = File.ReadAllBytes(file);
                size = (uint)data.GetLength(0);
                LoadIdentifier();
                path = file;
            }
        }

        /// <summary>
        /// Reads the 16-byte ASCII
        /// identifier out of the ROM.
        /// </summary>
        private void LoadIdentifier() {
            int i;
            var cd = new byte[16];
            var enc = Encoding.Default;
            for (i = 0; i < 16; i++) {
                cd[i] = data[0xA0+i];
            } code = enc.GetString(cd);
        }

        #endregion
        #region general functions

        /// <summary>
        /// Sets the stream position
        /// to the specified offset.
        /// </summary>
        public void Seek(uint offset) {
            if (off < size) {
                off = offset;
            }
        }

        /// <summary>
        /// Saves the ROM by writing all
        /// buffered bytes to the file.
        /// </summary>
        public void Save() {
            if (DirectoryExists()) {
                File.WriteAllBytes(path, data);
            } else {
                Directory.CreateDirectory(GetDirectory());
                File.WriteAllBytes(path, data);
            }
        }

        /// <summary>
        /// Disposes of the array and
        /// resets other resources.
        /// </summary>
        public void Dispose() {
            data = null;
            code = null;
            size = 0x0;
            off = 0x0;
        }

        /// <summary>
        /// Checks if the ROM path is valid.
        /// Doesn't check for the file itself.
        /// </summary>
        private bool DirectoryExists() {
            var str = GetDirectory();
            return Directory.Exists(str);
        }

        /// <summary>
        /// Generates a directory from file.
        /// </summary>
        /// <returns></returns>
        private string GetDirectory() {
            return Path.GetDirectoryName(path);
        }

        #endregion
        #region class methods

        /// <summary>
        /// Reads an unsigned byte and
        /// increases position by one.
        /// </summary>
        public byte ReadByte() {
            return data[off++];
        }

        /// <summary>
        /// Reads an unsigned short and
        /// increases position by two.
        /// </summary>
        public ushort ReadHWord() {
            byte lo = data[off++];
            byte hi = data[off++];
            return (ushort)((hi << 8) | lo);
        }

        /// <summary>
        /// Reads an unsigned integer and
        /// increases the position by four.
        /// </summary>
        public uint ReadWord() {
            byte lo16 = data[off++];
            byte hi16 = data[off++];
            byte lo32 = data[off++];
            byte hi32 = data[off++];
            return (uint)((hi32 << 24) |
                          (lo32 << 16) |
                           (hi16 << 8) |
                               (lo16));
        }

        /// <summary>
        /// Reads a 32-bit pointer and
        /// increases position by four.
        /// </summary>
        public uint ReadPtr() {
            return ReadWord()
                - 0x08000000;
        }

        /// <summary>
        /// Reads the specified amount of bytes
        /// and increases the position by amount.
        /// </summary>
        public byte[] Read(int cnt) {
            Byte[] array = new Byte[cnt];
            Buffer.BlockCopy(data,(int)off,array,0,cnt);
            return array;
        }

        /// <summary>
        /// Reads the specified amount of pointers
        /// and increases the position by amount.
        /// </summary>
        public uint[] ReadPtrTable(int cnt) {
            int i; // iteration var
            var arr = new uint[cnt];
            for (i = 0; i < cnt; i++) {
                arr[i] = ReadPtr();
            }

            return arr;
        }

        /// <summary>
        /// Reads a pokémon string and increases the
        /// position by the amount of read 0xFF bytes.
        /// </summary>
        public string ReadPokeString() {
            var lst = new List<byte>();
            while (true) {
                var b = data[off++];
                if (b == 0xFF) {
                    break;
                } else {
                    lst.Add(b);
                }
            }

            return Text.Decode(lst.ToArray());
        }

        /// <summary>
        /// Writes an unsigned byte and
        /// increases the position by one.
        /// </summary>
        public void WriteByte(byte u8) {
            data[off++] = u8;
        }

        /// <summary>
        /// Writes an unsigned short and
        /// increases the position by two.
        /// </summary>
        public void WriteHWord(ushort u16) {
            data[off++] = (byte)(u16 & 255);
            data[off++] = (byte)(u16 >> 08);
        }

        /// <summary>
        /// Writes an unsigned int and
        /// increases the position by four.
        /// </summary>
        public void WriteWord(uint u32) {
            data[off++] = (byte)(u32 & 255);
            data[off++] = (byte)(u32 >> 24);
            data[off++] = (byte)(u32 >> 16);
            data[off++] = (byte)(u32 >> 08);
        }

        /// <summary>
        /// Writes a 32-bit pointer and
        /// increases the position by four.
        /// </summary>
        public void WritePtr(uint off) {
            WriteWord(off+0x08000000);
        }

        /// <summary>
        /// Writes a byte array to ROM and
        /// increases position by array length.
        /// </summary>
        public void Write(byte[] arr) {
            Buffer.BlockCopy(arr,0,data,(int)off,arr.GetLength(0));
        }

        /// <summary>
        /// Writes a pokémon string and increases
        /// position by the encoded string length.
        /// </summary>
        public void WritePokeString(string str) {
            this.Write(Text.Encode(str));
        }

        /// <summary>
        /// Finds free space in the ROM and returns the
        /// offset which is nearest to param. "begin" with
        /// the given conditions. Returns 0 if no offset has
        /// been found or the search failed otherwise.
        /// </summary>
        public uint FreeSpace(uint begin, int len, byte src) {
            int hcount = 0;
            uint fnd = begin;
            this.off = begin;
            while (off < size && hcount < len) {
                if (data[off++] != src) {
                    fnd = off;
                    hcount = 0;
                } else {
                    hcount += 1;
                }
            } if (len != hcount) {
                return 0x0;
            } else {
                return fnd;
            }
        }

        #endregion
    }
}