"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "../api";

export default function LoginPage() {
  const router = useRouter();
  const [username, setUsername] = useState("");
  const [role, setRole] = useState("Accountant");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await api.auth.login(username, role);
      router.push("/");
    } catch (err: any) {
      setError(err.message || "Failed to log in.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ display: "flex", minHeight: "100vh", alignItems: "center", justifyContent: "center", backgroundColor: "#0b0f19" }}>
      <div style={{ width: "100%", padding: "40px", backgroundColor: "#111827", borderRadius: "12px", border: "1px solid #1f2937", maxWidth: "420px" }}>
        
        <div style={{ display: "flex", alignItems: "center", gap: "12px", justifyContent: "center", marginBottom: "32px" }}>
          <div style={{ background: "linear-gradient(135deg, #3b82f6, #8b5cf6)", borderRadius: "8px", height: "36px", width: "36px", display: "flex", alignItems: "center", justifyContent: "center", fontWeight: "bold", color: "#fff", fontSize: "20px" }}>Δ</div>
          <span style={{ fontSize: "24px", fontWeight: "700", background: "linear-gradient(to right, #60a5fa, #a78bfa)", WebkitBackgroundClip: "text", WebkitTextFillColor: "transparent" }}>BalanceFlow</span>
        </div>

        <h2 style={{ fontSize: "20px", fontWeight: "600", color: "#fff", marginBottom: "8px", textAlign: "center" }}>Mock Identity Gateway</h2>
        <p style={{ fontSize: "14px", color: "#9ca3af", textAlign: "center", marginBottom: "32px" }}>Select a mock role to obtain a valid JWT token.</p>

        {error && (
          <div style={{ padding: "12px", backgroundColor: "rgba(239, 68, 68, 0.1)", border: "1px solid rgba(239, 68, 68, 0.2)", borderRadius: "8px", color: "#f87171", fontSize: "14px", marginBottom: "20px", textAlign: "center" }}>
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label" htmlFor="username">Username</label>
            <input
              id="username"
              type="text"
              className="form-input"
              placeholder="e.g. gregg"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="role">Mock Role Claim</label>
            <select
              id="role"
              className="form-input"
              value={role}
              onChange={(e) => setRole(e.target.value)}
              style={{ cursor: "pointer" }}
            >
              <option value="Accountant">Accountant (Ledger manager)</option>
              <option value="Auditor">Auditor (Compliance engine checker)</option>
              <option value="Admin">Administrator (Full permission)</option>
            </select>
          </div>

          <button
            type="submit"
            className="btn btn-primary"
            style={{ width: "100%", marginTop: "12px" }}
            disabled={loading}
          >
            {loading ? "Authenticating..." : "Obtain Token & Log In"}
          </button>
        </form>
      </div>
    </div>
  );
}
