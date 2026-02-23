"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { api } from "@/lib/api-client";
import type { TrendsResponse } from "@/lib/api-client";
import { formatCurrency, formatPercent } from "@/lib/format";
import StatsCard from "@/components/StatsCard";
import { RevenueChart, CostPercentChart, CustomersChart } from "@/components/MetricChart";
import clsx from "clsx";

export default function TrendsPage() {
  const { data: session } = useSession();
  const [data, setData] = useState<TrendsResponse | null>(null);
  const [days, setDays] = useState(30);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!session?.accessToken) return;
    setLoading(true);
    api.getTrends(days)
      .then(setData)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [session?.accessToken, days]);

  if (loading || !data) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-teal-600 border-t-transparent" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Trends</h1>
          <p className="text-gray-500">Revenue, costs, and customer trends over time</p>
        </div>
        <div className="flex gap-2">
          {[30, 60, 90].map((d) => (
            <button
              key={d}
              onClick={() => setDays(d)}
              className={clsx(
                "rounded-lg px-3 py-1.5 text-sm font-medium transition",
                days === d ? "bg-teal-600 text-white" : "bg-white border border-gray-300 text-gray-600 hover:bg-gray-50"
              )}
            >
              {d}d
            </button>
          ))}
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatsCard title="Avg Daily Revenue" value={formatCurrency(data.avgDailyRevenue)} />
        <StatsCard
          title="Avg Food Cost"
          value={formatPercent(data.avgFoodCostPercent)}
          variant={data.avgFoodCostPercent > 35 ? "danger" : "success"}
        />
        <StatsCard
          title="Avg Labour Cost"
          value={formatPercent(data.avgLabourCostPercent)}
          variant={data.avgLabourCostPercent > 32 ? "danger" : "success"}
        />
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-5">
        <h2 className="mb-4 text-lg font-semibold text-gray-900">Revenue</h2>
        <RevenueChart data={data.daily} height={300} />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-gray-200 bg-white p-5">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">Food & Labour Cost %</h2>
          <CostPercentChart data={data.daily} height={280} />
        </div>

        <div className="rounded-xl border border-gray-200 bg-white p-5">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">Daily Customers</h2>
          <CustomersChart data={data.daily} height={280} />
        </div>
      </div>
    </div>
  );
}
