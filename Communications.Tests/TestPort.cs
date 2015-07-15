using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Communications.Tests
{
    internal class TestPort<TFrame> : IPort<TFrame>
    {
        private readonly IDisposable _connection;

        public TestPort(Func<IObservable<TFrame>, IObservable<TFrame>> AnswerSelector, bool Loopback)
        {
            Options = Loopback
                          ? new PortOptions<TFrame>(new LambdaLoopbackInspector<TFrame>((a, b) => Equals(a, b)))
                          : new PortOptions<TFrame>();

            var subject = new Subject<TFrame>();
            Tx = subject;
            IObservable<TFrame> x = AnswerSelector(subject);
            if (Loopback)
                x = subject.Merge(x);
            IConnectableObservable<TFrame> p = x.Publish();
            Rx = p;
            Rx.Subscribe(j => Debug.WriteLine(j));
            _connection = p.Connect();
        }

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<TFrame> Rx { get; private set; }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<TFrame> Tx { get; private set; }

        public PortOptions<TFrame> Options { get; private set; }

        public void Dispose() { _connection.Dispose(); }
    }
}
