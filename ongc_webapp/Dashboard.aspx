<%@ Page Title="System Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="ongc_webapp.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>

        body {
            background-color: #f4f6f9;
            color: #334155;
        }

        .welcome-identity-strip {
            background: linear-gradient(135deg, #7a0616 0%, #4a030c 100%);
            color: #ffffff;
            padding: 24px 35px;
            border-radius: 0 0 12px 12px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.06);
            margin-bottom: 35px;
        }

        .stat-card {
            background: #ffffff;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            padding: 20px;
            box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);
        }

        .action-card {
            background: #1e293b;
            color: #ffffff;
            border-radius: 8px;
            padding: 25px;
        }

        .table th {
            font-size: 0.8rem;
            letter-spacing: 0.5px;
            color: #64748b;
        }

        .table td {
            font-size: 0.95rem;
            color: #334155;
        }

        .ongc-portal-footer {
            background-color: #4a030c;
            color: #f1f5f9;
            border-top: 4px solid #7a0616;
            padding: 45px 0 20px 0;
            margin-top: 55px;
        }

        .footer-col-header {
            font-size: 0.85rem;
            font-weight: 700;
            text-transform: uppercase;
            color: #fcd34d;
            letter-spacing: 1px;
            margin-bottom: 18px;
        }

        .footer-nav-links {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .footer-nav-links li {
            margin-bottom: 10px;
        }

        .footer-nav-links a {
            color: #cbd5e1;
            text-decoration: none;
            font-size: 0.9rem;
        }

        .footer-nav-links a:hover {
            color: #ffffff;
        }

        .footer-social-box a {
            color: #fcd34d !important;
            font-size: 1.4rem;
            margin-right: 22px;
            text-decoration: none;
        }

        .footer-copyright-bar {
            border-top: 1px solid #7a0616;
            padding-top: 20px;
            margin-top: 40px;
            font-size: 0.8rem;
            color: #94a3b8;
        }

    </style>

    <div class="welcome-identity-strip">
        <div class="row align-items-center">
            <div class="col-md-12">
                <h2 class="fw-bold m-0">
                    Welcome Back,
                    <asp:Label ID="lblUserName" runat="server" Text="User"></asp:Label>
                </h2>
            </div>
        </div>
    </div>

    <div class="container-fluid px-4">

        <!-- Dashboard Cards -->

        <div class="row g-4 mb-4">

            <div class="col-md-4">
                <div class="stat-card text-center" style="border-left: 5px solid #64748b;">
                    <span class="text-muted text-uppercase fw-bold small">
                        Total Files
                    </span>

                    <h2 class="fw-bold mt-2 text-dark">
                        <asp:Label ID="lblTotalFiles" runat="server" Text="0"></asp:Label>
                    </h2>
                </div>
            </div>

            <div class="col-md-4">
                <div class="stat-card text-center" style="border-left: 5px solid #10b981;">
                    <span class="text-uppercase fw-bold small" style="color: #10b981 !important;">
                        Indexed Successfully
                    </span>

                    <h2 class="fw-bold mt-2" style="color: #10b981;">
                        <asp:Label ID="lblIndexedSuccess" runat="server" Text="0"></asp:Label>
                    </h2>
                </div>
            </div>

            <div class="col-md-4">
                <div class="stat-card text-center" style="border-left: 5px solid #f59e0b;">
                    <span class="text-uppercase fw-bold small" style="color: #f59e0b !important;">
                        Pending Indexing
                    </span>

                    <h2 class="fw-bold mt-2" style="color: #f59e0b;">
                        <asp:Label ID="lblPendingIndexing" runat="server" Text="0"></asp:Label>
                    </h2>
                </div>
            </div>

        </div>

        <!-- Recent Logs -->

        <div class="row g-4 mb-5">

            <div class="col-lg-8">

                <div class="card p-4 border-0 shadow-sm bg-white"
                     style="border-radius: 8px; height: 100%;">

                    <h5 class="fw-bold mb-4 text-dark">
                        <i class="fas fa-list-alt text-secondary me-2"></i>
                        Recent Indexed Files
                    </h5>

                    <div class="table-responsive">

                        <table class="table table-hover align-middle mb-0">

                            <thead class="table-light text-uppercase">
                                <tr>
                                    <th>ID</th>
                                    <th>File Name</th>
                                    <th>File Path</th>
                                    <th>Metadata</th>
                                </tr>
                            </thead>

                            <tbody>

                                <asp:Repeater ID="rptRecentLogs" runat="server">

                                    <ItemTemplate>

                                        <tr>

                                            <td class="text-secondary font-monospace fw-bold">
                                                <%# Eval("id") %>
                                            </td>

                                            <td class="fw-bold text-dark">
                                                <%# Eval("file_name") %>
                                            </td>

                                            <td class="text-muted">
                                                <%# Eval("file_path") %>
                                            </td>

                                            <td class="text-muted">
                                                <%# Eval("dynamic_metadata") %>
                                            </td>

                                        </tr>

                                    </ItemTemplate>

                                </asp:Repeater>

                            </tbody>

                        </table>

                    </div>

                </div>

            </div>

            <!-- Quick Actions -->

            <div class="col-lg-4">

                <div class="action-card h-100 d-flex flex-column justify-content-between">

                    <div>
                        <h5 class="fw-bold mb-2">Quick Actions</h5>

                        <p class="text-white-50 small mb-4">
                            Direct access to indexing and search operations.
                        </p>
                    </div>

                    <div class="d-grid gap-3">

                        <a href="Upload.aspx"
                           class="btn btn-primary fw-bold py-2 shadow-sm">
                            Upload New Batch
                        </a>

                        <a href="Indexing.aspx"
                           class="btn btn-outline-light fw-bold py-2">
                            Search Index
                        </a>

                    </div>

                </div>

            </div>

        </div>

    </div>

    <!-- Footer -->

    <footer class="ongc-portal-footer">

        <div class="container-fluid px-5">

            <div class="row g-4">

                <div class="col-md-3">

                    <div class="footer-col-header">
                        Legal & Compliance
                    </div>

                    <ul class="footer-nav-links">
                        <li><a href="#">Privacy Policy</a></li>
                        <li><a href="#">Data Disclaimer</a></li>
                        <li><a href="#">Terms of Portal Use</a></li>
                    </ul>

                </div>

                <div class="col-md-3">

                    <div class="footer-col-header">
                        IT Support
                    </div>

                    <ul class="footer-nav-links">
                        <li><a href="#">Helpdesk</a></li>
                        <li><a href="#">Security FAQ</a></li>
                        <li><a href="#">Classification Policy</a></li>
                    </ul>

                </div>

                <div class="col-md-3">

                    <div class="footer-col-header">
                        Operational Assets
                    </div>

                    <ul class="footer-nav-links">
                        <li><a href="#">Northern Region</a></li>
                        <li><a href="#">Eastern Region</a></li>
                        <li><a href="#">Assam Assets</a></li>
                    </ul>

                </div>

                <div class="col-md-3">

                    <div class="footer-col-header">
                        Stay Connected
                    </div>

                    <div class="footer-social-box">

                        <a href="https://in.linkedin.com/company/oilandnaturalgascorporation"
                           target="_blank">
                            <i class="fab fa-linkedin"></i>
                        </a>

                        <a href="https://x.com/ONGC_"
                           target="_blank">
                            <i class="fab fa-twitter"></i>
                        </a>

                        <a href="https://www.instagram.com/ongcofficial/"
                           target="_blank">
                            <i class="fab fa-instagram"></i>
                        </a>

                    </div>

                </div>

            </div>

            <div class="footer-copyright-bar d-flex flex-column flex-md-row justify-content-between align-items-center">

                <p class="mb-0">
                    © 2026 Powered by ONGC Foundation & Systems Architecture Suite.
                </p>

            </div>

        </div>

    </footer>

</asp:Content>