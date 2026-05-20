using System;
using System.Configuration;
using System.Data;
using Npgsql;

namespace ongc_webapp
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string connString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Keep your secure login verification check active
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return; // Stop processing the page if not logged in
            }

            // 2. Fetch data from PostgreSQL only on initial clean page load
            if (!IsPostBack)
            {
                LoadLiveSystemSummary();
            }
        }

        private void LoadLiveSystemSummary()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Fetch live file count aggregates from the database matrix
                string countQuery = @"
                    SELECT 
                        COUNT(*) as total,
                        COUNT(CASE WHEN doc_type IS NOT NULL THEN 1 END) as indexed,
                        COUNT(CASE WHEN doc_type IS NULL THEN 1 END) as pending
                    FROM public.indexed_documents";

                using (NpgsqlCommand cmd = new NpgsqlCommand(countQuery, conn))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate your frontend layout dashboard counters
                            lblTotalFiles.Text = string.Format("{0:N0}", reader["total"]);
                            lblIndexedSuccess.Text = string.Format("{0:N0}", reader["indexed"]);
                            lblPendingIndexing.Text = string.Format("{0:N0}", reader["pending"]);
                        }
                    }
                }

                // Pull down the 3 most recently uploaded files for the activity table logs
                string logQuery = @"
                    SELECT index_id, file_name, upload_date, upload_time, 
                           CASE WHEN doc_type IS NOT NULL THEN 'Success' ELSE 'In Progress' END as file_status
                    FROM public.indexed_documents 
                    ORDER BY upload_date DESC, upload_time DESC 
                    LIMIT 3";

                using (NpgsqlCommand cmd2 = new NpgsqlCommand(logQuery, conn))
                {
                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd2))
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