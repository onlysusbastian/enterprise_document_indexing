using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Web.UI;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ongc_webapp
{
    public partial class Indexing : System.Web.UI.Page
    {
        private string connString =
            ConfigurationManager
            .ConnectionStrings["PostgresConn"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Session protection
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindDynamicVaultData();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        private void BindDynamicVaultData()
        {
            string searchTerm = txtSearch.Text.Trim();

            string query = @"
                SELECT
                    id,
                    source_excel_file,
                    file_name,
                    file_path,
                    dynamic_metadata
                FROM indexed_documents
                WHERE
                (
                    @search = ''
                    OR file_name ILIKE @wildSearch
                    OR file_path ILIKE @wildSearch
                    OR dynamic_metadata::text ILIKE @wildSearch
                    OR source_excel_file ILIKE @wildSearch
                )
                ORDER BY uploaded_at DESC";

            // Final dynamic table
            DataTable finalTable = new DataTable();

            // Fixed columns
            finalTable.Columns.Add("id");
            finalTable.Columns.Add("source_excel_file");
            finalTable.Columns.Add("file_name");
            finalTable.Columns.Add("file_path");

            // Temporary storage
            List<Dictionary<string, string>> allRows =
                new List<Dictionary<string, string>>();

            // Dynamic metadata keys
            HashSet<string> metadataColumns =
                new HashSet<string>();

            using (NpgsqlConnection conn =
                new NpgsqlConnection(connString))
            {
                conn.Open();

                using (NpgsqlCommand cmd =
                    new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@search",
                        searchTerm);

                    cmd.Parameters.AddWithValue(
                        "@wildSearch",
                        "%" + searchTerm + "%");

                    using (NpgsqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, string> rowMap =
                                new Dictionary<string, string>();

                            // Fixed fields
                            rowMap["id"] =
                                reader["id"].ToString();

                            rowMap["source_excel_file"] =
                                reader["source_excel_file"].ToString();

                            rowMap["file_name"] =
                                reader["file_name"].ToString();

                            rowMap["file_path"] =
                                reader["file_path"].ToString();

                            // Read metadata JSON
                            string metadataJson =
                                reader["dynamic_metadata"].ToString();

                            if (!string.IsNullOrWhiteSpace(metadataJson))
                            {
                                JObject metadata =
                                    JsonConvert.DeserializeObject<JObject>(
                                        metadataJson);

                                foreach (var item in metadata)
                                {
                                    string key = item.Key;

                                    string value =
                                        item.Value != null
                                        ? item.Value.ToString()
                                        : "";

                                    // Add metadata to row
                                    rowMap[key] = value;

                                    // Track dynamic columns
                                    metadataColumns.Add(key);
                                }
                            }

                            allRows.Add(rowMap);
                        }
                    }
                }
            }

            // Add metadata columns dynamically
            foreach (string column in metadataColumns)
            {
                if (!finalTable.Columns.Contains(column))
                {
                    finalTable.Columns.Add(column);
                }
            }

            // Populate rows
            foreach (var rowMap in allRows)
            {
                DataRow row = finalTable.NewRow();

                foreach (DataColumn col in finalTable.Columns)
                {
                    if (rowMap.ContainsKey(col.ColumnName))
                    {
                        row[col.ColumnName] =
                            rowMap[col.ColumnName];
                    }
                    else
                    {
                        row[col.ColumnName] = "NULL";
                    }
                }

                finalTable.Rows.Add(row);
            }

            // Dynamic GridView generation
            gvDocuments.AutoGenerateColumns = true;

            gvDocuments.DataSource = finalTable;
            gvDocuments.DataBind();

            lblStatus.Text =
                finalTable.Rows.Count +
                " result(s) found.";

            lblStatus.Style["display"] = "block";
        }
    }
}