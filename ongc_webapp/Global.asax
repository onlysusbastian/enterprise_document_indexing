<%@ Application Language="C#" %>

<script runat="server">
    void Application_BeginRequest(object sender, EventArgs e)
    {
        // Capture the incoming URL path requested by the browser
        string currentPath = Request.AppRelativeCurrentExecutionFilePath.ToLower();

        // Server-Side Guardrail: Hijack the root path or default/login extensions cleanly
        if (currentPath == "~/" || 
            currentPath == "~/default.aspx" || 
            currentPath == "~/default" || 
            currentPath == "~/login")
        {
            // Forcefully redirect to the exact physical web forms page asset file
            Response.Redirect("~/Login.aspx", true);
        }
    }
</script>