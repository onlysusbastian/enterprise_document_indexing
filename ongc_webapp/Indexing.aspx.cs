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
        private string connString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDynamicVaultData();
            }
        }

        // Handles the interactive dropdown selections
        protected void FilterTriggered(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        // Handles the "Apply Filters" button click
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindDynamicVaultData();
        }

        // Handles the "Reset All Filters" button click
        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlDocType.SelectedIndex = 0;
            ddlDepartment.SelectedIndex = 0;
            ddlYear.SelectedIndex = 0;
            foreach (ListItem item in cblRegions.Items)
            {
                item.Selected = false;
            }
            BindDynamicVaultData();
        }

        // NEW: Handles the Real-Time Node Document Ingestion Upload Form Click Event
        protected void btnExecuteUpload_Click(object sender, EventArgs e)
        {
            // 1. Sanity Check: Verify a file resource actually exists
            if (!fileVaultUpload.HasFile)
            {
                lblUploadMsg.ForeColor = System.Drawing.Color.Orange;
                lblUploadMsg.Text = "Warning: Please select a valid document payload resource before submitting.";
                return;
            }

            // 2. Validation Check: Ensure all dropdown criteria are explicitly chosen
            if (string.IsNullOrEmpty(ddlUploadRegion.SelectedValue) ||
                string.IsNullOrEmpty(ddlUploadDept.SelectedValue) ||
                string.IsNullOrEmpty(ddlUploadProject.SelectedValue))
            {
                lblUploadMsg.ForeColor = System.Drawing.Color.Red;
                lblUploadMsg.Text = "Validation Error: All tracking metadata fields (Asset, Department, Project) must be selected.";
                return;
            }

            try
            {
                // 3. Extract metadata dimensions
                string rawFileName = System.IO.Path.GetFileName(fileVaultUpload.FileName);
                string fileTypeExt = System.IO.Path.GetExtension(rawFileName).Replace(".", "").ToUpper();

                // Track active operator session or fallback to system agent identity safely
                string currentSessionUser = Session["UserID"] != null ? Session["UserID"].ToString() : "Authorized Agent";
                string docAbstract = string.IsNullOrWhiteSpace(txtDescription.Text) ? "Central repository manual submission record." : txtDescription.Text.Trim();

                DateTime executionStamp = DateTime.Now;

                // 4. Formulate PostgreSQL Insertion Command
                string insertSqlCommand = @"
                    INSERT INTO public.indexed_documents 
                    (file_name, region, doc_type, department, upload_date, upload_time, upload_year, employee_assigned, project_name, uploader_identity, description) 
                    VALUES 
                    (@FileName, @Region, @DocType, @Dept, @UploadDate, @UploadTime, @UploadYear, @Employee, @Project, 'Portal_Ingest_Form', @Desc)";

                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(insertSqlCommand, conn))
                    {
                        cmd.Parameters.AddWithValue("@FileName", rawFileName);
                        cmd.Parameters.AddWithValue("@Region", ddlUploadRegion.SelectedValue);
                        cmd.Parameters.AddWithValue("@DocType", fileTypeExt);
                        cmd.Parameters.AddWithValue("@Dept", ddlUploadDept.SelectedValue);
                        cmd.Parameters.AddWithValue("@UploadDate", executionStamp.Date);
                        cmd.Parameters.AddWithValue("@UploadTime", executionStamp.TimeOfDay);
                        cmd.Parameters.AddWithValue("@UploadYear", executionStamp.Year);
                        cmd.Parameters.AddWithValue("@Employee", currentSessionUser);
                        cmd.Parameters.AddWithValue("@Project", ddlUploadProject.SelectedValue);
                        cmd.Parameters.AddWithValue("@Desc", docAbstract);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // 5. Transaction Invalidation Success Step: Reset input controls
                ddlUploadRegion.SelectedIndex = 0;
                ddlUploadDept.SelectedIndex = 0;
                ddlUploadProject.SelectedIndex = 0;
                txtDescription.Text = "";

                lblUploadMsg.ForeColor = System.Drawing.Color.Green;
                lblUploadMsg.Text = "Success: Document successfully committed and synchronized into ONGC Vault cluster.";

                // 6. Refresh the data grid view instantly to show record #111
                BindDynamicVaultData();
            }
            catch (Exception ex)
            {
                lblUploadMsg.ForeColor = System.Drawing.Color.Red;
                lblUploadMsg.Text = "Ingestion Pipeline Exception: " + ex.Message;
            }
        }

        private void BindDynamicVaultData()
        {
            List<string> selectedCircles = new List<string>();
            foreach (ListItem item in cblRegions.Items)
            {
                if (item.Selected) selectedCircles.Add(item.Value);
            }

            // Comprehensive SQL engine targeting your metadata schema parameters
            string query = @"SELECT index_id, file_name, region, upload_date, upload_time, 
                             doc_type, department, employee_assigned, project_name, uploader_identity 
                             FROM public.indexed_documents WHERE 1=1";

            // Conditional parameters injection
            if (selectedCircles.Count > 0)
                query += " AND region = ANY(@Circles)";

            if (!string.IsNullOrEmpty(ddlDocType.SelectedValue))
                query += " AND doc_type = @DocType";

            if (!string.IsNullOrEmpty(ddlDepartment.SelectedValue))
                query += " AND department = @Department";

            if (!string.IsNullOrEmpty(ddlYear.SelectedValue))
                query += " AND upload_year = @UploadYear";

            if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                query += @" AND (LOWER(file_name) LIKE @Term 
                            OR LOWER(employee_assigned) LIKE @Term 
                            OR LOWER(project_name) LIKE @Term 
                            OR LOWER(uploader_identity) LIKE @Term 
                            OR @RawTerm = ANY(metadata_tags))";
            }

            query += " ORDER BY upload_date DESC, upload_time DESC";

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    // Safe mapping parameters against SQL injection vulnerabilities
                    if (selectedCircles.Count > 0)
                        cmd.Parameters.AddWithValue("Circles", selectedCircles.ToArray());

                    if (!string.IsNullOrEmpty(ddlDocType.SelectedValue))
                        cmd.Parameters.AddWithValue("DocType", ddlDocType.SelectedValue);

                    if (!string.IsNullOrEmpty(ddlDepartment.SelectedValue))
                        cmd.Parameters.AddWithValue("Department", ddlDepartment.SelectedValue);

                    if (!string.IsNullOrEmpty(ddlYear.SelectedValue))
                        cmd.Parameters.AddWithValue("UploadYear", Convert.ToInt32(ddlYear.SelectedValue));

                    if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
                    {
                        string cleanTerm = txtSearch.Text.Trim().ToLower();
                        cmd.Parameters.AddWithValue("Term", "%" + cleanTerm + "%");
                        cmd.Parameters.AddWithValue("RawTerm", cleanTerm);
                    }

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
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