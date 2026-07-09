"use client";

import { useEffect, useState } from "react";
import { useRouter, usePathname } from "next/navigation";
import { getSessionUser, clearToken } from "../api";
import Link from "next/link";

interface ClientLayoutProps {
  children: React.ReactNode;
}

export default function ClientLayout({ children }: ClientLayoutProps) {
  const router = useRouter();
  const pathname = usePathname();
  const [user, setUser] = useState<{ username: string; role: string } | null>(null);
  const [loading, setLoading] = useState(true);

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

  if (loading) {
    return (
      <div style={{ display: "flex", height: "100vh", alignItems: "center", justifyContent: "center", backgroundColor: "#0b0f19", color: "#9ca3af" }}>
        Loading BalanceFlow Session...
      </div>
    );
  }

  const navItems = [
    { name: "Dashboard", path: "/", icon: "📊" },
    { name: "Chart of Accounts", path: "/accounts", icon: "📖" },
    { name: "Journal Entries", path: "/journal-entries", icon: "📝" },
    { name: "Invoices & Auditing", path: "/invoices", icon: "🔍" },
  ];

  return (
    <div className="app-container">
      {/* Left Sidebar */}
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">Δ</div>
          <span className="brand-name">BalanceFlow</span>
        </div>

        <nav>
          <ul className="nav-list">
            {navItems.map((item) => {
              const isActive = pathname === item.path;
              return (
                <li key={item.path} className={`nav-item ${isActive ? "active" : ""}`}>
                  <Link href={item.path}>
                    <span>{item.icon}</span>
                    <span>{item.name}</span>
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>

        {/* User Session Widget */}
        {user && (
          <div className="user-widget">
            <div className="user-role">{user.role}</div>
            <div style={{ fontSize: "14px", fontWeight: "600", color: "#f3f4f6" }}>{user.username}</div>
            <button onClick={handleLogout} className="user-logout">
              Log Out
            </button>
          </div>
        )}
      </aside>

      {/* Main Content Area */}
      <main className="main-content">{children}</main>
    </div>
  );
}
