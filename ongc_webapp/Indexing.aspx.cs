using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;

namespace ongc_webapp
{
    public partial class Indexing : System.Web.UI.Page
    {
        private string connString =
            ConfigurationManager
            .ConnectionStrings["PostgresConnection"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Session protection
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
                BindDynamicVaultData();
            }
        }

        // ==========================================
        // FILTER EVENTS
        // ==========================================

        protected void FilterTriggered(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";

            ddlDocType.SelectedIndex = 0;
            ddlDepartment.SelectedIndex = 0;
            ddlYear.SelectedIndex = 0;

            foreach (ListItem item in cblRegions.Items)
            {
                item.Selected = false;
            }

            BindDynamicVaultData();
        }

        // ==========================================
        // DOCUMENT UPLOAD SYSTEM
        // ==========================================

        protected void btnExecuteUpload_Click(object sender, EventArgs e)
        {
            // File validation
            if (!fileVaultUpload.HasFile)
            {
                lblUploadMsg.ForeColor =
                    System.Drawing.Color.Orange;

                lblUploadMsg.Text =
                    "Please select a document before uploading.";

                return;
            }

            // Dropdown validation
            if (string.IsNullOrEmpty(ddlUploadRegion.SelectedValue) ||
                string.IsNullOrEmpty(ddlUploadDept.SelectedValue) ||
                string.IsNullOrEmpty(ddlUploadProject.SelectedValue))
            {
                lblUploadMsg.ForeColor =
                    System.Drawing.Color.Red;

                lblUploadMsg.Text =
                    "Please complete all metadata fields.";

                return;
            }

            try
            {
                // ==========================================
                // FILE METADATA
                // ==========================================

                string rawFileName =
                    System.IO.Path.GetFileName(
                        fileVaultUpload.FileName);

                string fileTypeExt =
                    System.IO.Path
                    .GetExtension(rawFileName)
                    .Replace(".", "")
                    .ToUpper();

                string currentSessionUser =
                    Session["UserID"] != null
                    ? Session["UserID"].ToString()
                    : "Authorized Agent";

                string docAbstract =
                    string.IsNullOrWhiteSpace(txtDescription.Text)
                    ? "Central repository manual submission record."
                    : txtDescription.Text.Trim();

                DateTime executionStamp = DateTime.Now;

                // ==========================================
                // INSERT QUERY
                // ==========================================

                string insertSqlCommand = @"
                    INSERT INTO public.indexed_documents
                    (
                        file_name,
                        region,
                        doc_type,
                        department,
                        upload_date,
                        upload_time,
                        upload_year,
                        employee_assigned,
                        project_name,
                        uploader_identity,
                        description
                    )
                    VALUES
                    (
                        @FileName,
                        @Region,
                        @DocType,
                        @Dept,
                        @UploadDate,
                        @UploadTime,
                        @UploadYear,
                        @Employee,
                        @Project,
                        @Uploader,
                        @Desc
                    )";

                using (NpgsqlConnection conn =
                    new NpgsqlConnection(connString))
                {
                    using (NpgsqlCommand cmd =
                        new NpgsqlCommand(insertSqlCommand, conn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@FileName",
                            rawFileName);

                        cmd.Parameters.AddWithValue(
                            "@Region",
                            ddlUploadRegion.SelectedValue);

                        cmd.Parameters.AddWithValue(
                            "@DocType",
                            fileTypeExt);

                        cmd.Parameters.AddWithValue(
                            "@Dept",
                            ddlUploadDept.SelectedValue);

                        cmd.Parameters.AddWithValue(
                            "@UploadDate",
                            executionStamp.Date);

                        // FIXED TIMESTAMP
                        cmd.Parameters.AddWithValue(
                            "@UploadTime",
                            executionStamp);

                        cmd.Parameters.AddWithValue(
                            "@UploadYear",
                            executionStamp.Year);

                        cmd.Parameters.AddWithValue(
                            "@Employee",
                            currentSessionUser);

                        cmd.Parameters.AddWithValue(
                            "@Project",
                            ddlUploadProject.SelectedValue);

                        cmd.Parameters.AddWithValue(
                            "@Uploader",
                            currentSessionUser);

                        cmd.Parameters.AddWithValue(
                            "@Desc",
                            docAbstract);

                        conn.Open();

                        cmd.ExecuteNonQuery();
                    }
                }

                // ==========================================
                // RESET CONTROLS
                // ==========================================

                ddlUploadRegion.SelectedIndex = 0;
                ddlUploadDept.SelectedIndex = 0;
                ddlUploadProject.SelectedIndex = 0;

                txtDescription.Text = "";

                lblUploadMsg.ForeColor =
                    System.Drawing.Color.Green;

                lblUploadMsg.Text =
                    "Document uploaded successfully.";

                // Refresh grid
                BindDynamicVaultData();
            }
            catch (Exception ex)
            {
                lblUploadMsg.ForeColor =
                    System.Drawing.Color.Red;

                lblUploadMsg.Text =
                    "Upload Error: " + ex.Message;
            }
        }

        // ==========================================
        // GRIDVIEW SEARCH ENGINE
        // ==========================================

        private void BindDynamicVaultData()
        {
            List<string> selectedCircles =
                new List<string>();

            foreach (ListItem item in cblRegions.Items)
            {
                if (item.Selected)
                {
                    selectedCircles.Add(item.Value);
                }
            }

            string query = @"
                SELECT
                    index_id,
                    file_name,
                    region,
                    upload_date,
                    upload_time,
                    doc_type,
                    department,
                    employee_assigned,
                    project_name,
                    uploader_identity
                FROM public.indexed_documents
                WHERE 1=1";

            // Dynamic filters
            if (selectedCircles.Count > 0)
            {
                query += " AND region = ANY(@Circles)";
            }

            if (!string.IsNullOrEmpty(ddlDocType.SelectedValue))
            {
                query += " AND doc_type = @DocType";
            }

            if (!string.IsNullOrEmpty(ddlDepartment.SelectedValue))
            {
                query += " AND department = @Department";
            }

            if (!string.IsNullOrEmpty(ddlYear.SelectedValue))
            {
                query += " AND upload_year = @UploadYear";
            }

            if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                query += @"
                    AND
                    (
                        LOWER(file_name) LIKE @Term
                        OR LOWER(employee_assigned) LIKE @Term
                        OR LOWER(project_name) LIKE @Term
                        OR LOWER(uploader_identity) LIKE @Term
                        OR @RawTerm = ANY(metadata_tags)
                    )";
            }

            query +=
                " ORDER BY upload_date DESC, upload_time DESC";

            using (NpgsqlConnection conn =
                new NpgsqlConnection(connString))
            {
                using (NpgsqlCommand cmd =
                    new NpgsqlCommand(query, conn))
                {
                    // Region filter
                    if (selectedCircles.Count > 0)
                    {
                        cmd.Parameters.AddWithValue(
                            "Circles",
                            selectedCircles.ToArray());
                    }

                    // Type filter
                    if (!string.IsNullOrEmpty(
                        ddlDocType.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue(
                            "DocType",
                            ddlDocType.SelectedValue);
                    }

                    // Department filter
                    if (!string.IsNullOrEmpty(
                        ddlDepartment.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue(
                            "Department",
                            ddlDepartment.SelectedValue);
                    }

                    // Year filter
                    if (!string.IsNullOrEmpty(
                        ddlYear.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue(
                            "UploadYear",
                            Convert.ToInt32(
                                ddlYear.SelectedValue));
                    }

                    // Search term
                    if (!string.IsNullOrEmpty(
                        txtSearch.Text.Trim()))
                    {
                        string cleanTerm =
                            txtSearch.Text
                            .Trim()
                            .ToLower();

                        cmd.Parameters.AddWithValue(
                            "Term",
                            "%" + cleanTerm + "%");

                        cmd.Parameters.AddWithValue(
                            "RawTerm",
                            cleanTerm);
                    }

                    using (NpgsqlDataAdapter da =
                        new NpgsqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvDocuments.DataSource = dt;

                        gvDocuments.DataBind();
                    }
                }
            }
        }
    }
}