using System.Text;

namespace NetRpc.Core.Types;

public class Error
{
    public Document Document { get; set; }
    public Point Point { get; set; }
    public string Message { get; set; }

    public Error(Document document, Point point, string message)
    {
        Document = document;
        Point = point;
        Message = message;
    }

    public string ToMsBuildFormat()
    {
        return $"{Document.Uri}({Point.Line},{Point.Column})";
    }

    public string ToConsoleOutput()
    {
        var builder = new StringBuilder();

        var content = Document.Content.Split("\n");
        var start = Math.Max(Point.Line - 3, 0);
        var end = Math.Min(Point.Line + 3, content.Length);

        var context = content[start..end];

        var lineNumber = start;
        foreach (var line in context)
        {
            builder.AppendFormat("{0}| {1}\n", (lineNumber + 1).ToString().PadLeft(4), line);
            if (Point.Line - 1 == lineNumber)
            {
                builder.Append((new string('^', Math.Max(Point.Length, 1))).PadLeft(Point.Length + Point.Column + 6) + "\n");
            }

            lineNumber++;
        }

        builder.AppendLine();

        return builder.ToString();
    }
}