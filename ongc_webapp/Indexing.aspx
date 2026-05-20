<%@ Page Title="Enterprise Indexing Matrix" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexing.aspx.cs" Inherits="ongc_webapp.Indexing" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body { background-color: #f4f6f9; }
        .control-panel-card { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); padding: 20px; }
        .filter-sidebar { background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }
        .grid-engine th { background-color: #7a0616 !important; color: #ffffff !important; font-size: 0.8rem; text-transform: uppercase; padding: 12px; letter-spacing: 0.5px; }
        .grid-engine td { padding: 12px; font-size: 0.9rem; border-bottom: 1px solid #e2e8f0; color: #334155; }
        .filter-label { font-size: 0.75rem; font-weight: 700; color: #475569; text-transform: uppercase; margin-bottom: 5px; display: block; letter-spacing: 0.5px; }
        .circle-list input[type="checkbox"] { margin-right: 8px; transform: scale(1.1); accent-color: #7a0616; }
        .circle-list label { font-size: 0.9rem; color: #334155; cursor: pointer; display: inline-block; margin-bottom: 6px; }
    </style>

    <div class="container-fluid py-4">
        <div class="mb-4">
            <h3 class="fw-bold text-dark" style="border-left: 5px solid #7a0616; padding-left: 15px;">ONGC Document Repository Hub</h3>
            <p class="text-muted ms-3">Multi-Dimensional Structural Search Engine Infrastructure</p>
        </div>

        <div class="card control-panel-card mb-4">
            <div class="row g-3">
                <div class="col-md-4">
                    <span class="filter-label">Unified Keyword Search (Employee/Project/Uploader/Tags)</span>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="Type query filter parameter..."></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <span class="filter-label">Doc Type</span>
                    <asp:DropDownList ID="ddlDocType" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterTriggered">
                        <asp:ListItem Text="-- All Types --" Value=""></asp:ListItem>
                        <asp:ListItem Text="PDF" Value="PDF"></asp:ListItem>
                        <asp:ListItem Text="XLSX" Value="XLSX"></asp:ListItem>
                        <asp:ListItem Text="CSV" Value="CSV"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <span class="filter-label">Department</span>
                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterTriggered">
                        <asp:ListItem Text="-- All Depts --" Value=""></asp:ListItem>
                        <asp:ListItem Text="Exploration" Value="Exploration"></asp:ListItem>
                        <asp:ListItem Text="Logistics" Value="Logistics"></asp:ListItem>
                        <asp:ListItem Text="HR" Value="HR"></asp:ListItem>
                        <asp:ListItem Text="Safety" Value="Safety"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <span class="filter-label">Upload Year</span>
                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterTriggered">
                        <asp:ListItem Text="-- All Years --" Value=""></asp:ListItem>
                        <asp:ListItem Text="2026" Value="2026"></asp:ListItem>
                        <asp:ListItem Text="2025" Value="2025"></asp:ListItem>
                        <asp:ListItem Text="2024" Value="2024"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2 d-flex align-items-end">
                    <asp:Button ID="btnSearch" runat="server" Text="Apply Matrix Filters" CssClass="btn w-100 text-white fw-bold" style="background-color: #7a0616;" OnClick="btnSearch_Click" />
                </div>
            </div>
        </div>

        <div class="row g-4">
            <div class="col-lg-3">
                <div class="filter-sidebar">
                    <h6 class="fw-bold mb-3 text-uppercase text-secondary" style="letter-spacing:1px;"><i class="fas fa-map-marker-alt me-2"></i>Operational Circles</h6>
                    <asp:CheckBoxList ID="cblRegions" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterTriggered" CssClass="circle-list w-100">
                        <asp:ListItem Value="Northern Region">Northern Region</asp:ListItem>
                        <asp:ListItem Value="Eastern Region">Eastern Region</asp:ListItem>
                        <asp:ListItem Value="Central Region">Central Region</asp:ListItem>
                        <asp:ListItem Value="Southern Region">Southern Region</asp:ListItem>
                        <asp:ListItem Value="Assam Asset">North Eastern</asp:ListItem>
                    </asp:CheckBoxList>
                    <hr />
                    <asp:Button ID="btnClear" runat="server" Text="Reset All Filters" CssClass="btn btn-sm btn-outline-danger w-100 fw-bold" OnClick="btnClear_Click" />
                </div>
            </div>

            <div class="col-lg-9">
                <div class="card p-3 shadow-sm border-0 bg-white" style="border-radius:12px; overflow-x: auto;">
                    <asp:GridView ID="gvDocuments" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover grid-engine mb-0" GridLines="None" EmptyDataText="No matching records register under current parameter conditions.">
                        <Columns>
                            <asp:BoundField DataField="index_id" HeaderText="File ID" ItemStyle-Font-Names="monospace" />
                            <asp:BoundField DataField="file_name" HeaderText="Filename" ItemStyle-CssClass="fw-bold" />
                            <asp:BoundField DataField="region" HeaderText="Circle" />
                            <asp:BoundField DataField="doc_type" HeaderText="Type" />
                            <asp:BoundField DataField="department" HeaderText="Department" />
                            <asp:BoundField DataField="project_name" HeaderText="Project" />
                            <asp:BoundField DataField="employee_assigned" HeaderText="Employee" />
                            <asp:BoundField DataField="uploader_identity" HeaderText="Uploader" />
                            <asp:BoundField DataField="upload_date" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                            <asp:BoundField DataField="upload_time" HeaderText="Time" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>