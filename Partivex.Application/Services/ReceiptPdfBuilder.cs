using System.Globalization;
using System.Text;

namespace Partivex.Application.Services;

internal static class ReceiptPdfBuilder
{
    private const decimal PageWidth = 595m;
    private const decimal PageHeight = 842m;
    private const decimal Margin = 42m;
    private const decimal ContentWidth = PageWidth - (Margin * 2m);

    public static byte[] Build(ReceiptPdfDocument document)
    {
        var writer = new PdfWriter();
        writer.StartPage(document, false);

        writer.DrawDocumentHeader(document);
        writer.DrawCustomerSummary(document);
        writer.DrawItemsTable(document);
        writer.DrawTotals(document);
        writer.DrawFooter(document);

        return writer.ToPdf();
    }

    private sealed class PdfWriter
    {
        private readonly List<StringBuilder> _pages = [];
        private StringBuilder _content = new();
        private decimal _y;

        public void StartPage(ReceiptPdfDocument document, bool continued)
        {
            _content = new StringBuilder();
            _pages.Add(_content);
            _y = 770m;

            Fill(0m, 0m, PageWidth, PageHeight, "F6F7F9");
            Fill(32m, 32m, 531m, 778m, "FFFFFF");
            Stroke(32m, 32m, 531m, 778m, "D9DEE7", 0.8m);
            Fill(32m, 782m, 531m, 28m, "EF233C");

            if (!continued)
            {
                return;
            }

            DrawText("PARTIVEX", Margin, 754m, 16m, "F2", "111827");
            DrawText($"{document.ReceiptTitle} continued", Margin, 732m, 10m, "F1", "64748B");
            DrawText(document.InvoiceNumber, 455m, 754m, 10m, "F2", "111827");
            _y = 700m;
        }

        public void DrawDocumentHeader(ReceiptPdfDocument document)
        {
            DrawText("PARTIVEX", Margin, 748m, 22m, "F2", "111827");
            DrawText("Auto parts, service appointments and customer support", Margin, 728m, 9.5m, "F1", "64748B");

            Fill(420m, 717m, 98m, 30m, StatusColor(document.Status));
            DrawCenteredText(document.Status.ToUpperInvariant(), 469m, 728m, 9.5m, "F2", "FFFFFF");

            DrawText(document.ReceiptTitle, Margin, 680m, 24m, "F2", "111827");
            DrawText($"Receipt #{document.InvoiceNumber}", Margin, 659m, 11m, "F1", "475569");
            DrawRightText(FormatDate(document.InvoiceDate), Margin + ContentWidth, 662m, 10.5m, "F2", "111827");

            Fill(Margin, 625m, ContentWidth, 1m, "E5E7EB");
            _y = 592m;
        }

        public void DrawCustomerSummary(ReceiptPdfDocument document)
        {
            DrawInfoCard(Margin, _y - 76m, 245m, 76m, "BILLED TO",
                [document.CustomerName, string.IsNullOrWhiteSpace(document.CustomerEmail) ? "Email not available" : document.CustomerEmail]);

            var details = document.Details.Take(3).Select(detail => $"{detail.Label}: {detail.Value}").ToArray();
            DrawInfoCard(307m, _y - 76m, 246m, 76m, "RECEIPT DETAILS", details);
            _y -= 112m;
        }

        public void DrawItemsTable(ReceiptPdfDocument document)
        {
            EnsureSpace(document, 88m);
            DrawText("Items", Margin, _y, 15m, "F2", "111827");
            _y -= 26m;

            Fill(Margin, _y - 21m, ContentWidth, 24m, "111827");
            DrawText("Description", Margin + 14m, _y - 13m, 9m, "F2", "FFFFFF");
            DrawCenteredText("Qty", 342m, _y - 13m, 9m, "F2", "FFFFFF");
            DrawRightText("Unit", 438m, _y - 13m, 9m, "F2", "FFFFFF");
            DrawRightText("Amount", 540m, _y - 13m, 9m, "F2", "FFFFFF");
            _y -= 34m;

            foreach (var item in document.Items)
            {
                EnsureSpace(document, 44m);
                Fill(Margin, _y - 23m, ContentWidth, 31m, "FFFFFF");
                Stroke(Margin, _y - 23m, ContentWidth, 31m, "E5E7EB", 0.5m);

                DrawText(TrimTo(item.Description, 42), Margin + 14m, _y - 2m, 9.8m, "F2", "111827");
                if (!string.IsNullOrWhiteSpace(item.Code))
                {
                    DrawText(TrimTo(item.Code, 24), Margin + 14m, _y - 15m, 8m, "F1", "64748B");
                }

                DrawCenteredText(item.Quantity, 342m, _y - 8m, 9m, "F1", "111827");
                DrawRightText(FormatCurrency(item.UnitAmount), 438m, _y - 8m, 9m, "F1", "111827");
                DrawRightText(FormatCurrency(item.LineAmount), 540m, _y - 8m, 9m, "F2", "111827");
                _y -= 36m;
            }

            _y -= 12m;
        }

        public void DrawTotals(ReceiptPdfDocument document)
        {
            EnsureSpace(document, 122m);
            var totalsHeight = Math.Max(72m, (document.Totals.Count * 23m) + 30m);
            var x = 318m;
            var y = _y - totalsHeight;

            Fill(x, y, 235m, totalsHeight, "F8FAFC");
            Stroke(x, y, 235m, totalsHeight, "E2E8F0", 0.7m);

            var rowY = _y - 26m;
            foreach (var total in document.Totals)
            {
                var font = total.IsGrandTotal ? "F2" : "F1";
                var size = total.IsGrandTotal ? 13m : 9.5m;
                var color = total.IsGrandTotal ? "111827" : "475569";

                if (total.IsGrandTotal)
                {
                    Fill(x + 14m, rowY + 11m, 207m, 1m, "CBD5E1");
                    rowY -= 3m;
                }

                DrawText(total.Label, x + 16m, rowY, size, font, color);
                DrawRightText(total.Value, x + 218m, rowY, size, font, color);
                rowY -= total.IsGrandTotal ? 27m : 21m;
            }

            _y = y - 30m;
        }

        public void DrawFooter(ReceiptPdfDocument document)
        {
            EnsureSpace(document, 88m);
            if (!string.IsNullOrWhiteSpace(document.Notes))
            {
                DrawText("Notes", Margin, _y, 11m, "F2", "111827");
                DrawText(TrimTo(document.Notes, 98), Margin, _y - 17m, 9m, "F1", "475569");
                _y -= 45m;
            }

            Fill(Margin, 78m, ContentWidth, 1m, "E5E7EB");
            DrawText("Thank you for choosing Partivex.", Margin, 56m, 10m, "F2", "111827");
            DrawRightText("This is a system generated receipt.", Margin + ContentWidth, 56m, 8.5m, "F1", "64748B");
        }

        public byte[] ToPdf()
        {
            var objects = new List<string>
            {
                "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
                string.Empty,
                "3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n",
                "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n"
            };

            var pageObjectIds = new List<int>();
            var nextId = 5;
            foreach (var page in _pages)
            {
                var pageId = nextId++;
                var contentId = nextId++;
                pageObjectIds.Add(pageId);
                var content = page.ToString();

                objects.Add($"{pageId} 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentId} 0 R >>\nendobj\n");
                objects.Add($"{contentId} 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream\nendobj\n");
            }

            objects[1] = $"2 0 obj\n<< /Type /Pages /Kids [{string.Join(' ', pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {_pages.Count} >>\nendobj\n";

            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new List<int> { 0 };
            foreach (var obj in objects)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append(obj);
            }

            var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append("xref\n0 ").Append(objects.Count + 1).Append("\n0000000000 65535 f \n");
            for (var i = 1; i < offsets.Count; i++)
            {
                pdf.Append(offsets[i].ToString("0000000000", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            }

            pdf.Append("trailer\n<< /Root 1 0 R /Size ").Append(objects.Count + 1).Append(" >>\nstartxref\n")
                .Append(xrefOffset.ToString(CultureInfo.InvariantCulture))
                .Append("\n%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private void EnsureSpace(ReceiptPdfDocument document, decimal requiredHeight)
        {
            if (_y - requiredHeight > 112m)
            {
                return;
            }

            StartPage(document, true);
        }

        private void DrawInfoCard(decimal x, decimal y, decimal width, decimal height, string title, IReadOnlyList<string> values)
        {
            Fill(x, y, width, height, "F8FAFC");
            Stroke(x, y, width, height, "E2E8F0", 0.7m);
            DrawText(title, x + 14m, y + height - 20m, 8.5m, "F2", "EF233C");

            var lineY = y + height - 40m;
            foreach (var value in values.Where(value => !string.IsNullOrWhiteSpace(value)).Take(3))
            {
                DrawText(TrimTo(value, 36), x + 14m, lineY, 9.2m, "F1", "111827");
                lineY -= 15m;
            }
        }

        private void DrawText(string text, decimal x, decimal y, decimal size, string font, string color)
        {
            TextColor(color);
            _content.Append("BT\n/")
                .Append(font)
                .Append(' ')
                .Append(Num(size))
                .Append(" Tf\n")
                .Append(Num(x))
                .Append(' ')
                .Append(Num(y))
                .Append(" Td\n(")
                .Append(Escape(text))
                .Append(") Tj\nET\n");
        }

        private void DrawCenteredText(string text, decimal centerX, decimal y, decimal size, string font, string color)
        {
            var width = EstimateTextWidth(text, size);
            DrawText(text, centerX - (width / 2m), y, size, font, color);
        }

        private void DrawRightText(string text, decimal rightX, decimal y, decimal size, string font, string color)
        {
            DrawText(text, rightX - EstimateTextWidth(text, size), y, size, font, color);
        }

        private void Fill(decimal x, decimal y, decimal width, decimal height, string hex)
        {
            FillColor(hex);
            _content.Append(Num(x)).Append(' ')
                .Append(Num(y)).Append(' ')
                .Append(Num(width)).Append(' ')
                .Append(Num(height))
                .Append(" re f\n");
        }

        private void Stroke(decimal x, decimal y, decimal width, decimal height, string hex, decimal lineWidth)
        {
            StrokeColor(hex);
            _content.Append(Num(lineWidth)).Append(" w\n")
                .Append(Num(x)).Append(' ')
                .Append(Num(y)).Append(' ')
                .Append(Num(width)).Append(' ')
                .Append(Num(height))
                .Append(" re S\n");
        }

        private void FillColor(string hex)
        {
            var (r, g, b) = Rgb(hex);
            _content.Append(Num(r)).Append(' ').Append(Num(g)).Append(' ').Append(Num(b)).Append(" rg\n");
        }

        private void StrokeColor(string hex)
        {
            var (r, g, b) = Rgb(hex);
            _content.Append(Num(r)).Append(' ').Append(Num(g)).Append(' ').Append(Num(b)).Append(" RG\n");
        }

        private void TextColor(string hex) => FillColor(hex);
    }

    private static string StatusColor(string status)
    {
        return string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase)
            ? "16A34A"
            : "F59E0B";
    }

    private static string FormatCurrency(decimal value) => $"NPR {value:0.00}";

    private static string FormatDate(DateTimeOffset value) => value.ToLocalTime().ToString("MMM dd, yyyy, h:mm tt", CultureInfo.InvariantCulture);

    private static string TrimTo(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : $"{trimmed[..Math.Max(0, maxLength - 3)]}...";
    }

    private static decimal EstimateTextWidth(string text, decimal size) => text.Length * size * 0.52m;

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static (decimal R, decimal G, decimal B) Rgb(string hex)
    {
        var value = hex.TrimStart('#');
        return (
            Convert.ToInt32(value[..2], 16) / 255m,
            Convert.ToInt32(value.Substring(2, 2), 16) / 255m,
            Convert.ToInt32(value.Substring(4, 2), 16) / 255m);
    }

    private static string Num(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

internal sealed record ReceiptPdfDocument(
    string ReceiptTitle,
    string InvoiceNumber,
    string Status,
    DateTimeOffset InvoiceDate,
    string CustomerName,
    string CustomerEmail,
    IReadOnlyList<ReceiptPdfDetail> Details,
    IReadOnlyList<ReceiptPdfLineItem> Items,
    IReadOnlyList<ReceiptPdfTotal> Totals,
    string? Notes);

internal sealed record ReceiptPdfDetail(string Label, string Value);

internal sealed record ReceiptPdfLineItem(string Code, string Description, string Quantity, decimal UnitAmount, decimal LineAmount);

internal sealed record ReceiptPdfTotal(string Label, string Value, bool IsGrandTotal = false);
