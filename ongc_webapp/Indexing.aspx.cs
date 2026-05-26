using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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

        protected void btnApplyFilters_Click(object sender, EventArgs e)
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
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            foreach (ListItem item in cblColumns.Items)
            {
                item.Selected = false;
            }
        }

        private void GenerateDynamicFilters(
            HashSet<string> availableColumns)
        {
            phDynamicFilters.Controls.Clear();

            foreach (string column in
                availableColumns.OrderBy(c => c))
            {
                Panel panel = new Panel();
                panel.CssClass = "filter-row";

                CheckBox cb = new CheckBox();

                cb.ID = "cb_" + column;

                // persist checked state

                bool isChecked =
                    Request.Form[cb.UniqueID] == "on";

                cb.Checked = isChecked;

                Literal lbl = new Literal();

                lbl.Text =
                    "<span class='filter-column-name'>" +
                    column +
                    "</span>";

                Panel textboxPanel = new Panel();

                string panelId =
                    "box_" +
                    column.Replace(" ", "_");

                textboxPanel.Attributes["id"] =
                    panelId;

                bool shouldShow =
                    Request.Form[cb.UniqueID] == "on";

                textboxPanel.Style["display"] =
                    shouldShow
                    ? "block"
                    : "none";

                textboxPanel.CssClass =
                    "filter-input-box";

                cb.InputAttributes.Add(
                    "onclick",
                    "toggleFilterTextbox('" +
                    panelId +
                    "')");

                TextBox txt = new TextBox();

                txt.ID = "txt_" + column;

                // persist textbox value

                string textboxValue =
                    Request.Form[txt.UniqueID];

                if (!string.IsNullOrWhiteSpace(textboxValue))
                {
                    txt.Text = textboxValue;
                }

                txt.CssClass =
                    "form-control";

                txt.Attributes["placeholder"] =
                    "Filter value...";

                textboxPanel.Controls.Add(txt);

                panel.Controls.Add(cb);
                panel.Controls.Add(lbl);
                panel.Controls.Add(textboxPanel);

                phDynamicFilters.Controls.Add(panel);

                if (cblColumns.Items.FindByValue(column)
                    == null)
                {
                    cblColumns.Items.Add(
                        new ListItem(column, column));
                }
            }
        }

        private HashSet<string> GetFilteredMetadataColumns(
        List<Dictionary<string, string>> allRows)
        {
            HashSet<string> columns =
                new HashSet<string>();

            // visible columns from old filter

            HashSet<string> selectedColumns =
                new HashSet<string>();

            bool anySelected = false;

            foreach (ListItem item in cblColumns.Items)
            {
                if (item.Selected)
                {
                    anySelected = true;
                    selectedColumns.Add(item.Value);
                }
            }

            foreach (var row in allRows)
            {
                foreach (var kv in row)
                {
                    string key = kv.Key;

                    string value =
                        kv.Value != null
                        ? kv.Value.ToString()
                        : "";

                    // skip system columns

                    if (
                        key == "id" ||
                        key == "source_excel_file" ||
                        key == "file_name" ||
                        key == "file_path"
                    )
                    {
                        continue;
                    }

                    // skip empty values

                    if (string.IsNullOrWhiteSpace(value) ||
                        value == "NULL")
                    {
                        continue;
                    }

                    // apply old column filter

                    if (anySelected)
                    {
                        if (!selectedColumns.Contains(key))
                        {
                            continue;
                        }
                    }

                    columns.Add(key);
                }
            }

            return columns;
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

            string query = @"
                SELECT
                    id,
                    source_excel_file,
                    file_name,
                    file_path,
                    dynamic_metadata
                FROM indexed_documents";

            List<string> whereConditions =
                new List<string>();

            // KEYWORD SEARCH

            if (keywords.Count > 0)
            {
                List<string> keywordConditions =
                    new List<string>();

                for (int i = 0; i < keywords.Count; i++)
                {
                    keywordConditions.Add($@"
                    (
                        file_name ILIKE @kw{i}
                        OR source_excel_file ILIKE @kw{i}
                        OR dynamic_metadata::text ILIKE @kw{i}
                    )");
                }

                whereConditions.Add(
                    "(" +
                    string.Join(
                        $" {searchMode} ",
                        keywordConditions) +
                    ")");
            }

            // METADATA FILTERS

            foreach (string key in Request.Form.AllKeys)
            {
                if (key != null &&
                    key.Contains("txt_"))
                {
                    string value =
                        Request.Form[key];

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        string column =
                            key.Substring(
                                key.IndexOf("txt_") + 4);

                        string param =
                            "@filter_" +
                            column.Replace(" ", "_");

                        whereConditions.Add(
                            $"dynamic_metadata->>'{column}' ILIKE {param}");
                    }
                }
            }

            if (whereConditions.Count > 0)
            {
                query +=
                    " WHERE " +
                    string.Join(
                        " AND ",
                        whereConditions);
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
                    // KEYWORDS

                    for (int i = 0; i < keywords.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(
                            $"@kw{i}",
                            "%" + keywords[i] + "%");
                    }

                    // FILTER PARAMETERS

                    foreach (string key in Request.Form.AllKeys)
                    {
                        if (key != null &&
                            key.Contains("txt_"))
                        {
                            string value =
                                Request.Form[key];

                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                string column =
                                    key.Substring(
                                        key.IndexOf("txt_") + 4);

                                cmd.Parameters.AddWithValue(
                                    "@filter_" +
                                    column.Replace(" ", "_"),
                                    "%" + value.Trim() + "%");
                            }
                        }
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

                            string originalPath =
                                reader["file_path"].ToString();

                            string encodedPath =
                                Server.UrlEncode(originalPath);

                            rowMap["file_path"] =
                                "<a target='_blank' " +
                                "style='color:#2563eb;text-decoration:underline;' " +
                                "href='OpenFolder.aspx?path=" +
                                encodedPath +
                                "'>" +
                                originalPath +
                                "</a>";

                            string metadataJson =
                                reader["dynamic_metadata"]
                                .ToString();

                            if (!string.IsNullOrWhiteSpace(metadataJson))
                            {
                                JObject metadata =
                                    JsonConvert
                                    .DeserializeObject<JObject>(
                                        metadataJson);

                                foreach (var item in metadata)
                                {
                                    string key =
                                        item.Key;

                                    string value =
                                        item.Value != null
                                        ? item.Value.ToString()
                                        : "NULL";

                                    rowMap[key] = value;

                                    metadataColumns.Add(key);
                                }
                            }

                            allRows.Add(rowMap);
                        }
                    }
                }
            }

            // FILTERS BASED ON CURRENT RESULTS

            HashSet<string> filteredColumns =
                GetFilteredMetadataColumns(allRows);

            GenerateDynamicFilters(filteredColumns);

            // COLUMN FILTERS

            bool anyColumnsSelected = false;

            foreach (ListItem item in cblColumns.Items)
            {
                if (item.Selected)
                {
                    anyColumnsSelected = true;
                    break;
                }
            }

            if (!anyColumnsSelected)
            {
                foreach (string column in filteredColumns)
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
                        if (filteredColumns.Contains(item.Value))
                        {
                            if (!finalTable.Columns.Contains(item.Value))
                            {
                                finalTable.Columns.Add(item.Value);
                            }
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

            gvDocuments.DataSource =
                finalTable;

            gvDocuments.DataBind();

            lblStatus.Text =
                finalTable.Rows.Count +
                " result(s) found.";
        }

        protected void gvDocuments_PageIndexChanging(
        object sender,
        GridViewPageEventArgs e)
        {
            gvDocuments.PageIndex =
                e.NewPageIndex;

            BindDynamicVaultData();
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