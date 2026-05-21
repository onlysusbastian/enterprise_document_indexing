using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;

namespace ongc_webapp
{
    public partial class Upload : System.Web.UI.Page
    {
        private string connString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgresConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnIngestData_Click(object sender, EventArgs e)
        {
            // Clean Corporate Validation Check
            if (string.IsNullOrWhiteSpace(txtTableName.Text) || string.IsNullOrWhiteSpace(hdnGridHeaders.Value))
            {
                lblStatusFeedback.Text = "⚠️ Validation Error: Please provide a company name and enter table records.";
                lblStatusFeedback.ForeColor = System.Drawing.Color.OrangeRed;
                return;
            }

            try
            {
                var serializer = new JavaScriptSerializer();

                // 1. Deserialize the dynamic spreadsheet arrays sent via hidden fields
                List<string> headers = serializer.Deserialize<List<string>>(hdnGridHeaders.Value);
                List<List<string>> rows = serializer.Deserialize<List<List<string>>>(hdnGridRows.Value);

                // Safe fallback for file logging
                string fileName = filePayload.HasFile ? filePayload.FileName : "Manual_Grid_Entry";

                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // 2. Loop through every data row added by your sir
                    foreach (var rowCells in rows)
                    {
                        var dynamicRecordMap = new Dictionary<string, string>();

                        // CRITICAL FIX: We loop up to headers.Count. 
                        // Because rowCells contains an extra trailing cell for the "Delete" button, 
                        // matching it precisely to headers.Count automatically ignores the Action column!
                        for (int i = 0; i < headers.Count; i++)
                        {
                            if (i < rowCells.Count)
                            {
                                dynamicRecordMap[headers[i]] = rowCells[i];
                            }
                        }

                        // 3. Serialize this specific row's fields into optimized JSONB layout
                        string serializedRowJSON = serializer.Serialize(dynamicRecordMap);

                        string insertQuery = @"INSERT INTO company_documents (company_name, file_name, dynamic_metadata) 
                                             VALUES (@comp, @file, @meta::jsonb)";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("comp", txtTableName.Text.Trim());
                            cmd.Parameters.AddWithValue("file", fileName);
                            cmd.Parameters.AddWithValue("meta", serializedRowJSON);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Clean Success Status Display Reset
                lblStatusFeedback.Text = "🚀 Data saved successfully. Structural rows processed into repository schema.";
                lblStatusFeedback.ForeColor = System.Drawing.Color.MediumSeaGreen;

                // Clear controls data state for subsequent entries
                txtTableName.Text = "";
                hdnGridHeaders.Value = "";
                hdnGridRows.Value = "";
            }
            catch (Exception ex)
            {
                lblStatusFeedback.Text = "❌ Process Failure: " + ex.Message;
                lblStatusFeedback.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}