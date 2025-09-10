// TranslatorCore/Tokenizer.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace TranslatorCore
{
    public enum TokenType
    {
        Identifier,
        Number,
        String,
        Keyword,
        Symbol,
        EndOfFile
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }
    }

    public class Tokenizer
    {
        private readonly string _source;
        private int _position;
        private static readonly HashSet<string> Keywords = new() { "if", "else", "while", "for", "def", "return", "input", "print" };
        private static readonly HashSet<string> TwoCharSymbols = new() { "==", "!=", "<=", ">=", "//", "**" };
        private static readonly HashSet<char> SingleSymbols = new() { '(', ')', ':', ',', '+', '-', '*', '/', '%', '=', '[', ']', '{', '}', '<', '>' };

        public Tokenizer(string source)
        {
            _source = source;
            _position = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (true)
            {
                SkipWhitespace();
                if (_position >= _source.Length)
                {
                    tokens.Add(new Token(TokenType.EndOfFile, string.Empty, _position));
                    break;
                }

                char current = _source[_position];

                // Identifier or keyword
                if (char.IsLetter(current) || current == '_')
                {
                    int start = _position;
                    var sb = new StringBuilder();
                    while (_position < _source.Length && (char.IsLetterOrDigit(_source[_position]) || _source[_position] == '_'))
                    {
                        sb.Append(_source[_position]);
                        _position++;
                    }
                    string word = sb.ToString();
                    tokens.Add(new Token(Keywords.Contains(word) ? TokenType.Keyword : TokenType.Identifier, word, start));
                    continue;
                }

                // Number literal
                if (char.IsDigit(current))
                {
                    int start = _position;
                    var sb = new StringBuilder();
                    while (_position < _source.Length && char.IsDigit(_source[_position]))
                    {
                        sb.Append(_source[_position]);
                        _position++;
                    }
                    tokens.Add(new Token(TokenType.Number, sb.ToString(), start));
                    continue;
                }

                // String literal
                if (current == '"' || current == '\'')
                {
                    char quote = current;
                    int start = _position;
                    _position++;
                    var sb = new StringBuilder();
                    while (_position < _source.Length && _source[_position] != quote)
                    {
                        sb.Append(_source[_position]);
                        _position++;
                    }
                    _position++; // closing quote
                    tokens.Add(new Token(TokenType.String, sb.ToString(), start));
                    continue;
                }

                // Two-character symbols
                if (_position + 1 < _source.Length)
                {
                    string two = _source.Substring(_position, 2);
                    if (TwoCharSymbols.Contains(two))
                    {
                        tokens.Add(new Token(TokenType.Symbol, two, _position));
                        _position += 2;
                        continue;
                    }
                }

                // Single-character symbols
                if (SingleSymbols.Contains(current))
                {
                    tokens.Add(new Token(TokenType.Symbol, current.ToString(), _position));
                    _position++;
                    continue;
                }

                throw new TranslationException($"Неожиданный символ '{current}' на позиции {_position}");
            }

            return tokens;
        }

        private void SkipWhitespace()
        {
            while (_position < _source.Length && char.IsWhiteSpace(_source[_position]))
            {
                _position++;
            }
        }
    }
}
