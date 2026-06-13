using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using SwimmingPool.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SwimmingPool.Services
{
    public static class PdfExportService
    {
        public static void GenerateSchedule(ObservableCollection<CalendarDay> days, string filePath, string monthTitle)
        {
            try
            {
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIAL.TTF");
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font titleFont = new Font(baseFont, 18, Font.BOLD);
                Font headerFont = new Font(baseFont, 10, Font.BOLD, BaseColor.WHITE);
                Font dayNumberFont = new Font(baseFont, 12, Font.BOLD);
                Font classFont = new Font(baseFont, 8, Font.NORMAL);

                Document document = new Document(PageSize.A4.Rotate());
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                Paragraph title = new Paragraph(monthTitle, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(title);

                PdfPTable table = new PdfPTable(7) { WidthPercentage = 100 };

                string[] dayNames = { "ПОНЕДЕЛЬНИК", "ВТОРНИК", "СРЕДА", "ЧЕТВЕРГ", "ПЯТНИЦА", "СУББОТА", "ВОСКРЕСЕНЬЕ" };
                foreach (string dayName in dayNames)
                {
                    PdfPCell headerCell = new PdfPCell(new Phrase(dayName, headerFont))
                    {
                        BackgroundColor = new BaseColor(45, 45, 48),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(headerCell);
                }

                foreach (var day in days)
                {
                    PdfPCell dayCell = new PdfPCell
                    {
                        MinimumHeight = 70,
                        VerticalAlignment = Element.ALIGN_TOP
                    };

                    var dayNumberPara = new Paragraph(day.DayNumber.ToString(), dayNumberFont);
                    if (!day.IsTargetMonth) dayNumberPara.Font.Color = BaseColor.LIGHT_GRAY;
                    dayCell.AddElement(dayNumberPara);

                    foreach (var cls in day.Classes)
                    {
                        string classText = $"{cls.StartTime:HH:mm}-{cls.EndTime:HH:mm}\n{cls.ActivityTypeName}";
                        dayCell.AddElement(new Paragraph(classText, classFont));
                    }

                    table.AddCell(dayCell);
                }

                document.Add(table);
                document.Close();
                writer.Close();

                MessageBox.Show($"Расписание успешно экспортировано в файл:\n{filePath}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при экспорте в PDF: {ex.Message}", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void GenerateClientPass(Client client, string filePath)
        {
            try
            {
                Document document = new Document(PageSize.A4); 
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();

                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIAL.TTF");
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                Font schoolNameFont = new Font(baseFont, 16, Font.BOLD, BaseColor.WHITE);
                Font headerFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.WHITE);
                Font nameFont = new Font(baseFont, 18, Font.BOLD, BaseColor.BLACK);
                Font infoFont = new Font(baseFont, 10, Font.NORMAL, BaseColor.DARK_GRAY);
                Font idFont = new Font(baseFont, 8, Font.NORMAL, BaseColor.GRAY);

                PdfPTable outerTable = new PdfPTable(1);
                outerTable.WidthPercentage = 100;
                outerTable.DefaultCell.Border = Rectangle.NO_BORDER;

                PdfPTable cardTable = new PdfPTable(1);
                cardTable.TotalWidth = 300f; 
                cardTable.LockedWidth = true;

                PdfPCell cardCell = new PdfPCell();
                cardCell.Border = Rectangle.BOX;
                cardCell.BorderWidth = 1f;
                cardCell.BorderColor = new BaseColor(200, 200, 200);
                cardCell.Padding = 0;

                PdfPTable headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100;

                PdfPCell headerCell = new PdfPCell();
                headerCell.BackgroundColor = new BaseColor(0, 171, 169); 
                headerCell.Padding = 15;
                headerCell.Border = Rectangle.NO_BORDER;
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;

                headerCell.AddElement(new Paragraph("КЛИМОВСКАЯ", schoolNameFont) { Alignment = Element.ALIGN_CENTER });
                headerCell.AddElement(new Paragraph("СПОРТИВНАЯ ШКОЛА", headerFont) { Alignment = Element.ALIGN_CENTER });

                headerTable.AddCell(headerCell);
                cardCell.AddElement(headerTable);

                PdfPTable bodyTable = new PdfPTable(1);
                bodyTable.WidthPercentage = 100;
                bodyTable.DefaultCell.Border = Rectangle.NO_BORDER;
                bodyTable.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                bodyTable.DefaultCell.Padding = 5;

                Paragraph namePara = new Paragraph(client.FullName, nameFont);
                namePara.Alignment = Element.ALIGN_CENTER;
                namePara.SpacingBefore = 20;
                namePara.SpacingAfter = 10;

                PdfPCell nameCell = new PdfPCell(namePara);
                nameCell.Border = Rectangle.NO_BORDER;
                nameCell.HorizontalAlignment = Element.ALIGN_CENTER;
                bodyTable.AddCell(nameCell);

                Image qrImage = GenerateQrImageForPdf(client.ClientToken.ToString());
                if (qrImage != null)
                {
                    qrImage.ScaleAbsolute(150f, 150f);
                    qrImage.Alignment = Element.ALIGN_CENTER;

                    PdfPCell qrCell = new PdfPCell(qrImage);
                    qrCell.Border = Rectangle.NO_BORDER;
                    qrCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    qrCell.PaddingTop = 10;
                    qrCell.PaddingBottom = 10;
                    bodyTable.AddCell(qrCell);
                }

                bodyTable.AddCell(new Phrase("Предъявляйте этот код при входе", infoFont));

                Paragraph idPara = new Paragraph($"ID: {client.ClientToken}", idFont);
                idPara.Alignment = Element.ALIGN_CENTER;
                idPara.SpacingBefore = 20;
                idPara.SpacingAfter = 10;

                PdfPCell idCell = new PdfPCell(idPara);
                idCell.Border = Rectangle.NO_BORDER;
                idCell.HorizontalAlignment = Element.ALIGN_CENTER;
                bodyTable.AddCell(idCell);

                cardCell.AddElement(bodyTable);
                cardTable.AddCell(cardCell);

                PdfPCell outerCell = new PdfPCell(cardTable);
                outerCell.Border = Rectangle.NO_BORDER;
                outerCell.PaddingTop = 100;
                outerCell.HorizontalAlignment = Element.ALIGN_CENTER;

                outerTable.AddCell(outerCell);
                document.Add(outerTable);

                document.Close();
                writer.Close();

                MessageBox.Show($"Пропуск клиента успешно сохранен:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании пропуска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static Image GenerateQrImageForPdf(string token)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (System.Drawing.Bitmap bitmap = qrCode.GetGraphic(20))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return Image.GetInstance(stream.ToArray());
                }
            }
        }
    }
}