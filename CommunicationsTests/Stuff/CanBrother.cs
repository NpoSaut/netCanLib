using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Sockets;

namespace CommunicationsTests.Stuff
{

    public class CanBrother : CanSocket
    {
        private Brotherhood Family { get; set; }

        private CanBrother(string Name, Brotherhood Family) : base(Name) { this.Family = Family; }

        /// <summary>
        /// Отправляет дейтаграммы в сокет
        /// </summary>
        public override void Send(IEnumerable<CanFrame> Data) { Family.SendToOthers(Data, this); }

        public static IList<CanBrother> TakeBrothers(int Count)
        {
            return new Brotherhood(Count).Brothers;
        }

        private class Brotherhood
        {
            public List<CanBrother> Brothers { get; private set; }

            public Brotherhood(int Count)
            {
                Brothers =
                    Enumerable.Range(0, Count)
                              .Select(i => new CanBrother(string.Format("Brother #{0}", i), this))
                              .ToList();
            }

            public void SendToOthers(IEnumerable<CanFrame> Data, CanBrother Author)
            {
                var dataList = Data.ToList();
                foreach (var brother in Brothers.Where(br => br != Author))
                {
                    (brother as IBufferedStore<CanFrame>).Enqueue(dataList);
                }
            }
        }
    }
}