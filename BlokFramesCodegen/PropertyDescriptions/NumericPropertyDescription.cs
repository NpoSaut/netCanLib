﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BlokFramesCodegen.CodeGeneration;

namespace BlokFramesCodegen.PropertyDescriptions
{
    [PropertyTypeName("int", "int32", "uint32", "int16", "uint16", "byte")]
    public class NumericPropertyDescription : PropertyDescription
    {
        private int _sizeof()
        {
            if (TypeName.Contains("32")) return 4;
            else if (TypeName.Contains("16")) return 2;
            else if (TypeName.ToLower() == "byte") return 1;
            else throw new Exception("Формат не поддерживается");
        }

        protected override CodeElement GetPropertyDecoderBody()
        {
            return new CodeBlock()
            {
                Placement.GetExtractor("buff", "raw"),
                new CodeLine("return unchecked(({0})raw);", TypeName)
            };
        }

        protected override CodeElement GetPropertyEncoderBody()
        {
            return Placement.GetSetter("buff", "value");
        }
    }
}
