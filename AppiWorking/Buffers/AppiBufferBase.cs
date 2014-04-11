using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Communications.Appi.Buffers
{
    public abstract class AppiBufferBase
    {
        public int SequentNumber { get; set; }
        public byte BufferIdentifier { get { return GetIdentifier(this.GetType()); } }

        private static readonly Lazy<Dictionary<byte, Type>> _identifiers = new Lazy<Dictionary<byte, Type>>(InitializeIdentifiers, true);
        public static Dictionary<byte, Type> Identifiers
        {
            get { return _identifiers.Value; }
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
            if (!Identifiers.ContainsKey(id)) return null;
            var res = (AppiBufferBase)Activator.CreateInstance(Identifiers[id]);
            res.DecodeIt(Buffer);
            res.SequentNumber = Buffer[5];
            return res;
        }

        private static Dictionary<byte, Type> InitializeIdentifiers()
        {
            return
                System.Reflection.Assembly.GetAssembly(typeof(AppiBufferBase))
                    .GetTypes()
                    .Where(T => T.IsSubclassOf(typeof(AppiBufferBase)))
                    .ToDictionary(GetIdentifier);
        }

        public static byte GetIdentifier<TBuffer>() { return GetIdentifier(typeof(TBuffer)); }
        public static byte GetIdentifier(Type MessageType)
        {
            var attr = MessageType.GetCustomAttributes(typeof(AppiBufferIdentifierAttribute), false).OfType<AppiBufferIdentifierAttribute>().FirstOrDefault();
            if (attr == null) throw new AppiBufferIdentifierAttributeNotSetException(MessageType);
            return attr.Id;
        }
    }

    internal class AppiBufferIdentifierAttributeNotSetException : Exception
    {
        public AppiBufferIdentifierAttributeNotSetException(Type MessageType) { throw new NotImplementedException(); }
    }
}
