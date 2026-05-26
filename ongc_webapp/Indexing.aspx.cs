using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            gvDocuments.RowDataBound +=
                gvDocuments_RowDataBound;

            if (!IsPostBack)
            {
                BindDynamicVaultData();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        protected void btnApplyColumns_Click(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        protected void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem item in cblColumns.Items)
            {
                item.Selected = true;
            }

            BindDynamicVaultData();
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem item in cblColumns.Items)
            {
                item.Selected = false;
            }

            BindDynamicVaultData();
        }

        private void BindDynamicVaultData()
        {
            string rawSearch =
                txtSearch.Text.Trim();

            List<string> keywords =
                rawSearch
                .Split(
                    new char[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .Take(3)
                .ToList();

            string searchMode =
                rblSearchMode.SelectedValue;

            List<string> searchableMetadataColumns =
                new List<string>();

            bool anyFilterSelected = false;

            foreach (ListItem item in cblColumns.Items)
            {
                if (item.Selected)
                {
                    anyFilterSelected = true;
                    searchableMetadataColumns.Add(item.Value);
                }
            }

            DataTable finalTable =
                new DataTable();

            finalTable.Columns.Add("id");
            finalTable.Columns.Add("source_excel_file");
            finalTable.Columns.Add("file_name");
            finalTable.Columns.Add("file_path");

            List<Dictionary<string, string>> allRows =
                new List<Dictionary<string, string>>();

            HashSet<string> metadataColumns =
                new HashSet<string>();

            Dictionary<string, int> columnMatchCount =
                new Dictionary<string, int>();

            HashSet<string> matchedColumns =
                new HashSet<string>();

            string query = @"
                SELECT
                    id,
                    source_excel_file,
                    file_name,
                    file_path,
                    dynamic_metadata
                FROM indexed_documents";

            List<string> keywordConditions =
                new List<string>();

            if (keywords.Count > 0)
            {
                for (int i = 0; i < keywords.Count; i++)
                {
                    List<string> fieldConditions =
                        new List<string>();

                    fieldConditions.Add(
                        $"file_name ILIKE @kw{i}");

                    fieldConditions.Add(
                        $"source_excel_file ILIKE @kw{i}");

                    if (!anyFilterSelected)
                    {
                        fieldConditions.Add(
                            $"dynamic_metadata::text ILIKE @kw{i}");
                    }
                    else
                    {
                        foreach (string column in searchableMetadataColumns)
                        {
                            fieldConditions.Add(
                                $"dynamic_metadata->>'{column}' ILIKE @kw{i}");
                        }
                    }

                    keywordConditions.Add(
                        "(" +
                        string.Join(" OR ", fieldConditions) +
                        ")");
                }

                query +=
                    " WHERE " +
                    string.Join(
                        $" {searchMode} ",
                        keywordConditions);
            }

            query +=
                " ORDER BY uploaded_at DESC";

            using (NpgsqlConnection conn =
                new NpgsqlConnection(connString))
            {
                conn.Open();

                using (NpgsqlCommand cmd =
                    new NpgsqlCommand(query, conn))
                {
                    for (int i = 0; i < keywords.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(
                            $"@kw{i}",
                            "%" + keywords[i] + "%");
                    }

                    using (NpgsqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, string> rowMap =
                                new Dictionary<string, string>();

                            rowMap["id"] =
                                reader["id"].ToString();

                            rowMap["source_excel_file"] =
                                reader["source_excel_file"].ToString();

                            rowMap["file_name"] =
                                reader["file_name"].ToString();

                            // CLICKABLE FILE PATH

                            string originalPath =
                                reader["file_path"].ToString();

                            string encodedPath =
                                Server.UrlEncode(originalPath);

                            string clickablePath =
                                "<a target='_blank' " +
                                "style='color:#2563eb;text-decoration:underline;font-weight:500;' " +
                                "href='OpenFolder.aspx?path=" +
                                encodedPath +
                                "'>" +
                                originalPath +
                                "</a>";

                            rowMap["file_path"] =
                                clickablePath;

                            string metadataJson =
                                reader["dynamic_metadata"].ToString();

                            if (!string.IsNullOrWhiteSpace(metadataJson))
                            {
                                JObject metadata =
                                    JsonConvert.DeserializeObject<JObject>(
                                        metadataJson);

                                foreach (var item in metadata)
                                {
                                    string key =
                                        item.Key;

                                    string value =
                                        item.Value != null
                                        ? item.Value.ToString()
                                        : "";

                                    rowMap[key] = value;

                                    metadataColumns.Add(key);

                                    foreach (string kw in keywords)
                                    {
                                        if (value
                                            .ToLower()
                                            .Contains(
                                                kw.ToLower()))
                                        {
                                            matchedColumns.Add(key);

                                            if (!columnMatchCount
                                                .ContainsKey(key))
                                            {
                                                columnMatchCount[key] = 0;
                                            }

                                            columnMatchCount[key]++;
                                        }
                                    }
                                }
                            }

                            allRows.Add(rowMap);
                        }
                    }
                }
            }

            foreach (string column in metadataColumns
                .OrderByDescending(c =>
                    columnMatchCount.ContainsKey(c)
                    ? columnMatchCount[c]
                    : 0)
                .ThenBy(c => c))
            {
                if (cblColumns.Items.FindByValue(column) == null)
                {
                    ListItem item =
                        new ListItem(column, column);

                    item.Selected = false;

                    cblColumns.Items.Add(item);
                }
            }

            bool anyDisplayFilterSelected = false;

            foreach (ListItem item in cblColumns.Items)
            {
                if (item.Selected)
                {
                    anyDisplayFilterSelected = true;
                    break;
                }
            }

            List<string> orderedColumns =
                metadataColumns
                .OrderByDescending(c =>
                    columnMatchCount.ContainsKey(c)
                    ? columnMatchCount[c]
                    : 0)
                .ThenBy(c => c)
                .ToList();

            if (!anyDisplayFilterSelected)
            {
                foreach (string column in orderedColumns)
                {
                    if (!finalTable.Columns.Contains(column))
                    {
                        finalTable.Columns.Add(column);
                    }
                }
            }
            else
            {
                foreach (ListItem item in cblColumns.Items)
                {
                    if (item.Selected)
                    {
                        if (!finalTable.Columns.Contains(item.Value))
                        {
                            finalTable.Columns.Add(item.Value);
                        }
                    }
                }
            }

            foreach (var rowMap in allRows)
            {
                DataRow row =
                    finalTable.NewRow();

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

            gvDocuments.AutoGenerateColumns = true;

            gvDocuments.DataSource =
                finalTable;

            gvDocuments.DataBind();

            if (gvDocuments.HeaderRow != null)
            {
                for (int i = 0;
                    i < gvDocuments.HeaderRow.Cells.Count;
                    i++)
                {
                    string headerText =
                        gvDocuments.HeaderRow.Cells[i].Text;

                    if (matchedColumns.Contains(headerText))
                    {
                        gvDocuments.HeaderRow
                            .Cells[i]
                            .CssClass =
                            "highlight-column";
                    }
                }
            }

            lblStatus.Text =
                finalTable.Rows.Count +
                " result(s) found.";
        }

        protected void gvDocuments_RowDataBound(
            object sender,
            GridViewRowEventArgs e)
        {
            if (e.Row.RowType ==
                DataControlRowType.DataRow)
            {
                for (int i = 0;
                    i < e.Row.Cells.Count;
                    i++)
                {
                    e.Row.Cells[i].Text =
                        Server.HtmlDecode(
                            e.Row.Cells[i].Text);
                }
            }
        }
    }
}