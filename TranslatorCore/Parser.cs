// TranslatorCore/Parser.cs
using System;
using System.Collections.Generic;

namespace TranslatorCore
{
    public abstract class AstNode { }

    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } = new();
    }

    public abstract class ExpressionNode : AstNode { }

    public class NumberNode : ExpressionNode
    {
        public string Value { get; }
        public NumberNode(string value) => Value = value;
    }

    public class StringNode : ExpressionNode
    {
        public string Value { get; }
        public StringNode(string value) => Value = value;
    }

    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; }
        public IdentifierNode(string name) => Name = name;
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(ExpressionNode left, string op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    public class CallExpressionNode : ExpressionNode
    {
        public string FunctionName { get; }
        public List<ExpressionNode> Arguments { get; } = new();
        public CallExpressionNode(string functionName) => FunctionName = functionName;
    }

    public class ListNode : ExpressionNode
    {
        public List<ExpressionNode> Elements { get; } = new();
    }

    public abstract class StatementNode : AstNode { }

    public class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; }
        public ExpressionStatementNode(ExpressionNode expr) => Expression = expr;
    }

    public class AssignmentNode : StatementNode
    {
        public IdentifierNode Target { get; }
        public ExpressionNode Value { get; }
        public AssignmentNode(IdentifierNode target, ExpressionNode value)
        {
            Target = target;
            Value = value;
        }
    }

    public class IfNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public List<AstNode> ThenBranch { get; } = new();
        public List<AstNode> ElseBranch { get; } = new();
        public IfNode(ExpressionNode cond) => Condition = cond;
    }

    public class WhileNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public List<AstNode> Body { get; } = new();
        public WhileNode(ExpressionNode cond) => Condition = cond;
    }

    public class ForNode : StatementNode
    {
        public IdentifierNode Iterator { get; }
        public ExpressionNode Start { get; }
        public ExpressionNode End { get; }
        public List<AstNode> Body { get; } = new();
        public ForNode(IdentifierNode iter, ExpressionNode start, ExpressionNode end)
        {
            Iterator = iter;
            Start = start;
            End = end;
        }
    }

    public class FunctionNode : StatementNode
    {
        public string Name { get; }
        public List<string> Parameters { get; } = new();
        public List<AstNode> Body { get; } = new();
        public FunctionNode(string name) => Name = name;
    }

    public class ReturnNode : StatementNode
    {
        public ExpressionNode Expression { get; }
        public ReturnNode(ExpressionNode expr) => Expression = expr;
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        private Token Current => _tokens[_position];

        private Token Consume() => _tokens[_position++];

        private bool Match(string value)
        {
            if (Current.Value == value)
            {
                _position++;
                return true;
            }
            return false;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            while (Current.Type != TokenType.EndOfFile)
                program.Statements.Add(ParseStatement());
            return program;
        }

        private AstNode ParseStatement()
        {
            if (Current.Value == "if") return ParseIf();
            if (Current.Value == "while") return ParseWhile();
            if (Current.Value == "for") return ParseFor();
            if (Current.Value == "def") return ParseFunction();
            if (Current.Value == "return") return ParseReturn();
            return ParseExpressionOrAssignment();
        }

        private IfNode ParseIf()
        {
            Consume(); // if
            var cond = ParseExpression();
            Match(":");
            var node = new IfNode(cond);
            while (Current.Type != TokenType.EndOfFile && Current.Value != "else")
                node.ThenBranch.Add(ParseStatement());
            if (Match("else"))
            {
                Match(":");
                while (Current.Type != TokenType.EndOfFile)
                    node.ElseBranch.Add(ParseStatement());
            }
            return node;
        }

        private WhileNode ParseWhile()
        {
            Consume(); // while
            var cond = ParseExpression();
            Match(":");
            var node = new WhileNode(cond);
            while (Current.Type != TokenType.EndOfFile)
                node.Body.Add(ParseStatement());
            return node;
        }

        private ForNode ParseFor()
        {
            Consume(); // for
            var iter = new IdentifierNode(Consume().Value);
            Match("in");
            Match("range");
            Match("(");
            var start = ParseExpression();
            Match(",");
            var end = ParseExpression();
            Match(")");
            Match(":");
            var node = new ForNode(iter, start, end);
            while (Current.Type != TokenType.EndOfFile)
                node.Body.Add(ParseStatement());
            return node;
        }

        private FunctionNode ParseFunction()
        {
            Consume(); // def
            var name = Consume().Value;
            Match("(");
            var fn = new FunctionNode(name);
            if (!Match(")"))
            {
                do { fn.Parameters.Add(Consume().Value); }
                while (Match(","));
                Match(")");
            }
            Match(":");
            while (Current.Type != TokenType.EndOfFile)
                fn.Body.Add(ParseStatement());
            return fn;
        }

        private ReturnNode ParseReturn()
        {
            Consume(); // return
            var expr = ParseExpression();
            return new ReturnNode(expr);
        }

        private AstNode ParseExpressionOrAssignment()
        {
            var expr = ParseExpression();
            if (expr is IdentifierNode id && Match("="))
            {
                var value = ParseExpression();
                return new AssignmentNode(id, value);
            }
            if (Match("["))
            {
                var list = new ListNode();
                if (!Match("]"))
                {
                    do { list.Elements.Add(ParseExpression()); }
                    while (Match(","));
                    Match("]");
                }
                return new ExpressionStatementNode(list);
            }
            return new ExpressionStatementNode(expr);
        }

        private ExpressionNode ParseExpression()
        {
            var left = ParsePrimary();
            while (new[] { "+", "-", "*", "/", "%", "==", "!=", "<", "<=", ">", ">=" }.Contains(Current.Value))
            {
                var op = Consume().Value;
                var right = ParsePrimary();
                left = new BinaryExpressionNode(left, op, right);
            }
            return left;
        }

        private ExpressionNode ParsePrimary()
        {
            if (Current.Type == TokenType.Number)
                return new NumberNode(Consume().Value);
            if (Current.Type == TokenType.String)
                return new StringNode(Consume().Value);
            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.Keyword)
            {
                var name = Consume().Value;
                if (Match("("))
                {
                    var call = new CallExpressionNode(name);
                    if (!Match(")"))
                    {
                        do { call.Arguments.Add(ParseExpression()); }
                        while (Match(","));
                        Match(")");
                    }
                    return call;
                }
                return new IdentifierNode(name);
            }
            if (Match("("))
            {
                var expr = ParseExpression();
                Match(")");
                return expr;
            }
            throw new TranslationException($"Неожиданный токен '{Current.Value}' на позиции {Current.Position}");
        }
    }
}
