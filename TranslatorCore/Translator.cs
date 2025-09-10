using System;
using System.Collections.Generic;
using System.Text;

namespace TranslatorCore
{
    public class Translator
    {
        private readonly StringBuilder _sb = new();
        private readonly HashSet<string> _declared = new();
        private int _indentLevel;
        public List<string> Warnings { get; } = new();

        public string Translate(ProgramNode program)
        {
            _sb.Clear();
            _indentLevel = 0;
            _declared.Clear();
            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections.Generic;");
            _sb.AppendLine("namespace Translated");
            _sb.AppendLine("{");
            Indent();
            _sb.AppendLine("public class Program");
            _sb.AppendLine("{");
            Indent();
            _sb.AppendLine("public static void Main()");
            _sb.AppendLine("{");
            _indentLevel++;
            foreach (var stmt in program.Statements)
            {
                VisitStatement(stmt);
            }
            _indentLevel--;
            Dedent();
            _sb.AppendLine("}");
            Dedent();
            _sb.AppendLine("}");
            _sb.AppendLine("}");
            return _sb.ToString();
        }

        private void VisitStatement(AstNode node)
        {
            switch (node)
            {
                case AssignmentNode a:
                    WriteIndent();
                    var name = a.Target.Name;
                    var expr = VisitExpression(a.Value);
                    if (!_declared.Contains(name))
                    {
                        _declared.Add(name);
                        _sb.AppendLine($"var {name} = {expr};");
                    }
                    else
                    {
                        _sb.AppendLine($"{name} = {expr};");
                    }
                    return;

                case ExpressionStatementNode e:
                    WriteIndent();
                    _sb.AppendLine($"{VisitExpression(e.Expression)};");
                    return;

                case IfNode ifn:
                    WriteIndent();
                    _sb.AppendLine($"if ({VisitExpression(ifn.Condition)})");
                    WriteBlock(ifn.ThenBranch);
                    if (ifn.ElseBranch.Count > 0)
                    {
                        WriteIndent();
                        _sb.AppendLine("else");
                        WriteBlock(ifn.ElseBranch);
                    }
                    return;

                case WhileNode w:
                    WriteIndent();
                    _sb.AppendLine($"while ({VisitExpression(w.Condition)})");
                    WriteBlock(w.Body);
                    return;

                case ForNode f:
                    WriteIndent();
                    _sb.AppendLine($"for (int {f.Iterator.Name} = {VisitExpression(f.Start)}; {f.Iterator.Name} < {VisitExpression(f.End)}; {f.Iterator.Name}++)");
                    WriteBlock(f.Body);
                    return;

                case FunctionNode fn:
                    WriteIndent();
                    _sb.Append($"public static object {fn.Name}(");
                    _sb.Append(string.Join(", ", fn.Parameters.ConvertAll(p => $"object {p}")));
                    _sb.AppendLine(")");
                    WriteBlock(fn.Body);
                    return;

                case ReturnNode r:
                    WriteIndent();
                    _sb.AppendLine($"return {VisitExpression(r.Expression)};");
                    return;

                default:
                    Warnings.Add($"Unsupported statement type: {node.GetType().Name}");
                    return;
            }
        }

        private string VisitExpression(ExpressionNode expr)
        {
            switch (expr)
            {
                case NumberNode n:
                    return n.Value;

                case StringNode s:
                    return $"\"{s.Value}\"";

                case IdentifierNode id:
                    return id.Name;

                case BinaryExpressionNode b:
                    return $"{VisitExpression(b.Left)} {b.Operator} {VisitExpression(b.Right)}";

                case CallExpressionNode c:
                    var args = string.Join(", ", c.Arguments.ConvertAll(VisitExpression));
                    if (c.FunctionName == "input")
                        return "Console.ReadLine()";
                    if (c.FunctionName == "print")
                        return $"Console.WriteLine({args})";
                    return $"{c.FunctionName}({args})";

                case ListNode l:
                    var elems = string.Join(", ", l.Elements.ConvertAll(VisitExpression));
                    return $"new object[] {{ {elems} }}";

                default:
                    Warnings.Add($"Unsupported expression type: {expr.GetType().Name}");
                    return "null";
            }
        }

        private void WriteBlock(List<AstNode> stmts)
        {
            WriteIndent();
            _sb.AppendLine("{");
            _indentLevel++;
            foreach (var stmt in stmts)
                VisitStatement(stmt);
            _indentLevel--;
            WriteIndent();
            _sb.AppendLine("}");
        }

        private void WriteIndent()
        {
            _sb.Append(new string(' ', _indentLevel * 4));
        }

        private void Indent() => _indentLevel++;
        private void Dedent() => _indentLevel--;
    }
}
