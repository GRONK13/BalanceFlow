"use client";

import { useEffect, useState } from "react";
import ClientLayout from "../components/ClientLayout";
import { api } from "../api";

export default function InvoicesPage() {
  const [invoices, setInvoices] = useState<any[]>([]);
  const [accounts, setAccounts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Upload/Audit stage state
  const [uploading, setUploading] = useState(false);
  const [ocrResult, setOcrResult] = useState<any | null>(null);
  const [selectedAPAccount, setSelectedAPAccount] = useState("");
  const [approving, setApproving] = useState(false);

  const loadInvoices = async () => {
    setLoading(true);
    setError(null);
    try {
      const invoicesData = await api.invoices.getAll(1, 50);
      const accountsData = await api.accounts.getAll(1, 100);
      setInvoices(invoicesData.items);
      setAccounts(accountsData.items.filter((a) => a.isActive));
    } catch (err: any) {
      setError(err.message || "Failed to load invoices.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadInvoices();
  }, []);

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    setError(null);
    setOcrResult(null);

    try {
      // 1. Upload and run Mock OCR
      const result = await api.invoices.upload(file);
      setOcrResult(result);
    } catch (err: any) {
      setError(err.message || "Failed to upload document.");
    } finally {
      setUploading(false);
    }
  };

  const handleCreateAndAuditInvoice = async () => {
    if (!ocrResult) return;

    setApproving(true);
    setError(null);

    // Format DTO for CreateInvoiceCommand
    const invoicePayload = {
      invoiceNumber: ocrResult.invoiceNumber,
      vendorName: ocrResult.vendorName,
      issueDate: ocrResult.issueDate,
      dueDate: ocrResult.dueDate,
      taxAmount: ocrResult.taxAmount,
      totalAmount: ocrResult.totalAmount,
      uploadedFilePath: ocrResult.uploadedFilePath,
      contentType: ocrResult.contentType,
      lineItems: ocrResult.lineItems.map((l: any) => ({
        accountId: l.suggestedAccountId || accounts.find((a) => a.type === 4)?.id || "", // Fallback to first expense account
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitPrice,
      })),
    };

    try {
      // 1. Save draft invoice
      const invoice = await api.invoices.create(invoicePayload);
      
      // 2. Audit compliance checks
      const audited = await api.invoices.audit(invoice.id);
      
      // Update result state with audited details
      setOcrResult(audited);
      loadInvoices();
    } catch (err: any) {
      setError(err.message || "Failed to save and audit invoice.");
    } finally {
      setApproving(false);
    }
  };

  const handleApproveInvoice = async () => {
    if (!ocrResult || !selectedAPAccount) return;

    setApproving(true);
    setError(null);

    try {
      // Approve and post ledger entry
      await api.invoices.approve(ocrResult.id, selectedAPAccount);
      setOcrResult(null);
      setSelectedAPAccount("");
      loadInvoices();
    } catch (err: any) {
      setError(err.message || "Failed to approve and post to ledger.");
    } finally {
      setApproving(false);
    }
  };

  return (
    <ClientLayout>
      <div className="page-header">
        <div>
          <h1 className="page-title">Invoice Compliance & Audit Center</h1>
          <p className="page-subtitle">Verify document compliance checks and post audited vendor invoices to accounts payable.</p>
        </div>
      </div>

      {error && (
        <div style={{ padding: "12px", backgroundColor: "rgba(239, 68, 68, 0.1)", border: "1px solid rgba(239, 68, 68, 0.2)", borderRadius: "8px", color: "#f87171", marginBottom: "20px" }}>
          {error}
        </div>
      )}

      {/* OCR Auditing Interface Panel */}
      {ocrResult && (
        <div className="content-card" style={{ border: "1px solid #3b82f6", background: "radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.02), transparent)" }}>
          <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "20px", borderBottom: "1px solid #1f2937", paddingBottom: "16px" }}>
            <h3 style={{ fontSize: "16px", fontWeight: "700", color: "#60a5fa" }}>
              🔍 OCR Extraction Preview: {ocrResult.vendorName}
            </h3>
            <button onClick={() => setOcrResult(null)} style={{ background: "none", border: "none", color: "#9ca3af", cursor: "pointer" }}>Close Preview ✕</button>
          </div>

          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "32px" }}>
            
            {/* Left Panel: Invoice Details & AP Setup */}
            <div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px", marginBottom: "20px" }}>
                <div>
                  <span style={{ fontSize: "12px", color: "#9ca3af" }}>Invoice Number</span>
                  <div style={{ fontSize: "14px", fontWeight: "700", marginTop: "4px" }}>{ocrResult.invoiceNumber}</div>
                </div>
                <div>
                  <span style={{ fontSize: "12px", color: "#9ca3af" }}>Vendor Name</span>
                  <div style={{ fontSize: "14px", fontWeight: "700", marginTop: "4px" }}>{ocrResult.vendorName}</div>
                </div>
                <div>
                  <span style={{ fontSize: "12px", color: "#9ca3af" }}>Issue Date</span>
                  <div style={{ fontSize: "14px", marginTop: "4px" }}>{ocrResult.issueDate ? new Date(ocrResult.issueDate).toLocaleDateString() : "—"}</div>
                </div>
                <div>
                  <span style={{ fontSize: "12px", color: "#9ca3af" }}>Due Date</span>
                  <div style={{ fontSize: "14px", marginTop: "4px" }}>{ocrResult.dueDate ? new Date(ocrResult.dueDate).toLocaleDateString() : "—"}</div>
                </div>
                <div>
                  <span style={{ fontSize: "12px", color: "#9ca3af" }}>Tax Amount</span>
                  <div style={{ fontSize: "14px", marginTop: "4px" }}>${ocrResult.taxAmount.toFixed(2)}</div>
                </div>
                <div>
                  <span style={{ fontSize: "12px", color: "#9ca3af" }}>Total Amount</span>
                  <div style={{ fontSize: "16px", fontWeight: "800", color: "#34d399", marginTop: "4px" }}>${ocrResult.totalAmount.toFixed(2)}</div>
                </div>
              </div>

              {/* Status and Actions */}
              <div style={{ padding: "16px", backgroundColor: "#0b0f19", borderRadius: "8px", border: "1px solid #1f2937", marginBottom: "24px" }}>
                <div style={{ display: "flex", gap: "8px", alignItems: "center", marginBottom: "12px" }}>
                  <span style={{ fontSize: "13px", color: "#9ca3af" }}>Auditing Logs:</span>
                  <span className={`badge ${ocrResult.auditStatus === 2 ? "passed" : ocrResult.auditStatus === 3 ? "failed" : "draft"}`}>
                    {ocrResult.auditStatusName || "Ready to Audit"}
                  </span>
                </div>
                <p style={{ fontSize: "13px", color: "#e5e7eb" }}>{ocrResult.auditNotes || "Press save to run compliance engine checks."}</p>
              </div>

              {/* Approve Stage Actions */}
              {ocrResult.status === 2 && ocrResult.auditStatus === 2 ? (
                <div className="form-group" style={{ margin: 0 }}>
                  <label className="form-label" htmlFor="ap">Post to Accounts Payable (Liability)</label>
                  <div style={{ display: "flex", gap: "12px" }}>
                    <select
                      id="ap"
                      className="form-input"
                      value={selectedAPAccount}
                      onChange={(e) => setSelectedAPAccount(e.target.value)}
                      required
                    >
                      <option value="">Select Liability Account...</option>
                      {accounts.filter((a) => a.type === 1).map((acc) => (
                        <option key={acc.id} value={acc.id}>
                          {acc.accountCode} — {acc.name}
                        </option>
                      ))}
                    </select>
                    <button
                      onClick={handleApproveInvoice}
                      className="btn btn-primary"
                      disabled={!selectedAPAccount || approving}
                    >
                      {approving ? "Posting..." : "Approve & Post"}
                    </button>
                  </div>
                </div>
              ) : ocrResult.status === 1 ? (
                <button
                  onClick={handleCreateAndAuditInvoice}
                  className="btn btn-primary"
                  style={{ width: "100%" }}
                  disabled={approving}
                >
                  {approving ? "Saving..." : "Save Draft & Audit"}
                </button>
              ) : (
                <div style={{ fontSize: "14px", color: "#ef4444", fontWeight: "700" }}>
                  🚫 This invoice has been approved or rejected. Modifications locked.
                </div>
              )}
            </div>

            {/* Right Panel: Itemized Line Items & suggestion */}
            <div style={{ borderLeft: "1px solid #1f2937", paddingLeft: "32px" }}>
              <h4 style={{ fontSize: "14px", fontWeight: "600", color: "#9ca3af", marginBottom: "16px" }}>Itemized Line Items</h4>
              <div className="table-wrapper">
                <table className="data-table" style={{ fontSize: "13px" }}>
                  <thead>
                    <tr>
                      <th>Description</th>
                      <th>Qty</th>
                      <th style={{ textAlign: "right" }}>Unit Price</th>
                      <th style={{ textAlign: "right" }}>Total</th>
                      <th>Suggested Account</th>
                    </tr>
                  </thead>
                  <tbody>
                    {ocrResult.lineItems.map((line: any, idx: number) => (
                      <tr key={idx}>
                        <td>{line.description}</td>
                        <td>{line.quantity}</td>
                        <td style={{ textAlign: "right" }}>${line.unitPrice.toFixed(2)}</td>
                        <td style={{ textAlign: "right", fontWeight: "700" }}>${line.lineTotal?.toFixed(2) || (line.quantity * line.unitPrice).toFixed(2)}</td>
                        <td>
                          {line.suggestedAccountId ? (
                            <span className="badge asset" style={{ color: "#34d399" }}>
                              Suggested Category
                            </span>
                          ) : (
                            <span className="badge draft">User Selected</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

          </div>
        </div>
      )}

      {/* Drag & Drop Upload Zone (Show when no active preview is being audited) */}
      {!ocrResult && (
        <div style={{ marginBottom: "32px" }}>
          <label className="upload-zone" style={{ display: "block" }}>
            <div className="upload-icon">📁</div>
            <div className="upload-text">
              {uploading ? "Analyzing document content..." : "Click or drag your invoice PDF/Image here"}
            </div>
            <div className="upload-subtext">PDF, PNG, JPEG formats supported (Max 10MB)</div>
            <input
              type="file"
              onChange={handleFileUpload}
              style={{ display: "none" }}
              accept=".pdf,.png,.jpg,.jpeg"
              disabled={uploading}
            />
          </label>
        </div>
      )}

      {/* Invoices List Table */}
      <div className="content-card">
        <h3 className="card-title">Audited Invoices Log</h3>
        {loading ? (
          <div style={{ color: "#9ca3af" }}>Loading invoices registry...</div>
        ) : invoices.length === 0 ? (
          <div style={{ color: "#4b5563", padding: "12px 0", textAlign: "center" }}>No invoices audited in the system.</div>
        ) : (
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Invoice Number</th>
                  <th>Vendor</th>
                  <th>Issue Date</th>
                  <th style={{ textAlign: "right" }}>Total Amount</th>
                  <th>Audit Check</th>
                  <th>Workflow Status</th>
                  <th>Logs / Notes</th>
                </tr>
              </thead>
              <tbody>
                {invoices.map((inv) => (
                  <tr key={inv.id}>
                    <td style={{ fontWeight: "700", fontFamily: "monospace" }}>{inv.invoiceNumber}</td>
                    <td>{inv.vendorName}</td>
                    <td>{new Date(inv.issueDate).toLocaleDateString()}</td>
                    <td style={{ textAlign: "right", fontWeight: "700" }}>${inv.totalAmount.toFixed(2)}</td>
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
                    <td style={{ color: "#9ca3af", maxWidth: "250px", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                      {inv.auditNotes || "—"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </ClientLayout>
  );
}
