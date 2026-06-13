using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace SwimmingPool.Services
{
    public static class ExcelExportService
    {

        public static void ExportToExcel<T>(IEnumerable<T> data, Dictionary<string, string> columnMappings, string fileNamePrefix)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv",
                    FileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd}.csv",
                    Title = "Экспорт в Excel"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sb = new StringBuilder();

                    var headers = new List<string>();
                    var properties = new List<PropertyInfo>();

                    foreach (var mapping in columnMappings)
                    {
                        headers.Add(mapping.Value);
                        properties.Add(typeof(T).GetProperty(mapping.Key));
                    }

                    sb.AppendLine(string.Join(";", headers));

                    foreach (var item in data)
                    {
                        var row = new List<string>();
                        foreach (var prop in properties)
                        {
                            if (prop != null)
                            {
                                var value = prop.GetValue(item);
                                string valStr = value?.ToString() ?? "";

                                if (valStr.Contains(";") || valStr.Contains("\"") || valStr.Contains("\n"))
                                {
                                    valStr = $"\"{valStr.Replace("\"", "\"\"")}\"";
                                }
                                row.Add(valStr);
                            }
                            else
                            {
                                row.Add("");
                            }
                        }
                        sb.AppendLine(string.Join(";", row));
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Данные успешно экспортированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Diagnostics.Process.Start(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}