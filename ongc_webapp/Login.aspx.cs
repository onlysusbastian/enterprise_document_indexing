using System;
using System.Collections.Generic;
using System.Configuration; // Added for reading ConnectionStrings directly from Web.config
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql; // Added to handle database interactions cleanly

namespace ongc_webapp
{
    public partial class Login : System.Web.UI.Page
    {
        // Pull the connection string directly from your Web.config configurations
        private string connString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // KEEP: If a user is already logged in, redirect them to the Dashboard automatically
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
            // 1. Capture the data from the textboxes and remove extra spaces
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // 2. Clear out any legacy message placeholders if your frontend UI contains them
            // (Standard validation check to ensure credentials aren't blank)
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                string blankScript = "alert('Validation Error: Both Username and Password fields are required.');";
                ClientScript.RegisterStartupScript(this.GetType(), "LoginBlankError", blankScript, true);
                return;
            }

            // 3. Dynamic Secure SQL Query pointing to your live pgAdmin setup
            string authQuery = @"
                SELECT employee_name, account_status 
                FROM public.portal_users 
                WHERE LOWER(username) = LOWER(@Username) AND password_hash = @Password";

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(authQuery, conn))
                    {
                        // Safe parameter bindings to defeat script-injection vulnerabilities completely
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        conn.Open();

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Matching row found inside your portal_users table!
                            {
                                string officialName = reader["employee_name"].ToString();
                                string status = reader["account_status"].ToString();

                                if (status != "Active")
                                {
                                    string inactiveScript = "alert('Access Suspended: This personnel node profile has been deactivated.');";
                                    ClientScript.RegisterStartupScript(this.GetType(), "LoginStatusError", inactiveScript, true);
                                    return;
                                }

                                // 4. Establish a Session using the database's actual matching Employee Name
                                // This makes your Dashboard load "Welcome Back, Debangana Dutta" instantly!
                                Session["UserID"] = officialName;
                                Session["RawUsername"] = username;
                                Session["LoginTime"] = DateTime.Now.ToString();

                                // 5. Redirect to Dashboard
                                Response.Redirect("Dashboard.aspx", true);
                            }
                            else
                            {
                                // 6. Failed login - Row parameters do not match database table values
                                string failedScript = "alert('Access Denied: Invalid Credentials. Please check your Employee ID and Password.');";
                                ClientScript.RegisterStartupScript(this.GetType(), "LoginError", failedScript, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Gracefully intercept pipeline exceptions or down connection states safely
                string errorMsg = ex.Message.Replace("'", "\\'"); // Escape quotes so the alert doesn't break
                string exceptionScript = $"alert('Database Engine Pipeline Exception: {errorMsg}');";
                ClientScript.RegisterStartupScript(this.GetType(), "LoginException", exceptionScript, true);
            }
        }
    }
}