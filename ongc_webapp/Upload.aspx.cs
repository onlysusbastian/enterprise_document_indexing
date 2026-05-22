using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using ClosedXML.Excel;
using Npgsql;
using Newtonsoft.Json;

namespace ongc_webapp
{
    public partial class Upload : System.Web.UI.Page
    {
        private string connString =
            System.Configuration.ConfigurationManager
            .ConnectionStrings["PostgresConn"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnIngestData_Click(object sender, EventArgs e)
        {
            // Validate file upload
            if (!filePayload.HasFile)
            {
                lblStatusFeedback.Text =
                    "⚠️ Please upload an Excel file.";

                lblStatusFeedback.ForeColor =
                    System.Drawing.Color.OrangeRed;

                return;
            }

            try
            {
                using (var stream = filePayload.PostedFile.InputStream)
                using (var workbook = new XLWorkbook(stream))
                {
                    // First worksheet
                    var worksheet = workbook.Worksheet(1);

                    // IMPORTANT:
                    // Headers start from row 6
                    int headerRow = 6;

                    // Last used row/column
                    int lastRow =
                        worksheet.LastRowUsed().RowNumber();

                    int lastColumn =
                        worksheet.LastColumnUsed().ColumnNumber();

                    // Read headers
                    List<string> headers =
                        new List<string>();

                    for (int col = 1; col <= lastColumn; col++)
                    {
                        string header =
                            worksheet.Cell(headerRow, col)
                            .GetValue<string>()
                            .Trim()
                            .ToLower()
                            .Replace(" ", "_");

                        headers.Add(header);
                    }

                    using (NpgsqlConnection conn =
                        new NpgsqlConnection(connString))
                    {
                        conn.Open();

                        // Data starts AFTER header row
                        for (int row = headerRow + 1;
                             row <= lastRow;
                             row++)
                        {
                            string actualFileName = "";
                            string actualFilePath = "";

                            var dynamicMetadata =
                                new Dictionary<string, string>();

                            for (int col = 1;
                                 col <= lastColumn;
                                 col++)
                            {
                                string header =
                                    headers[col - 1];

                                string cellValue =
                                    worksheet.Cell(row, col)
                                    .GetValue<string>()
                                    .Trim();

                                // Skip empty cells
                                if (string.IsNullOrWhiteSpace(cellValue))
                                    continue;

                                // Fixed fields
                                if (header == "file_name")
                                {
                                    actualFileName = cellValue;
                                }
                                else if (header == "file_path")
                                {
                                    actualFilePath = cellValue;
                                }
                                else
                                {
                                    // Dynamic metadata
                                    dynamicMetadata[header] =
                                        cellValue;
                                }
                            }

                            // Skip invalid rows
                            if (string.IsNullOrWhiteSpace(actualFileName) ||
                                string.IsNullOrWhiteSpace(actualFilePath))
                            {
                                continue;
                            }

                            // Convert metadata to JSON
                            string metadataJson =
                                JsonConvert.SerializeObject(dynamicMetadata);

                            string insertQuery = @"
                                INSERT INTO indexed_documents
                                (
                                    file_name,
                                    file_path,
                                    dynamic_metadata
                                )
                                VALUES
                                (
                                    @file_name,
                                    @file_path,
                                    @meta::jsonb
                                )";

                            using (NpgsqlCommand cmd =
                                new NpgsqlCommand(insertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue(
                                    "file_name",
                                    actualFileName);

                                cmd.Parameters.AddWithValue(
                                    "file_path",
                                    actualFilePath);

                                cmd.Parameters.AddWithValue(
                                    "meta",
                                    metadataJson);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                lblStatusFeedback.Text =
                    "🚀 Excel data uploaded successfully.";

                lblStatusFeedback.ForeColor =
                    System.Drawing.Color.MediumSeaGreen;
            }
            catch (Exception ex)
            {
                lblStatusFeedback.Text =
                    "❌ Upload failed: " + ex.Message;

                lblStatusFeedback.ForeColor =
                    System.Drawing.Color.Red;
            }
        }
    }
}