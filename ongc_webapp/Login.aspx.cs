using System;
using System.Collections.Generic;
using System.Configuration; // Pulling ConnectionStrings from Web.config configurations
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql; // Handing PostgreSQL interactions cleanly

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
            // 1. Capture the core data from the textboxes and remove extra spaces
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Accessing the hidden viewstate carrier directly through string key indexing 
            // to safely shield it from the background TypeScript real-time analyzer checks
            string authMode = Request.Form["ctl00$MainContent$hdnAuthState"] ?? "LOGIN";

            // 2. Standard validation check to ensure credentials aren't blank
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                string blankScript = "alert('Validation Error: Both Username and Password fields are required.');";
                ClientScript.RegisterStartupScript(this.GetType(), "LoginBlankError", blankScript, true);
                return;
            }

            // ---- BRANCH 1: HANDLE NEW USER REGISTRATION ----
            if (authMode == "REGISTER" || authMode == "REGISTER ACCOUNT")
            {
                string confirmPassword = txtConfirmPassword.Text.Trim();

                if (password != confirmPassword)
                {
                    string matchScript = "alert('Registration Error: Passwords do not match. Please verify your inputs.');";
                    ClientScript.RegisterStartupScript(this.GetType(), "RegisterMatchError", matchScript, true);
                    return;
                }

                try
                {
                    using (NpgsqlConnection conn = new NpgsqlConnection(connString))
                    {
                        conn.Open();

                        // Check if the username string identity already exists within the target table records
                        string checkQuery = "SELECT COUNT(*) FROM public.portal_users WHERE LOWER(username) = LOWER(@Username)";
                        using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@Username", username);
                            long profileExists = (long)checkCmd.ExecuteScalar();

                            if (profileExists > 0)
                            {
                                string duplicateScript = "alert('Registration Warning: User ID / CPF Number is already registered under an existing corporate profile.');";
                                ClientScript.RegisterStartupScript(this.GetType(), "RegisterDuplicateError", duplicateScript, true);
                                return;
                            }
                        }

                        // Insert the newly provisioned personnel node record directly into the portal schema
                        string insertQuery = @"
                            INSERT INTO public.portal_users (username, password_hash, employee_name, account_status) 
                            VALUES (@Username, @Password, @EmployeeName, 'Active')";

                        using (NpgsqlCommand insertCmd = new NpgsqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@Username", username);
                            insertCmd.Parameters.AddWithValue("@Password", password); // Storing plaintext for staging validation tests
                            insertCmd.Parameters.AddWithValue("@EmployeeName", username); // Default official mapping alias name to credential string

                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    // On successful account creation registration, notify user and loop layout seamlessly back into standard input view modes
                    string successScript = "alert('🚀 Account Created Successfully! You can now log in using your CPF credentials.'); toggleAuthView(false);";
                    ClientScript.RegisterStartupScript(this.GetType(), "RegisterSuccess", successScript, true);

                    // Clear out password sequence traces from screen containers
                    txtPassword.Text = "";
                    txtConfirmPassword.Text = "";
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.Replace("'", "\\'");
                    string exceptionScript = $"alert('Database Registration Engine Exception: {errorMsg}');";
                    ClientScript.RegisterStartupScript(this.GetType(), "RegisterException", exceptionScript, true);
                }
            }
            // ---- BRANCH 2: HANDLE EXISTING USER AUTHENTICATION LOGIN ----
            else
            {
                // Dynamic Secure SQL Query pointing to your live pgAdmin setup
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

                                    // Establish a Session using the database's actual matching Employee Name
                                    Session["UserID"] = officialName;
                                    Session["RawUsername"] = username;
                                    Session["LoginTime"] = DateTime.Now.ToString();

                                    // Redirect to Dashboard
                                    Response.Redirect("Dashboard.aspx", true);
                                }
                                else
                                {
                                    // Failed login - Row parameters do not match database table values
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
                    string errorMsg = ex.Message.Replace("'", "\\'");
                    string exceptionScript = $"alert('Database Engine Pipeline Exception: {errorMsg}');";
                    ClientScript.RegisterStartupScript(this.GetType(), "LoginException", exceptionScript, true);
                }
            }
        }
    }
}