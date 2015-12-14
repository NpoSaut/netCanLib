using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using Communications.Options;
using Communications.Transactions;

namespace Communications.Tests
{
    internal class TestPort<TFrame> : IPort<TFrame>
    {
        private readonly Func<TFrame, TFrame> _answerSelector;
        private readonly bool _loopback;
        private readonly Subject<ITransaction<TFrame>> _subject;

        public TestPort(Func<TFrame, TFrame> AnswerSelector, bool Loopback)
        {
            _answerSelector = AnswerSelector;
            _loopback = Loopback;
            Options = Loopback
                          ? new PortOptions<TFrame>(new LambdaLoopbackInspector<TFrame>((a, b) => Equals(a, b)))
                          : new PortOptions<TFrame>();

            _subject = new Subject<ITransaction<TFrame>>();
            Rx = _subject;
            Rx.Subscribe(j => Debug.WriteLine(j));
        }

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<ITransaction<TFrame>> Rx { get; private set; }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<TFrame> Tx { get; private set; }

        public IPortOptions<TFrame> Options { get; private set; }

        /// <summary>Начинает отправку кадра</summary>
        /// <param name="Frame">Кадр для отправки</param>
        /// <returns>Транзакция передачи</returns>
        public ITransaction<TFrame> BeginSend(TFrame Frame)
        {
            if (_loopback)
                _subject.OnNext(new InstantaneousTransaction<TFrame>(Frame));
            _subject.OnNext(new InstantaneousTransaction<TFrame>(_answerSelector(Frame)));

            return new InstantaneousTransaction<TFrame>(Frame);
        }

        public void Dispose() { _subject.Dispose(); }
    }
}
