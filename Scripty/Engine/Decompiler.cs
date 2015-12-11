using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gameboy32;

namespace Scripty.Engine
{
    /// <summary>
    /// Stores information about data/code that is refered to by script commands
    /// </summary>
    public sealed class Reference
    {
        /// <summary>
        /// Enumeration of the possible types of references
        /// </summary>
        public enum ReferenceType
        {
            Ignore,
            Code,
            Text,
            Movement,
            Market
        }

        /// <summary>
        /// Location of the data/code refered to
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// The type of reference. <see cref="ReferenceType"/>
        /// </summary>
        public ReferenceType Type { get; set; }

        /// <summary>
        /// Creates a new Reference object
        /// </summary>
        public Reference()
        { }

        /// <summary>
        /// Creates a new Reference object with predefined data
        /// </summary>
        /// <param name="offset">The offset refered to</param>
        /// <param name="type">The kind of data refered to</param>
        public Reference(uint offset, ReferenceType type)
        {
            Offset = offset;
            Type = type;
        }
    }

    /// <summary>
    /// Class capable of decompiling scipts from a given rom.
    /// </summary>
    public sealed class Decompiler
    {
        /// <summary>
        /// Stores the given rom <seealso cref="Rom"/>
        /// </summary>
        private Rom rom;

        /// <summary>
        /// Stores the given configuration file <seealso cref="Config"/>
        /// </summary>
        private Config config;

        /// <summary>
        /// Creates a new instance of the Decompiler.
        /// </summary>
        /// <param name="rom">An object giving access to the rom <seealso cref="Rom"/></param>
        /// <param name="config"></param>
        public Decompiler(Rom rom, Config config)
        {
            this.rom = rom;
            this.config = config;
        }

        /// <summary>
        /// Decompiles the script located at the given location.
        /// </summary>
        /// <param name="offset">The offset of the script</param>
        /// <returns>A list of script lines</returns>
        public List<string> Decompile(uint offset)
        {
            List<string> code = new List<string>();
            Queue<Reference> queue = new Queue<Reference>();
            Hashtable linetoref = new Hashtable();
            Reference root = new Reference(offset, Reference.ReferenceType.Code);
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                Reference current = queue.Dequeue();
                if (current.Type == Reference.ReferenceType.Ignore) {
                    continue;
                }
                rom.Seek(current.Offset & 0x1FFFFFF);
                code.Add(string.Format("#org 0x{0:X4}", current.Offset));
                switch (current.Type)
                {
                    case Reference.ReferenceType.Code:
                    {
#region "Code decompiling and macro stuff"
                        bool end = false;
                        while (!end) {
                            byte hexcode = rom.ReadByte();
                            Command command = config.Commands[hexcode];
                            if (command != null) {
                                // command processing
                                string text = command.Name;
                                foreach (Parameter parameter in command.Parameters) {
                                    switch (parameter.Type)
                                    {
                                        case Parameter.ParameterType.Byte:
                                        {
                                            // Even tho bytes don't referenciate we need to create a padding in the hashtable list for correct indexing
                                            Reference reference = new Reference();
                                            uint value = rom.ReadByte();
                                            text += string.Format(" 0x{0:X2}", value);
                                            reference.Type = Reference.ReferenceType.Ignore;
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            break;
                                        }
                                        case Parameter.ParameterType.HWord:
                                        {
                                            // Even tho hwords don't referenciate we need to create a padding in the hashtable list for correct indexing
                                            Reference reference = new Reference();
                                            uint value = rom.ReadHWord();
                                            text += string.Format(" 0x{0:X2}", value);
                                            reference.Type = Reference.ReferenceType.Ignore;
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            break;
                                        }
                                        case Parameter.ParameterType.Word:
                                        {
                                            Reference reference = new Reference();
                                            uint value = rom.ReadWord();
                                            reference.Offset = value;
                                            reference.Type = Reference.ReferenceType.Ignore;
                                            text += string.Format(" 0x{0:X8}", value);
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            queue.Enqueue(reference);
                                            break;
                                        }
                                        case Parameter.ParameterType.CodePointer:
                                        {
                                            Reference reference = new Reference();
                                            uint value = rom.ReadWord();
                                            reference.Offset = value;
                                            reference.Type = Reference.ReferenceType.Code;
                                            text += string.Format(" 0x{0:X8}", value);
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            queue.Enqueue(reference);
                                            break;
                                        }
                                        case Parameter.ParameterType.TextPointer:
                                        {
                                            Reference reference = new Reference();
                                            uint value = rom.ReadWord();
                                            reference.Offset = value;
                                            reference.Type = Reference.ReferenceType.Text;
                                            text += string.Format(" 0x{0:X8}", value);
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            queue.Enqueue(reference);
                                            break;
                                        }
                                        case Parameter.ParameterType.MovementPointer:
                                        {
                                            Reference reference = new Reference();
                                            uint value = rom.ReadWord();
                                            reference.Offset = value;
                                            reference.Type = Reference.ReferenceType.Movement;
                                            text += string.Format(" 0x{0:X8}", value);
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            queue.Enqueue(reference);
                                            break;
                                        }
                                        case Parameter.ParameterType.MarketPointer:
                                        {
                                            Reference reference = new Reference();
                                            uint value = rom.ReadWord();
                                            reference.Offset = value;
                                            reference.Type = Reference.ReferenceType.Market;
                                            text += string.Format(" 0x{0:X8}", value);
                                            if (!linetoref.ContainsKey(code.Count)) {
                                                linetoref[code.Count] = new List<Reference>();
                                            }
                                            List<Reference> list = (List<Reference>)linetoref[code.Count];
                                            list.Add(reference);
                                            queue.Enqueue(reference);
                                            break;
                                        }
                                    }
                                }
                                if (hexcode == 0x02 || hexcode == 0x03) {
                                    end = true;
                                }
                                code.Add(text);
                                
                            }
                            else {
                                /***** THROW DERIVED EXCEPTION *****/
                            }
                        }
                        // macro processing
                        string[] commands = code.ToArray();
                        int commandcount = commands.Length;
                        for (int i = 0; i < commandcount; i++) {
                            foreach (Macro macro in config.Macros) {
                                int templatelength = macro.Template.Length;
                                if (i + templatelength <= commandcount) {
                                    bool mismatch = false;
                                    for (int j = 0; j < templatelength; j++) {
                                        string template = macro.Template[j];
                                        string pattern = Regex.Replace(template, "{[0-9]}", "0x[0-9a-fA-F]+");
                                        if (!Regex.IsMatch(commands[i + j], pattern)) {
                                            mismatch = true;
                                            break;
                                        }
                                    }
                                    if (!mismatch) {
                                        string renamed = macro.Name;
                                        int parametercount = macro.Parameters.Count;
                                        if (parametercount != 0) {
                                            for (int j = 0; j < parametercount; j++) {
                                                for (int k = 0; k < templatelength; k++) {
                                                    Match match = Regex.Match(macro.Template[k], "\\{" + j + "\\}");
                                                    if (match.Success) {
                                                        string[] templatetokens = macro.Template[k].Split(' ');
                                                        string[] commandtokens = commands[i + k].Split(' ');
                                                        int templatetokenslength = templatetokens.Length;
                                                        int index = 0;
                                                        for (int l = 1; l < templatetokenslength; l++) {
                                                            Match match2 = Regex.Match(templatetokens[l], "\\{" + j + "\\}");
                                                            if (match2.Success) {
                                                                index = l;
                                                            }
                                                        }
                                                        if (linetoref.ContainsKey(i + k)) {
                                                            List<Reference> list = (List<Reference>)linetoref[i + k];
                                                            switch (macro.Parameters[j].Type)
                                                            {
                                                                case Parameter.ParameterType.CodePointer: list[index - 1].Type = Reference.ReferenceType.Code; break;
                                                                case Parameter.ParameterType.TextPointer: list[index - 1].Type = Reference.ReferenceType.Text; break;
                                                                case Parameter.ParameterType.MovementPointer: list[index - 1].Type = Reference.ReferenceType.Movement; break;
                                                                case Parameter.ParameterType.MarketPointer: list[index - 1].Type = Reference.ReferenceType.Market; break;
                                                            }
                                                        }
                                                        renamed += " " + commandtokens[index];
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        for (int j = 0; j < templatelength; j++) {
                                            code.RemoveAt(i);
                                        }
                                        code.Insert(i, renamed);
                                        i += templatelength - 1;
                                    }
                                }
                            }
                        }
                        break;
#endregion
                    }
                    case Reference.ReferenceType.Text:
                    {
                        List<byte> text = new List<byte>();
                        string str;
                        while (true) {
                            byte value = rom.ReadByte();
                            if (value == 0xFF) break;
                            text.Add(value);
                        }
                        str = Text.Decode(text.ToArray());
                        code.Add("= " + str);
                        break;
                    }
                    case Reference.ReferenceType.Movement: break;
                    case Reference.ReferenceType.Market: break;
                }
                code.Add("\n");
            }
            return code;
        }

    }
}
