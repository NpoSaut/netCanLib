using System;

namespace Communications
{
    /// <summary>
    /// Объект, имеющий имя
    /// </summary>
    public interface INamable
    {
        /// <summary>
        /// Имя объекта
        /// </summary>
        String Name { get; }
    }
}