"use client";

import { useEffect, useState } from "react";
import ClientLayout from "../components/ClientLayout";
import { api, getToken } from "../api";
import { Plus, X, Search, AlertCircle, CheckCircle2, Circle } from "lucide-react";

export default function AccountsPage() {
  const [accounts, setAccounts] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Creation modal state
  const [showModal, setShowModal] = useState(false);
  const [code, setCode] = useState("");
  const [name, setName] = useState("");
  const [type, setType] = useState(0); // 0=Asset, 1=Liability, 2=Equity, 3=Revenue, 4=Expense
  const [description, setDescription] = useState("");
  const [saving, setSaving] = useState(false);

  const loadAccounts = async () => {
    if (!getToken()) return;
    setLoading(true);
    setError(null);
    try {
      const data = await api.accounts.getAll(1, 50);
      setAccounts(data.items);
    } catch (err: any) {
      setError(err.message || "Failed to load accounts.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAccounts();
  }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    try {
      await api.accounts.create(code, name, type, description);
      setShowModal(false);
      // Reset form
      setCode("");
      setName("");
      setType(0);
      setDescription("");
      // Refresh list
      loadAccounts();
    } catch (err: any) {
      setError(err.message || "Failed to create account.");
    } finally {
      setSaving(false);
    }
  };

  const getAccountTypeLabel = (typeValue: number) => {
    const labels = ["Asset", "Liability", "Equity", "Revenue", "Expense"];
    return labels[typeValue] || "Unknown";
  };

  return (
    <ClientLayout>
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-10">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-text-main">Chart of Accounts</h1>
          <p className="text-sm text-text-sub mt-1">Manage and configure general ledger accounts to categorize transactions.</p>
        </div>
        <button 
          onClick={() => setShowModal(true)} 
          className="flex items-center gap-2 px-4 py-2.5 bg-brand hover:bg-brand-hover text-white text-sm font-semibold rounded-xl transition-all duration-200 cursor-pointer shadow-lg shadow-brand/10"
        >
          <Plus className="h-4 w-4" />
          <span>Create Account</span>
        </button>
      </div>

      {error && !showModal && (
        <div className="flex gap-2 items-center p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-sm mb-6">
          <AlertCircle className="h-4 w-4 shrink-0" />
          <div className="leading-tight font-medium">{error}</div>
        </div>
      )}

      <div className="bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg">
        {loading ? (
          <div className="flex justify-center items-center py-12 text-text-sub gap-3">
            <div className="h-5 w-5 animate-spin rounded-full border-2 border-brand border-t-transparent"></div>
            <span className="text-sm">Loading ledger chart...</span>
          </div>
        ) : accounts.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-text-sub gap-3">
            <Search className="h-10 w-10 text-text-sub/50" />
            <span className="text-sm">No ledger accounts registered yet. Click Create Account above to begin.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="border-b border-border-main">
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Account Code</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Account Name</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Classification</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Status</th>
                  <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Description</th>
                </tr>
              </thead>
              <tbody>
                {accounts.map((acc) => (
                  <tr key={acc.id} className="border-b border-border-main/40 hover:bg-bg-main/20 transition-all duration-150">
                    <td className="py-4 font-bold text-text-main font-mono text-xs tracking-wider">{acc.accountCode}</td>
                    <td className="py-4 text-text-main font-semibold">{acc.name}</td>
                    <td className="py-4">
                      <span className={`inline-flex px-2.5 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                        acc.type === 0 ? "bg-blue-500/10 border-blue-500/20 text-blue-500" :
                        acc.type === 1 ? "bg-amber-500/10 border-amber-500/20 text-amber-500" :
                        acc.type === 2 ? "bg-emerald-500/10 border-emerald-500/20 text-emerald-500" :
                        acc.type === 3 ? "bg-purple-500/10 border-purple-500/20 text-purple-500" :
                        "bg-rose-500/10 border-rose-500/20 text-rose-500"
                      }`}>
                        {getAccountTypeLabel(acc.type)}
                      </span>
                    </td>
                    <td className="py-4">
                      <span className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                        acc.isActive
                          ? "bg-emerald-500/10 border-emerald-500/20 text-emerald-500"
                          : "bg-rose-500/10 border-rose-500/20 text-rose-500"
                      }`}>
                        {acc.isActive ? (
                          <>
                            <CheckCircle2 className="h-3 w-3" />
                            <span>Active</span>
                          </>
                        ) : (
                          <>
                            <Circle className="h-3 w-3" />
                            <span>Inactive</span>
                          </>
                        )}
                      </span>
                    </td>
                    <td className="py-4 text-text-sub text-xs italic">{acc.description || "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Creation Modal (shadcn/ui style Dialog) */}
      {showModal && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-in fade-in duration-200">
          <div className="w-full max-w-[500px] bg-bg-card border border-border-main rounded-2xl p-8 shadow-2xl relative">
            <button 
              onClick={() => setShowModal(false)}
              className="absolute top-6 right-6 text-text-sub hover:text-text-main transition-all duration-200 cursor-pointer"
            >
              <X className="h-5 w-5" />
            </button>

            <h3 className="text-xl font-bold text-text-main mb-6">Create Ledger Account</h3>
            
            {error && (
              <div className="flex gap-2 items-center p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-sm mb-6">
                <AlertCircle className="h-4 w-4 shrink-0" />
                <div className="leading-tight font-medium">{error}</div>
              </div>
            )}

            <form onSubmit={handleCreate} className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="code">
                  Account Code (Unique)
                </label>
                <input
                  id="code"
                  type="text"
                  className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200"
                  placeholder="e.g. 1010, 2100"
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  required
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="name">
                  Account Name
                </label>
                <input
                  id="name"
                  type="text"
                  className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200"
                  placeholder="e.g. Cash in Hand, Server Expenses"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="type">
                  Classification
                </label>
                <div className="relative">
                  <select
                    id="type"
                    className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 transition-all duration-200 cursor-pointer appearance-none"
                    value={type}
                    onChange={(e) => setType(parseInt(e.target.value))}
                  >
                    <option value={0}>Asset (Cash, Receivables)</option>
                    <option value={1}>Liability (Payables, Loans)</option>
                    <option value={2}>Equity (Retained Earnings, Capital)</option>
                    <option value={3}>Revenue (Sales, Service Fees)</option>
                    <option value={4}>Expense (Rent, Supplies, Taxes)</option>
                  </select>
                  <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-text-sub text-xs">
                    ▼
                  </div>
                </div>
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="desc">
                  Description
                </label>
                <textarea
                  id="desc"
                  className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200 min-h-[90px] resize-vertical"
                  placeholder="Detail ledger utilization guidelines"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
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
                  disabled={saving}
                >
                  {saving ? "Saving..." : "Create Account"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </ClientLayout>
  );
}
