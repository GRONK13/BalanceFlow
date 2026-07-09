"use client";

import { useEffect, useState } from "react";
import ClientLayout from "../components/ClientLayout";
import { api } from "../api";

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
      <div className="page-header">
        <div>
          <h1 className="page-title">Chart of Accounts</h1>
          <p className="page-subtitle">Add and configure ledger accounts to categorize transactions.</p>
        </div>
        <button onClick={() => setShowModal(true)} className="btn btn-primary">
          ➕ Create Account
        </button>
      </div>

      {error && !showModal && (
        <div style={{ padding: "12px", backgroundColor: "rgba(239, 68, 68, 0.1)", border: "1px solid rgba(239, 68, 68, 0.2)", borderRadius: "8px", color: "#f87171", marginBottom: "20px" }}>
          {error}
        </div>
      )}

      <div className="content-card">
        {loading ? (
          <div style={{ color: "#9ca3af", padding: "20px 0" }}>Loading ledger chart...</div>
        ) : accounts.length === 0 ? (
          <div style={{ color: "#4b5563", padding: "20px 0", textAlign: "center" }}>
            No accounts found. Create your first ledger account above!
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Account Code</th>
                  <th>Account Name</th>
                  <th>Classification</th>
                  <th>Status</th>
                  <th>Description</th>
                </tr>
              </thead>
              <tbody>
                {accounts.map((acc) => (
                  <tr key={acc.id}>
                    <td style={{ fontWeight: "700", fontFamily: "monospace" }}>{acc.accountCode}</td>
                    <td>{acc.name}</td>
                    <td>
                      <span className={`badge ${getAccountTypeLabel(acc.type).toLowerCase()}`}>
                        {getAccountTypeLabel(acc.type)}
                      </span>
                    </td>
                    <td>
                      <span className={`badge ${acc.isActive ? "approved" : "rejected"}`}>
                        {acc.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td style={{ color: "#9ca3af" }}>{acc.description || "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Creation Modal */}
      {showModal && (
        <div style={{ position: "fixed", top: 0, left: 0, width: "100%", height: "100%", backgroundColor: "rgba(0, 0, 0, 0.6)", backdropFilter: "blur(4px)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100 }}>
          <div style={{ width: "100%", maxWidth: "500px", padding: "32px", backgroundColor: "#111827", borderRadius: "12px", border: "1px solid #1f2937" }}>
            <h3 style={{ fontSize: "20px", fontWeight: "600", color: "#fff", marginBottom: "20px" }}>Create Ledger Account</h3>
            
            {error && (
              <div style={{ padding: "12px", backgroundColor: "rgba(239, 68, 68, 0.1)", border: "1px solid rgba(239, 68, 68, 0.2)", borderRadius: "8px", color: "#f87171", fontSize: "14px", marginBottom: "20px" }}>
                {error}
              </div>
            )}

            <form onSubmit={handleCreate}>
              <div className="form-group">
                <label className="form-label" htmlFor="code">Account Code (Unique)</label>
                <input
                  id="code"
                  type="text"
                  className="form-input"
                  placeholder="e.g. 1010, 2100"
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="name">Account Name</label>
                <input
                  id="name"
                  type="text"
                  className="form-input"
                  placeholder="e.g. Cash in Hand, Rent Expense"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="type">Classification</label>
                <select
                  id="type"
                  className="form-input"
                  value={type}
                  onChange={(e) => setType(parseInt(e.target.value))}
                >
                  <option value={0}>Asset (Cash, Receivables)</option>
                  <option value={1}>Liability (Payables, Loans)</option>
                  <option value={2}>Equity (Retained Earnings, Capital)</option>
                  <option value={3}>Revenue (Sales, Service Fees)</option>
                  <option value={4}>Expense (Rent, Supplies, Taxes)</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="desc">Description (Optional)</label>
                <textarea
                  id="desc"
                  className="form-input"
                  placeholder="Memo detailing account utilization rules"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  style={{ minHeight: "80px", resize: "vertical" }}
                />
              </div>

              <div style={{ display: "flex", gap: "12px", justifyContent: "flex-end", marginTop: "24px" }}>
                <button type="button" onClick={() => setShowModal(false)} className="btn btn-secondary" disabled={saving}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
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
