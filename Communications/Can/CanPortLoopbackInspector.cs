using System.Linq;

namespace Communications.Can
{
    /// <summary>
    ///     <seealso cref="ILoopbackInspector{TFrame}" /> для Can-портов без заморочек
    /// </summary>
    public class CanPortLoopbackInspector : ILoopbackInspector<CanFrame>
    {
        /// <summary>Определяет, является ли один пакет Loopback-пакетом для другого</summary>
        /// <param name="Candidate">Проверяемый пакет</param>
        /// <param name="Original">Оригинальный пакет</param>
        /// <returns>True, если <paramref name="Candidate" /> является Loopback-пакетом для <paramref name="Original" /></returns>
        public bool IsLoopback(CanFrame Candidate, CanFrame Original)
        {
            if (!Candidate.IsLoopback) return false;
            if (Candidate.Id != Original.Id) return false;
            if (!Candidate.Data.SequenceEqual(Original.Data)) return false;
            return true;
        }
    }
}
