"use client";

import { useEffect, useState, Fragment } from "react";
import ClientLayout from "../components/ClientLayout";
import { api, getToken } from "../api";
import { 
  Plus, 
  Trash2, 
  Scale, 
  X, 
  ChevronDown, 
  ChevronUp, 
  AlertCircle, 
  Calendar,
  FileSpreadsheet
} from "lucide-react";

interface JournalLineInput {
  accountId: string;
  debitAmount: number;
  creditAmount: number;
  description: string;
}

export default function JournalEntriesPage() {
  const [entries, setEntries] = useState<any[]>([]);
  const [accounts, setAccounts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Expanded rows state
  const [expandedId, setExpandedId] = useState<string | null>(null);

  // Creation modal state
  const [showModal, setShowModal] = useState(false);
  const [refNum, setRefNum] = useState("");
  const [date, setDate] = useState(new Date().toISOString().split("T")[0]);
  const [desc, setDesc] = useState("");
  const [lines, setLines] = useState<JournalLineInput[]>([
    { accountId: "", debitAmount: 0, creditAmount: 0, description: "" },
    { accountId: "", debitAmount: 0, creditAmount: 0, description: "" },
  ]);
  const [saving, setSaving] = useState(false);

  const loadData = async () => {
    if (!getToken()) return;
    setLoading(true);
    setError(null);
    try {
      const entriesData = await api.journalEntries.getAll(1, 100);
      const accountsData = await api.accounts.getAll(1, 100);
      setEntries(entriesData.items);
      setAccounts(accountsData.items.filter((a) => a.isActive));
    } catch (err: any) {
      setError(err.message || "Failed to load entries.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const toggleExpand = (id: string) => {
    setExpandedId(expandedId === id ? null : id);
  };

  // Calculations for balanced double entry checks (using epsilon math to prevent rounding errors)
  const totalDebits = lines.reduce((sum, l) => sum + (l.debitAmount || 0), 0);
  const totalCredits = lines.reduce((sum, l) => sum + (l.creditAmount || 0), 0);
  const difference = Math.abs(totalDebits - totalCredits);
  const isBalanced = difference < 0.001 && totalDebits > 0 && lines.every(l => l.accountId !== "");

  const handleAddLine = () => {
    setLines([...lines, { accountId: "", debitAmount: 0, creditAmount: 0, description: "" }]);
  };

  const handleRemoveLine = (index: number) => {
    if (lines.length <= 2) return; // Keep at least 2 lines
    setLines(lines.filter((_, i) => i !== index));
  };

  const handleLineChange = (index: number, field: keyof JournalLineInput, value: any) => {
    const updated = [...lines];
    if (field === "debitAmount" || field === "creditAmount") {
      const numVal = parseFloat(value) || 0;
      updated[index][field] = numVal;
      // Mutually exclusive: if debit > 0, credit must be 0, and vice versa
      if (field === "debitAmount" && numVal > 0) {
        updated[index].creditAmount = 0;
      } else if (field === "creditAmount" && numVal > 0) {
        updated[index].debitAmount = 0;
      }
    } else {
      updated[index][field] = value;
    }
    setLines(updated);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isBalanced) return;

    setSaving(true);
    setError(null);

    const formattedLines = lines.map((l) => ({
      accountId: l.accountId,
      debitAmount: l.debitAmount,
      creditAmount: l.creditAmount,
      description: l.description,
    }));

    try {
      const entryResult = await api.journalEntries.create(refNum, date, desc, formattedLines);
      await api.journalEntries.post(entryResult.id);

      setShowModal(false);
      // Reset form
      setRefNum("");
      setDesc("");
      setLines([
        { accountId: "", debitAmount: 0, creditAmount: 0, description: "" },
        { accountId: "", debitAmount: 0, creditAmount: 0, description: "" },
      ]);
      loadData();
    } catch (err: any) {
      setError(err.message || "Failed to submit journal entry.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <ClientLayout>
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-text-main">Journal Entries Ledger</h1>
          <p className="text-sm text-text-sub mt-1">Post balanced double-entry adjustments and cash transfers directly to the general ledger.</p>
        </div>
        <button 
          onClick={() => setShowModal(true)} 
          className="flex items-center gap-2 px-4 py-2.5 bg-brand hover:bg-brand-hover text-white text-sm font-semibold rounded-xl transition-all duration-200 cursor-pointer shadow-lg shadow-brand/10"
        >
          <Plus className="h-4 w-4" />
          <span>New Posting</span>
        </button>
      </div>

      {error && !showModal && (
        <div className="flex gap-2 items-center p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-sm mb-6">
          <AlertCircle className="h-4 w-4 shrink-0" />
          <div className="leading-tight font-medium">{error}</div>
        </div>
      )}

      {/* Ledger Entries Registry Card */}
      <div className="bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg">
        {loading ? (
          <div className="flex justify-center items-center py-12 text-text-sub gap-3">
            <div className="h-5 w-5 animate-spin rounded-full border-2 border-brand border-t-transparent"></div>
            <span className="text-sm">Loading ledger entries...</span>
          </div>
        ) : entries.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-text-sub gap-3">
            <FileSpreadsheet className="h-10 w-10 text-text-sub/50" />
            <span className="text-sm">No postings recorded. Click New Posting above to draft a ledger entry.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border-main">
                  <th style={{ width: "40px" }}></th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Reference Number</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Posting Date</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Description</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-right">Ledger Status</th>
                </tr>
              </thead>
              <tbody>
                {entries.map((entry) => {
                  const isExpanded = expandedId === entry.id;
                  return (
                    <Fragment key={entry.id}>
                      <tr 
                        onClick={() => toggleExpand(entry.id)} 
                        className={`border-b border-border-main/40 hover:bg-bg-main/20 transition-all duration-150 cursor-pointer ${isExpanded ? "bg-bg-main/10" : ""}`}
                      >
                        <td className="py-4 text-center">
                          {isExpanded ? (
                            <ChevronUp className="h-4 w-4 text-brand inline" />
                          ) : (
                            <ChevronDown className="h-4 w-4 text-text-sub/60 inline" />
                          )}
                        </td>
                        <td className="py-4 font-bold text-text-main font-mono text-xs tracking-wider">{entry.referenceNumber}</td>
                        <td className="py-4 text-text-main">
                          <span className="inline-flex items-center gap-1.5">
                            <Calendar className="h-3.5 w-3.5 text-text-sub" />
                            <span>{new Date(entry.transactionDate).toLocaleDateString()}</span>
                          </span>
                        </td>
                        <td className="py-4 text-text-main font-semibold">{entry.description || "—"}</td>
                        <td className="py-4 text-right">
                          <span className={`inline-flex px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                            entry.isPosted 
                              ? "bg-brand/10 border-brand/20 text-brand" 
                              : "bg-bg-main border-border-main text-text-sub"
                          }`}>
                            {entry.isPosted ? "Posted" : "Draft"}
                          </span>
                        </td>
                      </tr>
                      
                      {/* Expanded Posting Details Panel */}
                      {isExpanded && (
                        <tr>
                          <td colSpan={5} className="bg-bg-main/80 border-b border-border-main/50 p-6">
                            <div className="border-l-2 border-brand/60 pl-6 max-w-5xl">
                              <h4 className="text-xs font-bold text-text-sub uppercase tracking-widest mb-4">
                                Double-Entry Audit Logs
                              </h4>
                              <table className="w-full text-left text-xs border-collapse">
                                <thead>
                                  <tr className="border-b border-border-main/80">
                                    <th className="pb-2 font-semibold text-text-sub/60">Ledger Account</th>
                                    <th className="pb-2 font-semibold text-text-sub/60">Classification</th>
                                    <th className="pb-2 font-semibold text-text-sub/60 text-right">Debit</th>
                                    <th className="pb-2 font-semibold text-text-sub/60 text-right">Credit</th>
                                    <th className="pb-2 font-semibold text-text-sub/60 pl-6">Memo</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {entry.lines.map((line: any) => (
                                    <tr key={line.id} className="border-b border-border-main/20 hover:bg-bg-main/10">
                                      <td className="py-3 font-semibold text-text-main">{line.accountCode} — {line.accountName}</td>
                                      <td className="py-3">
                                        <span className={`inline-flex px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-wider border ${
                                          line.accountType === "Asset" ? "bg-blue-500/10 border-blue-500/20 text-blue-500" :
                                          line.accountType === "Liability" ? "bg-amber-500/10 border-amber-500/20 text-amber-500" :
                                          line.accountType === "Equity" ? "bg-emerald-500/10 border-emerald-500/20 text-emerald-500" :
                                          line.accountType === "Revenue" ? "bg-purple-500/10 border-purple-500/20 text-purple-500" :
                                          "bg-rose-500/10 border-rose-500/20 text-rose-500"
                                        }`}>
                                          {line.accountType}
                                        </span>
                                      </td>
                                      <td className={`py-3 text-right font-mono font-medium ${line.debitAmount > 0 ? "text-brand" : "text-text-sub/50"}`}>
                                        {line.debitAmount > 0 ? `$${line.debitAmount.toFixed(2)}` : "—"}
                                      </td>
                                      <td className={`py-3 text-right font-mono font-medium ${line.creditAmount > 0 ? "text-emerald-500" : "text-text-sub/50"}`}>
                                        {line.creditAmount > 0 ? `$${line.creditAmount.toFixed(2)}` : "—"}
                                      </td>
                                      <td className="py-3 text-text-sub pl-6 italic">{line.description || "—"}</td>
                                    </tr>
                                  ))}
                                  <tr className="font-bold border-t border-border-main/80 bg-bg-main/20">
                                    <td colSpan={2} className="py-3 text-text-sub">Total Sum (Balanced)</td>
                                    <td className="py-3 text-right text-brand font-mono">
                                      ${entry.lines.reduce((sum: number, l: any) => sum + l.debitAmount, 0).toFixed(2)}
                                    </td>
                                    <td className="py-3 text-right text-emerald-500 font-mono">
                                      ${entry.lines.reduce((sum: number, l: any) => sum + l.creditAmount, 0).toFixed(2)}
                                    </td>
                                    <td></td>
                                  </tr>
                                </tbody>
                              </table>
                            </div>
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Creation Modal Dialog */}
      {showModal && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-in fade-in duration-200">
          <div className="w-full max-w-[900px] bg-bg-card border border-border-main rounded-2xl p-8 shadow-2xl relative max-h-[90vh] overflow-y-auto">
            <button 
              onClick={() => setShowModal(false)}
              className="absolute top-6 right-6 text-text-sub hover:text-text-main transition-all duration-200 cursor-pointer"
            >
              <X className="h-5 w-5" />
            </button>

            <h3 className="text-xl font-bold text-text-main mb-6">Draft Double-Entry Transaction</h3>
            
            {error && (
              <div className="flex gap-2 items-center p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-sm mb-6">
                <AlertCircle className="h-4 w-4 shrink-0" />
                <div className="leading-tight font-medium">{error}</div>
              </div>
            )}

            <form onSubmit={handleCreate} className="flex flex-col gap-5">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="ref">
                    Reference Number
                  </label>
                  <input
                    id="ref"
                    type="text"
                    className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200"
                    placeholder="e.g. JE-0012"
                    value={refNum}
                    onChange={(e) => setRefNum(e.target.value)}
                    required
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="date">
                    Transaction Date
                  </label>
                  <input
                    id="date"
                    type="date"
                    className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 transition-all duration-200"
                    value={date}
                    onChange={(e) => setDate(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="desc">
                  Header Memo / Narration
                </label>
                <input
                  id="desc"
                  type="text"
                  className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200"
                  placeholder="e.g. Adjust office supplies expense accrual"
                  value={desc}
                  onChange={(e) => setDesc(e.target.value)}
                  required
                />
              </div>

              <div className="mt-4">
                <h4 className="text-xs font-bold text-text-sub uppercase tracking-widest mb-4">
                  Ledger Postings Rows
                </h4>
                
                <div className="flex flex-col gap-3">
                  {lines.map((line, index) => (
                    <div key={index} className="flex gap-3 items-center">
                      <div className="flex-[3] relative">
                        <select
                          className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-xs outline-none focus:border-brand/60 transition-all duration-200 cursor-pointer appearance-none animate-none"
                          value={line.accountId}
                          onChange={(e) => handleLineChange(index, "accountId", e.target.value)}
                          required
                        >
                          <option value="">Select Ledger Account...</option>
                          {accounts.map((acc) => (
                            <option key={acc.id} value={acc.id}>
                              {acc.accountCode} — {acc.name}
                            </option>
                          ))}
                        </select>
                        <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-text-sub text-xs">
                          ▼
                        </div>
                      </div>

                      <div className="flex-[1.5]">
                        <input
                          type="number"
                          step="0.01"
                          className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-xs outline-none focus:border-brand/60 text-right disabled:opacity-30 placeholder:text-text-sub/30"
                          placeholder="Debit ($)"
                          value={line.debitAmount || ""}
                          onChange={(e) => handleLineChange(index, "debitAmount", e.target.value)}
                          disabled={line.creditAmount > 0}
                        />
                      </div>

                      <div className="flex-[1.5]">
                        <input
                          type="number"
                          step="0.01"
                          className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-xs outline-none focus:border-brand/60 text-right disabled:opacity-30 placeholder:text-text-sub/30"
                          placeholder="Credit ($)"
                          value={line.creditAmount || ""}
                          onChange={(e) => handleLineChange(index, "creditAmount", e.target.value)}
                          disabled={line.debitAmount > 0}
                        />
                      </div>

                      <div className="flex-[3]">
                        <input
                          type="text"
                          className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-xs outline-none focus:border-brand/60 placeholder:text-text-sub/30"
                          placeholder="Line description..."
                          value={line.description}
                          onChange={(e) => handleLineChange(index, "description", e.target.value)}
                        />
                      </div>

                      <button
                        type="button"
                        onClick={() => handleRemoveLine(index)}
                        className="text-rose-500 hover:text-rose-400 p-2 cursor-pointer disabled:opacity-20"
                        disabled={lines.length <= 2}
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  ))}
                </div>

                <button
                  type="button"
                  onClick={handleAddLine}
                  className="flex items-center gap-1.5 px-3 py-1.5 bg-bg-main border border-border-main text-text-main hover:bg-bg-main/60 text-xs font-semibold rounded-lg transition-all duration-200 mt-4 cursor-pointer"
                >
                  <Plus className="h-3.5 w-3.5" />
                  <span>Add Ledger Row</span>
                </button>
              </div>

              {/* Real-time Balanced State Panel (shadcn style Alert) */}
              <div className={`mt-6 p-4 rounded-xl border flex items-center justify-between gap-4 text-xs font-semibold ${
                isBalanced 
                  ? "bg-emerald-500/5 border-emerald-500/20 text-emerald-500" 
                  : "bg-rose-500/5 border-rose-500/20 text-rose-500"
              }`}>
                <div className="flex gap-4">
                  <div>Debits: <strong className="font-mono text-sm text-brand">${totalDebits.toFixed(2)}</strong></div>
                  <div>Credits: <strong className="font-mono text-sm text-emerald-500">${totalCredits.toFixed(2)}</strong></div>
                </div>
                <div className="flex items-center gap-1.5">
                  {isBalanced ? (
                    <>
                      <Scale className="h-4 w-4 text-emerald-500" />
                      <span>Ledger Balanced</span>
                    </>
                  ) : (
                    <>
                      <AlertCircle className="h-4 w-4 text-rose-500" />
                      <span>Out of Balance by ${difference.toFixed(2)}</span>
                    </>
                  )}
                </div>
              </div>

              <div className="flex gap-3 justify-end mt-6">
                <button 
                  type="button" 
                  onClick={() => setShowModal(false)} 
                  className="px-4 py-2.5 bg-bg-main border border-border-main text-text-main hover:bg-bg-main/60 text-xs font-semibold rounded-xl transition-all duration-200 cursor-pointer"
                  disabled={saving}
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  className="px-4 py-2.5 bg-brand hover:bg-brand-hover text-white text-xs font-semibold rounded-xl transition-all duration-200 cursor-pointer disabled:opacity-50"
                  disabled={saving || !isBalanced}
                >
                  {saving ? "Posting..." : "Post to Ledger"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </ClientLayout>
  );
}
