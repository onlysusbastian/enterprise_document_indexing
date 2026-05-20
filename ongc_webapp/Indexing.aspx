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

        <div class="card p-4 border-0 shadow-sm bg-white mb-4" style="border-radius: 8px; border-top: 4px solid #7a0616 !important;">
            <h5 class="fw-bold mb-3 text-dark"><i class="fas fa-cloud-upload-alt me-2" style="color: #7a0616;"></i>Execute Node Document Ingestion</h5>
            
            <div class="row g-3">
                <div class="col-md-4">
                    <span class="filter-label">Select File Asset Resource</span>
                    <asp:FileUpload ID="fileVaultUpload" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-4">
                    <span class="filter-label">Operational Asset Jurisdiction</span>
                    <asp:DropDownList ID="ddlUploadRegion" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Text="-- Select Asset Field --" Value=""></asp:ListItem>
                        <asp:ListItem Text="Western Offshore - Mumbai Assets" Value="Western Offshore - Mumbai Assets"></asp:ListItem>
                        <asp:ListItem Text="Eastern Frontier - ONGC Jorhat Asset" Value="Eastern Frontier - ONGC Jorhat Asset"></asp:ListItem>
                        <asp:ListItem Text="Onshore Basin - Cambay Asset" Value="Onshore Basin - Cambay Asset"></asp:ListItem>
                        <asp:ListItem Text="Krishna Godavari (KG) Deepwater Basin" Value="Krishna Godavari (KG) Deepwater Basin"></asp:ListItem>
                        <asp:ListItem Text="Cauvery Asset Operational Grid" Value="Cauvery Asset Operational Grid"></asp:ListItem>
                        <asp:ListItem Text="Assam Asset Development Hub" Value="Assam Asset Development Hub"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-4">
                    <span class="filter-label">Functional Enterprise Department</span>
                    <asp:DropDownList ID="ddlUploadDept" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Text="-- Select Department --" Value=""></asp:ListItem>
                        <asp:ListItem Text="Exploration & Drilling Services" Value="Exploration & Drilling Services"></asp:ListItem>
                        <asp:ListItem Text="Production & Pipeline Logistics" Value="Production & Pipeline Logistics"></asp:ListItem>
                        <asp:ListItem Text="HSE Safety & Effluent Treatment" Value="HSE Safety & Effluent Treatment"></asp:ListItem>
                        <asp:ListItem Text="Corporate Instrumentation & IT" Value="Corporate Instrumentation & IT"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="row g-3 mt-1">
                <div class="col-md-4">
                    <span class="filter-label">Target Project Framework</span>
                    <asp:DropDownList ID="ddlUploadProject" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Text="-- Select Active Project --" Value=""></asp:ListItem>
                        <asp:ListItem Text="Daman Upside Development (DUDP)" Value="Daman Upside Development (DUDP)"></asp:ListItem>
                        <asp:ListItem Text="Samudra Manthan Deepwater Initiative" Value="Samudra Manthan Deepwater Initiative"></asp:ListItem>
                        <asp:ListItem Text="KG-DWN-98/2 Field Infrastructure" Value="KG-DWN-98/2 Field Infrastructure"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-5">
                    <span class="filter-label">Brief Content Abstract / Description</span>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control form-control-sm" placeholder="e.g., Valve structural pressure logging metadata description..."></asp:TextBox>
                </div>
                <div class="col-md-3 d-grid align-items-end">
                    <asp:Button ID="btnExecuteUpload" runat="server" Text="Commit Ingestion Transaction" CssClass="btn btn-sm text-white fw-bold py-2" style="background-color: #7a0616; border: none;" OnClick="btnExecuteUpload_Click" />
                </div>
            </div>

            <div class="mt-2">
                <asp:Label ID="lblUploadMsg" runat="server" CssClass="small fw-bold d-block"></asp:Label>
            </div>
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
                    <span class="filter-label">Department Filter</span>
                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterTriggered">
                        <asp:ListItem Text="-- All Depts --" Value=""></asp:ListItem>
                        <asp:ListItem Text="Exploration & Drilling Services" Value="Exploration & Drilling Services"></asp:ListItem>
                        <asp:ListItem Text="Production & Pipeline Logistics" Value="Production & Pipeline Logistics"></asp:ListItem>
                        <asp:ListItem Text="HSE Safety & Effluent Treatment" Value="HSE Safety & Effluent Treatment"></asp:ListItem>
                        <asp:ListItem Text="Corporate Instrumentation & IT" Value="Corporate Instrumentation & IT"></asp:ListItem>
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
                        <asp:ListItem Value="Western Offshore - Mumbai Assets">Western Offshore - Mumbai</asp:ListItem>
                        <asp:ListItem Value="Eastern Frontier - ONGC Jorhat Asset">Eastern Frontier - Jorhat</asp:ListItem>
                        <asp:ListItem Value="Onshore Basin - Cambay Asset">Onshore Basin - Cambay</asp:ListItem>
                        <asp:ListItem Value="Krishna Godavari (KG) Deepwater Basin">KG Deepwater Basin</asp:ListItem>
                        <asp:ListItem Value="Cauvery Asset Operational Grid">Cauvery Asset Grid</asp:ListItem>
                        <asp:ListItem Value="Assam Asset Development Hub">Assam Development Hub</asp:ListItem>
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