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

        .filter-label {
            font-size: 0.75rem;
            font-weight: 700;
            color: #475569;
            text-transform: uppercase;
            margin-bottom: 5px;
            display: block;
        }

        .results-wrapper {
            overflow-x: auto;
            max-width: 100%;
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

        .layout-wrapper {
            display: flex;
            gap: 20px;
            align-items: flex-start;
        }

        .filter-sidebar {
            width: 300px;
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }

        .main-results {
            flex: 1;
        }

        .checkbox-list {
            max-height: 500px;
            overflow-y: auto;
            border: 1px solid #e2e8f0;
            padding: 10px;
            border-radius: 6px;
        }

        .checkbox-list input {
            margin-right: 8px;
        }

        .checkbox-list label {
            display: inline-block;
            margin-bottom: 6px;
        }

        .highlight-column {
            background-color: #ffedd5 !important;
            color: #9a3412 !important;
            font-weight: bold;
        }

        .filter-search-wrapper {
            display: flex;
            gap: 8px;
            margin-top: 15px;
            margin-bottom: 15px;
        }

    </style>

    <script type="text/javascript">

function filterCheckboxes() {

    var input =
        document.getElementById("filterSearch");

    if (!input)
        return;

    var filter =
        input.value.toLowerCase();

    var checkboxList =
        document.getElementById("<%= cblColumns.ClientID %>");

            if (!checkboxList)
                return;

            var labels =
                checkboxList.getElementsByTagName("label");

            for (var i = 0; i < labels.length; i++) {

                var label =
                    labels[i];

                var text =
                    (label.innerText || label.textContent)
                        .toLowerCase();

                var parent =
                    label.parentNode;

                if (text.indexOf(filter) > -1) {

                    parent.style.display = "";
                }
                else {

                    parent.style.display = "none";
                }
            }
        }

    </script>

    <div class="container-fluid mt-4">

        <!-- MAIN SEARCH -->

        <div class="card-box">

            <div class="search-header">
                Enterprise Metadata Search Engine
            </div>

            <div class="row">

                <div class="col-md-8">

                    <span class="filter-label">
                        Search Keywords (Maximum 3)
                    </span>

                    <asp:TextBox ID="txtSearch"
                        runat="server"
                        CssClass="form-control"
                        placeholder="Example: finance assam vendor">
                    </asp:TextBox>

                </div>

                <div class="col-md-2">

                    <span class="filter-label">
                        Search Mode
                    </span>

                    <asp:RadioButtonList ID="rblSearchMode"
                        runat="server">

                        <asp:ListItem Text="Any One Matching"
                            Value="OR"
                            Selected="True">
                        </asp:ListItem>

                        <asp:ListItem Text="All Matching"
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

        </div>

        <!-- MAIN LAYOUT -->

        <div class="layout-wrapper">

            <!-- FILTER SIDEBAR -->

            <div class="filter-sidebar">

                <h5>Visible Metadata Columns</h5>

                <!-- FILTER SEARCH -->

                <div class="filter-search-wrapper">

                    <input type="text"
                        id="filterSearch"
                        class="form-control"
                        placeholder="Search filters..." />

                    <button type="button"
                        class="btn btn-dark"
                        onclick="filterCheckboxes()">
                        Go
                    </button>

                </div>

                <!-- CHECKBOX LIST -->

                <asp:CheckBoxList ID="cblColumns"
                    runat="server"
                    CssClass="checkbox-list">
                </asp:CheckBoxList>

                <br />

                <!-- FILTER BUTTONS -->

                <div class="d-grid gap-2">

                    <asp:Button ID="btnSelectAll"
                        runat="server"
                        Text="Select All"
                        CssClass="btn btn-success"
                        OnClick="btnSelectAll_Click" />

                    <br />

                    <asp:Button ID="btnClearAll"
                        runat="server"
                        Text="Clear All"
                        CssClass="btn btn-secondary"
                        OnClick="btnClearAll_Click" />

                    <br />

                    <asp:Button ID="btnApplyColumns"
                        runat="server"
                        Text="Apply Column Filters"
                        CssClass="btn btn-primary"
                        OnClick="btnApplyColumns_Click" />

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
                            BorderWidth="0">

                        </asp:GridView>

                    </div>

                </div>

            </div>

        </div>

    </div>

</asp:Content>