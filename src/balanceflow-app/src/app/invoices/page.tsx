"use client";

import { useEffect, useState } from "react";
import ClientLayout from "../components/ClientLayout";
import { api, getToken } from "../api";
import { 
  UploadCloud, 
  CheckCircle2, 
  AlertTriangle, 
  X, 
  FileText, 
  ArrowRight,
  ClipboardList,
  AlertCircle
} from "lucide-react";

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
    if (!getToken()) return;
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
        accountId: l.suggestedAccountId || accounts.find((a) => a.type === 4)?.id || "",
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitPrice,
      })),
    };

    try {
      const invoice = await api.invoices.create(invoicePayload);
      const audited = await api.invoices.audit(invoice.id);
      
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
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-text-main">Invoice Audit & Posting</h1>
          <p className="text-sm text-text-sub mt-1">Upload billing documents, execute automated compliance audits, and approve accounts payable.</p>
        </div>
      </div>

      {error && (
        <div className="flex gap-2 items-center p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-sm mb-6">
          <AlertCircle className="h-4 w-4 shrink-0" />
          <div className="leading-tight font-medium">{error}</div>
        </div>
      )}

      {/* OCR Split Preview Panel */}
      {ocrResult && (
        <div className="bg-bg-card border border-brand/30 rounded-2xl p-6 shadow-xl mb-8 relative overflow-hidden bg-radial-at-br from-brand/5 to-transparent">
          <div className="flex justify-between items-center pb-4 border-b border-border-main mb-6">
            <div className="flex items-center gap-2">
              <span className="text-xs h-2 w-2 rounded-full bg-brand animate-pulse"></span>
              <h3 className="text-sm font-bold text-brand uppercase tracking-widest">
                Intelligent Document Scan: {ocrResult.vendorName}
              </h3>
            </div>
            <button 
              onClick={() => setOcrResult(null)} 
              className="text-text-sub hover:text-text-main transition-all duration-200 cursor-pointer"
            >
              <X className="h-5 w-5" />
            </button>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            
            {/* Left Column: Headers & Actions */}
            <div className="flex flex-col gap-6">
              <div className="grid grid-cols-2 gap-x-6 gap-y-4 bg-bg-main/40 p-4 rounded-xl border border-border-main">
                <div>
                  <span className="text-[10px] font-bold text-text-sub uppercase tracking-wider">Invoice No</span>
                  <div className="text-sm font-bold text-text-main font-mono mt-1">{ocrResult.invoiceNumber}</div>
                </div>
                <div>
                  <span className="text-[10px] font-bold text-text-sub uppercase tracking-wider">Vendor Name</span>
                  <div className="text-sm font-bold text-text-main mt-1">{ocrResult.vendorName}</div>
                </div>
                <div>
                  <span className="text-[10px] font-bold text-text-sub uppercase tracking-wider">Issue Date</span>
                  <div className="text-sm text-text-main mt-1">{ocrResult.issueDate ? new Date(ocrResult.issueDate).toLocaleDateString() : "—"}</div>
                </div>
                <div>
                  <span className="text-[10px] font-bold text-text-sub uppercase tracking-wider">Due Date</span>
                  <div className="text-sm text-text-main mt-1">{ocrResult.dueDate ? new Date(ocrResult.dueDate).toLocaleDateString() : "—"}</div>
                </div>
                <div>
                  <span className="text-[10px] font-bold text-text-sub uppercase tracking-wider">Tax Amount</span>
                  <div className="text-sm text-text-main mt-1">${ocrResult.taxAmount.toFixed(2)}</div>
                </div>
                <div>
                  <span className="text-[10px] font-bold text-text-sub uppercase tracking-wider">Total Amount</span>
                  <div className="text-sm font-extrabold text-emerald-500 mt-1">${ocrResult.totalAmount.toFixed(2)}</div>
                </div>
              </div>

              {/* Audit Status log */}
              <div className={`p-4 rounded-xl border flex flex-col gap-2 text-xs font-semibold ${
                ocrResult.auditStatus === 2 
                  ? "bg-emerald-500/5 border-emerald-500/20 text-emerald-500" 
                  : ocrResult.auditStatus === 3 
                  ? "bg-rose-500/5 border-rose-500/20 text-rose-500" 
                  : "bg-bg-main border-border-main text-text-sub"
              }`}>
                <div className="flex gap-2 items-center">
                  {ocrResult.auditStatus === 2 ? (
                    <CheckCircle2 className="h-4 w-4 text-emerald-500" />
                  ) : ocrResult.auditStatus === 3 ? (
                    <AlertTriangle className="h-4 w-4 text-rose-500" />
                  ) : (
                    <span className="h-3 w-3 rounded-full bg-text-sub/50"></span>
                  )}
                  <span className="uppercase tracking-wider">Compliance: {ocrResult.auditStatusName || "Ready to Audit"}</span>
                </div>
                <p className="text-text-main font-normal leading-relaxed">{ocrResult.auditNotes || "Save the draft invoice to trigger automated audit checks."}</p>
              </div>

              {/* Action Stage Buttons */}
              {ocrResult.status === 2 && ocrResult.auditStatus === 2 ? (
                <div className="flex flex-col gap-2">
                  <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="ap">
                    Post to Accounts Payable (Liability)
                  </label>
                  <div className="flex gap-3">
                    <div className="flex-1 relative">
                      <select
                        id="ap"
                        className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-xs outline-none focus:border-brand/60 transition-all duration-200 cursor-pointer appearance-none animate-none"
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
                      <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-text-sub text-xs">
                        ▼
                      </div>
                    </div>
                    <button
                      onClick={handleApproveInvoice}
                      className="px-6 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold rounded-xl transition-all duration-200 cursor-pointer disabled:opacity-50 flex items-center gap-1.5 shadow-lg shadow-emerald-600/10"
                      disabled={!selectedAPAccount || approving}
                    >
                      <span>Approve & Post</span>
                      <ArrowRight className="h-3.5 w-3.5" />
                    </button>
                  </div>
                </div>
              ) : ocrResult.status === 1 ? (
                <button
                  onClick={handleCreateAndAuditInvoice}
                  className="w-full py-3 bg-brand hover:bg-brand-hover text-white text-sm font-semibold rounded-xl border border-brand/20 shadow-lg shadow-brand/10 transition-all duration-200 cursor-pointer"
                  disabled={approving}
                >
                  {approving ? "Running Audit Engine..." : "Save Draft & Trigger Audit"}
                </button>
              ) : (
                <div className="text-sm font-bold text-rose-500 flex items-center gap-1.5 p-3 bg-rose-500/5 border border-rose-500/10 rounded-xl">
                  <AlertTriangle className="h-4 w-4" />
                  <span>Approved & posted to general ledger. Entry modification locked.</span>
                </div>
              )}
            </div>

            {/* Right Column: Line items details */}
            <div className="border-t lg:border-t-0 lg:border-l border-border-main pt-6 lg:pt-0 lg:pl-8">
              <h4 className="text-xs font-bold text-text-sub uppercase tracking-widest mb-4">Itemized Line Items</h4>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-xs border-collapse">
                  <thead>
                    <tr className="border-b border-border-main">
                      <th className="pb-2 text-text-sub">Description</th>
                      <th className="pb-2 text-text-sub">Qty</th>
                      <th className="pb-2 text-text-sub text-right">Unit Price</th>
                      <th className="pb-2 text-text-sub text-right">Total</th>
                      <th className="pb-2 text-text-sub text-right">Category</th>
                    </tr>
                  </thead>
                  <tbody>
                    {ocrResult.lineItems.map((line: any, idx: number) => (
                      <tr key={idx} className="border-b border-border-main/20 hover:bg-bg-main/10">
                        <td className="py-3 text-text-main font-semibold">{line.description}</td>
                        <td className="py-3 text-text-sub">{line.quantity}</td>
                        <td className="py-3 text-right text-text-sub">${line.unitPrice.toFixed(2)}</td>
                        <td className="py-3 text-right font-bold text-text-main">${line.lineTotal?.toFixed(2) || (line.quantity * line.unitPrice).toFixed(2)}</td>
                        <td className="py-3 text-right">
                          <span className={`inline-flex px-2 py-0.5 rounded text-[8px] font-bold uppercase tracking-wider ${
                            line.suggestedAccountId 
                              ? "bg-emerald-500/10 text-emerald-500" 
                              : "bg-bg-main border border-border-main text-text-sub"
                          }`}>
                            {line.suggestedAccountId ? "Suggested" : "Manual"}
                          </span>
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

      {/* Drag & Drop zone cards */}
      {!ocrResult && (
        <div className="mb-10">
          <label className="flex flex-col items-center justify-center border-2 border-dashed border-border-main hover:border-brand/50 bg-bg-card hover:bg-brand/[0.01] rounded-2xl p-10 text-center cursor-pointer transition-all duration-300 shadow-inner group">
            <UploadCloud className="h-12 w-12 text-text-sub/50 group-hover:text-brand group-hover:scale-105 transition-all duration-300 mb-4" />
            <div className="text-sm font-semibold text-text-main mb-1">
              {uploading ? "Extracting invoice metrics..." : "Upload Invoice Document"}
            </div>
            <div className="text-xs text-text-sub mb-4">Click to select or drag PDF, PNG, or JPEG files (Max 10MB)</div>
            <input
              type="file"
              onChange={handleFileUpload}
              className="hidden"
              accept=".pdf,.png,.jpg,.jpeg"
              disabled={uploading}
            />
          </label>
        </div>
      )}

      {/* Audited Invoices grid list (TailAdmin style) */}
      <div className="bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg">
        <div className="flex items-center gap-2 mb-6">
          <ClipboardList className="h-5 w-5 text-brand" />
          <h3 className="text-lg font-bold text-text-main">Invoice Compliance Register</h3>
        </div>
        
        {loading ? (
          <div className="flex justify-center items-center py-12 text-text-sub gap-3">
            <div className="h-5 w-5 animate-spin rounded-full border-2 border-brand border-t-transparent"></div>
            <span className="text-sm">Loading audited registry...</span>
          </div>
        ) : invoices.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-text-sub gap-3">
            <FileText className="h-10 w-10 text-text-sub/50" />
            <span className="text-sm">No invoices recorded in ledger logs.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border-main">
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Invoice No</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Vendor</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Issue Date</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-right">Total Amount</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-center">Compliance</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-center">Workflow</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub pl-6">Logs / Notes</th>
                </tr>
              </thead>
              <tbody>
                {invoices.map((inv) => (
                  <tr key={inv.id} className="border-b border-border-main/40 hover:bg-bg-main/20 transition-all duration-150">
                    <td className="py-4 font-bold text-text-main font-mono text-xs tracking-wider">{inv.invoiceNumber}</td>
                    <td className="py-4 font-semibold text-text-main">{inv.vendorName}</td>
                    <td className="py-4 text-text-sub">{new Date(inv.issueDate).toLocaleDateString()}</td>
                    <td className="py-4 text-right font-bold text-text-main font-mono">${inv.totalAmount.toFixed(2)}</td>
                    <td className="py-4 text-center">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                        inv.auditStatusName === "Passed"
                          ? "bg-brand/10 border-brand/20 text-brand"
                          : inv.auditStatusName === "Failed"
                          ? "bg-rose-500/10 border-rose-500/20 text-rose-500"
                          : "bg-bg-main border border-border-main text-text-sub"
                      }`}>
                        {inv.auditStatusName}
                      </span>
                    </td>
                    <td className="py-4 text-center">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                        inv.statusName === "Approved"
                          ? "bg-brand/10 border-brand/20 text-brand"
                          : "bg-bg-main border border-border-main text-text-sub"
                      }`}>
                        {inv.statusName}
                      </span>
                    </td>
                    <td className="py-4 text-xs text-text-sub pl-6 italic truncate max-w-[200px]" title={inv.auditNotes}>
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
