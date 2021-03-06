﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    /// <summary>
    /// INPUT_DATA
    /// Запись постоянной характеристики
    /// </summary>
    [FrameDescriptor(0x6205)]
    public class InputData : BlokFrame
    {
        private int _Index;

        public int Index
        {
            get { return _Index; }
            set
            {
                if ( value >= 1 && value <= 127 )
                    _Index = value;
                else
                    throw new IndexOutOfRangeException(String.Format("Значение номера свойства должно быть в диапазоне [1, 127] (получено: {0})", value));
            }
        }
        
        virtual public Int32 Data { get; set; }
        
        protected override byte[] Encode()
        {
            byte[] data =
                new byte[]
                {
                    (byte)(Index & 0x7F),
                    (byte)(Data >> 8*3),
                    (byte)(Data >> 8*2),
                    (byte)(Data >> 8*1),
                    (byte)(Data >> 8*0)
                };
            return data.Cast<byte>().ToArray();
        }

        protected override void Decode(byte[] Data)
        {
            this.Index = Data[0] & 0x7F;
            this.Data = BitConverter.ToInt32(new byte[4] { Data[4], Data[3], Data[2], Data[1] }, 0);
        }

        public InputData()
        { }
        public InputData(int Index, int Data)
        {
            this.Index = Index;
            this.Data = Data;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", Index, Data);
        }
    }
}
