using System;

namespace TranslatorCore
{
    public class TranslationException : Exception
    {
        public TranslationException(string message) : base(message) { }
    }
}
