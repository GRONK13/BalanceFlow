"use client";

import { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { getSessionUser, clearToken } from "../api";
import Link from "next/link";
import { 
  LayoutDashboard, 
  BookOpen, 
  FileSpreadsheet, 
  ScanEye, 
  LogOut,
  Sparkles,
  Sun,
  Moon,
  ShieldAlert,
  ArrowLeft
} from "lucide-react";

interface ClientLayoutProps {
  children: React.ReactNode;
}

export default function ClientLayout({ children }: ClientLayoutProps) {
  const router = useRouter();
  const pathname = usePathname();
  const [user, setUser] = useState<{ username: string; role: string } | null>(null);
  const [loading, setLoading] = useState(true);
  const [theme, setTheme] = useState("dark"); // Default theme

  // Initialize theme from local storage or preferences
  useEffect(() => {
    const savedTheme = localStorage.getItem("theme") || 
      (window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    
    setTheme(savedTheme);
    document.documentElement.setAttribute("data-theme", savedTheme);
  }, []);

  const toggleTheme = () => {
    const nextTheme = theme === "dark" ? "light" : "dark";
    setTheme(nextTheme);
    document.documentElement.setAttribute("data-theme", nextTheme);
    localStorage.setItem("theme", nextTheme);
  };

  useEffect(() => {
    const session = getSessionUser();
    if (!session) {
      router.push("/login");
    } else {
      setUser(session);
      setLoading(false);
    }
  }, [router]);

  const handleLogout = () => {
    clearToken();
    router.push("/login");
  };

  const getRequiredRoleMessage = () => {
    if (pathname === "/accounts" || pathname === "/journal-entries") {
      return "This view is restricted to Accountant and Administrator roles. General ledger writes and account modifications require writing credentials.";
    }
    if (pathname === "/invoices") {
      return "This view is restricted to Auditor and Administrator roles. Compliance audits, OCR file uploads, and accounts payable postings require auditing claims.";
    }
    return "This section is restricted.";
  };

  const isAuthorized = () => {
    if (!user) return false;
    const role = user.role.toLowerCase();
    if (role === "admin") return true; // Admin overrides all checks

    if (pathname === "/accounts" || pathname === "/journal-entries") {
      return role === "accountant";
    }
    if (pathname === "/invoices") {
      return role === "auditor";
    }
    return true; // Dashboard is open to all roles
  };

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-bg-main text-text-sub">
        <div className="flex flex-col items-center gap-3">
          <div className="h-8 w-8 animate-spin rounded-full border-2 border-brand border-t-transparent"></div>
          <span className="text-sm font-medium tracking-wider">Loading BalanceFlow Session...</span>
        </div>
      </div>
    );
  }

  const navItems = [
    { name: "Dashboard", path: "/", icon: LayoutDashboard },
    { name: "Chart of Accounts", path: "/accounts", icon: BookOpen },
    { name: "Journal Entries", path: "/journal-entries", icon: FileSpreadsheet },
    { name: "Invoices & Auditing", path: "/invoices", icon: ScanEye },
  ];

  const authorized = isAuthorized();

  return (
    <div className="grid grid-cols-[260px_1fr] min-h-screen bg-bg-main text-text-main">
      
      {/* Sidebar (TailAdmin / shadcn/ui style) */}
      <aside className="flex flex-col bg-bg-sidebar border-r border-border-main p-6">
        <div className="flex items-center justify-between gap-3 mb-10">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-brand to-emerald-600 text-white font-bold text-lg shadow-lg shadow-brand/20">
              Δ
            </div>
            <span className="text-xl font-bold bg-gradient-to-r from-text-main to-brand bg-clip-text text-transparent">
              BalanceFlow
            </span>
          </div>
          
          {/* Theme Toggler Button */}
          <button
            onClick={toggleTheme}
            className="p-2 bg-bg-main/40 hover:bg-bg-main border border-border-main rounded-lg text-text-sub hover:text-text-main cursor-pointer"
            aria-label="Toggle visual theme"
          >
            {theme === "dark" ? <Sun className="h-4 w-4 text-amber-400" /> : <Moon className="h-4 w-4 text-indigo-500" />}
          </button>
        </div>

        <nav className="flex-1">
          <ul className="flex flex-col gap-2 list-none">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname === item.path;
              // Check navigation item role restriction
              const userRole = user?.role.toLowerCase();
              const isRestrictedNav = 
                (userRole !== "admin") && (
                  ((item.path === "/accounts" || item.path === "/journal-entries") && userRole !== "accountant") ||
                  (item.path === "/invoices" && userRole !== "auditor")
                );

              return (
                <li key={item.path} className={isRestrictedNav ? "opacity-30" : ""}>
                  <Link
                    href={item.path}
                    className={`flex items-center gap-3 px-4 py-3 rounded-lg text-[14px] font-medium transition-all duration-200 border ${
                      isActive
                        ? "bg-brand/10 border-brand/35 text-brand font-semibold shadow-inner"
                        : "border-transparent text-text-sub hover:bg-bg-main/60 hover:text-text-main"
                    }`}
                  >
                    <Icon className={`h-4 w-4 ${isActive ? "text-brand" : "text-text-sub"}`} />
                    <span>{item.name}</span>
                    {isRestrictedNav && (
                      <span className="ml-auto text-[8px] bg-border-main text-text-sub px-1.5 py-0.5 rounded border border-border-main font-bold uppercase tracking-wide">
                        Locked
                      </span>
                    )}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>

        {/* User Session Widget */}
        {user && (
          <div className="mt-auto p-4 bg-bg-main border border-border-main rounded-xl flex flex-col gap-3">
            <div className="flex items-center gap-2">
              <Sparkles className="h-3.5 w-3.5 text-brand" />
              <div className="text-[11px] font-bold text-brand uppercase tracking-widest">
                {user.role}
              </div>
            </div>
            <div>
              <div className="text-sm font-semibold text-text-main truncate">{user.username}</div>
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center justify-center gap-2 w-full py-2 bg-rose-500/10 hover:bg-rose-500/20 text-rose-500 hover:text-rose-400 border border-rose-500/20 rounded-lg text-xs font-semibold transition-all duration-200 cursor-pointer"
            >
              <LogOut className="h-3.5 w-3.5" />
              <span>Log Out</span>
            </button>
          </div>
        )}
      </aside>

      {/* Main Content Area */}
      <main className="flex flex-col p-8 md:p-12 overflow-y-auto max-h-screen">
        {authorized ? (
          children
        ) : (
          /* Premium Access Denied / Segment Locked Page (shadcn/ui style alert) */
          <div className="flex flex-col items-center justify-center flex-1 text-center py-20 animate-in fade-in duration-200">
            <div className="h-16 w-16 bg-rose-500/10 border border-rose-500/25 rounded-2xl flex items-center justify-center text-rose-500 mb-6 shadow-lg shadow-rose-500/5">
              <ShieldAlert className="h-8 w-8 animate-bounce" />
            </div>
            
            <h2 className="text-2xl font-bold tracking-tight text-text-main mb-2">
              Access Restriction Enforced
            </h2>
            <p className="text-sm text-text-sub max-w-[500px] leading-relaxed mb-8">
              {getRequiredRoleMessage()}
            </p>

            <div className="flex gap-4">
              <Link
                href="/"
                className="flex items-center gap-2 px-5 py-2.5 bg-brand hover:bg-brand-hover text-white text-sm font-semibold rounded-xl transition-all duration-200 shadow-lg shadow-brand/10 cursor-pointer"
              >
                <ArrowLeft className="h-4 w-4" />
                <span>Return to Dashboard</span>
              </Link>
              
              <button
                onClick={handleLogout}
                className="px-5 py-2.5 bg-bg-card border border-border-main text-text-main hover:bg-bg-main/60 text-sm font-semibold rounded-xl transition-all duration-200 cursor-pointer"
              >
                Switch Account Role
              </button>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
