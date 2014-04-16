using System;

namespace IsoTpTest.StatesTests
{
    public abstract class IsoTpStateTest
    {
        protected readonly Random Rnd = new Random();

        protected Byte[] GetRandomBytes(int Count)
        {
            var res = new byte[Count];
            Rnd.NextBytes(res);
            return res;
        }
    }
}