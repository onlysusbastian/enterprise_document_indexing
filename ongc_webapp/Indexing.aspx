<%@ Page Title="Enterprise Indexing Matrix" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexing.aspx.cs" Inherits="ongc_webapp.Indexing" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    body {
        background-color: #f4f6f9;
    }

    .grid-table {
        width: 100%;
        table-layout: fixed;
        border-collapse: collapse;
    }

    .grid-table th {
        background-color: #7a0616;
        color: white;
        padding: 10px;
        text-align: left;
    }

    .grid-table td {
        padding: 10px;
        border: 1px solid #ddd;
        vertical-align: middle;
        word-wrap: break-word;
    }

    .grid-table input[type="text"] {
        width: 100%;
        box-sizing: border-box;
        padding: 5px;
    }

    .grid-table td:first-child,
    .grid-table th:first-child {
        width: 90px;
    }

    .grid-table td:nth-child(2),
    .grid-table th:nth-child(2) {
        width: 120px;
    }

    .grid-table td:nth-child(3),
    .grid-table th:nth-child(3) {
        width: 250px;
    }

    .filter-label {
        font-size: 0.75rem;
        font-weight: 700;
        color: #475569;
        text-transform: uppercase;
        margin-bottom: 5px;
        display: block;
    }

    .card-box {
        background: white;
        border-radius: 10px;
        padding: 20px;
        margin-bottom: 20px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }
</style>

<div class="container-fluid mt-4">

    <div class="card-box">

        <div class="row">

            <div class="col-md-4">
                <span class="filter-label">Search</span>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="col-md-2">
                <span class="filter-label">Doc Type</span>
                <asp:DropDownList ID="ddlDocType" runat="server" CssClass="form-control">
                    <asp:ListItem Text="All" Value=""></asp:ListItem>
                    <asp:ListItem Text="PDF" Value="PDF"></asp:ListItem>
                    <asp:ListItem Text="PNG" Value="PNG"></asp:ListItem>
                    <asp:ListItem Text="XLSX" Value="XLSX"></asp:ListItem>
                    <asp:ListItem Text="DOCX" Value="DOCX"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-md-2">
                <span class="filter-label">Department</span>
                <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-control">
                    <asp:ListItem Text="All" Value=""></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="col-md-2">
                <span class="filter-label">Year</span>
                <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-control">
                    <asp:ListItem Text="All" Value=""></asp:ListItem>
                </asp:DropDownList>
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

    <div class="card-box">

        <asp:Label ID="lblStatus"
            runat="server"
            ForeColor="Green"
            Font-Bold="true">
        </asp:Label>

        <asp:GridView ID="gvDocuments"
            runat="server"
            CssClass="grid-table"
            AutoGenerateColumns="False"
            DataKeyNames="index_id"
            OnRowEditing="gvDocuments_RowEditing"
            OnRowCancelingEdit="gvDocuments_RowCancelingEdit"
            OnRowUpdating="gvDocuments_RowUpdating">

            <Columns>

                <asp:CommandField ShowEditButton="True" />

                <asp:BoundField
                    DataField="index_id"
                    HeaderText="Index ID"
                    ReadOnly="True" />

                <asp:TemplateField HeaderText="File Name">
                    <ItemTemplate>
                        <%# Eval("file_name") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtFileName"
                            runat="server"
                            Text='<%# Bind("file_name") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Region">
                    <ItemTemplate>
                        <%# Eval("region") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtRegion"
                            runat="server"
                            Text='<%# Bind("region") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Doc Type">
                    <ItemTemplate>
                        <%# Eval("doc_type") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtDocType"
                            runat="server"
                            Text='<%# Bind("doc_type") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Department">
                    <ItemTemplate>
                        <%# Eval("department") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtDepartment"
                            runat="server"
                            Text='<%# Bind("department") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Employee">
                    <ItemTemplate>
                        <%# Eval("employee_assigned") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtEmployee"
                            runat="server"
                            Text='<%# Bind("employee_assigned") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Project">
                    <ItemTemplate>
                        <%# Eval("project_name") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtProject"
                            runat="server"
                            Text='<%# Bind("project_name") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Uploader">
                    <ItemTemplate>
                        <%# Eval("uploader_identity") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtUploader"
                            runat="server"
                            Text='<%# Bind("uploader_identity") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

            </Columns>

        </asp:GridView>

    </div>

</div>

</asp:Content>