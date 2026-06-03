using System;
using System.Configuration;
using System.Data;
using Npgsql;

namespace ongc_webapp
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string connString =
            ConfigurationManager
            .ConnectionStrings["PostgresConn"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Session validation
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // Load dashboard only once
            if (!IsPostBack)
            {
                LoadLiveSystemSummary();
            }
        }

        private void LoadLiveSystemSummary()
        {
            using (NpgsqlConnection conn =
                new NpgsqlConnection(connString))
            {
                conn.Open();

                // =========================
                // DASHBOARD COUNTS
                // =========================

                string countQuery = @"
                    SELECT
                        COUNT(*) AS total_files,
                        MAX(uploaded_at) AS latest_upload
                    FROM indexed_documents";

                using (NpgsqlCommand cmd =
                    new NpgsqlCommand(countQuery, conn))
                {
                    using (NpgsqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Total files
                            lblTotalFiles.Text =
                                string.Format(
                                    "{0:N0}",
                                    reader["total_files"]);

                            // Since there is no indexing pipeline yet,
                            // show same count temporarily
                            lblIndexedSuccess.Text =
                                string.Format(
                                    "{0:N0}",
                                    reader["total_files"]);

                            // Placeholder until future processing states exist
                            lblPendingIndexing.Text = "0";
                        }
                    }
                }

                // =========================
                // RECENT UPLOAD LOGS
                // =========================

                string logQuery = @"
                SELECT
                    id,
                    file_name,
                    file_path,
                    dynamic_metadata
                FROM indexed_documents
                ORDER BY uploaded_at DESC
                LIMIT 5";

                using (NpgsqlCommand cmd2 =
                    new NpgsqlCommand(logQuery, conn))
                {
                    using (NpgsqlDataAdapter da =
                        new NpgsqlDataAdapter(cmd2))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        rptRecentLogs.DataSource = dt;
                        rptRecentLogs.DataBind();
                    }
                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("Login.aspx");
        }
    }
}