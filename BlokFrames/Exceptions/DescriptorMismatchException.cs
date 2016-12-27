using System;

namespace BlokFrames.Exceptions
{
    /// <summary>Исключение несоответствия дескриптора</summary>
    public class DescriptorMismatchException : BlokFrameException
    {
        public DescriptorMismatchException() { }

        public DescriptorMismatchException(String message)
            : base(message) { }
    }
}
