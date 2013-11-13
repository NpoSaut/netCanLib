using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Communications.Appi.Buffers
{
    internal abstract class AppiBufferBase
    {
        public int SequentNumber { get; set; }
        public byte BufferIdentifer { get { return GetIdentifer(this.GetType()); } }

        private static readonly Lazy<Dictionary<byte, Type>> _identifers = new Lazy<Dictionary<byte, Type>>(InitializeIdentifers, true);
        public static Dictionary<byte, Type> Identifers
        {
            get { return _identifers.Value; }
        }

        public abstract Byte[] Encode();

        protected abstract void DecodeIt(Byte[] Buffer);

        public static TBuffer Decode<TBuffer>(Byte[] Buffer) where TBuffer : AppiBufferBase, new()
        {
            var res = new TBuffer();
            res.DecodeIt(Buffer);
            res.SequentNumber = Buffer[5];
            return res;
        }

        public static AppiBufferBase Decode(Byte[] Buffer)
        {
            byte id = Buffer[0];
            if (!Identifers.ContainsKey(id)) return null;
            var res = (AppiBufferBase)Activator.CreateInstance(Identifers[id]);
            res.DecodeIt(Buffer);
            res.SequentNumber = Buffer[5];
            return res;
        }

        private static Dictionary<byte, Type> InitializeIdentifers()
        {
            return
                System.Reflection.Assembly.GetAssembly(typeof(AppiBufferBase))
                    .GetTypes()
                    .Where(T => T.IsSubclassOf(typeof(AppiBufferBase)))
                    .ToDictionary(GetIdentifer);
        }

        public static byte GetIdentifer<TBuffer>() { return GetIdentifer(typeof(TBuffer)); }
        public static byte GetIdentifer(Type MessageType)
        {
            var attr = MessageType.GetCustomAttributes(typeof(AppiBufferIdentiferAttribute), false).OfType<AppiBufferIdentiferAttribute>().FirstOrDefault();
            if (attr == null) throw new AppiBufferIdentiferAttributeNotSetException(MessageType);
            return attr.Id;
        }
    }

    internal class AppiBufferIdentiferAttributeNotSetException : Exception
    {
        public AppiBufferIdentiferAttributeNotSetException(Type MessageType) { throw new NotImplementedException(); }
    }
}
