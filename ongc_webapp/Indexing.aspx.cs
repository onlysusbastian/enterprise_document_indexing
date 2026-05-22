using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using Npgsql;

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
            // Optional session protection
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
                    file_name,
                    file_path,
                    dynamic_metadata,
                    uploaded_at
                FROM indexed_documents
                WHERE
                    (
                        @search = ''
                        OR file_name ILIKE @wildSearch
                        OR file_path ILIKE @wildSearch
                        OR dynamic_metadata::text ILIKE @wildSearch
                    )
                ORDER BY uploaded_at DESC";

            using (NpgsqlConnection conn =
                new NpgsqlConnection(connString))
            {
                using (NpgsqlCommand cmd =
                    new NpgsqlCommand(query, conn))
                {
                    // Search parameters
                    cmd.Parameters.AddWithValue(
                        "@search",
                        searchTerm);

                    cmd.Parameters.AddWithValue(
                        "@wildSearch",
                        "%" + searchTerm + "%");

                    using (NpgsqlDataAdapter da =
                        new NpgsqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvDocuments.DataSource = dt;
                        gvDocuments.DataBind();

                        // Optional status label
                        lblStatus.Text =
                            dt.Rows.Count +
                            " result(s) found.";

                        lblStatus.Style["display"] = "block";
                    }
                }
            }
        }
    }
}