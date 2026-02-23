"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { api } from "@/lib/api-client";
import type { DashboardResponse, WeeklyBriefResponse } from "@/lib/api-client";
import { formatCurrency, formatCurrencyExact, formatPercent } from "@/lib/format";
import StatsCard from "@/components/StatsCard";
import AlertBanner from "@/components/AlertBanner";
import { RevenueChart } from "@/components/MetricChart";
import Link from "next/link";

export default function DashboardPage() {
  const { data: session } = useSession();
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null);
  const [brief, setBrief] = useState<WeeklyBriefResponse | null>(null);
  const [revenueData, setRevenueData] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!session?.accessToken) return;

    async function load() {
      try {
        const [dashData, trendData] = await Promise.all([
          api.getDashboard(),
          api.getTrends(14),
        ]);
        setDashboard(dashData);
        setRevenueData(trendData.daily);

        try {
          const briefData = await api.getWeeklyBrief();
          setBrief(briefData);
        } catch {
          // No brief available yet
        }
      } catch (err) {
        console.error("Dashboard load error:", err);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [session?.accessToken]);

  if (loading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-teal-600 border-t-transparent" />
      </div>
    );
  }

  if (!dashboard) {
    return <p className="text-gray-500">Failed to load dashboard data.</p>;
  }

  const { metrics, trends, alerts } = dashboard;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">
          Welcome back, {session?.user?.name?.split(" ")[0]}
        </h1>
        <p className="text-gray-500">
          Here&apos;s how {(session as any)?.cafeName} is performing
        </p>
      </div>

      {/* Alerts */}
      {alerts.length > 0 && (
        <div className="space-y-2">
          {alerts.map((alert, i) => (
            <AlertBanner key={i} alert={alert} />
          ))}
        </div>
      )}

      {/* Stats Grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatsCard
          title="Today's Revenue"
          value={formatCurrency(metrics.todayRevenue)}
          trend={trends.revenueVsLastWeek}
          trendLabel="vs last week"
        />
        <StatsCard
          title="This Week"
          value={formatCurrency(metrics.weekRevenue)}
          trend={trends.revenueVsLastWeek}
          trendLabel="vs last week"
          variant="success"
        />
        <StatsCard
          title="This Month"
          value={formatCurrency(metrics.monthRevenue)}
        />
        <StatsCard
          title="Food Cost"
          value={formatPercent(metrics.foodCostPercent)}
          trend={trends.foodCostVsLastWeek}
          trendLabel="vs last week"
          variant={metrics.foodCostPercent > 35 ? "danger" : metrics.foodCostPercent > 32 ? "warning" : "success"}
        />
        <StatsCard
          title="Labour Cost"
          value={formatPercent(metrics.labourCostPercent)}
          trend={trends.labourCostVsLastWeek}
          trendLabel="vs last week"
          variant={metrics.labourCostPercent > 32 ? "danger" : metrics.labourCostPercent > 30 ? "warning" : "success"}
        />
        <StatsCard
          title="Net Profit Margin"
          value={formatPercent(metrics.netProfitMargin)}
          variant={metrics.netProfitMargin > 25 ? "success" : metrics.netProfitMargin > 15 ? "warning" : "danger"}
        />
      </div>

      {/* Revenue Chart + Weekly Brief */}
      <div className="grid gap-6 lg:grid-cols-5">
        <div className="rounded-xl border border-gray-200 bg-white p-5 lg:col-span-3">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">Revenue (Last 14 Days)</h2>
          <RevenueChart data={revenueData} height={280} />
        </div>

        <div className="rounded-xl border border-gray-200 bg-white p-5 lg:col-span-2">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">AI Weekly Brief</h2>
          {brief ? (
            <div className="space-y-3 text-sm">
              <p className="text-gray-700">{brief.summary}</p>
              <div>
                <p className="font-medium text-green-700">Highlights</p>
                <p className="whitespace-pre-line text-gray-600">{brief.highlights}</p>
              </div>
              <div>
                <p className="font-medium text-amber-700">Concerns</p>
                <p className="whitespace-pre-line text-gray-600">{brief.concerns}</p>
              </div>
              <div>
                <p className="font-medium text-teal-700">Recommendations</p>
                <p className="whitespace-pre-line text-gray-600">{brief.recommendations}</p>
              </div>
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <p className="text-sm text-gray-500">No weekly brief generated yet.</p>
              <Link
                href="/insights"
                className="mt-3 rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-teal-700"
              >
                Generate with AI
              </Link>
            </div>
          )}
        </div>
      </div>

      {/* Quick Links */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {[
          { href: "/menu", label: "Menu Analysis", desc: "Check item profitability" },
          { href: "/staff", label: "Staff Costs", desc: "Review labour expenses" },
          { href: "/trends", label: "Trends", desc: "30/60/90 day charts" },
          { href: "/insights", label: "AI Insights", desc: "Get profit recommendations" },
        ].map((link) => (
          <Link
            key={link.href}
            href={link.href}
            className="group rounded-xl border border-gray-200 bg-white p-4 transition hover:border-teal-300 hover:shadow-sm"
          >
            <p className="font-medium text-gray-900 group-hover:text-teal-700">{link.label}</p>
            <p className="text-sm text-gray-500">{link.desc}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
