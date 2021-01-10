using System;

namespace LazyPersonality.Exceptions
{
    public class DbGettingException : Exception
    {
        public DbGettingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}