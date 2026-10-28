using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Front.Equipments.Utils
{
    /// <summary>
    /// Перетворення чека з термінала у рядки фіксованої ширини для екрана і друку на принтері/РРО.
    /// Android-термінали ПриватБанку можуть передавати чек в HTML (div/br з класами font-xl, pull-left/pull-right ...),
    /// або звичайним текстом з \n - обидва варіанти визначаються автоматично.
    /// </summary>
    public static class PosReceiptText
    {
        public const int DefaultWidth = 32;

        static readonly Regex IsHtmlRegex = new Regex(@"<\s*/?\s*(div|br|p|span|b|table|tr|td)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex TokenRegex = new Regex(@"<!--.*?-->|<\s*(/?)\s*([a-zA-Z0-9]+)([^>]*)>|[^<]+|<", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex ClassRegex = new Regex(@"class\s*=\s*(?:""([^""]*)""|'([^']*)')", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex SpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);

        static readonly HashSet<string> VoidTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "br", "img", "hr", "meta", "link", "input" };
        static readonly HashSet<string> BlockTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "div", "p", "tr", "table", "body", "html", "h1", "h2", "h3", "h4", "li", "ul" };
        static readonly HashSet<string> SkipTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "style", "script", "head", "title" };

        enum eAlign { Left, Center, Right }
        /// <summary>
        /// Службові маркери у тексті рядка: роздільник на всю ширину / межа між лівою і правою частиною
        /// </summary>
        const string SeparatorMark = "\u0001";
        const char PullMark = '\u0002';

        class Node
        {
            public string Tag;
            public string[] Classes = Array.Empty<string>();
            public string Text; // для текстового вузла
            public List<Node> Children = new List<Node>();
            public bool HasClass(string pClass) => Classes.Contains(pClass, StringComparer.OrdinalIgnoreCase);
        }

        class Line
        {
            public string Text;
            public eAlign Align;
            public Line(string pText, eAlign pAlign) { Text = pText; Align = pAlign; }
        }

        /// <summary>
        /// Чек термінала (HTML або текст) => рядки шириною не більше pWidth
        /// </summary>
        public static List<string> ToLines(string pReceipt, int pWidth = DefaultWidth)
        {
            if (string.IsNullOrWhiteSpace(pReceipt))
                return null;
            if (pWidth <= 0)
                pWidth = DefaultWidth;
            return IsHtmlRegex.IsMatch(pReceipt) ? HtmlToLines(pReceipt, pWidth) : TextToLines(pReceipt, pWidth);
        }

        /// <summary>
        /// Звичайний текст (варіант, коли термінал налаштовано на текстові чеки): тільки перенос задовгих рядків
        /// </summary>
        static List<string> TextToLines(string pReceipt, int pWidth)
        {
            var Res = new List<string>();
            foreach (var el in pReceipt.Replace("\r", string.Empty).Replace("\t", " ").Split('\n'))
            {
                string Str = el.TrimEnd();
                if (Str.Length <= pWidth)
                    Res.Add(Str);
                else
                    Res.AddRange(Wrap(Str.Trim(), pWidth));
            }
            return Res;
        }

        static List<string> HtmlToLines(string pHtml, int pWidth)
        {
            var Root = Parse(pHtml);
            var Lines = new List<Line>();
            RenderBlock(Root, eAlign.Left, Lines);
            var Res = new List<string>();
            foreach (var el in Lines)
                foreach (var Str in FitLine(el, pWidth))
                    Res.Add(Str);
            return Res;
        }

        #region Parse
        static Node Parse(string pHtml)
        {
            var Root = new Node() { Tag = "root" };
            var Stack = new Stack<Node>();
            Stack.Push(Root);
            int SkipDepth = 0;
            foreach (Match m in TokenRegex.Matches(pHtml))
            {
                if (m.Value.StartsWith("<!--"))
                    continue;
                if (m.Groups[2].Success)
                {
                    string Tag = m.Groups[2].Value;
                    bool IsClose = m.Groups[1].Value == "/";
                    string Attr = m.Groups[3].Value;
                    if (SkipTags.Contains(Tag))
                    {
                        SkipDepth += IsClose ? -1 : (Attr.TrimEnd().EndsWith("/") ? 0 : 1);
                        if (SkipDepth < 0) SkipDepth = 0;
                        continue;
                    }
                    if (SkipDepth > 0)
                        continue;
                    if (IsClose)
                    {
                        // Закриваємо до найближчого відповідного тега (стійкість до невалідного HTML)
                        if (Stack.Any(el => el.Tag.Equals(Tag, StringComparison.OrdinalIgnoreCase)))
                            while (Stack.Count > 1 && !Stack.Pop().Tag.Equals(Tag, StringComparison.OrdinalIgnoreCase)) { }
                        continue;
                    }
                    var Cm = ClassRegex.Match(Attr);
                    var Node = new Node()
                    {
                        Tag = Tag.ToLowerInvariant(),
                        Classes = Cm.Success ? (Cm.Groups[1].Success ? Cm.Groups[1].Value : Cm.Groups[2].Value).Split(' ', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>()
                    };
                    Stack.Peek().Children.Add(Node);
                    if (!VoidTags.Contains(Tag) && !Attr.TrimEnd().EndsWith("/"))
                        Stack.Push(Node);
                }
                else if (SkipDepth == 0)
                    Stack.Peek().Children.Add(new Node() { Tag = "#text", Text = WebUtility.HtmlDecode(m.Value) });
            }
            return Root;
        }
        #endregion

        #region Render
        static eAlign GetAlign(Node pNode, eAlign pParent)
        {
            if (pNode.HasClass("align-right") || pNode.HasClass("pull-right") || pNode.HasClass("text-right")) return eAlign.Right;
            if (pNode.HasClass("align-left") || pNode.HasClass("pull-left") || pNode.HasClass("text-left")) return eAlign.Left;
            if (pNode.HasClass("align-center") || pNode.HasClass("text-center") || pNode.HasClass("font-xl") || pNode.HasClass("oper")) return eAlign.Center;
            return pParent;
        }

        /// <summary>
        /// Блок (div): текст і br всередині - рядки, вкладені блоки - окремі рядки. Пара pull-left + pull-right - один рядок.
        /// </summary>
        static void RenderBlock(Node pNode, eAlign pAlign, List<Line> pLines)
        {
            var Align = GetAlign(pNode, pAlign);
            var Pull = TryPullPair(pNode);
            if (Pull != null)
            {
                pLines.Add(Pull);
                return;
            }
            var Sb = new StringBuilder();
            void Flush()
            {
                AddText(Sb.ToString(), Align, pLines);
                Sb.Clear();
            }
            foreach (var el in pNode.Children)
            {
                if (el.Tag == "#text")
                    Sb.Append(el.Text);
                else if (el.Tag == "br")
                {
                    Flush();
                }
                else if (el.Tag == "hr")
                {
                    Flush();
                    pLines.Add(new Line(SeparatorMark, eAlign.Left)); // роздільник на всю ширину
                }
                else if (BlockTags.Contains(el.Tag) || el.Tag == "td")
                {
                    Flush();
                    RenderBlock(el, Align, pLines);
                }
                else if (el.Tag == "img")
                    continue;
                else
                    Sb.Append(InlineText(el)); // span, b, font ...
            }
            Flush();
        }

        /// <summary>
        /// Блок з двох дочірніх блоків pull-left і pull-right => "ліве ... праве"
        /// </summary>
        static Line TryPullPair(Node pNode)
        {
            var Blocks = pNode.Children.Where(el => el.Tag != "#text" || !string.IsNullOrWhiteSpace(el.Text)).ToList();
            if (Blocks.Count != 2 || !Blocks[0].HasClass("pull-left") || !Blocks[1].HasClass("pull-right"))
                return null;
            string Left = Normalize(InlineText(Blocks[0]));
            string Right = Normalize(InlineText(Blocks[1]));
            return new Line(Left + PullMark + Right, eAlign.Left);
        }

        static string InlineText(Node pNode)
        {
            if (pNode.Tag == "#text") return pNode.Text;
            if (pNode.Tag == "br") return " ";
            return string.Concat(pNode.Children.Select(InlineText));
        }

        static string Normalize(string pText) => SpacesRegex.Replace(pText ?? string.Empty, " ").Trim();

        static void AddText(string pText, eAlign pAlign, List<Line> pLines)
        {
            if (string.IsNullOrWhiteSpace(pText))
                return;
            // Термінал вирівнює заголовок пробілами під свою ширину - такий текст центруємо
            var Align = pAlign == eAlign.Left && pText.Replace(" ", " ").TakeWhile(c => c == ' ').Count() >= 2 ? eAlign.Center : pAlign;
            pLines.Add(new Line(Normalize(pText.Replace(' ', ' ')), Align));
        }
        #endregion

        #region Layout
        static IEnumerable<string> FitLine(Line pLine, int pWidth)
        {
            string Text = pLine.Text;
            if (Text == SeparatorMark)
            {
                yield return new string('-', pWidth);
                yield break;
            }
            int PullPos = Text.IndexOf(PullMark);
            if (PullPos >= 0)
            {
                string Left = Text[..PullPos], Right = Text[(PullPos + 1)..];
                if (Left.Length == 0 && Right.Length == 0) yield break;
                if (Left.Length > 0 && Right.Length > 0 && Left.Length + 1 + Right.Length <= pWidth)
                    yield return Left + new string(' ', pWidth - Left.Length - Right.Length) + Right;
                else
                {
                    foreach (var el in Wrap(Left, pWidth)) yield return el;
                    foreach (var el in Wrap(Right, pWidth)) yield return Align(el, eAlign.Right, pWidth);
                }
                yield break;
            }
            // Роздільник "=====" довший за ширину - обрізаємо під ширину
            if (Text.Length >= 3 && Text.Distinct().Count() == 1 && "=-*_".Contains(Text[0]))
            {
                yield return new string(Text[0], pWidth);
                yield break;
            }
            foreach (var el in Wrap(Text, pWidth))
                yield return Align(el, pLine.Align, pWidth);
        }

        static string Align(string pText, eAlign pAlign, int pWidth)
        {
            if (pText.Length >= pWidth || pAlign == eAlign.Left) return pText;
            int Pad = pWidth - pText.Length;
            return pAlign == eAlign.Right ? new string(' ', Pad) + pText : new string(' ', Pad / 2) + pText;
        }

        /// <summary>
        /// Перенос по словах; слово довше за ширину ріжеться
        /// </summary>
        static List<string> Wrap(string pText, int pWidth)
        {
            var Res = new List<string>();
            var Sb = new StringBuilder();
            foreach (var W in pText.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string Word = W;
                while (Word.Length > pWidth)
                {
                    if (Sb.Length > 0) { Res.Add(Sb.ToString()); Sb.Clear(); }
                    Res.Add(Word[..pWidth]);
                    Word = Word[pWidth..];
                }
                if (Sb.Length > 0 && Sb.Length + 1 + Word.Length > pWidth) { Res.Add(Sb.ToString()); Sb.Clear(); }
                if (Sb.Length > 0) Sb.Append(' ');
                Sb.Append(Word);
            }
            if (Sb.Length > 0) Res.Add(Sb.ToString());
            return Res;
        }
        #endregion
    }
}
