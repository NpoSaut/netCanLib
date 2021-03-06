﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen.PropertyDescriptions
{
    public abstract class PropertyDescription : CodeGenerator
    {
        public String TypeName { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public PropertyPlacement Placement { get; private set; }

        protected virtual void FillFromXml(XElement XPropertyDescription)
        {
        }

        private void InternalFillFromXml(XElement XPropertyDescription)
        {
            this.Name = (string)XPropertyDescription.Attribute("Name");
            this.TypeName = (string)XPropertyDescription.Attribute("Type");
            this.Description = (string)XPropertyDescription.Attribute("Description");
            Placement = PropertyPlacement.GetPlacement(XPropertyDescription);
        }

        public static T GetProperty<T>(XElement XPropertyDescription)
            where T : PropertyDescription, new()
        {
            T res = new T();
            res.InternalFillFromXml(XPropertyDescription);
            res.FillFromXml(XPropertyDescription);
            return res;
        }

        public static PropertyDescription GetProperty(XElement XPropertyDescription)
        {
            string LowerTypeName = XPropertyDescription.Attribute("Type").Value.ToLower();

            if (GetPropertyTypenames<NumericPropertyDescription>().Contains(LowerTypeName))
                return GetProperty<NumericPropertyDescription>(XPropertyDescription);

            else if (GetPropertyTypenames<BooleanPropertyDescription>().Contains(LowerTypeName))
                return GetProperty<BooleanPropertyDescription>(XPropertyDescription);

            else if (GetPropertyTypenames<EnumPropertyDescription>().Contains(LowerTypeName))
                return GetProperty<EnumPropertyDescription>(XPropertyDescription);

            else throw new Exception("Не поддерживаемый тип свойства");
        }

        public virtual CodeBlock PropertyDefinition
        {
            get
            {
                return new CodeBlock()
                {
                    new CodeLine("[System.ComponentModel.Description(\"{0}\")]", Description),
                    new CodeLine("/// <summary>{0}</summary>", Description),
                    new CodeLine("public {0} {1} {{ get; set; }}", TypeName, Name),
                    new CodeLine()
                };
            }
        }

        protected abstract CodeElement GetPropertyEncoderBody();
        public CodeElement PropertyEncoder
        {
            get
            {
                return new CodeHeaderedBlock("private void Encode{0}(Byte[] buff, {1} value)", Name, TypeName)
                {
                    GetPropertyEncoderBody()
                };
            }
        }

        protected abstract CodeElement GetPropertyDecoderBody();
        public CodeElement PropertyDecoder
        {
            get
            {
                return new CodeHeaderedBlock("private {1} Decode{0}(Byte[] buff)", Name, TypeName)
                {
                    GetPropertyDecoderBody()
                };
            }
        }

        #region Получение списка типов свойства
        protected static HashSet<String> GetPropertyTypenames(Type t)
        {
            return PropertyTypeNameAttribute.Get(t).TypeNames;
        }
        protected static HashSet<String> GetPropertyTypenames<T>() where T : PropertyDescription
        {
            return GetPropertyTypenames(typeof(T));
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, TypeName);
        }
    }
}
