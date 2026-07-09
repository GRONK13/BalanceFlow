import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "BalanceFlow — Micro-Accounting & Auditing Platform",
  description: "Enterprise double-entry ledger auditing and invoice validation system.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
