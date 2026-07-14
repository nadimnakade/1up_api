using HtmlAgilityPack;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PickupAPi.Utils;

namespace PickupAPi.Services
{
    public class TableService
    {
        public void Add(Section section, HtmlNode node, HtmlParserService parser)
        {
            if (node == null) return;

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            var firstRow = node.SelectSingleNode(".//tr");
            if (firstRow == null) return;

            var cells = firstRow.SelectNodes(".//th|.//td");
            if (cells == null || cells.Count == 0) return;

            double columnWidth = 16.0 / cells.Count;
            for (int i = 0; i < cells.Count; i++)
                table.AddColumn(Unit.FromCentimeter(columnWidth));

            foreach (var rowNode in node.SelectNodes(".//tr"))
            {
                Row row = table.AddRow();
                var cellNodes = rowNode.SelectNodes(".//th|.//td");
                if (cellNodes == null) continue;

                for (int i = 0; i < cellNodes.Count && i < table.Columns.Count; i++)
                {
                    var cell = row.Cells[i];
                    foreach (var inner in cellNodes[i].ChildNodes)
                    {
                        parser.ProcessNode(inner, section, cell.AddParagraph());
                    }
                }
            }
        }

        public void AddList(Section section, HtmlNode node, bool ordered)
        {
            int i = 1;
            foreach (var li in node.SelectNodes("./li"))
            {
                var text = HtmlUtils.Clean(li.InnerText);
                if (string.IsNullOrWhiteSpace(text)) continue;

                var para = section.AddParagraph();
                para.Format.LeftIndent = Unit.FromCentimeter(0.5);
                para.AddText(ordered ? $"{i++}. " : "• ");
                para.AddText(text);
            }
        }
    }
}