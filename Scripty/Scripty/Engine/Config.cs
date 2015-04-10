using System;
using System.Xml;
using System.Collections.Generic;

namespace Scripty.Engine
{
    
    /// <summary>
    /// Indicates the properties of a command's parameter
    /// </summary>
    public sealed class Parameter
    {
        /// <summary>
        /// Describes various types of parameters
        /// </summary>
        public enum ParameterType
        {
            Byte,
            HWord,
            Word,
            CodePointer,
            TextPointer,
            MovementPointer,
            MarketPointer,
            Variable,
            Pokemon,
            Item,
            Npc
        }

        /// <summary>
        /// Which kind of parameter (see ParameterType)
        /// </summary>
        public ParameterType Type { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Describes what the parameter does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a new Parameter instance
        /// </summary>
        public Parameter()
        { }

        /// <summary>
        /// Creates a new Parameter instance
        /// </summary>
        /// <param name="type">The parameter kind</param>
        /// <param name="name">The parameter name</param>
        /// <param name="description">The parameter description</param>
        public Parameter(ParameterType type, string name, string description)
        {
            Type = type;
            Name = name;
            Description = description;
        }
    }
    
    /// <summary>
    /// Contains information about a certain command
    /// </summary>
    public sealed class Command
    {
        /// <summary>
        /// The command id corresponding to the script's byte representation
        /// </summary>
        public byte Id { get; set; }

        /// <summary>
        /// The name used to display the command
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short text describing what the command does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of the command's parameters
        /// </summary>
        public List<Parameter> Parameters { get; set; }

        /// <summary>
        /// Creates a new Command instance
        /// </summary>
        public Command()
        {
            Parameters = new List<Parameter>();
        }

        /// <summary>
        /// Created a new Command instance
        /// </summary>
        /// <param name="id">The command id</param>
        /// <param name="name">The command name</param>
        /// <param name="description">The command description</param>
        /// <param name="parameters">The command's parameters</param>
        public Command(byte id, string name, string description, List<Parameter> parameters)
        {
            Id = id;
            Name = name;
            Description = description;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// Contains information about a certain macro
    /// </summary>
    public sealed class Macro
    {
        /// <summary>
        /// The name of the macro
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// A description of what the macro does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Code the macro translates to
        /// </summary>
        public string[] Template { get; set; }

        /// <summary>
        /// List of the macro's parameters
        /// </summary>
        public List<Parameter> Parameters { get; set; }

        /// <summary>
        /// Creates a new instance of Macro
        /// </summary>
        public Macro()
        {
            Parameters = new List<Parameter>();
        }

        /// <summary>
        /// Creates a new instance of Macro
        /// </summary>
        /// <param name="name">The name of the macro</param>
        /// <param name="description">The description of the macro</param>
        /// <param name="template">The code the macro translates to</param>
        /// <param name="parameters">The parameters the macro accepts</param>
        public Macro(string name, string description, string[] template, List<Parameter> parameters)
        {
            Name = name;
            Description = description;
            Template = template;
            Parameters = parameters;
        }
    }

    /// <summary>
    /// Maintains Scripty's configuration data
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Provides the contents of the configuration file in an OOP approach
        /// </summary>
        private XmlNode root;

        /// <summary>
        /// The list of commands 
        /// </summary>
        public Command[] Commands { get; set; }

        /// <summary>
        /// The list of macros
        /// </summary>
        public Macro[] Macros { get; set; }

        /// <summary>
        /// Initializes a new instance of Config.
        /// </summary>
        /// <param name="file">The path to Scripty's configuration file</param>
        public Config(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            root = doc.DocumentElement;
            if (root.Name != "scripty") {
                throw new NotAValidConfigException();
            }
            LoadCommands();
            Macros = new Macro[1];
            Macros[0] = new Macro();
            Macros[0].Name = "message";
            Macros[0].Description = "Displays a textbox with custom text";
            Macros[0].Template = new string[2];
            Macros[0].Template[0] = "loadpointer 0x00 {0}";
            Macros[0].Template[1] = "callstd {1}";
            Macros[0].Parameters.Add(new Parameter(Parameter.ParameterType.TextPointer, "textpointer", "blub"));
            Macros[0].Parameters.Add(new Parameter(Parameter.ParameterType.Byte, "boxtype", "blub"));
        }

        /// <summary>
        /// Loads the command configuration
        /// </summary>
        private void LoadCommands()
        {
            XmlNodeList commands = root.SelectSingleNode("commands").ChildNodes;
            Commands = new Command[256];
            foreach (XmlNode commandnode in commands)
            {
                Command command;
                XmlNode nodeid;
                XmlNode nodename;
                XmlNode nodedescription;
                XmlNode nodeparams;
                byte id;
                if (commandnode.Name != "command") {
                    throw new NotAValidConfigException();
                }
                command = new Command();
                nodeid = commandnode.SelectSingleNode("id");
                nodename = commandnode.SelectSingleNode("name");
                nodedescription = commandnode.SelectSingleNode("description");
                nodeparams = commandnode.SelectSingleNode("params");
                if (nodeid != null && nodename != null && nodedescription != null && nodeparams != null) {
                    byte.TryParse(nodeid.InnerText, System.Globalization.NumberStyles.HexNumber, null, out id);
                    command.Id = id;
                    command.Name = nodename.InnerText;
                    command.Description = nodedescription.InnerText;
                    command.Parameters = HandleParameterEntry(nodeparams.ChildNodes);
                }
                else { throw new NotAValidConfigException(); }
                Commands[command.Id] = command;
            }
        }

        /// <summary>
        /// Creates Parameter object from Xml data
        /// </summary>
        /// <param name="parameters">XmlNodeList which describes the parameters
        /// <returns>A list of Parameter objects</returns>
        private List<Parameter> HandleParameterEntry(XmlNodeList parameters)
        {
            List<Parameter> list = new List<Parameter>();
            foreach (XmlNode entry in parameters)
            {
                Parameter parameter = new Parameter();
                XmlNode nodename = entry.SelectSingleNode("name");
                XmlNode nodedescription = entry.SelectSingleNode("description");
                switch (entry.Name)
                {
                    case "byte": parameter.Type = Parameter.ParameterType.Byte; break;
                    case "hword": parameter.Type = Parameter.ParameterType.HWord; break;
                    case "word": parameter.Type = Parameter.ParameterType.Word; break;
                    case "code": parameter.Type = Parameter.ParameterType.CodePointer; break;
                    case "text": parameter.Type = Parameter.ParameterType.TextPointer; break;
                    case "movement": parameter.Type = Parameter.ParameterType.MovementPointer; break;
                    case "market": parameter.Type = Parameter.ParameterType.MarketPointer; break;
                    default: throw new NotAValidConfigException();
                }
                if (nodename != null && nodedescription != null) {
                    parameter.Name = nodename.InnerText;
                    parameter.Description = nodedescription.InnerText;
                }
                else { throw new NotAValidConfigException(); }
                list.Add(parameter);
            }
            return list;
        }

        /// <summary>
        /// An exception that is thrown when there is an error in the xml configuration
        /// </summary>
        public class NotAValidConfigException : Exception { }

    }
}
