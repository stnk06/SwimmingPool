using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using SwimmingPool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SwimmingPool.Services
{
    public static class AdvancedPdfService
    {
        private static Font _titleFont;
        private static Font _headerFont;
        private static Font _cellFont;
        private static Font _groupHeaderFont;
        private static Font _signFont;
        private static Font _footerFont;
        private static bool _isCyrillicSupported = false;

        private static readonly BaseColor BrandColor = new BaseColor(0, 171, 169);
        private static readonly BaseColor ZebraColor = new BaseColor(245, 245, 245);
        private static readonly BaseColor BorderColor = new BaseColor(220, 220, 220);

        static AdvancedPdfService()
        {
            InitializeFonts();
        }

        private static void InitializeFonts()
        {
            try
            {
                string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                string[] possibleNames = { "arial.ttf", "ARIAL.TTF", "Arial.ttf" };
                string fontPath = null;

                foreach (var name in possibleNames)
                {
                    string path = Path.Combine(fontsFolder, name);
                    if (File.Exists(path))
                    {
                        fontPath = path;
                        break;
                    }
                }

                if (fontPath != null)
                {
                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    _titleFont = new Font(baseFont, 16, Font.BOLD, new BaseColor(50, 50, 50));
                    _headerFont = new Font(baseFont, 10, Font.BOLD, BaseColor.WHITE);
                    _cellFont = new Font(baseFont, 10, Font.NORMAL, new BaseColor(30, 30, 30));
                    _groupHeaderFont = new Font(baseFont, 12, Font.BOLD, BrandColor);
                    _signFont = new Font(baseFont, 11, Font.NORMAL, new BaseColor(50, 50, 50));
                    _footerFont = new Font(baseFont, 9, Font.NORMAL, BaseColor.GRAY);
                    _isCyrillicSupported = true;
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            catch (Exception)
            {
                BaseFont fallback = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                _titleFont = new Font(fallback, 16, Font.BOLD);
                _headerFont = new Font(fallback, 10, Font.BOLD, BaseColor.WHITE);
                _cellFont = new Font(fallback, 10, Font.NORMAL);
                _groupHeaderFont = new Font(fallback, 12, Font.BOLD);
                _signFont = new Font(fallback, 11, Font.NORMAL);
                _footerFont = new Font(fallback, 9, Font.NORMAL);
                _isCyrillicSupported = false;
            }
        }

        public static void ExportStandardRevenueReport(List<RevenueReportItem> data, DateTime start, DateTime end, string filePath)
        {
            try
            {
                Document document = InitDocument(filePath, out PdfWriter writer);
                AddBrandedHeader(document, $"ОТЧЕТ О ДОХОДАХ\nПериод: {start:dd.MM.yyyy} - {end:dd.MM.yyyy}");

                PdfPTable table = new PdfPTable(3) { WidthPercentage = 100, SpacingBefore = 20f, SpacingAfter = 30f };
                table.SetWidths(new float[] { 3f, 1.2f, 1.5f });

                AddTableHeader(table, "Тип абонемента");
                AddTableHeader(table, "Продано (шт)");
                AddTableHeader(table, "Сумма");

                int rowIndex = 0;
                foreach (var item in data)
                {
                    bool isZebra = rowIndex % 2 != 0;
                    AddCell(table, item.MembershipType, false, isZebra, Element.ALIGN_LEFT);
                    AddCell(table, item.SalesCount.ToString(), false, isZebra);
                    AddCell(table, item.TotalRevenue.ToString("C"), true, isZebra, Element.ALIGN_RIGHT);
                    rowIndex++;
                }

                decimal total = data.Sum(x => x.TotalRevenue);
                PdfPCell totalCell = new PdfPCell(new Phrase($"ИТОГО: {total:C}", new Font(_headerFont.BaseFont, 11, Font.BOLD, BaseColor.BLACK)))
                {
                    Colspan = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BackgroundColor = new BaseColor(230, 230, 230),
                    Padding = 12,
                    BorderColor = BorderColor
                };
                table.AddCell(totalCell);

                document.Add(table);
                AddSignatureBlock(document);

                FinalizeDocument(document);
                OpenDocument(filePath);
            }
            catch (Exception ex) { HandleError(ex); }
        }

        public static void ExportFinancialReport(List<MonthlyRevenueDTO> data, string filePath)
        {
            try
            {
                Document document = InitDocument(filePath, out PdfWriter writer);
                AddBrandedHeader(document, "РАСШИРЕННЫЙ ФИНАНСОВЫЙ ОТЧЕТ");

                var grouped = data.GroupBy(x => x.MonthYear.Contains(' ') ? x.MonthYear.Split(' ').Last() : "Year").OrderByDescending(g => g.Key);

                foreach (var group in grouped)
                {
                    document.Add(new Paragraph($"Отчетный год: {group.Key}", _groupHeaderFont) { SpacingBefore = 20f, SpacingAfter = 10f });

                    PdfPTable table = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 15f };
                    table.SetWidths(new float[] { 2f, 1f, 1.5f, 1.5f });

                    AddTableHeader(table, "Месяц");
                    AddTableHeader(table, "Продано");
                    AddTableHeader(table, "Ср. чек");
                    AddTableHeader(table, "Выручка");

                    int rowIndex = 0;
                    foreach (var item in group)
                    {
                        bool isZebra = rowIndex % 2 != 0;
                        AddCell(table, item.MonthYear, false, isZebra, Element.ALIGN_LEFT);
                        AddCell(table, item.MembershipsSold.ToString(), false, isZebra);
                        AddCell(table, item.AvgCheck.ToString("C"), false, isZebra, Element.ALIGN_RIGHT);
                        AddCell(table, item.TotalRevenue.ToString("C"), true, isZebra, Element.ALIGN_RIGHT);
                        rowIndex++;
                    }

                    decimal yearTotal = group.Sum(x => x.TotalRevenue);
                    PdfPCell totalCell = new PdfPCell(new Phrase($"Итого за {group.Key}: {yearTotal:C}", new Font(_headerFont.BaseFont, 10, Font.BOLD, BaseColor.BLACK)))
                    {
                        Colspan = 4,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        BackgroundColor = new BaseColor(220, 240, 240),
                        Padding = 10,
                        BorderColor = BorderColor
                    };
                    table.AddCell(totalCell);

                    document.Add(table);
                }

                AddSignatureBlock(document);
                FinalizeDocument(document);
                OpenDocument(filePath);
            }
            catch (Exception ex) { HandleError(ex); }
        }

        public static void ExportTrainerReport(List<TrainerPerformanceDTO> data, string filePath)
        {
            try
            {
                Document document = InitDocument(filePath, out PdfWriter writer);
                AddBrandedHeader(document, "ОТЧЕТ ПО ЭФФЕКТИВНОСТИ ТРЕНЕРОВ");

                var grouped = data.GroupBy(x => x.Specialization).OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    document.Add(new Paragraph($"Специализация: {group.Key}", _groupHeaderFont) { SpacingBefore = 20f, SpacingAfter = 10f });

                    PdfPTable table = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
                    table.SetWidths(new float[] { 3f, 1f, 1f, 1f });

                    AddTableHeader(table, "Тренер");
                    AddTableHeader(table, "Занятий");
                    AddTableHeader(table, "Учеников");
                    AddTableHeader(table, "Часов");

                    int rowIndex = 0;
                    foreach (var item in group)
                    {
                        bool isZebra = rowIndex % 2 != 0;
                        AddCell(table, item.TrainerName, false, isZebra, Element.ALIGN_LEFT);
                        AddCell(table, item.TotalClasses.ToString(), false, isZebra);
                        AddCell(table, item.TotalStudents.ToString(), false, isZebra);
                        AddCell(table, item.TotalHours.ToString("F1"), true, isZebra);
                        rowIndex++;
                    }
                    document.Add(table);
                }

                FinalizeDocument(document);
                OpenDocument(filePath);
            }
            catch (Exception ex) { HandleError(ex); }
        }

        public static void ExportPoolReport(List<PoolOccupancyDTO> data, string filePath)
        {
            try
            {
                Document document = InitDocument(filePath, out PdfWriter writer);
                AddBrandedHeader(document, "ЗАГРУЗКА БАССЕЙНОВ");

                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100, SpacingBefore = 20f };
                table.SetWidths(new float[] { 2.5f, 1f, 1.2f, 1.4f });

                AddTableHeader(table, "Бассейн");
                AddTableHeader(table, "Занятий");
                AddTableHeader(table, "Посетителей");
                AddTableHeader(table, "Загруженность");

                int rowIndex = 0;
                foreach (var item in data)
                {
                    bool isZebra = rowIndex % 2 != 0;
                    AddCell(table, item.PoolName, false, isZebra, Element.ALIGN_LEFT);
                    AddCell(table, item.TotalClasses.ToString(), false, isZebra);
                    AddCell(table, item.TotalVisitors.ToString(), false, isZebra);

                    PdfPCell occupancyCell = new PdfPCell(new Phrase($"{item.OccupancyRate}%", new Font(_cellFont.BaseFont, 10, Font.BOLD)))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 8,
                        BorderColor = BorderColor
                    };

                    if (item.OccupancyRate > 80) occupancyCell.BackgroundColor = new BaseColor(255, 230, 230);
                    else if (item.OccupancyRate < 30) occupancyCell.BackgroundColor = new BaseColor(230, 255, 230);
                    else occupancyCell.BackgroundColor = isZebra ? ZebraColor : BaseColor.WHITE;

                    table.AddCell(occupancyCell);
                    rowIndex++;
                }
                document.Add(table);

                FinalizeDocument(document);
                OpenDocument(filePath);
            }
            catch (Exception ex) { HandleError(ex); }
        }

        private static Document InitDocument(string filePath, out PdfWriter writer)
        {
            Document document = new Document(PageSize.A4, 40, 40, 50, 60);
            writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            writer.PageEvent = new PdfFooterEvent(_footerFont);
            document.Open();
            return document;
        }

        private static void AddBrandedHeader(Document doc, string titleText)
        {
            PdfPTable headerTable = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 10f };
            headerTable.SetWidths(new float[] { 1f, 4f });

            PdfPCell logoCell = new PdfPCell(new Phrase("СШ", new Font(_titleFont.BaseFont, 24, Font.BOLD, BaseColor.WHITE)))
            {
                BackgroundColor = BrandColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 15,
                Border = Rectangle.NO_BORDER
            };

            PdfPCell titleCell = new PdfPCell(new Phrase(titleText, _titleFont))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingLeft = 15,
                Border = Rectangle.NO_BORDER
            };

            headerTable.AddCell(logoCell);
            headerTable.AddCell(titleCell);
            doc.Add(headerTable);

            if (!_isCyrillicSupported)
            {
                doc.Add(new Paragraph("[WARNING: Кириллические шрифты не загружены. Форматирование может быть нарушено.]", _cellFont) { SpacingAfter = 10f });
            }

            LineSeparator line = new LineSeparator(1f, 100f, BrandColor, Element.ALIGN_CENTER, -5f);
            doc.Add(new Chunk(line));
        }

        private static void AddSignatureBlock(Document doc)
        {
            PdfPTable sigTable = new PdfPTable(3) { WidthPercentage = 100, SpacingBefore = 50f };
            sigTable.SetWidths(new float[] { 4f, 1f, 4f });

            PdfPCell c1 = new PdfPCell(new Phrase("Директор ___________________ / _________________ /", _signFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell c2 = new PdfPCell() { Border = Rectangle.NO_BORDER };
            PdfPCell c3 = new PdfPCell(new Phrase("Администратор ___________________ / _________________ /", _signFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            sigTable.AddCell(c1);
            sigTable.AddCell(c2);
            sigTable.AddCell(c3);
            doc.Add(sigTable);
        }

        private static void AddTableHeader(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, _headerFont))
            {
                BackgroundColor = BrandColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 10,
                PaddingBottom = 10,
                PaddingLeft = 4,
                PaddingRight = 4,
                BorderColor = BaseColor.WHITE,
                BorderWidth = 1f
            };
            table.AddCell(cell);
        }

        private static void AddCell(PdfPTable table, string text, bool isBold, bool isZebra, int align = Element.ALIGN_CENTER)
        {
            Font font = isBold ? new Font(_cellFont.BaseFont, 10, Font.BOLD, _cellFont.Color) : _cellFont;
            PdfPCell cell = new PdfPCell(new Phrase(text ?? "", font))
            {
                HorizontalAlignment = align,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 8,
                BorderColor = BorderColor,
                BackgroundColor = isZebra ? ZebraColor : BaseColor.WHITE
            };
            table.AddCell(cell);
        }

        private static void FinalizeDocument(Document doc) => doc.Close();

        private static void OpenDocument(string filePath)
        {
            try
            {
                MessageBox.Show($"Отчет успешно создан:\n{filePath}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start(filePath);
            }
            catch { }
        }

        private static void HandleError(Exception ex)
        {
            MessageBox.Show($"Ошибка при создании PDF: {ex.Message}\nУбедитесь, что файл не открыт в другой программе.", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private class PdfFooterEvent : PdfPageEventHelper
        {
            private readonly Font _footerFont;
            private PdfTemplate _totalPagesTemplate;

            public PdfFooterEvent(Font footerFont)
            {
                _footerFont = footerFont;
            }

            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                _totalPagesTemplate = writer.DirectContent.CreateTemplate(30, 16);
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfContentByte cb = writer.DirectContent;
                Rectangle pageSize = document.PageSize;

                // Слева: дата и время формирования
                Phrase leftPhrase = new Phrase($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}", _footerFont);
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, leftPhrase, pageSize.GetLeft(40), pageSize.GetBottom(30), 0);

                // Справа: страницы. Жестко убираем пробел из текста и считаем его отдельно
                string pageText = $"Страница {writer.PageNumber} из";
                Phrase rightPhrase = new Phrase(pageText, _footerFont);

                float textLen = _footerFont.BaseFont.GetWidthPoint(pageText, _footerFont.Size);
                float spaceLen = _footerFont.BaseFont.GetWidthPoint(" ", _footerFont.Size); // Истинная ширина пробела
                float templateWidth = 15f; // Резерв под двузначное/трехзначное количество страниц

                // Вычисляем стартовую точку так, чтобы вся конструкция (текст + пробел + шаблон) прижималась вправо
                float startX = pageSize.GetRight(40) - (textLen + spaceLen + templateWidth);

                // Рисуем текст "Страница X из" (выравнивание слева от вычисленной стартовой точки)
                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, rightPhrase, startX, pageSize.GetBottom(30), 0);

                // Рисуем шаблон с общим количеством ровно через 1 ширину пробела после текста
                cb.AddTemplate(_totalPagesTemplate, startX + textLen + spaceLen, pageSize.GetBottom(30));

                // Линия отбивки над подвалом
                cb.SetColorStroke(BorderColor);
                cb.MoveTo(pageSize.GetLeft(40), pageSize.GetBottom(45));
                cb.LineTo(pageSize.GetRight(40), pageSize.GetBottom(45));
                cb.Stroke();
            }

            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                _totalPagesTemplate.BeginText();
                _totalPagesTemplate.SetFontAndSize(_footerFont.BaseFont, _footerFont.Size);
                _totalPagesTemplate.SetColorFill(_footerFont.Color);
                _totalPagesTemplate.SetTextMatrix(0, 0);
                _totalPagesTemplate.ShowText((writer.PageNumber).ToString());
                _totalPagesTemplate.EndText();
            }
        }
    }
}