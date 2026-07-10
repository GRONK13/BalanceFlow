"use client";

import { useEffect, useState } from "react";
import ClientLayout from "./components/ClientLayout";
import { api, getToken } from "./api";
import Link from "next/link";
import { 
  TrendingUp, 
  Wallet, 
  Scale, 
  ArrowUpRight, 
  ClipboardCheck, 
  AlertCircle,
  Activity,
  CheckCircle2
} from "lucide-react";

export default function DashboardPage() {
  const [stats, setStats] = useState({
    totalAssets: 0,
    totalLiabilities: 0,
    totalEquity: 0,
    accountsCount: 0,
    journalEntriesCount: 0,
    invoicesCount: 0,
    recentEntries: [] as any[],
    recentInvoices: [] as any[],
    assetTrend: [0, 0, 0, 0, 0, 0] as number[],
    equityTrend: [0, 0, 0, 0, 0, 0] as number[]
  });
  const [loading, setLoading] = useState(true);

  const getMonthLabels = () => {
    const labels = [];
    const today = new Date();
    for (let i = 5; i >= 0; i--) {
      const d = new Date(today.getFullYear(), today.getMonth() - i, 1);
      labels.push(d.toLocaleString("default", { month: "short" }));
    }
    return labels;
  };

  useEffect(() => {
    async function loadDashboardData() {
      if (!getToken()) return;
      try {
        const data = await api.dashboard.getSummary();
        setStats({
          totalAssets: data.totalAssets,
          totalLiabilities: data.totalLiabilities,
          totalEquity: data.totalEquity,
          accountsCount: data.accountsCount,
          journalEntriesCount: data.journalEntriesCount,
          invoicesCount: data.invoicesCount,
          recentEntries: data.recentEntries,
          recentInvoices: data.recentInvoices,
          assetTrend: data.assetTrend,
          equityTrend: data.equityTrend
        });
      } catch (err) {
        console.error("Failed to load dashboard statistics:", err);
      } finally {
        setLoading(false);
      }
    }

    loadDashboardData();
  }, []);

  // Helper to convert trend array to SVG coordinate path
  const getSvgPath = (trend: number[]) => {
    if (!trend || trend.length === 0) return "M 0,170 L 600,170";
    
    const maxVal = Math.max(...trend, 1000);
    const minVal = Math.min(...trend, 0);
    const range = maxVal - minVal || 1;

    const getY = (val: number) => {
      const scaled = ((val - minVal) / range) * 140; // max height inside canvas 
      return 170 - scaled;
    };

    let path = `M 0,${getY(trend[0])}`;
    for (let i = 1; i < trend.length; i++) {
      const x = i * 120;
      const y = getY(trend[i]);
      path += ` L ${x},${y}`;
    }
    return path;
  };

  // Helper to build the closed area for the glow gradient fill
  const getFillPath = (trend: number[]) => {
    const linePath = getSvgPath(trend);
    return `${linePath} L 600,200 L 0,200 Z`;
  };

  const getPointY = (val: number, trend: number[]) => {
    const maxVal = Math.max(...trend, 1000);
    const minVal = Math.min(...trend, 0);
    const range = maxVal - minVal || 1;
    return 170 - ((val - minVal) / range) * 140;
  };

  const formatCurrency = (amount: number) => {
    return amount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  };

  // Dynamic Audit calculations
  const discrepancy = Math.abs(stats.totalAssets - (stats.totalLiabilities + stats.totalEquity));
  const isEquationBalanced = discrepancy < 0.01;
  const complianceScore = isEquationBalanced ? 100 : Math.max(0, 100 - Math.round((discrepancy / (stats.totalAssets || 1)) * 100));

  const monthLabels = getMonthLabels();

  return (
    <ClientLayout>
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-text-main bg-gradient-to-r from-text-main via-text-main to-brand bg-clip-text text-transparent">
            System Overview & Metrics
          </h1>
          <p className="text-sm text-text-sub mt-1">Real-time monitoring of ledger balance sheet compliance and OCR document audits.</p>
        </div>
      </div>

      {/* Financial Stat Cards (TailAdmin Grid Style with dynamic values) */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        
        {/* Assets Card */}
        <div className="relative overflow-hidden bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg shadow-text-main/5 hover:-translate-y-1 hover:border-brand/40 transition-all duration-300 group">
          <div className="absolute left-0 top-0 h-full w-1 bg-brand"></div>
          <div className="flex justify-between items-start">
            <div>
              <span className="text-[11px] font-bold text-text-sub uppercase tracking-widest">Total Assets</span>
              <div className="text-3xl font-extrabold text-text-main mt-2">${formatCurrency(stats.totalAssets)}</div>
            </div>
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand/10 text-brand group-hover:scale-110 transition-all duration-300">
              <TrendingUp className="h-5 w-5" />
            </div>
          </div>
          <div className="text-xs text-brand mt-3 font-medium flex items-center gap-1">
            <span>Dynamic</span> <span className="text-text-sub">calculated from posted accounts</span>
          </div>
        </div>

        {/* Liabilities Card */}
        <div className="relative overflow-hidden bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg shadow-text-main/5 hover:-translate-y-1 hover:border-amber-500/40 transition-all duration-300 group">
          <div className="absolute left-0 top-0 h-full w-1 bg-amber-500"></div>
          <div className="flex justify-between items-start">
            <div>
              <span className="text-[11px] font-bold text-text-sub uppercase tracking-widest">Total Liabilities</span>
              <div className="text-3xl font-extrabold text-text-main mt-2">${formatCurrency(stats.totalLiabilities)}</div>
            </div>
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-amber-500/10 text-amber-400 group-hover:scale-110 transition-all duration-300">
              <Wallet className="h-5 w-5" />
            </div>
          </div>
          <div className="text-xs text-amber-500 mt-3 font-medium flex items-center gap-1">
            <span>Dynamic</span> <span className="text-text-sub">ledger accounts payable</span>
          </div>
        </div>

        {/* Equity Card */}
        <div className="relative overflow-hidden bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg shadow-text-main/5 hover:-translate-y-1 hover:border-brand/40 transition-all duration-300 group">
          <div className="absolute left-0 top-0 h-full w-1 bg-brand"></div>
          <div className="flex justify-between items-start">
            <div>
              <span className="text-[11px] font-bold text-text-sub uppercase tracking-widest">Total Owner Equity</span>
              <div className="text-3xl font-extrabold text-text-main mt-2">${formatCurrency(stats.totalEquity)}</div>
            </div>
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand/10 text-brand group-hover:scale-110 transition-all duration-300">
              <Scale className="h-5 w-5" />
            </div>
          </div>
          <div className="text-xs text-brand mt-3 font-medium flex items-center gap-1">
            <span>Dynamic</span> <span className="text-text-sub">capital + net income summary</span>
          </div>
        </div>
      </div>

      {/* Balanced Ledger Equation Banner (Dynamic values) */}
      <div className="bg-bg-card/60 border border-border-main rounded-2xl p-6 flex flex-col md:flex-row items-center justify-around gap-6 mb-8 shadow-sm">
        <div className="flex flex-col items-center gap-1">
          <span className="text-xs font-bold text-text-sub uppercase tracking-wider">Total Assets</span>
          <span className="text-2xl font-extrabold text-brand">${formatCurrency(stats.totalAssets)}</span>
        </div>
        <div className="text-text-sub/50 font-extralight text-3xl hidden md:block">=</div>
        <div className="flex flex-col items-center gap-1">
          <span className="text-xs font-bold text-text-sub uppercase tracking-wider">Total Liabilities</span>
          <span className="text-2xl font-extrabold text-amber-500">${formatCurrency(stats.totalLiabilities)}</span>
        </div>
        <div className="text-text-sub/50 font-extralight text-3xl hidden md:block">+</div>
        <div className="flex flex-col items-center gap-1">
          <span className="text-xs font-bold text-text-sub uppercase tracking-wider">Total Owner Equity</span>
          <span className="text-2xl font-extrabold text-brand">${formatCurrency(stats.totalEquity)}</span>
        </div>
        <div className={`flex items-center gap-2 px-3.5 py-1.5 rounded-full text-xs font-bold uppercase tracking-wider shrink-0 border ${
          isEquationBalanced 
            ? "bg-brand/10 border-brand/20 text-brand animate-pulse" 
            : "bg-rose-500/10 border-rose-500/20 text-rose-500"
        }`}>
          <Scale className="h-3.5 w-3.5" />
          <span>{isEquationBalanced ? "Accounting Equation Balanced" : "Equation Out of Balance!"}</span>
        </div>
      </div>

      {/* General Ledger Analytics Line Chart */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
        
        {/* SVG Chart Panel (Fully dynamic paths) */}
        <div className="lg:col-span-2 bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg flex flex-col justify-between">
          <div className="flex justify-between items-start mb-6">
            <div>
              <div className="flex items-center gap-2">
                <Activity className="h-4 w-4 text-brand" />
                <h3 className="text-base font-bold text-text-main">General Ledger Activity Trend</h3>
              </div>
              <p className="text-xs text-text-sub mt-1">Monitored net capital flow over the last 6 months.</p>
            </div>
            <div className="flex gap-2">
              <span className="flex items-center gap-1 text-[11px] font-semibold text-brand bg-brand/5 px-2.5 py-1 rounded-md border border-brand/10">
                <span className="h-1.5 w-1.5 rounded-full bg-brand"></span> Assets
              </span>
              <span className="flex items-center gap-1 text-[11px] font-semibold text-text-sub bg-text-sub/5 px-2.5 py-1 rounded-md border border-border-main">
                <span className="h-1.5 w-1.5 rounded-full bg-text-sub"></span> Equity
              </span>
            </div>
          </div>

          <div className="relative h-48 w-full">
            <svg className="w-full h-full" viewBox="0 0 600 200" preserveAspectRatio="none">
              <defs>
                <linearGradient id="chartGlow" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--primary)" stopOpacity="0.25"/>
                  <stop offset="100%" stopColor="var(--primary)" stopOpacity="0"/>
                </linearGradient>
                <linearGradient id="equityGlow" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--muted-foreground)" stopOpacity="0.1"/>
                  <stop offset="100%" stopColor="var(--muted-foreground)" stopOpacity="0"/>
                </linearGradient>
              </defs>
              
              {/* Horizontal helper lines */}
              <line x1="0" y1="40" x2="600" y2="40" stroke="var(--border)" strokeWidth="1" strokeDasharray="4 4" />
              <line x1="0" y1="100" x2="600" y2="100" stroke="var(--border)" strokeWidth="1" strokeDasharray="4 4" />
              <line x1="0" y1="160" x2="600" y2="160" stroke="var(--border)" strokeWidth="1" strokeDasharray="4 4" />
              
              {/* Asset Trend Curves */}
              <path d={getFillPath(stats.assetTrend)} fill="url(#chartGlow)" />
              <path d={getSvgPath(stats.assetTrend)} fill="none" stroke="var(--primary)" strokeWidth="2.5" />
              
              {/* Equity Trend Curves */}
              <path d={getFillPath(stats.equityTrend)} fill="url(#equityGlow)" />
              <path d={getSvgPath(stats.equityTrend)} fill="none" stroke="var(--muted-foreground)" strokeWidth="2" strokeDasharray="2 2" />

              {/* Dynamic data points */}
              {stats.assetTrend.map((val, idx) => (
                <circle 
                  key={`asset-${idx}`} 
                  cx={idx * 120} 
                  cy={getPointY(val, stats.assetTrend)} 
                  r="4.5" 
                  fill="var(--primary)" 
                  stroke="var(--card)" 
                  strokeWidth="2.5" 
                />
              ))}
              {stats.equityTrend.map((val, idx) => (
                <circle 
                  key={`equity-${idx}`} 
                  cx={idx * 120} 
                  cy={getPointY(val, stats.equityTrend)} 
                  r="4" 
                  fill="var(--muted-foreground)" 
                  stroke="var(--card)" 
                  strokeWidth="2" 
                />
              ))}
            </svg>
          </div>

          <div className="flex justify-between items-center text-[10px] text-text-sub font-bold uppercase tracking-widest mt-4">
            {monthLabels.map((lbl, idx) => (
              <span key={idx}>{lbl}</span>
            ))}
          </div>
        </div>

        {/* Double-Entry Compliance Audit Health gauge (Dynamic) */}
        <div className="bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg flex flex-col justify-between">
          <div>
            <h3 className="text-base font-bold text-text-main">Compliance Health Check</h3>
            <p className="text-xs text-text-sub mt-1">Audit assessment indicators and discrepancies.</p>
          </div>

          <div className="flex flex-col items-center justify-center my-4">
            <div className="relative flex items-center justify-center">
              <svg className="w-32 h-32 transform -rotate-90">
                <circle cx="64" cy="64" r="50" stroke="var(--border)" strokeWidth="8" fill="transparent" />
                <circle 
                  cx="64" 
                  cy="64" 
                  r="50" 
                  stroke={isEquationBalanced ? "var(--primary)" : "var(--destructive)"} 
                  strokeWidth="8" 
                  fill="transparent" 
                  strokeDasharray={314.16} 
                  strokeDashoffset={314.16 * (1 - complianceScore / 100)} 
                  strokeLinecap="round" 
                  className="transition-all duration-500" 
                />
              </svg>
              <div className="absolute text-center">
                <div className="text-2xl font-extrabold text-text-main">{complianceScore}%</div>
                <div className="text-[9px] font-bold text-text-sub uppercase tracking-widest">Score</div>
              </div>
            </div>
          </div>

          <div className="flex flex-col gap-2.5">
            <div className="flex justify-between text-xs font-semibold text-text-main">
              <span className="flex items-center gap-1.5"><span className="h-2 w-2 rounded-full bg-brand"></span> Audited Invoices</span>
              <span>{stats.invoicesCount} Registered</span>
            </div>
            <div className="flex justify-between text-xs font-semibold text-text-main">
              <span className="flex items-center gap-1.5"><span className={`h-2 w-2 rounded-full ${isEquationBalanced ? "bg-emerald-500" : "bg-rose-500"}`}></span> Balancing Check</span>
              <span className={isEquationBalanced ? "text-brand" : "text-rose-500"}>
                {isEquationBalanced ? "0.00 Balanced" : `${formatCurrency(discrepancy)} Discrepancy`}
              </span>
            </div>
          </div>
        </div>

      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Recent Ledger Entries (TailAdmin Grid) */}
        <div className="bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg">
          <div className="flex justify-between items-center mb-6">
            <div className="flex items-center gap-2">
              <div className="h-2 w-2 rounded-full bg-brand"></div>
              <h3 className="text-base font-bold text-text-main">Recent Ledger Postings</h3>
            </div>
            <Link href="/journal-entries" className="text-xs font-semibold text-brand hover:text-brand-hover flex items-center gap-1 transition-all duration-200">
              <span>View All ({stats.journalEntriesCount})</span>
              <ArrowUpRight className="h-3.5 w-3.5" />
            </Link>
          </div>
          
          <div className="overflow-x-auto">
            {stats.recentEntries.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-8 text-text-sub text-sm gap-2">
                <AlertCircle className="h-8 w-8 text-text-sub/50" />
                <span>No journal entries posted yet.</span>
              </div>
            ) : (
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border-main">
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Reference</th>
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Date</th>
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-right">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.recentEntries.map((entry) => (
                    <tr key={entry.id} className="border-b border-border-main/40 hover:bg-bg-main/20 transition-all duration-150">
                      <td className="py-4 font-bold text-text-main font-mono text-xs">{entry.referenceNumber}</td>
                      <td className="py-4 text-text-sub">{new Date(entry.transactionDate).toLocaleDateString()}</td>
                      <td className="py-4 text-right">
                        <span className="inline-flex px-2 py-1 rounded-full bg-brand/10 border border-brand/20 text-brand text-[10px] font-bold uppercase tracking-wider">
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

        {/* Recent Compliance Audits */}
        <div className="bg-bg-card border border-border-main rounded-2xl p-6 shadow-lg">
          <div className="flex justify-between items-center mb-6">
            <div className="flex items-center gap-2">
              <div className="h-2 w-2 rounded-full bg-brand"></div>
              <h3 className="text-base font-bold text-text-main">Compliance Audits</h3>
            </div>
            <Link href="/invoices" className="text-xs font-semibold text-brand hover:text-brand-hover flex items-center gap-1 transition-all duration-200">
              <span>Audit Center ({stats.invoicesCount})</span>
              <ArrowUpRight className="h-3.5 w-3.5" />
            </Link>
          </div>

          <div className="overflow-x-auto">
            {stats.recentInvoices.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-8 text-text-sub text-sm gap-2">
                <ClipboardCheck className="h-8 w-8 text-text-sub/50" />
                <span>No invoices audited yet.</span>
              </div>
            ) : (
              <table className="w-full text-left text-sm border-collapse">
                <thead>
                  <tr className="border-b border-border-main">
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Invoice</th>
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub">Vendor</th>
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-center">Compliance</th>
                    <th className="pb-3 text-xs font-bold uppercase tracking-wider text-text-sub text-right">Workflow</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.recentInvoices.map((inv) => (
                    <tr key={inv.id} className="border-b border-border-main/40 hover:bg-bg-main/20 transition-all duration-150">
                      <td className="py-4 font-bold text-text-main font-mono text-xs">{inv.invoiceNumber}</td>
                      <td className="py-4 text-text-sub truncate max-w-[120px]">{inv.vendorName}</td>
                      <td className="py-4 text-center">
                        <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                          inv.auditStatusName === "Passed"
                            ? "bg-brand/10 border-brand/20 text-brand"
                            : inv.auditStatusName === "Failed"
                            ? "bg-rose-500/10 border-rose-500/20 text-rose-500"
                            : "bg-bg-main border-border-main text-text-sub"
                        }`}>
                          {inv.auditStatusName}
                        </span>
                      </td>
                      <td className="py-4 text-right">
                        <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider border ${
                          inv.statusName === "Approved"
                            ? "bg-brand/10 border-brand/20 text-brand"
                            : "bg-bg-main border-border-main text-text-sub"
                        }`}>
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
