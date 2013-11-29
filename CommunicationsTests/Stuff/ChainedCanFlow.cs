using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Communications.Can;

namespace CommunicationsTests.Stuff
{
    public class ChainedCanFlow
    {
        private IList<ChainedCanFlow> Brothers { get; set; }

        /// <summary>Отправляет Can-сообщение в поток</summary>
        public void Send(CanFrame Frame, bool ClearBeforeSend = false)
        {
            foreach (var brother in Brothers)
            {
                //brother.Enqueue(Frame);
            }
        }

        /// <summary>Отправляет несколько Can-сообщений в поток</summary>
        public void Send(IList<CanFrame> Frames, bool ClearBeforeSend = false)
        {
            foreach (var brother in Brothers)
            {
                //brother.Enqueue(Frames);
            }
        }

        /// <summary>
        /// Список дескрипторов, отлавливаемых в данный поток
        /// </summary>
        public ReadOnlyCollection<int> Descriptors { get; private set; }

        public ChainedCanFlow(params int[] Descriptors) { this.Descriptors = new ReadOnlyCollection<int>(Descriptors); }

        public class Brotherhood : List<ChainedCanFlow>
        {
            public Brotherhood(int Count, params int[] Descriptors)
                : base(Enumerable.Range(0, Count).Select(i => new ChainedCanFlow(Descriptors)))
            {
                foreach (var brother in this)
                {
                    brother.Brothers = this;
                }
            }
        }

        public static Brotherhood Take(int Count, params int[] Descriptors)
        {
            return new Brotherhood(Count, Descriptors);
        }
    }
}