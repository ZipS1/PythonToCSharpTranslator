// TranslatorCore/Parser.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace TranslatorCore
{
    public abstract class AstNode { }

    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } = new();
    }

    public abstract class ExpressionNode : AstNode { }

    public class NumberNode : ExpressionNode { public string Value; public NumberNode(string v) => Value = v; }
    public class StringNode : ExpressionNode { public string Value; public StringNode(string v) => Value = v; }
    public class IdentifierNode : ExpressionNode { public string Name; public IdentifierNode(string n) => Name = n; }
    public class BinaryExpressionNode : ExpressionNode { public ExpressionNode Left; public string Operator; public ExpressionNode Right; public BinaryExpressionNode(ExpressionNode l, string op, ExpressionNode r) { Left = l; Operator = op; Right = r; } }
    public class CallExpressionNode : ExpressionNode { public string FunctionName; public List<ExpressionNode> Arguments = new(); public CallExpressionNode(string fn) => FunctionName = fn; }
    public class ListNode : ExpressionNode { public List<ExpressionNode> Elements = new(); }
    public class IndexExpressionNode : ExpressionNode { public ExpressionNode Collection, Index; public IndexExpressionNode(ExpressionNode c, ExpressionNode i) { Collection = c; Index = i; } }

    public abstract class StatementNode : AstNode { }

    public class ExpressionStatementNode : StatementNode { public ExpressionNode Expression; public ExpressionStatementNode(ExpressionNode e) => Expression = e; }
    public class AssignmentNode : StatementNode { public IdentifierNode Target; public ExpressionNode Value; public AssignmentNode(IdentifierNode t, ExpressionNode v) { Target = t; Value = v; } }
    public class IfNode : StatementNode { public ExpressionNode Condition; public List<AstNode> ThenBranch = new(), ElseBranch = new(); public IfNode(ExpressionNode c) => Condition = c; }
    public class WhileNode : StatementNode { public ExpressionNode Condition; public List<AstNode> Body = new(); public WhileNode(ExpressionNode c) => Condition = c; }
    public class ForNode : StatementNode { public IdentifierNode Iterator; public ExpressionNode Start, End; public List<AstNode> Body = new(); public ForNode(IdentifierNode i, ExpressionNode s, ExpressionNode e) { Iterator = i; Start = s; End = e; } }
    public class FunctionNode : StatementNode { public string Name; public List<string> Parameters = new(); public List<AstNode> Body = new(); public FunctionNode(string n) => Name = n; }
    public class ReturnNode : StatementNode { public ExpressionNode Expression; public ReturnNode(ExpressionNode e) => Expression = e; }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private static readonly string[] BinOps = { "+", "-", "*", "/", "%", "==", "!=", ">=", "<=", "<", ">" };

        public Parser(List<Token> tokens) { _tokens = tokens; _pos = 0; }
        private Token Curr => _tokens[_pos];
        private Token Consume() => _tokens[_pos++];
        private bool Match(TokenType t) { if (Curr.Type == t) { _pos++; return true; } return false; }
        private bool MatchValue(string v) { if (Curr.Value == v) { _pos++; return true; } return false; }

        public ProgramNode Parse()
        {
            var prog = new ProgramNode();
            while (!Match(TokenType.EndOfFile))
                prog.Statements.Add(ParseStmt());
            return prog;
        }

        private StatementNode ParseStmt()
        {
            if (MatchValue("def")) return ParseDef();
            if (MatchValue("if")) return ParseIf();
            if (MatchValue("while")) return ParseWhile();
            if (MatchValue("for")) return ParseFor();
            if (MatchValue("return")) return ParseReturn();
            return ParseExprOrAssign();
        }

        private FunctionNode ParseDef()
        {
            var name = Consume().Value;
            Consume(); // '('
            var fn = new FunctionNode(name);
            if (!MatchValue(")"))
            {
                do { fn.Parameters.Add(Consume().Value); }
                while (MatchValue(","));
                Consume(); // ')'
            }
            Consume(); // ':'
            Match(TokenType.NewLine);
            Consume(); // Indent
            while (!Match(TokenType.Dedent))
                fn.Body.Add(ParseStmt());
            return fn;
        }

        private IfNode ParseIf()
        {
            var cond = ParseExpr();
            Consume(); // ':'
            Match(TokenType.NewLine);
            Consume(); // Indent
            var node = new IfNode(cond);
            while (!Match(TokenType.Dedent))
                node.ThenBranch.Add(ParseStmt());
            if (MatchValue("else"))
            {
                Consume(); // ':'
                Match(TokenType.NewLine);
                Consume(); // Indent
                while (!Match(TokenType.Dedent))
                    node.ElseBranch.Add(ParseStmt());
            }
            return node;
        }

        private WhileNode ParseWhile()
        {
            var cond = ParseExpr();
            Consume(); // ':'
            Match(TokenType.NewLine);
            Consume(); // Indent
            var node = new WhileNode(cond);
            while (!Match(TokenType.Dedent))
                node.Body.Add(ParseStmt());
            return node;
        }

        private ForNode ParseFor()
        {
            var iter = new IdentifierNode(Consume().Value);
            Consume(); // 'in'
            Consume(); // 'range'
            Consume(); // '('
            var start = ParseExpr();
            Consume(); // ','
            var end = ParseExpr();
            Consume(); // ')'
            Consume(); // ':'
            Match(TokenType.NewLine);
            Consume(); // Indent
            var node = new ForNode(iter, start, end);
            while (!Match(TokenType.Dedent))
                node.Body.Add(ParseStmt());
            return node;
        }

        private ReturnNode ParseReturn()
        {
            var expr = ParseExpr();
            Consume(); // NewLine
            return new ReturnNode(expr);
        }

        private StatementNode ParseExprOrAssign()
        {
            var expr = ParseExpr();
            if (expr is IdentifierNode id && MatchValue("="))
            {
                var val = ParseExpr();
                Consume(); // NewLine
                return new AssignmentNode(id, val);
            }
            Consume(); // NewLine
            return new ExpressionStatementNode(expr);
        }

        private ExpressionNode ParseExpr()
        {
            var left = ParsePrimary();
            while (BinOps.Contains(Curr.Value))
            {
                var op = Consume().Value;
                var right = ParsePrimary();
                left = new BinaryExpressionNode(left, op, right);
            }
            return left;
        }

        private ExpressionNode ParsePrimary()
        {
            if (Match(TokenType.Number)) return new NumberNode(_tokens[_pos - 1].Value);
            if (Match(TokenType.String)) return new StringNode(_tokens[_pos - 1].Value);
            if (Match(TokenType.Identifier) || Match(TokenType.Keyword))
            {
                var nm = _tokens[_pos - 1].Value;
                if (MatchValue("("))
                {
                    var call = new CallExpressionNode(nm);
                    if (!MatchValue(")"))
                    {
                        do { call.Arguments.Add(ParseExpr()); }
                        while (MatchValue(","));
                        Consume(); // ')'
                    }
                    return ParseIndexOrCall(call);
                }
                return ParseIndexOrCall(new IdentifierNode(nm));
            }
            if (MatchValue("("))
            {
                var e = ParseExpr();
                Consume();
                return e;
            }
            if (MatchValue("["))
            {
                var list = new ListNode();
                if (!MatchValue("]"))
                {
                    do { list.Elements.Add(ParseExpr()); }
                    while (MatchValue(","));
                    Consume(); // ']'
                }
                return list;
            }
            throw new TranslationException($"Неожиданный токен '{Curr.Value}' на позиции {Curr.Position}");
        }

        private ExpressionNode ParseIndexOrCall(ExpressionNode target)
        {
            while (MatchValue("["))
            {
                var idx = ParseExpr();
                Consume(); // ']'
                target = new IndexExpressionNode(target, idx);
            }
            return target;
        }
    }
}
