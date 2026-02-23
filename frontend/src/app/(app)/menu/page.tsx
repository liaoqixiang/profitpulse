"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { api } from "@/lib/api-client";
import type { MenuPerformanceResponse } from "@/lib/api-client";
import { formatCurrency, formatCurrencyExact, formatPercent } from "@/lib/format";
import StatsCard from "@/components/StatsCard";
import clsx from "clsx";

export default function MenuPage() {
  const { data: session } = useSession();
  const [data, setData] = useState<MenuPerformanceResponse | null>(null);
  const [days, setDays] = useState(7);
  const [loading, setLoading] = useState(true);
  const [sortBy, setSortBy] = useState<"revenue" | "margin" | "sold">("revenue");

  useEffect(() => {
    if (!session?.accessToken) return;
    setLoading(true);
    api.getMenuPerformance(days)
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

  const sorted = [...data.items].sort((a, b) => {
    if (sortBy === "revenue") return b.revenue - a.revenue;
    if (sortBy === "margin") return b.marginPercent - a.marginPercent;
    return b.totalSold - a.totalSold;
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Menu Analysis</h1>
          <p className="text-gray-500">Item profitability and performance</p>
        </div>
        <div className="flex gap-2">
          {[7, 14, 30].map((d) => (
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

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatsCard title="Total Revenue" value={formatCurrency(data.totalRevenue)} />
        <StatsCard title="Avg Margin" value={formatPercent(data.averageMargin)} variant={data.averageMargin > 65 ? "success" : "warning"} />
        <StatsCard title="Best Performer" value={data.bestPerformer} variant="success" />
        <StatsCard title="Worst Margin" value={data.worstMargin} variant="danger" />
      </div>

      <div className="rounded-xl border border-gray-200 bg-white">
        <div className="flex items-center justify-between border-b border-gray-200 px-5 py-3">
          <h2 className="font-semibold text-gray-900">Item Performance</h2>
          <div className="flex gap-2">
            {(["revenue", "margin", "sold"] as const).map((s) => (
              <button
                key={s}
                onClick={() => setSortBy(s)}
                className={clsx(
                  "rounded-md px-2.5 py-1 text-xs font-medium transition",
                  sortBy === s ? "bg-teal-100 text-teal-700" : "text-gray-500 hover:text-gray-700"
                )}
              >
                {s === "revenue" ? "Revenue" : s === "margin" ? "Margin" : "Units Sold"}
              </button>
            ))}
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
              <tr>
                <th className="px-5 py-3">Item</th>
                <th className="px-5 py-3">Category</th>
                <th className="px-5 py-3 text-right">Price</th>
                <th className="px-5 py-3 text-right">Cost</th>
                <th className="px-5 py-3 text-right">Margin</th>
                <th className="px-5 py-3 text-right">Units Sold</th>
                <th className="px-5 py-3 text-right">Revenue</th>
                <th className="px-5 py-3 text-right">Profit</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {sorted.map((item) => {
                const rowColor =
                  item.marginPercent >= 65 ? "bg-green-50/50" :
                  item.marginPercent >= 50 ? "" :
                  "bg-red-50/50";

                return (
                  <tr key={item.id} className={clsx("hover:bg-gray-50", rowColor)}>
                    <td className="px-5 py-3 font-medium text-gray-900">{item.name}</td>
                    <td className="px-5 py-3 text-gray-500">{item.category}</td>
                    <td className="px-5 py-3 text-right">{formatCurrencyExact(item.price)}</td>
                    <td className="px-5 py-3 text-right text-gray-500">{formatCurrencyExact(item.costToMake)}</td>
                    <td className="px-5 py-3 text-right">
                      <span className={clsx(
                        "font-medium",
                        item.marginPercent >= 65 ? "text-green-700" :
                        item.marginPercent >= 50 ? "text-amber-700" :
                        "text-red-700"
                      )}>
                        {formatPercent(item.marginPercent)}
                      </span>
                    </td>
                    <td className="px-5 py-3 text-right">{item.totalSold}</td>
                    <td className="px-5 py-3 text-right font-medium">{formatCurrency(item.revenue)}</td>
                    <td className="px-5 py-3 text-right font-medium text-teal-700">{formatCurrency(item.profit)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
