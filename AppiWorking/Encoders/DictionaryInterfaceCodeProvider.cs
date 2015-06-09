using System.Collections.Generic;

namespace Communications.Appi.Encoders
{
    public class DictionaryInterfaceCodeProvider<TLineKey> : IInterfaceCodeProvider<TLineKey>
    {
        private readonly IDictionary<TLineKey, byte> _interfaceCodes;
        public DictionaryInterfaceCodeProvider(IDictionary<TLineKey, byte> InterfaceCodes) { _interfaceCodes = InterfaceCodes; }
        public byte GetInterfaceCode(TLineKey Interface) { return _interfaceCodes[Interface]; }
    }
}