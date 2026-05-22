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

        .filter-label {
            font-size: 0.75rem;
            font-weight: 700;
            color: #475569;
            text-transform: uppercase;
            margin-bottom: 5px;
            display: block;
        }

        .grid-table {
            width: 100%;
            border-collapse: collapse;
        }

        .grid-table th {
            background-color: #7a0616;
            color: white;
            padding: 12px;
            text-align: left;
            font-size: 0.85rem;
            border: 1px solid #ddd;
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
        }

        .grid-table tr:nth-child(even) {
            background-color: #f8fafc;
        }

        .grid-table tr:hover td {
            background-color: #f1f5f9;
        }

        .results-wrapper {
            overflow-x: auto;
            max-width: 100%;
        }

        .search-header {
            font-size: 1.3rem;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 20px;
        }

        .status-label {
            display: block;
            margin-bottom: 15px;
            font-size: 0.95rem;
        }

    </style>

    <div class="container-fluid mt-4">

        <!-- Search Section -->

        <div class="card-box">

            <div class="search-header">
                Enterprise Metadata Search
            </div>

            <div class="row">

                <div class="col-md-10">

                    <span class="filter-label">
                        Search Documents
                    </span>

                    <asp:TextBox ID="txtSearch"
                        runat="server"
                        CssClass="form-control"
                        placeholder="Search file name, path, metadata values...">
                    </asp:TextBox>

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

        <!-- Results Section -->

        <div class="card-box">

            <asp:Label ID="lblStatus"
                runat="server"
                CssClass="status-label"
                ForeColor="Green"
                Font-Bold="true">
            </asp:Label>

            <div class="results-wrapper">

                <asp:GridView ID="gvDocuments"
                    runat="server"
                    CssClass="grid-table"
                    AutoGenerateColumns="True"
                    GridLines="None"
                    BorderWidth="0"
                    HeaderStyle-Wrap="false">

                </asp:GridView>

            </div>

        </div>

    </div>

</asp:Content>