using SwimmingPool.Models;
using Xceed.Words.NET;
using Xceed.Document.NET; // Добавлено это пространство имен для Alignment и TableDesign
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Drawing;

namespace SwimmingPool.Services
{
    public class WordExportService
    {
        private const string TemplateFileName = "ContractTemplate.docx";
        private const string FontFamily = "Times New Roman";
        private const double FontSize = 14;

        public void GenerateClientContract(Client client)
        {
            try
            {
                // 1. Проверяем/Создаем шаблон
                EnsureTemplateExists();

                // 2. Определяем пути
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateFileName);
                string outputFileName = $"Договор_{client.FullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.docx";
                string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SwimmingPool_Contracts", outputFileName);

                // Создаем папку, если нет
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                // 3. Загружаем документ и меняем метки
                using (var doc = DocX.Load(templatePath))
                {
                    // Замена меток на реальные данные
                    // При замене форматирование исходного текста (метки) сохраняется
                    doc.ReplaceText("{Date}", DateTime.Now.ToString("dd MMMM yyyy"));
                    doc.ReplaceText("{FullName}", client.FullName ?? "_________________");
                    doc.ReplaceText("{Passport}", client.PassportNumber ?? "_________________");
                    doc.ReplaceText("{Address}", client.Address ?? "_________________");
                    doc.ReplaceText("{Phone}", client.PhoneNumber ?? "_________________");

                    // 4. Сохраняем
                    doc.SaveAs(outputPath);
                }

                // 5. Открываем файл
                Process.Start(outputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании Word-документа: {ex.Message}\n\nУбедитесь, что файл не открыт в другой программе.",
                    "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnsureTemplateExists()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplateFileName);

            if (!File.Exists(path))
            {
                using (var doc = DocX.Create(path))
                {
                    doc.InsertParagraph("ДОГОВОР ОКАЗАНИЯ УСЛУГ № ______")
                       .Font(FontFamily).FontSize(16).Bold().Alignment = Alignment.center;

                    doc.InsertParagraph($"г. Климово                                                                  {DateTime.Now:yyyy} г.")
                       .Font(FontFamily).FontSize(FontSize).Alignment = Alignment.both;

                    doc.InsertParagraph().InsertText("\n");

                    doc.InsertParagraph("МБУ ДО «Климовская СШ», именуемое в дальнейшем «Исполнитель», в лице Директора, с одной стороны, и")
                       .Font(FontFamily).FontSize(FontSize).Alignment = Alignment.both;

                    // Здесь мы вставляем метки. Важно применить шрифт к ним, чтобы при замене он сохранился.
                    var pClient = doc.InsertParagraph();
                    pClient.Append($"Гр. {{FullName}}, паспорт {{Passport}}, проживающий по адресу: {{Address}},")
                           .Font(FontFamily).FontSize(FontSize).Bold();
                    pClient.Alignment = Alignment.both;

                    doc.InsertParagraph("именуемый(ая) в дальнейшем «Заказчик», заключили настоящий Договор о нижеследующем:")
                       .Font(FontFamily).FontSize(FontSize).Alignment = Alignment.both;

                    doc.InsertParagraph("\n1. ПРЕДМЕТ ДОГОВОРА")
                       .Font(FontFamily).FontSize(FontSize).Bold();

                    doc.InsertParagraph("1.1. Исполнитель обязуется предоставить Заказчику физкультурно-оздоровительные услуги (посещение бассейна), а Заказчик обязуется оплатить эти услуги согласно действующему прейскуранту.")
                       .Font(FontFamily).FontSize(FontSize).Alignment = Alignment.both;

                    doc.InsertParagraph("\n2. ПРАВА И ОБЯЗАННОСТИ СТОРОН")
                       .Font(FontFamily).FontSize(FontSize).Bold();

                    doc.InsertParagraph("2.1. Заказчик обязан соблюдать правила посещения бассейна, иметь при себе сменную обувь и предметы личной гигиены.")
                       .Font(FontFamily).FontSize(FontSize).Alignment = Alignment.both;
                    doc.InsertParagraph("2.2. Исполнитель обязан обеспечить безопасность оказания услуг и надлежащее санитарное состояние бассейна.")
                       .Font(FontFamily).FontSize(FontSize).Alignment = Alignment.both;

                    doc.InsertParagraph("\n3. РЕКВИЗИТЫ СТОРОН")
                       .Font(FontFamily).FontSize(FontSize).Bold().Alignment = Alignment.center;

                    var table = doc.AddTable(2, 2);
                    table.Design = TableDesign.TableGrid;
                    table.Alignment = Alignment.center;
                    table.SetWidths(new float[] { 250, 250 }); 

                    table.Rows[0].Cells[0].Paragraphs[0].Append("ИСПОЛНИТЕЛЬ")
                        .Font(FontFamily).FontSize(FontSize).Bold().Alignment = Alignment.center;
                    table.Rows[0].Cells[1].Paragraphs[0].Append("ЗАКАЗЧИК")
                        .Font(FontFamily).FontSize(FontSize).Bold().Alignment = Alignment.center;

                    table.Rows[1].Cells[0].Paragraphs[0].Append("МБУ ДО Климовская СШ\nАдрес: Брянская обл., рп. Климово\nИНН: 3200000000\n\n\nПодпись: ___________ (Директор)")
                        .Font(FontFamily).FontSize(FontSize);

                    table.Rows[1].Cells[1].Paragraphs[0].Append("ФИО: {FullName}\nПаспорт: {Passport}\nТел: {Phone}\n\n\nПодпись: ___________ (Заказчик)")
                        .Font(FontFamily).FontSize(FontSize);

                    doc.InsertTable(table);

                    doc.Save();
                }
            }
        }
    }
}