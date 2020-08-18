using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Parser;

namespace ROR2ModManager
{

    public interface IConfigLine
    {
        string Render();
    }

    public class LiteralLine : IConfigLine
    {
        public string Text { get; set; }
        public string Render() => Text;
    }

    public class SectionLine : IConfigLine
    {
        public string Name { get; set; }
        public string Render() => $"[{Name}]";
    }

    public class ConfigurationValueLine : IConfigLine
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string SectionName { get; set; }

        public string Render() => $"{Key} = {Value}";
    }

    class Configuration
    {
        public List<IConfigLine> Document;

        public Dictionary<SectionLine, List<ConfigurationValueLine>> StructuredValues
        {
            get
            {
                var newdict = new Dictionary<SectionLine, List<ConfigurationValueLine>>();
                foreach(var line in Document)
                {
                    if (line is ConfigurationValueLine)
                    {
                        SectionLine section;
                        if (((ConfigurationValueLine)line).SectionName == null)
                            section = new SectionLine { Name = "General" };
                        else
                            section = new SectionLine { Name = ((ConfigurationValueLine)line).SectionName };

                        if (!newdict.ContainsKey(section))
                        {
                            newdict[section] = new List<ConfigurationValueLine>();
                        }

                        newdict[section].Add((ConfigurationValueLine)line);
                    } else if (line is SectionLine)
                        newdict.Add((SectionLine)line, new List<ConfigurationValueLine>());  
                }
                return newdict;
            }
        }


        public Configuration()
        {
            Document = new List<IConfigLine>();
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static IniParser.Model.IniData LoadConfig(string text)
        {
            var parser = new FileIniDataParser(new IniDataParser(new IniParser.Model.Configuration.IniParserConfiguration
            {
                CommentString = "#"
            }));
            var data = parser.ReadData(new System.IO.StreamReader(GenerateStreamFromString(text)));
            return data;
        }

        public static string WriteConfig(IniParser.Model.IniData data)
        {
            var parser = new FileIniDataParser(new IniDataParser(new IniParser.Model.Configuration.IniParserConfiguration
            {
                CommentString = "#"
            }));
            var stream = new MemoryStream();
            parser.WriteData(new StreamWriter(stream), data);
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        public static Configuration LoadConfiguration(string text)
        {
            var config = new Configuration();

            SectionLine currentSection = null;
            foreach(var rawline in text.Split(Environment.NewLine))
            {
                var line = rawline.Trim();
                if (line[0] == '#')
                    config.Document.Add(new LiteralLine { Text = line });
                else if (line[0] == '[')
                {
                    var section = new SectionLine { Name = line.Replace("[", "").Replace("]", "").Trim() };
                    config.Document.Add(section);
                    currentSection = section;
                } else
                {
                    var split = new List<string>(line.Split("="));
                    var key = split[0];
                    var value = string.Join("=", split.Skip(1));
                    config.Document.Add(new ConfigurationValueLine { Key = key, Value = value, SectionName = currentSection?.Name });
                }
                    
            }
            return config;
        }

        public string Render()
        {
            return string.Join(Environment.NewLine, from ln in Document select ln.Render());
        }
    }
}
