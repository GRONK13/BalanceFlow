"use client";

import { useEffect, useState } from "react";
import ClientLayout from "./components/ClientLayout";
import { api } from "./api";
import Link from "next/link";

export default function DashboardPage() {
  const [stats, setStats] = useState({
    accountsCount: 0,
    journalEntriesCount: 0,
    invoicesCount: 0,
    recentEntries: [] as any[],
    recentInvoices: [] as any[]
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadDashboardData() {
      try {
        const accountsData = await api.accounts.getAll(1, 5);
        const entriesData = await api.journalEntries.getAll(1, 5);
        const invoicesData = await api.invoices.getAll(1, 5);

        setStats({
          accountsCount: accountsData.totalCount,
          journalEntriesCount: entriesData.totalCount,
          invoicesCount: invoicesData.totalCount,
          recentEntries: entriesData.items,
          recentInvoices: invoicesData.items
        });
      } catch (err) {
        console.error("Failed to load dashboard statistics:", err);
      } finally {
        setLoading(false);
      }
    }

    loadDashboardData();
  }, []);

  return (
    <ClientLayout>
      <div className="page-header">
        <div>
          <h1 className="page-title">Ledger & Auditing Overview</h1>
          <p className="page-subtitle">Real-time status of your general ledger and compliance audits.</p>
        </div>
      </div>

      {/* Financial Stat Cards */}
      <div className="stats-grid">
        <div className="stat-card assets">
          <div className="stat-label">Total Assets</div>
          <div className="stat-value">$250,000.00</div>
          <div style={{ fontSize: "12px", color: "#60a5fa", marginTop: "4px" }}>Active cash & receivables</div>
        </div>
        <div className="stat-card liabilities">
          <div className="stat-label">Total Liabilities</div>
          <div className="stat-value">$120,000.00</div>
          <div style={{ fontSize: "12px", color: "#fbbf24", marginTop: "4px" }}>Accounts Payable balance</div>
        </div>
        <div className="stat-card equity">
          <div className="stat-label">Total Owner Equity</div>
          <div className="stat-value">$130,000.00</div>
          <div style={{ fontSize: "12px", color: "#34d399", marginTop: "4px" }}>Retained earnings & capital</div>
        </div>
      </div>

      {/* Balanced Ledger Equation Banner */}
      <div className="equation-banner">
        <div className="eq-part">
          <span style={{ fontSize: "13px", color: "#9ca3af", textTransform: "uppercase" }}>Assets</span>
          <span className="eq-val" style={{ color: "#60a5fa" }}>$250,000.00</span>
        </div>
        <div className="eq-op">=</div>
        <div className="eq-part">
          <span style={{ fontSize: "13px", color: "#9ca3af", textTransform: "uppercase" }}>Liabilities</span>
          <span className="eq-val" style={{ color: "#fbbf24" }}>$120,000.00</span>
        </div>
        <div className="eq-op">+</div>
        <div className="eq-part">
          <span style={{ fontSize: "13px", color: "#9ca3af", textTransform: "uppercase" }}>Equity</span>
          <span className="eq-val" style={{ color: "#34d399" }}>$130,000.00</span>
        </div>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "24px" }}>
        
        {/* Recent Ledger Entries */}
        <div className="content-card">
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "20px" }}>
            <h3 className="card-title" style={{ margin: 0 }}>Recent Ledger Postings</h3>
            <Link href="/journal-entries" style={{ fontSize: "13px", color: "#3b82f6", textDecoration: "none" }}>View All ({stats.journalEntriesCount})</Link>
          </div>
          <div className="table-wrapper">
            {stats.recentEntries.length === 0 ? (
              <div style={{ color: "#4b5563", fontSize: "14px", padding: "12px 0" }}>No journal entries posted yet.</div>
            ) : (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Reference</th>
                    <th>Date</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.recentEntries.map((entry) => (
                    <tr key={entry.id}>
                      <td>{entry.referenceNumber}</td>
                      <td>{new Date(entry.transactionDate).toLocaleDateString()}</td>
                      <td>
                        <span className={`badge ${entry.isPosted ? "approved" : "draft"}`}>
                          {entry.isPosted ? "Posted" : "Draft"}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>

        {/* Recent Audited Invoices */}
        <div className="content-card">
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "20px" }}>
            <h3 className="card-title" style={{ margin: 0 }}>Compliance Audits</h3>
            <Link href="/invoices" style={{ fontSize: "13px", color: "#3b82f6", textDecoration: "none" }}>Audit Center ({stats.invoicesCount})</Link>
          </div>
          <div className="table-wrapper">
            {stats.recentInvoices.length === 0 ? (
              <div style={{ color: "#4b5563", fontSize: "14px", padding: "12px 0" }}>No invoices audited yet.</div>
            ) : (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Invoice No</th>
                    <th>Vendor</th>
                    <th>Audit</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.recentInvoices.map((inv) => (
                    <tr key={inv.id}>
                      <td>{inv.invoiceNumber}</td>
                      <td>{inv.vendorName}</td>
                      <td>
                        <span className={`badge ${inv.auditStatus.toLowerCase()}`}>
                          {inv.auditStatusName}
                        </span>
                      </td>
                      <td>
                        <span className={`badge ${inv.status.toLowerCase()}`}>
                          {inv.statusName}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>

      </div>
    </ClientLayout>
  );
}
