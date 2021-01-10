using System;

namespace LazyPersonality.Exceptions
{
    public class DbUpdatingException : Exception
    {
        public DbUpdatingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}