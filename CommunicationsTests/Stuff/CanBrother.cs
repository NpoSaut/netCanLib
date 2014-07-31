using System.Collections.Generic;
using System.Linq;
using Communications.Can;

namespace CommunicationsTests.Stuff
{
    public class CanBrother : CanSocket
    {
        private CanBrother(string Name, Brotherhood Family) : base(Name) { this.Family = Family; }
        private Brotherhood Family { get; set; }

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        public override void Send(IEnumerable<CanFrame> Data) { Family.SendToOthers(Data, this); }

        public static IList<CanBrother> TakeBrothers(int Count) { return new Brotherhood(Count).Brothers; }

        private class Brotherhood
        {
            public Brotherhood(int Count)
            {
                Brothers =
                    Enumerable.Range(0, Count)
                              .Select(i => new CanBrother(string.Format("Brother #{0}", i), this))
                              .ToList();
            }

            public List<CanBrother> Brothers { get; private set; }

            public void SendToOthers(IEnumerable<CanFrame> Data, CanBrother Author)
            {
                List<CanFrame> dataList = Data.ToList();
                foreach (CanBrother brother in Brothers.Where(br => br != Author))
                    (brother as IBufferedStore<CanFrame>).Enqueue(dataList);
            }
        }
    }
}
