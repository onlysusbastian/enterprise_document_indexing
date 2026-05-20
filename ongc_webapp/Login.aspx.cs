using System;
using System.Configuration;
using System.Web.UI;
using Npgsql;

namespace ongc_webapp
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // If already logged in, redirect to dashboard
            if (!IsPostBack)
            {
                if (Session["UserID"] != null)
                {
                    Response.Redirect("Dashboard.aspx");
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Get textbox values
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // PostgreSQL connection string from Web.config
            string connString =
                ConfigurationManager
                .ConnectionStrings["PostgresConnection"]
                .ConnectionString;

            try
            {
                using (NpgsqlConnection conn =
                    new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // Query users table
                    string query = @"
                        SELECT username, role
                        FROM public.users
                        WHERE username = @username
                        AND password_hash = @password";

                    using (NpgsqlCommand cmd =
                        new NpgsqlCommand(query, conn))
                    {
                        // Safe parameter mapping
                        cmd.Parameters.AddWithValue("@username", username);

                        cmd.Parameters.AddWithValue("@password", password);

                        using (NpgsqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            // LOGIN SUCCESS
                            if (reader.Read())
                            {
                                // Store session values
                                Session["UserID"] =
                                    reader["username"].ToString();

                                Session["Role"] =
                                    reader["role"].ToString();

                                Session["LoginTime"] =
                                    DateTime.Now.ToString();

                                // Redirect to dashboard
                                Response.Redirect("Dashboard.aspx");
                            }
                            else
                            {
                                // LOGIN FAILED
                                string script =
                                    "alert('Access Denied: Invalid Username or Password.');";

                                ClientScript.RegisterStartupScript(
                                    this.GetType(),
                                    "LoginError",
                                    script,
                                    true
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // DATABASE ERROR
                Response.Write(
                    "<script>alert('Database Error: " +
                    ex.Message.Replace("'", "") +
                    "');</script>"
                );
            }
        }
    }
}