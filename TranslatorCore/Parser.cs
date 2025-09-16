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
    public class UnaryExpressionNode : ExpressionNode { public string Operator; public ExpressionNode Operand; public UnaryExpressionNode(string op, ExpressionNode operand) { Operator = op; Operand = operand; } }
    public class BinaryExpressionNode : ExpressionNode { public ExpressionNode Left; public string Operator; public ExpressionNode Right; public BinaryExpressionNode(ExpressionNode l, string op, ExpressionNode r) { Left = l; Operator = op; Right = r; } }
    public class CallExpressionNode : ExpressionNode { public string FunctionName; public List<ExpressionNode> Arguments = new(); public CallExpressionNode(string fn) => FunctionName = fn; }
    public class ListNode : ExpressionNode { public List<ExpressionNode> Elements = new(); }
    public class IndexExpressionNode : ExpressionNode { public ExpressionNode Collection, Index; public IndexExpressionNode(ExpressionNode c, ExpressionNode i) { Collection = c; Index = i; } }
    public class ComprehensionNode : ExpressionNode { public string ItemName; public ExpressionNode Iterable, Condition; public ComprehensionNode(string item, ExpressionNode iter, ExpressionNode cond) { ItemName = item; Iterable = iter; Condition = cond; } }

    public abstract class StatementNode : AstNode { }

    public class ExpressionStatementNode : StatementNode { public ExpressionNode Expression; public ExpressionStatementNode(ExpressionNode e) => Expression = e; }
    public class AssignmentNode : StatementNode { public IdentifierNode Target; public ExpressionNode Value; public AssignmentNode(IdentifierNode t, ExpressionNode v) { Target = t; Value = v; } }
    public class AugmentedAssignmentNode : StatementNode { public IdentifierNode Target; public string Operator; public ExpressionNode Value; public AugmentedAssignmentNode(IdentifierNode t, string op, ExpressionNode v) { Target = t; Operator = op; Value = v; } }
    public class IfNode : StatementNode { public ExpressionNode Condition; public List<AstNode> ThenBranch = new(), ElseBranch = new(); public IfNode(ExpressionNode c) => Condition = c; }
    public class WhileNode : StatementNode { public ExpressionNode Condition; public List<AstNode> Body = new(); public WhileNode(ExpressionNode c) => Condition = c; }
    public class ForNode : StatementNode { public IdentifierNode Iterator; public ExpressionNode Start, End; public List<AstNode> Body = new(); public ForNode(IdentifierNode i, ExpressionNode s, ExpressionNode e) { Iterator = i; Start = s; End = e; } }
    public class FunctionNode : StatementNode { public string Name; public List<string> Parameters = new(); public List<AstNode> Body = new(); public FunctionNode(string n) => Name = n; }
    public class ReturnNode : StatementNode { public ExpressionNode Expression; public ReturnNode(ExpressionNode e) => Expression = e; }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private static readonly string[] BinOps = { "+", "-", "*", "/", "%", "==", "!=", ">=", "<=", "<", ">", "and", "or", "in", "not" };

        public Parser(List<Token> tokens) { _tokens = tokens; _pos = 0; }
        private Token Curr => _tokens[_pos];
        private Token Consume() => _tokens[_pos++];
        private bool Match(TokenType t) { if (Curr.Type == t) { _pos++; return true; } return false; }
        private bool MatchValue(string v) { if (Curr.Value == v) { _pos++; return true; } return false; }
        private Token Peek(int off) => _tokens[_pos + off];

        public ProgramNode Parse()
        {
            var prog = new ProgramNode();
            while (!Match(TokenType.EndOfFile))
            {
                if (Match(TokenType.NewLine) || MatchValue("import"))
                {
                    if (_tokens[_pos - 1].Value == "import")
                        while (!Match(TokenType.NewLine) && !Match(TokenType.EndOfFile))
                            Consume();
                    continue;
                }
                prog.Statements.Add(ParseStmt());
            }
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
            var name = Consume().Value; Consume(); // '('
            var fn = new FunctionNode(name);
            if (!MatchValue(")"))
            {
                do { fn.Parameters.Add(Consume().Value); } while (MatchValue(","));
                Consume(); // ')'
            }
            Consume(); Match(TokenType.NewLine);
            if (Curr.Type == TokenType.String) { Consume(); Match(TokenType.NewLine); }
            if (!Match(TokenType.Indent)) throw new TranslationException($"Expected indent after def at line {Curr.Line}");
            while (!Match(TokenType.Dedent))
            {
                if (Match(TokenType.NewLine)) continue;
                fn.Body.Add(ParseStmt());
            }
            return fn;
        }

        private IfNode ParseIf()
        {
            var cond = ParseExpr(); Consume(); Match(TokenType.NewLine); Consume();
            var n = new IfNode(cond);
            while (!Match(TokenType.Dedent))
            {
                if (Match(TokenType.NewLine)) continue;
                n.ThenBranch.Add(ParseStmt());
            }
            if (MatchValue("else")) { Consume(); Match(TokenType.NewLine); Consume(); while (!Match(TokenType.Dedent)) { if (Match(TokenType.NewLine)) continue; n.ElseBranch.Add(ParseStmt()); } }
            return n;
        }

        private WhileNode ParseWhile()
        {
            var cond = ParseExpr(); Consume(); Match(TokenType.NewLine); Consume();
            var w = new WhileNode(cond);
            while (!Match(TokenType.Dedent))
            {
                if (Match(TokenType.NewLine)) continue;
                w.Body.Add(ParseStmt());
            }
            return w;
        }

        private ForNode ParseFor()
        {
            var iter = new IdentifierNode(Consume().Value);
            Consume(); // in
            Consume(); // range
            Consume(); // '('
            var st = ParseExpr(); Consume();
            var en = ParseExpr(); Consume();
            Consume(); Match(TokenType.NewLine); Consume();
            var f = new ForNode(iter, st, en);
            while (!Match(TokenType.Dedent))
            {
                if (Match(TokenType.NewLine)) continue;
                f.Body.Add(ParseStmt());
            }
            return f;
        }

        private ReturnNode ParseReturn()
        {
            var e = ParseExpr(); Consume();
            return new ReturnNode(e);
        }

        private StatementNode ParseExprOrAssign()
        {
            var e = ParseExpr();
            if (e is IdentifierNode id && MatchValue("="))
            {
                var v = ParseExpr(); Consume();
                return new AssignmentNode(id, v);
            }
            if (e is IdentifierNode aid && (Peek(0).Value == "+=" || Peek(0).Value == "-=" || Peek(0).Value == "*=" || Peek(0).Value == "/=") && MatchValue(Peek(0).Value))
            {
                var op = _tokens[_pos - 1].Value;
                var v = ParseExpr(); Consume();
                return new AugmentedAssignmentNode(aid, op, v);
            }
            Consume();
            return new ExpressionStatementNode(e);
        }

        private ExpressionNode ParseExpr() => ParseBinaryOr();

        private ExpressionNode ParseBinaryOr()
        {
            var l = ParseBinaryAnd();
            while (MatchValue("or")) { var r = ParseBinaryAnd(); l = new BinaryExpressionNode(l, "or", r); }
            return l;
        }

        private ExpressionNode ParseBinaryAnd()
        {
            var l = ParseEquality();
            while (MatchValue("and")) { var r = ParseEquality(); l = new BinaryExpressionNode(l, "and", r); }
            return l;
        }

        private ExpressionNode ParseEquality()
        {
            var left = ParseRel();
            while (new[] { "==", "!=" }.Contains(Curr.Value))
            {
                var op = Consume().Value;
                var right = ParseRel();
                left = new BinaryExpressionNode(left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseRel()
        {
            var left = ParseAdd();
            while (new[] { "<", ">", "<=", ">=", "in" }.Contains(Curr.Value))
            {
                var op = Consume().Value;
                var right = ParseAdd();
                left = new BinaryExpressionNode(left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseAdd()
        {
            var l = ParseMul();
            while (Curr.Value == "+" || Curr.Value == "-")
            {
                var op = Consume().Value; var r = ParseMul(); l = new BinaryExpressionNode(l, op, r);
            }
            return l;
        }

        private ExpressionNode ParseMul()
        {
            var l = ParseUnary();
            while (new[] { "*", "/", "%" }.Contains(Curr.Value))
            {
                var op = Consume().Value; var r = ParseUnary(); l = new BinaryExpressionNode(l, op, r);
            }
            return l;
        }

        private ExpressionNode ParseUnary()
        {
            if (MatchValue("not")) return new UnaryExpressionNode("not", ParseUnary());
            if (MatchValue("-")) return new UnaryExpressionNode("-", ParseUnary());
            return ParsePrimary();
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
                        do { call.Arguments.Add(ParseExpr()); } while (MatchValue(","));
                        Consume();
                    }
                    return ParseIndexOrCall(call);
                }
                return ParseIndexOrCall(new IdentifierNode(nm));
            }
            if (MatchValue("(")) { var e = ParseExpr(); Consume(); return e; }
            if (MatchValue("["))
            {
                if (Curr.Type == TokenType.Identifier && Peek(1).Value == "for")
                {
                    var item = Consume().Value; Consume(); Consume();
                    var iter = ParseExpr();
                    ExpressionNode cond = null;
                    if (MatchValue("if")) cond = ParseExpr();
                    Consume();
                    return new ComprehensionNode(item, iter, cond);
                }
                var list = new ListNode();
                if (!MatchValue("]"))
                {
                    do { list.Elements.Add(ParseExpr()); } while (MatchValue(","));
                    Consume();
                }
                return list;
            }
            throw new TranslationException($"Unexpected '{Curr.Value}' at line {Curr.Line}, col {Curr.Column}");
        }

        private ExpressionNode ParseIndexOrCall(ExpressionNode target)
        {
            // Support chained calls like obj.method(arg) and indexing obj[idx]
            while (true)
            {
                if (MatchValue("("))
                {
                    if (target is IdentifierNode idNode)
                    {
                        var call = new CallExpressionNode(idNode.Name);
                        if (!MatchValue(")"))
                        {
                            do { call.Arguments.Add(ParseExpr()); } while (MatchValue(","));
                            Consume();
                        }
                        target = call;
                        continue;
                    }
                }
                if (MatchValue("["))
                {
                    var idx = ParseExpr(); Consume();
                    target = new IndexExpressionNode(target, idx);
                    continue;
                }
                break;
            }
            return target;
        }
    }
}
