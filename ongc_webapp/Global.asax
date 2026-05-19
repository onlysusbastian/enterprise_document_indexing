<%@ Application Language="C#" %>

<script runat="server">
    void Application_BeginRequest(object sender, EventArgs e)
    {
        string currentPath = Request.AppRelativeCurrentExecutionFilePath.ToLower();

        // Catch root requests, extensionless default, or extensionless login hits
        if (currentPath == "~/" || 
            currentPath == "~/default.aspx" || 
            currentPath == "~/default" || 
            currentPath == "~/login")
        {
            // Direct the server to the exact physical file asset
            Response.Redirect("~/Login.aspx", true);
        }
    }
</script>