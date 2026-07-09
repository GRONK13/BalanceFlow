"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { api } from "../api";
import { KeyRound, ShieldAlert, Sun, Moon, ShieldCheck } from "lucide-react";

export default function LoginPage() {
  const router = useRouter();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [theme, setTheme] = useState("dark");

  useEffect(() => {
    const savedTheme = localStorage.getItem("theme") || "dark";
    setTheme(savedTheme);
    document.documentElement.setAttribute("data-theme", savedTheme);
  }, []);

  const toggleTheme = () => {
    const nextTheme = theme === "dark" ? "light" : "dark";
    setTheme(nextTheme);
    document.documentElement.setAttribute("data-theme", nextTheme);
    localStorage.setItem("theme", nextTheme);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      await api.auth.login(username, password);
      router.push("/");
    } catch (err: any) {
      setError(err.message || "Failed to authenticate. Check credentials.");
    } finally {
      setLoading(false);
    }
  };

  const handleFillDemo = (userStr: string, passStr: string) => {
    setUsername(userStr);
    setPassword(passStr);
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-bg-main px-4 relative overflow-hidden text-text-main">
      {/* Visual background glow elements */}
      <div className="absolute top-1/4 left-1/4 h-[300px] w-[300px] -translate-x-1/2 -translate-y-1/2 rounded-full bg-brand/5 blur-[120px] pointer-events-none"></div>
      <div className="absolute bottom-1/4 right-1/4 h-[300px] w-[300px] translate-x-1/2 translate-y-1/2 rounded-full bg-emerald-500/5 blur-[120px] pointer-events-none"></div>

      <div className="w-full max-w-[440px] bg-bg-card border border-border-main rounded-2xl p-8 shadow-2xl relative z-10">
        
        {/* Theme Toggle */}
        <div className="absolute top-6 right-6">
          <button
            onClick={toggleTheme}
            className="p-2 bg-bg-main/40 hover:bg-bg-main border border-border-main rounded-lg text-text-sub hover:text-text-main cursor-pointer"
            aria-label="Toggle theme"
          >
            {theme === "dark" ? <Sun className="h-4 w-4 text-amber-400" /> : <Moon className="h-4 w-4 text-indigo-500" />}
          </button>
        </div>

        {/* Brand Header */}
        <div className="flex items-center gap-3 justify-center mb-8 pr-8">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-brand to-emerald-600 text-white font-bold text-lg shadow-lg shadow-brand/20">
            Δ
          </div>
          <span className="text-2xl font-bold bg-gradient-to-r from-text-main to-brand bg-clip-text text-transparent">
            BalanceFlow
          </span>
        </div>

        <h2 className="text-xl font-bold text-text-main text-center mb-1">Secure Sign In</h2>
        <p className="text-xs text-text-sub text-center mb-6">Database-backed enterprise ledger portal.</p>

        {error && (
          <div className="flex gap-2 items-center p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-sm mb-6">
            <ShieldAlert className="h-4 w-4 shrink-0" />
            <div className="leading-tight font-medium">{error}</div>
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-4 mb-6">
          
          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="username">
              Username
            </label>
            <input
              id="username"
              type="text"
              className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200"
              placeholder="e.g. accountant"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-xs font-semibold text-text-sub uppercase tracking-wider" htmlFor="password">
              Password
            </label>
            <input
              id="password"
              type="password"
              className="w-full px-4 py-3 bg-bg-main border border-border-main rounded-xl text-text-main text-sm outline-none focus:border-brand/60 placeholder:text-text-sub/30 transition-all duration-200"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button
            type="submit"
            className="flex items-center justify-center gap-2 w-full py-3 bg-brand hover:bg-brand-hover text-white border border-brand/20 rounded-xl text-sm font-semibold shadow-lg shadow-brand/10 hover:shadow-brand/20 hover:-translate-y-0.5 active:translate-y-0 transition-all duration-200 cursor-pointer disabled:opacity-50"
            disabled={loading}
          >
            {loading ? (
              <>
                <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
                <span>Verifying credentials...</span>
              </>
            ) : (
              <>
                <KeyRound className="h-4 w-4" />
                <span>Verify & Log In</span>
              </>
            )}
          </button>
        </form>

        {/* Demo Credentials Suggestion Box */}
        <div className="p-4 bg-bg-main border border-border-main rounded-xl">
          <div className="flex items-center gap-1.5 mb-2 text-xs font-bold text-brand uppercase tracking-wider">
            <ShieldCheck className="h-3.5 w-3.5" />
            <span>Reviewer Demo Accounts</span>
          </div>
          <div className="flex flex-col gap-2">
            <button
              onClick={() => handleFillDemo("accountant", "AccountantPass123!")}
              className="flex justify-between items-center text-left p-2 rounded-lg bg-bg-card hover:bg-brand/5 border border-border-main hover:border-brand/30 text-xs text-text-sub hover:text-text-main transition-all duration-150 cursor-pointer group"
            >
              <div>
                <div className="font-semibold">Accountant Role</div>
                <div className="text-[10px] text-text-sub/60">Pass: AccountantPass123!</div>
              </div>
              <div className="text-[10px] font-bold text-brand opacity-0 group-hover:opacity-100 transition-opacity">
                Autofill →
              </div>
            </button>

            <button
              onClick={() => handleFillDemo("auditor", "AuditorPass123!")}
              className="flex justify-between items-center text-left p-2 rounded-lg bg-bg-card hover:bg-brand/5 border border-border-main hover:border-brand/30 text-xs text-text-sub hover:text-text-main transition-all duration-150 cursor-pointer group"
            >
              <div>
                <div className="font-semibold">Auditor Role</div>
                <div className="text-[10px] text-text-sub/60">Pass: AuditorPass123!</div>
              </div>
              <div className="text-[10px] font-bold text-brand opacity-0 group-hover:opacity-100 transition-opacity">
                Autofill →
              </div>
            </button>

            <button
              onClick={() => handleFillDemo("admin", "AdminPass123!")}
              className="flex justify-between items-center text-left p-2 rounded-lg bg-bg-card hover:bg-brand/5 border border-border-main hover:border-brand/30 text-xs text-text-sub hover:text-text-main transition-all duration-150 cursor-pointer group"
            >
              <div>
                <div className="font-semibold">Administrator Role</div>
                <div className="text-[10px] text-text-sub/60">Pass: AdminPass123!</div>
              </div>
              <div className="text-[10px] font-bold text-brand opacity-0 group-hover:opacity-100 transition-opacity">
                Autofill →
              </div>
            </button>
          </div>
        </div>

      </div>
    </div>
  );
}
