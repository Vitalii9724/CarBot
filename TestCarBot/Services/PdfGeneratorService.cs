using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TestCarBot.Models;

namespace TestCarBot.Services
{
    public class PdfGeneratorService
    {
        public async Task<byte[]> GeneratePolicyPdfAsync(UserData user, string policyNumber, DateTime issuedAt)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => ComposeHeader(c, policyNumber));
                    page.Content().Element(c => ComposeContent(c, user));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return await Task.Run(() => document.GeneratePdf());
        }

        private void ComposeHeader(IContainer container, string policyNumber)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Bot-Assurance Inc.").Bold().FontSize(16);
                        col.Item().Text("ПОЛІС № " + policyNumber).SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);
                        col.Item().Text("Обов'язкового страхування цивільно-правової відповідальності").FontSize(9);
                    });
                    row.ConstantItem(120).AlignRight().Text(text =>
                    {
                        text.Span("Дійсний до: ").SemiBold();
                        text.Span($"{DateTime.UtcNow.AddYears(1):dd.MM.yyyy}");
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container, UserData user)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(25);
                column.Item().Element(c => ComposeSection(c, "Страхувальник (Власник ТЗ)", user, true));
                column.Item().Element(c => ComposeSection(c, "Застрахований Транспортний Засіб", user, false));
                column.Item().Element(ComposeTerms);
            });
        }

        private void ComposeSection(IContainer container, string title, UserData user, bool isPerson)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(column =>
            {
                column.Item().PaddingBottom(5).Text(title).SemiBold().FontSize(12);
                column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(150);
                        columns.RelativeColumn();
                    });

                    if (isPerson)
                    {
                        table.Cell().Text("Прізвище:").Medium();
                        table.Cell().Text(user.LastName ?? "не розпізнано").Bold();
                        table.Cell().Text("Ім'я:").Medium();
                        table.Cell().Text(user.FirstName ?? "не розпізнано").Bold();
                        table.Cell().Text("Документ (паспорт):").Medium();
                        table.Cell().Text(user.PassportNumber ?? "не розпізнано").Bold();
                    }
                    else
                    {
                        table.Cell().Text("Марка, Модель:").Medium();
                        table.Cell().Text(user.CarModel ?? "не розпізнано").Bold();
                        table.Cell().Text("Державний номер:").Medium();
                        table.Cell().Text(user.CarNumber ?? "не розпізнано").Bold();
                    }
                });
            });
        }

        private void ComposeTerms(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Умови страхування та обов'язки сторін").Bold();
                col.Item().Text("1. Страховик зобов'язується відшкодувати збитки, завдані третім особам внаслідок ДТП з вини Страхувальника. 2. Страхувальник зобов'язаний негайно повідомляти про страховий випадок. 3. Поліс є недійсним без підпису та печатки.")
                    .FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("____________________");
                    col.Item().Text("Підпис страхувальника").FontSize(8);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("____________________");
                    col.Item().Text("Представник Bot-Assurance").FontSize(8);
                });
            });
        }
    }
}