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

        private void BindDynamicVaultData()
        {
            List<string> selectedCircles = new List<string>();
            foreach (ListItem item in cblRegions.Items)
            {
                if (item.Selected) selectedCircles.Add(item.Value);
            }

            // Comprehensive SQL engine targeting your 11 metadata schema parameters
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