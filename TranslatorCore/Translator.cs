// TranslatorCore/Translator.cs
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
            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections.Generic;");
            _sb.AppendLine("namespace Translated");
            _sb.AppendLine("{");
            Indent();
            _sb.AppendLine("public class Program");
            _sb.AppendLine("{");
            Indent();

            // Emit function definitions first
            foreach (var stmt in program.Statements)
            {
                if (stmt is FunctionNode fn)
                {
                    VisitFunction(fn);
                    _sb.AppendLine();
                }
            }

            // Emit Main
            _sb.AppendLine("public static void Main()");
            _sb.AppendLine("{");
            _indentLevel++;
            foreach (var stmt in program.Statements)
            {
                if (!(stmt is FunctionNode))
                    VisitStatement(stmt);
            }
            _indentLevel--;
            Dedent();
            _sb.AppendLine("}");

            Dedent();
            _sb.AppendLine("}");
            Dedent();
            _sb.AppendLine("}");

            return _sb.ToString();
        }

        private void VisitFunction(FunctionNode fn)
        {
            WriteIndent();
            _sb.Append($"public static dynamic {fn.Name}(");
            _sb.Append(string.Join(", ", fn.Parameters.ConvertAll(p => $"dynamic {p}")));
            _sb.AppendLine(")");
            WriteBlock(fn.Body);
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
                    break;

                case ExpressionStatementNode e:
                    WriteIndent();
                    _sb.AppendLine($"{VisitExpression(e.Expression)};");
                    break;

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
                    break;

                case WhileNode w:
                    WriteIndent();
                    _sb.AppendLine($"while ({VisitExpression(w.Condition)})");
                    WriteBlock(w.Body);
                    break;

                case ForNode f:
                    WriteIndent();
                    _sb.AppendLine($"for (int {f.Iterator.Name} = {VisitExpression(f.Start)}; {f.Iterator.Name} < {VisitExpression(f.End)}; {f.Iterator.Name}++)");
                    WriteBlock(f.Body);
                    break;

                case ReturnNode r:
                    WriteIndent();
                    _sb.AppendLine($"return {VisitExpression(r.Expression)};");
                    break;

                default:
                    Warnings.Add($"Unsupported statement: {node.GetType().Name}");
                    break;
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
                case IndexExpressionNode idx:
                    return $"{VisitExpression(idx.Collection)}[{VisitExpression(idx.Index)}]";
                case BinaryExpressionNode b:
                    return $"{VisitExpression(b.Left)} {b.Operator} {VisitExpression(b.Right)}";
                case CallExpressionNode c:
                    var args = string.Join(", ", c.Arguments.ConvertAll(VisitExpression));
                    if (c.FunctionName == "input") return "Console.ReadLine()";
                    if (c.FunctionName == "print") return $"Console.WriteLine({args})";
                    if (c.FunctionName == "int") return $"int.Parse({args})";
                    if (c.FunctionName == "float") return $"double.Parse({args})";
                    if (c.FunctionName == "str") return $"{args}.ToString()";
                    if (c.FunctionName == "bool") return $"bool.Parse({args})";
                    return $"{c.FunctionName}({args})";
                case ListNode l:
                    var elems = string.Join(", ", l.Elements.ConvertAll(VisitExpression));
                    return $"new dynamic[] {{ {elems} }}";
                default:
                    Warnings.Add($"Unsupported expression: {expr.GetType().Name}");
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
