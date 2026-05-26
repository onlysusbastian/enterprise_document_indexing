<%@ Page Title="Enterprise Search Index"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Indexing.aspx.cs"
    Inherits="ongc_webapp.Indexing" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        body {
            background-color: #f4f6f9;
        }

        .card-box {
            background: white;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }

        .search-header {
            font-size: 1.3rem;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 20px;
        }

        .layout-wrapper {
            display: flex;
            gap: 20px;
            align-items: flex-start;
        }

        .filter-sidebar {
            width: 320px;
            min-width: 320px;
            max-width: 320px;
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
            max-height: 85vh;
            overflow: hidden;
            position: sticky;
            top: 20px;
            display: flex;
            flex-direction: column;
        }

        .main-results {
            flex: 1;
            min-width: 0;
        }

        .filter-row {
            border-bottom: 1px solid #e2e8f0;
            padding-bottom: 12px;
            margin-bottom: 12px;
        }

        .filter-column-name {
            font-size: 0.9rem;
            font-weight: 700;
            color: #334155;
            margin-left: 5px;
        }

        .filter-input-box {
            margin-top: 10px;
            display: none;
        }

        .results-wrapper {
            width: 100%;
            overflow-x: auto;
            overflow-y: auto;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
        }

        .grid-table {
            width: 100%;
            border-collapse: collapse;
            min-width: 1200px;
            background: white;
        }

        .grid-table th {
            background-color: #7a0616;
            color: white;
            padding: 12px;
            text-align: left;
            font-size: 0.85rem;
            border: 1px solid #ddd;
            white-space: nowrap;
            position: sticky;
            top: 0;
            z-index: 2;
        }

        .grid-table td {
            padding: 12px;
            border: 1px solid #ddd;
            vertical-align: top;
            word-break: break-word;
            font-size: 0.9rem;
            background-color: white;
            min-width: 140px;
        }

        .column-panel {
            margin-top: 20px;
            border-top: 1px solid #e2e8f0;
            padding-top: 15px;
            display: none;
        }

        .column-filter-wrapper {
            max-height: 180px;
            overflow-y: auto;
            overflow-x: hidden;
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 10px;
        }

        .checkbox-list {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
        }

        .checkbox-list input {
            margin-right: 4px;
        }

        .metadata-scroll-area {
            flex: 1;
            overflow-y: auto;
            padding-right: 6px;
            margin-top: 10px;
        }

        .metadata-filter-footer {
            margin-top: 15px;
            padding-top: 12px;
            border-top: 1px solid #e2e8f0;
            background: white;
            position: sticky;
            bottom: 0;
        }

    </style>

    <script type="text/javascript">

function toggleFilterTextbox(id) {

    var box =
        document.getElementById(id);

    if (!box)
        return;

    if (
        box.style.display === "none" ||
        box.style.display === ""
    ) {

        box.style.display = "block";
    }
    else {

        box.style.display = "none";
    }
}

function toggleColumnFilter() {

    var panel =
        document.getElementById("columnPanel");

    if (!panel)
        return;

    if (
        panel.style.display === "none" ||
        panel.style.display === ""
    ) {

        panel.style.display = "block";
    }
    else {

        panel.style.display = "none";
    }
}

</script>

    <div class="container-fluid mt-4">

        <!-- SEARCH -->

        <div class="card-box">

            <div class="search-header">
                Enterprise Metadata Search Engine
            </div>

            <div class="row">

                <div class="col-md-8">

                    <label>
                        Keyword Search (Maximum 3)
                    </label>

                    <asp:TextBox ID="txtSearch"
                        runat="server"
                        CssClass="form-control"
                        placeholder="finance assam vendor">
                    </asp:TextBox>

                </div>

                <div class="col-md-2">

                    <label>
                        Search Mode
                    </label>

                    <asp:RadioButtonList ID="rblSearchMode"
                        runat="server">

                        <asp:ListItem Text="UNION"
                            Value="OR"
                            Selected="True">
                        </asp:ListItem>

                        <asp:ListItem Text="INTERSECTION"
                            Value="AND">
                        </asp:ListItem>

                    </asp:RadioButtonList>

                </div>

                <div class="col-md-2 d-flex align-items-end">

                    <asp:Button ID="btnSearch"
                        runat="server"
                        Text="Search"
                        CssClass="btn btn-danger w-100"
                        OnClick="btnSearch_Click" />

                </div>

            </div>

            <!-- COLUMN FILTERS -->

            <button type="button"
                class="btn btn-secondary mt-4"
                onclick="toggleColumnFilter()">

                Show Column Filters

            </button>

            <div id="columnPanel"
                class="column-panel">

                <h5>
                    Column Visibility
                </h5>

                <div class="column-filter-wrapper">

                    <asp:CheckBoxList ID="cblColumns"
                        runat="server"
                        RepeatDirection="Horizontal"
                        RepeatLayout="Flow"
                        CssClass="checkbox-list">
                    </asp:CheckBoxList>

                </div>

                <br />

                <div class="row">

                    <div class="col-md-4">

                        <asp:Button ID="btnSelectAll"
                            runat="server"
                            Text="Select All"
                            CssClass="btn btn-success w-100"
                            OnClick="btnSelectAll_Click" />

                    </div>

                    <div class="col-md-4">

                        <asp:Button ID="btnClearAll"
                            runat="server"
                            Text="Clear All"
                            CssClass="btn btn-secondary w-100"
                            OnClick="btnClearAll_Click" />

                    </div>

                    <div class="col-md-4">

                        <asp:Button ID="btnApplyColumns"
                            runat="server"
                            Text="Apply Columns"
                            CssClass="btn btn-primary w-100"
                            OnClick="btnApplyColumns_Click" />

                    </div>

                </div>

            </div>

        </div>

        <!-- MAIN -->

        <div class="layout-wrapper">

            <!-- SIDEBAR -->

            <div class="filter-sidebar">

                <h5>
                    Metadata Filters
                </h5>

                <div class="metadata-scroll-area">

                    <asp:PlaceHolder ID="phDynamicFilters"
                        runat="server">
                    </asp:PlaceHolder>

                </div>

                <div class="metadata-filter-footer">

                    <asp:Button ID="btnApplyFilters"
                        runat="server"
                        Text="Apply Metadata Filters"
                        CssClass="btn btn-dark w-100"
                        OnClick="btnApplyFilters_Click" />

                </div>

            </div>

            <!-- RESULTS -->

            <div class="main-results">

                <div class="card-box">

                    <asp:Label ID="lblStatus"
                        runat="server"
                        ForeColor="Green"
                        Font-Bold="true">
                    </asp:Label>

                    <br />
                    <br />

                    <div class="results-wrapper">

                        <asp:GridView ID="gvDocuments"
                            runat="server"
                            CssClass="grid-table"
                            AutoGenerateColumns="True"
                            GridLines="Both"
                            BorderStyle="None"
                            BorderWidth="0"
                            EnableViewState="false"
                            AllowPaging="true"
                            PageSize="100"
                            OnPageIndexChanging="gvDocuments_PageIndexChanging">

                        </asp:GridView>

                    </div>

                </div>

            </div>

        </div>

    </div>

</asp:Content>