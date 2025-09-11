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
        NewLine,
        Indent,
        Dedent,
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
        private int _pos;
        private int _lineStart;
        private readonly Stack<int> _indentStack = new();

        private static readonly HashSet<string> Keywords = new() { "if", "else", "while", "for", "def", "return", "input", "print", "import", "set", "range" };
        private static readonly HashSet<string> TwoCharSymbols = new() { "==", "!=", "<=", ">=", "//", "**" };
        private static readonly HashSet<char> SingleSymbols = new() { '(', ')', ':', ',', '+', '-', '*', '/', '%', '=', '[', ']', '{', '}', '<', '>' };

        public Tokenizer(string source)
        {
            _source = source.Replace("\r\n", "\n");
            _pos = 0;
            _lineStart = 0;
            _indentStack.Push(0);
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (true)
            {
                if (_pos >= _source.Length)
                {
                    while (_indentStack.Count > 1)
                    {
                        _indentStack.Pop();
                        tokens.Add(new Token(TokenType.Dedent, "", _pos));
                    }
                    tokens.Add(new Token(TokenType.EndOfFile, "", _pos));
                    break;
                }

                if (_pos == _lineStart)
                {
                    int count = 0;
                    while (_pos < _source.Length && _source[_pos] == ' ')
                    {
                        count++;
                        _pos++;
                    }
                    if (_pos < _source.Length && _source[_pos] == '\n')
                    {
                        _pos++;
                        _lineStart = _pos;
                        tokens.Add(new Token(TokenType.NewLine, "", _pos));
                        continue;
                    }
                    int prev = _indentStack.Peek();
                    if (count > prev)
                    {
                        _indentStack.Push(count);
                        tokens.Add(new Token(TokenType.Indent, "", _pos));
                    }
                    else
                    {
                        while (count < prev)
                        {
                            _indentStack.Pop();
                            prev = _indentStack.Peek();
                            tokens.Add(new Token(TokenType.Dedent, "", _pos));
                        }
                    }
                }

                char c = _source[_pos];

                // Comments
                if (c == '#')
                {
                    while (_pos < _source.Length && _source[_pos] != '\n')
                        _pos++;
                    continue;
                }

                if (c == '\n')
                {
                    tokens.Add(new Token(TokenType.NewLine, "", _pos));
                    _pos++;
                    _lineStart = _pos;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    int start = _pos;
                    var sb = new StringBuilder();
                    while (_pos < _source.Length && (char.IsLetterOrDigit(_source[_pos]) || _source[_pos] == '_'))
                        sb.Append(_source[_pos++]);
                    string w = sb.ToString();
                    tokens.Add(new Token(Keywords.Contains(w) ? TokenType.Keyword : TokenType.Identifier, w, start));
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int start = _pos;
                    var sb = new StringBuilder();
                    while (_pos < _source.Length && char.IsDigit(_source[_pos]))
                        sb.Append(_source[_pos++]);
                    tokens.Add(new Token(TokenType.Number, sb.ToString(), start));
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    char q = c;
                    int start = _pos++;
                    var sb = new StringBuilder();
                    while (_pos < _source.Length && _source[_pos] != q)
                        sb.Append(_source[_pos++]);
                    _pos++;
                    tokens.Add(new Token(TokenType.String, sb.ToString(), start));
                    continue;
                }

                if (_pos + 1 < _source.Length)
                {
                    string two = _source.Substring(_pos, 2);
                    if (TwoCharSymbols.Contains(two))
                    {
                        tokens.Add(new Token(TokenType.Symbol, two, _pos));
                        _pos += 2;
                        continue;
                    }
                }

                if (SingleSymbols.Contains(c))
                {
                    tokens.Add(new Token(TokenType.Symbol, c.ToString(), _pos));
                    _pos++;
                    continue;
                }

                throw new TranslationException($"Неожиданный символ '{c}' на позиции {_pos}");
            }
            return tokens;
        }
    }
}
