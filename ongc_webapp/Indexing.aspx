<%@ Application Language="C#" %>

<script runat="server">
    void Application_BeginRequest(object sender, EventArgs e)
    {
        // Capture the incoming URL path requested by the browser
        string currentPath = Request.AppRelativeCurrentExecutionFilePath.ToLower();

        // Server-Side Guardrail: Force routing to clean .aspx files
        if (currentPath == "~/" || currentPath == "~/default.aspx" || currentPath == "~/default")
        {
            Response.Redirect("~/Login.aspx", true);
        }
        else if (currentPath == "~/login")
        {
            Response.Redirect("~/Login.aspx", true);
        }
        else if (currentPath == "~/dashboard")
        {
            Response.Redirect("~/Dashboard.aspx", true);
        }
    }
</script>