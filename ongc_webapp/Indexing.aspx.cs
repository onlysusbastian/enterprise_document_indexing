using System;
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
            ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
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
            string query = @"
                SELECT
                    index_id,
                    file_name,
                    region,
                    doc_type,
                    department,
                    employee_assigned,
                    project_name,
                    uploader_identity
                FROM public.indexed_documents
                ORDER BY index_id DESC";

            using (NpgsqlConnection conn = new NpgsqlConnection(connString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
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

        protected void gvDocuments_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDocuments.EditIndex = e.NewEditIndex;
            BindDynamicVaultData();
        }

        protected void gvDocuments_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDocuments.EditIndex = -1;
            BindDynamicVaultData();
        }

        protected void gvDocuments_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                string indexId =
                    gvDocuments.DataKeys[e.RowIndex].Value.ToString();

                GridViewRow row = gvDocuments.Rows[e.RowIndex];

                string fileName =
                    ((TextBox)row.FindControl("txtFileName")).Text;

                string region =
                    ((TextBox)row.FindControl("txtRegion")).Text;

                string docType =
                    ((TextBox)row.FindControl("txtDocType")).Text;

                string department =
                    ((TextBox)row.FindControl("txtDepartment")).Text;

                string employee =
                    ((TextBox)row.FindControl("txtEmployee")).Text;

                string project =
                    ((TextBox)row.FindControl("txtProject")).Text;

                string uploader =
                    ((TextBox)row.FindControl("txtUploader")).Text;

                string query = @"
                    UPDATE public.indexed_documents
                    SET
                        file_name = @file_name,
                        region = @region,
                        doc_type = @doc_type,
                        department = @department,
                        employee_assigned = @employee,
                        project_name = @project,
                        uploader_identity = @uploader
                    WHERE index_id = @index_id";

                using (NpgsqlConnection conn =
                    new NpgsqlConnection(connString))
                {
                    conn.Open();

                    using (NpgsqlCommand cmd =
                        new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@file_name", fileName);
                        cmd.Parameters.AddWithValue("@region", region);
                        cmd.Parameters.AddWithValue("@doc_type", docType);
                        cmd.Parameters.AddWithValue("@department", department);
                        cmd.Parameters.AddWithValue("@employee", employee);
                        cmd.Parameters.AddWithValue("@project", project);
                        cmd.Parameters.AddWithValue("@uploader", uploader);
                        cmd.Parameters.AddWithValue("@index_id", indexId);

                        cmd.ExecuteNonQuery();
                    }
                }

                gvDocuments.EditIndex = -1;

                BindDynamicVaultData();

                lblStatus.Style["display"] = "block";
                lblStatus.Style["opacity"] = "1";
                lblStatus.Text = "Updated successfully.";

                ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "fadeLabel",
                    @"
                    setTimeout(function () {
                        var lbl =
                        document.getElementById('" + lblStatus.ClientID + @"');

                        if (lbl) {

                            lbl.style.transition = 'opacity 1s';
                            lbl.style.opacity = '0';

                            setTimeout(function () {
                                lbl.style.display = 'none';
                            }, 1000);
                        }
                    }, 2000);
                    ",
                    true
                );
            }
            catch (Exception ex)
            {
                Response.Write(
                    "<script>alert('ERROR: " +
                    ex.Message.Replace("'", "") +
                    "');</script>"
                );
            }
        }
    }
}