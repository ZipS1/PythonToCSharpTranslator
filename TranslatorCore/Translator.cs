// TranslatorCore/Translator.cs
using System;
using System.Collections.Generic;
using System.Linq;
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
            _sb.AppendLine("using System.Linq;");
            _sb.AppendLine("namespace Translated");
            _sb.AppendLine("{");
            Indent();
            _sb.AppendLine("public class Program");
            _sb.AppendLine("{");
            Indent();

            foreach (var stmt in program.Statements)
                if (stmt is FunctionNode fn) { VisitFunction(fn); _sb.AppendLine(); }

            _sb.AppendLine("public static void Main()");
            _sb.AppendLine("{");
            _indentLevel++;
            foreach (var stmt in program.Statements)
                if (!(stmt is FunctionNode)) VisitStatement(stmt);
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
                        _sb.AppendLine($"{name} = {expr};");
                    break;

                case AugmentedAssignmentNode aa:
                    WriteIndent();
                    _sb.AppendLine($"{aa.Target.Name} {aa.Operator} {VisitExpression(aa.Value)};");
                    break;

                case ExpressionStatementNode es:
                    WriteIndent();
                    _sb.AppendLine($"{VisitExpression(es.Expression)};");
                    break;

                case IfNode ifn:
                    WriteIndent();
                    _sb.AppendLine($"if ({VisitExpression(ifn.Condition)})");
                    WriteBlock(ifn.ThenBranch);
                    if (ifn.ElseBranch.Any())
                    {
                        WriteIndent();
                        _sb.AppendLine("else");
                        WriteBlock(ifn.ElseBranch);
                    }
                    break;

                case WhileNode wn:
                    WriteIndent();
                    _sb.AppendLine($"while ({VisitExpression(wn.Condition)})");
                    WriteBlock(wn.Body);
                    break;

                case ForNode fn2:
                    WriteIndent();
                    _sb.AppendLine($"for(int {fn2.Iterator.Name} = {VisitExpression(fn2.Start)}; {fn2.Iterator.Name} < {VisitExpression(fn2.End)}; {fn2.Iterator.Name}++)");
                    WriteBlock(fn2.Body);
                    break;

                case ReturnNode ret:
                    WriteIndent();
                    _sb.AppendLine($"return {VisitExpression(ret.Expression)};");
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
                case NumberNode n: return n.Value;
                case StringNode s: return $"\"{s.Value}\"";
                case IdentifierNode id:
                    return id.Name == "random.randint"
                        ? "new Random().Next"
                        : id.Name;
                case UnaryExpressionNode u:
                    var o = VisitExpression(u.Operand);
                    return u.Operator == "not" ? $"!{o}" : $"-{o}";
                case BinaryExpressionNode b:
                    var l = VisitExpression(b.Left);
                    var r = VisitExpression(b.Right);
                    return b.Operator switch
                    {
                        "and" => $"{l}&&{r}",
                        "or" => $"{l}||{r}",
                        _ => $"{l}{b.Operator}{r}"
                    };
                case CallExpressionNode c:
                    var args = string.Join(", ", c.Arguments.ConvertAll(VisitExpression));
                    if (c.FunctionName == "random.randint")
                        return $"new Random().Next({args})";
                    return c.FunctionName switch
                    {
                        "input" => "Console.ReadLine()",
                        "print" => $"Console.WriteLine({args})",
                        "int" => $"int.Parse({args})",
                        "float" => $"double.Parse({args})",
                        "str" => $"{args}.ToString()",
                        "bool" => $"bool.Parse({args})",
                        "set" => $"new HashSet<dynamic>({args})",
                        "range" when c.Arguments.Count == 2 =>
                            $"Enumerable.Range({VisitExpression(c.Arguments[0])}, {VisitExpression(c.Arguments[1])} - {VisitExpression(c.Arguments[0])} + 1)",
                        _ => $"{c.FunctionName}({args})"
                    };
                case ListNode ln:
                    var el = string.Join(", ", ln.Elements.ConvertAll(VisitExpression));
                    return $"new dynamic[] {{ {el} }}";
                case IndexExpressionNode ix:
                    return $"{VisitExpression(ix.Collection)}[{VisitExpression(ix.Index)}]";
                case ComprehensionNode comp:
                    var seq = VisitExpression(comp.Iterable);
                    var itm = comp.ItemName;
                    var wh = comp.Condition != null
                              ? $".Where({itm} => {VisitExpression(comp.Condition)})"
                              : "";
                    return $"{seq}.Select({itm} => {itm}){wh}.ToArray()";
                default:
                    Warnings.Add($"Unsupported expression: {expr.GetType().Name}");
                    return "null";
            }
        }

        private void WriteBlock(List<AstNode> stmts)
        {
            WriteIndent(); _sb.AppendLine("{"); _indentLevel++;
            foreach (var s in stmts) VisitStatement(s);
            _indentLevel--; WriteIndent(); _sb.AppendLine("}");
        }

        private void WriteIndent() => _sb.Append(new string(' ', _indentLevel * 4));
        private void Indent() => _indentLevel++;
        private void Dedent() => _indentLevel--;
    }
}
