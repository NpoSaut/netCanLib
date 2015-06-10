using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Communications.Appi
{
    internal static class ReactiveHelper
    {
        public static IObservable<IList<T>> Limit<T>(this IObservable<T> source, IObservable<int> limitations) { return new Buffered<T>(source, limitations); }

        private class Buffered<T> : IObservable<IList<T>>
        {
            private readonly ConcurrentQueue<T> _buffer = new ConcurrentQueue<T>();
            private readonly IDisposable _limitationsSubscription;
            private readonly IDisposable _sourceSubscription;
            private readonly Subject<IList<T>> _subject = new Subject<IList<T>>();

            public Buffered(IObservable<T> Source, IObservable<int> Limitations)
            {
                _sourceSubscription = Source.Subscribe(OnNextData);
                _limitationsSubscription = Limitations.Subscribe(Push);
            }

            public IDisposable Subscribe(IObserver<IList<T>> observer) { return new DelegateDisposable(_subject.Subscribe(observer), CheckToUnsubscribe); }

            private void CheckToUnsubscribe()
            {
                if (!_subject.HasObservers)
                {
                    _sourceSubscription.Dispose();
                    _limitationsSubscription.Dispose();
                }
            }

            private void OnNextData(T Data) { _buffer.Enqueue(Data); }

            private void Push(int Count)
            {
                if (_buffer.IsEmpty) return;

                var list = new List<T>();
                T item;
                while (list.Count < Count && _buffer.TryDequeue(out item))
                    list.Add(item);

                if (list.Count > 0)
                    _subject.OnNext(list);
            }

            private class DelegateDisposable : IDisposable
            {
                private readonly IDisposable _disposable;
                private readonly Action _disposingAction;

                public DelegateDisposable(IDisposable Disposable, Action DisposingAction)
                {
                    _disposingAction = DisposingAction;
                    _disposable = Disposable;
                }

                public void Dispose()
                {
                    _disposingAction();
                    _disposable.Dispose();
                }
            }
        }
    }
}
