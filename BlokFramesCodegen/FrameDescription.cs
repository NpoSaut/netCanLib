using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;
using BlokFramesCodegen.PropertyDescriptions;

namespace BlokFramesCodegen
{
    public class FrameDescription
    {
        public int Descriptor { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public List<PropertyDescription> Properties { get; private set; }

        private void FromXml(XElement XFrameDescription)
        {
            Descriptor = Convert.ToInt32(XFrameDescription.Attribute("Descriptor").Value, 16);
            Name = (string)XFrameDescription.Attribute("Name");
            Description = XFrameDescription.Element("Description").Value.Trim();

            Properties = XFrameDescription.Elements("Property").Select(XProperty => PropertyDescription.GetProperty(XProperty)).ToList();
        }

        public static FrameDescription GetFrameDescription(XElement XFrameDescription)
        {
            var res = new FrameDescription();
            res.FromXml(XFrameDescription);
            return res;
        }

        public static string GetClassName(String MessageName)
        {
            string res = "";
            bool nextToUpper = true;
            foreach (char c in MessageName.ToLower())
            {
                if (c != '_')
                {
                    Char cc = nextToUpper ? c.ToString().ToUpper()[0] : c;
                    res += cc;
                    nextToUpper = false;
                }
                else nextToUpper = true;
            }
            return res;
        }

        public CodeElement GetCode()
        {
            return
                new CodeBlock
                {
                    new CodeLine("/// <summary>{0}</summary>", Description),
                    new CodeLine("[FrameDescriptor(0x{0:x4})]", Descriptor),
                    new CodeLine("public class {0} : BlokFrame", GetClassName(Name)),
                    new CodeFramedBlock
                    {
                        Properties.Select(p => p.PropertyDefinition),
                        new CodeLine(),
                        new CodeHeaderedBlock("protected override void Decode(byte[] buff)")
                        {
                            Properties.Select(p => new CodeLine("this.{0} = Decode{0}(buff);", p.Name))
                        },
                        new CodeHeaderedBlock("protected override byte[] Encode()")
                        {
                            new CodeLine("var buff = new Byte[FrameLength];"),
                            Properties.Select(p => new CodeLine("Encode{0}(buff, {0});", p.Name)),
                            new CodeLine("return buff;")
                        },
                        Properties.Select(p => new CodeBlock { p.PropertyDecoder, p.PropertyEncoder })
                    }
                };
        }
    }
}
