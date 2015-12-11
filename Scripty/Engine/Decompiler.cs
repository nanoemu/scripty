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
        /// Reads commands until end or return occurs and decompiles them
        /// </summary>
        /// <param name="output">Output list to which the decompiled lines will be added to</param>
        /// <param name="queue">Decompilation queue, any references will be enqueue'd in this queue</param>
        /// <param name="linetoreferences">Hashtable which saves the references made in the various decompiled lines. Required for macros</param>
        private void DissectCode(List<string> output, Queue<Reference> queue, Hashtable linetoreferences)
        {
            while (true) {
                byte hexcode = rom.ReadByte(); // Get command id
                Command command = config.Commands[hexcode]; // Get corresponding command definition
                string text;

                // Handle case that the command could be undefined
                if (command == null) {
                    /***** THROW DERIVED EXCEPTION *****/
                }

                // New line begins with the name of the command
                text = command.Name;

                #region "Parameter handling"
                // Process the individual parameters
                foreach (Parameter parameter in command.Parameters) {
                    switch (parameter.Type) {
                        case Parameter.ParameterType.Byte:
                            {
                                // Even though bytes don't referenciate we need to create a padding in the hashtable list for correct indexing
                                Reference reference = new Reference();
                                uint value = rom.ReadByte();
                                text += string.Format(" 0x{0:X2}", value);
                                reference.Type = Reference.ReferenceType.Ignore;
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
                                list.Add(reference);
                                break;
                            }
                        case Parameter.ParameterType.HWord:
                            {
                                // Even though hwords don't referenciate we need to create a padding in the hashtable list for correct indexing
                                Reference reference = new Reference();
                                uint value = rom.ReadHWord();
                                text += string.Format(" 0x{0:X2}", value);
                                reference.Type = Reference.ReferenceType.Ignore;
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
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
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
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
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
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
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
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
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
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
                                if (!linetoreferences.ContainsKey(output.Count)) {
                                    linetoreferences[output.Count] = new List<Reference>();
                                }
                                List<Reference> list = (List<Reference>)linetoreferences[output.Count];
                                list.Add(reference);
                                queue.Enqueue(reference);
                                break;
                            }
                    }
                }
                #endregion

                // Add the generated scriptcommand to the script list
                output.Add(text);

                // Exit condition (use break?)
                if (hexcode == 0x02 || hexcode == 0x03) {
                    break;
                }
            }
        }

        /// <summary>
        /// Macronizes code. Needs to be called after each newly decompiled command in order to enable "recursive" macros
        /// </summary>
        /// <param name="output">Output list to which the decompiled lines will be added to</param>
        /// <param name="linetoreferences">Hashtable which saves the references made in the various decompiled lines. Required for macros</param>
        private void SearchForPatternsAndSimplify(List<string> output, Hashtable linetoreferences)
        {
            string[] lines = output.ToArray();
            int linecount = lines.Length;

            // Iterate through all lines of the decompilation
            for (int i = 0; i < linecount; i++) {
                // Handle each possible macro
                foreach (Macro macro in config.Macros) {
                    int templatelength = macro.Template.Length;

                    // Do only check
                    if (i + templatelength <= linecount) {
                        bool mismatch = false;
                        for (int j = 0; j < templatelength; j++) {
                            string template = macro.Template[j];
                            string pattern = Regex.Replace(template, "{[0-9]}", "0x[0-9a-fA-F]+");
                            if (!Regex.IsMatch(lines[i + j], pattern)) {
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
                                            string[] commandtokens = lines[i + k].Split(' ');
                                            int templatetokenslength = templatetokens.Length;
                                            int index = 0;
                                            for (int l = 1; l < templatetokenslength; l++) {
                                                Match match2 = Regex.Match(templatetokens[l], "\\{" + j + "\\}");
                                                if (match2.Success) {
                                                    index = l;
                                                }
                                            }
                                            if (linetoreferences.ContainsKey(i + k)) {
                                                List<Reference> list = (List<Reference>)linetoreferences[i + k];
                                                switch (macro.Parameters[j].Type) {
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
                                output.RemoveAt(i);
                            }
                            output.Insert(i, renamed);
                            i += templatelength - 1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decompiles the script located at the given location.
        /// </summary>
        /// <param name="offset">The offset of the script</param>
        /// <returns>A list of script lines</returns>
        public List<string> Decompile(uint offset)
        {
            List<string> output = new List<string>();
            Queue<Reference> queue = new Queue<Reference>();
            Hashtable linetoreferences = new Hashtable();
            Reference root = new Reference(offset, Reference.ReferenceType.Code);

            // Enqueue the root element
            queue.Enqueue(root);

            // Run until the queue is empty
            while (queue.Count != 0)
            {
                Reference current = queue.Dequeue();

                // Check wether to ignore the reference or not
                if (current.Type == Reference.ReferenceType.Ignore) {
                    continue;
                }

                // Seek to the reference's position
                rom.Seek(current.Offset & 0x1FFFFFF);

                // Create "header" for reference
                output.Add(string.Format("#org 0x{0:X4}", current.Offset));

                // Dissect the reference
                switch (current.Type)
                {
                    case Reference.ReferenceType.Code:
                    {
                        DissectCode(output, queue, linetoreferences);
                        SearchForPatternsAndSimplify(output, linetoreferences);
                        break;
                    }
                    case Reference.ReferenceType.Text:
                    {
                        List<byte> text = new List<byte>();

                        // Run as long as the string isn't terminated by 0xFF
                        while (true) {
                            byte value = rom.ReadByte();
                            if (value == 0xFF) break;
                            text.Add(value);
                        }

                        // Convert data to string and add it to the code
                        output.Add("= " + Text.Decode(text.ToArray()));
                        break;
                    }
                    case Reference.ReferenceType.Movement: break;
                    case Reference.ReferenceType.Market: break;
                }

                // We wan't an empty line after each reference
                output.Add("\n");
            }

            // Return the generated code
            return output;
        }

    }
}
