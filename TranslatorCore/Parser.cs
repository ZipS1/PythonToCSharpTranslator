using System;
using System.Collections.Generic;

namespace TranslatorCore
{
    public abstract class AstNode { }

    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } = new();
    }

    public class ExpressionNode : AstNode { }

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
            Iterator = iter; Start = start; End = end;
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

        private Token Consume()
        {
            var tok = Current;
            _position++;
            return tok;
        }

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
            {
                program.Statements.Add(ParseStatement());
            }
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

        private AstNode ParseIf()
        {
            Consume(); // if
            var condition = ParseExpression();
            Match(":");
            var ifNode = new IfNode(condition);
            while (!Match("else") && Current.Value != "" && Current.Type != TokenType.EndOfFile)
            {
                ifNode.ThenBranch.Add(ParseStatement());
            }
            if (Match("else"))
            {
                Match(":");
                while (Current.Type != TokenType.EndOfFile && Current.Value != "")
                {
                    ifNode.ElseBranch.Add(ParseStatement());
                }
            }
            return ifNode;
        }

        private AstNode ParseWhile()
        {
            Consume(); // while
            var condition = ParseExpression();
            Match(":");
            var whileNode = new WhileNode(condition);
            while (Current.Type != TokenType.EndOfFile && Current.Value != "")
            {
                whileNode.Body.Add(ParseStatement());
            }
            return whileNode;
        }

        private AstNode ParseFor()
        {
            Consume(); // for
            var iter = new IdentifierNode(Consume().Value);
            Match("in");
            // only support range(start, end)
            Match("range");
            Match("(");
            var start = ParseExpression();
            Match(",");
            var end = ParseExpression();
            Match(")");
            Match(":");
            var forNode = new ForNode(iter, start, end);
            while (Current.Type != TokenType.EndOfFile && Current.Value != "")
            {
                forNode.Body.Add(ParseStatement());
            }
            return forNode;
        }

        private AstNode ParseFunction()
        {
            Consume(); // def
            string name = Consume().Value;
            Match("(");
            var fn = new FunctionNode(name);
            if (!Match(")"))
            {
                do
                {
                    fn.Parameters.Add(Consume().Value);
                } while (Match(","));
                Match(")");
            }
            Match(":");
            while (Current.Type != TokenType.EndOfFile && Current.Value != "")
            {
                fn.Body.Add(ParseStatement());
            }
            return fn;
        }

        private AstNode ParseReturn()
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
            return new ExpressionStatementNode(expr);
        }

        private ExpressionNode ParseExpression()
        {
            var left = ParsePrimary();
            while (Current.Value == "+" || Current.Value == "-" || Current.Value == "*" || Current.Value == "/" ||
                   Current.Value == "<" || Current.Value == ">" || Current.Value == "==")
            {
                string op = Consume().Value;
                var right = ParsePrimary();
                left = new BinaryExpressionNode(left, op, right);
            }
            return left;
        }

        private ExpressionNode ParsePrimary()
        {
            if (Current.Type == TokenType.Number)
            {
                var node = new NumberNode(Current.Value);
                Consume();
                return node;
            }
            if (Current.Type == TokenType.String)
            {
                var node = new StringNode(Current.Value);
                Consume();
                return node;
            }
            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.Keyword)
            {
                var name = Current.Value;
                Consume();
                return new IdentifierNode(name);
            }
            if (Match("("))
            {
                var expr = ParseExpression();
                Match(")");
                return expr;
            }
            throw new TranslationException($"Unexpected token {Current.Value} at position {Current.Position}");
        }
    }
}
