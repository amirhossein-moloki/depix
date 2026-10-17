using System.Text;

namespace Depix.Cli;

public static class TableFormatter
{
    public static string RenderTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        var headerList = headers.ToList();
        var rowList = rows.Select(r => r.ToList()).ToList();

        if (headerList.Count == 0) return string.Empty;

        var colCount = headerList.Count;
        var colWidths = new int[colCount];

        for (int i = 0; i < colCount; i++)
        {
            colWidths[i] = headerList[i].Length;
        }

        foreach (var row in rowList)
        {
            for (int i = 0; i < Math.Min(colCount, row.Count); i++)
            {
                var val = row[i] ?? string.Empty;
                colWidths[i] = Math.Max(colWidths[i], val.Length);
            }
        }

        var sb = new StringBuilder();

        // Top border
        sb.Append('+');
        for (int i = 0; i < colCount; i++)
        {
            sb.Append(new string('-', colWidths[i] + 2));
            sb.Append('+');
        }
        sb.AppendLine();

        // Header
        sb.Append('|');
        for (int i = 0; i < colCount; i++)
        {
            sb.Append(' ');
            sb.Append(headerList[i].PadRight(colWidths[i]));
            sb.Append(" |");
        }
        sb.AppendLine();

        // Header separator
        sb.Append('+');
        for (int i = 0; i < colCount; i++)
        {
            sb.Append(new string('-', colWidths[i] + 2));
            sb.Append('+');
        }
        sb.AppendLine();

        // Rows
        foreach (var row in rowList)
        {
            sb.Append('|');
            for (int i = 0; i < colCount; i++)
            {
                var val = i < row.Count ? (row[i] ?? string.Empty) : string.Empty;
                sb.Append(' ');
                sb.Append(val.PadRight(colWidths[i]));
                sb.Append(" |");
            }
            sb.AppendLine();
        }

        // Bottom border
        sb.Append('+');
        for (int i = 0; i < colCount; i++)
        {
            sb.Append(new string('-', colWidths[i] + 2));
            sb.Append('+');
        }
        sb.AppendLine();

        return sb.ToString();
    }

    public static string RenderKeyValue(IEnumerable<KeyValuePair<string, string>> pairs)
    {
        var rows = pairs.Select(p => new[] { p.Key, p.Value });
        return RenderTable(new[] { "Setting / Property", "Value" }, rows);
    }
}
