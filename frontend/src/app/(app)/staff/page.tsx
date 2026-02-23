"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { api } from "@/lib/api-client";
import type { StaffCostResponse } from "@/lib/api-client";
import { formatCurrency, formatPercent } from "@/lib/format";
import StatsCard from "@/components/StatsCard";
import { LabourCostChart } from "@/components/MetricChart";
import clsx from "clsx";

export default function StaffPage() {
  const { data: session } = useSession();
  const [data, setData] = useState<StaffCostResponse | null>(null);
  const [days, setDays] = useState(7);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!session?.accessToken) return;
    setLoading(true);
    api.getStaffCosts(days)
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
          <h1 className="text-2xl font-bold text-gray-900">Staff Costs</h1>
          <p className="text-gray-500">Labour breakdown and overtime analysis</p>
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
        <StatsCard title="Total Labour Cost" value={formatCurrency(data.totalLabourCost)} />
        <StatsCard
          title="Labour Cost %"
          value={formatPercent(data.labourCostPercent)}
          variant={data.labourCostPercent > 32 ? "danger" : data.labourCostPercent > 30 ? "warning" : "success"}
        />
        <StatsCard
          title="Overtime Hours"
          value={`${data.totalOvertimeHours}h`}
          variant={data.totalOvertimeHours > 20 ? "warning" : "default"}
        />
        <StatsCard title="Period Revenue" value={formatCurrency(data.totalRevenue)} />
      </div>

      <div className="grid gap-6 lg:grid-cols-5">
        {/* Staff Table */}
        <div className="rounded-xl border border-gray-200 bg-white lg:col-span-3">
          <div className="border-b border-gray-200 px-5 py-3">
            <h2 className="font-semibold text-gray-900">Staff Breakdown</h2>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                <tr>
                  <th className="px-5 py-3">Name</th>
                  <th className="px-5 py-3">Role</th>
                  <th className="px-5 py-3 text-right">Rate</th>
                  <th className="px-5 py-3 text-right">Hours</th>
                  <th className="px-5 py-3 text-right">OT</th>
                  <th className="px-5 py-3 text-right">Cost</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.staff.map((s) => (
                  <tr key={s.id} className={clsx("hover:bg-gray-50", s.hasOvertime && "bg-amber-50/50")}>
                    <td className="px-5 py-3 font-medium text-gray-900">
                      {s.name}
                      {s.hasOvertime && (
                        <span className="ml-2 inline-flex items-center rounded-full bg-amber-100 px-1.5 py-0.5 text-xs font-medium text-amber-700">
                          OT
                        </span>
                      )}
                    </td>
                    <td className="px-5 py-3 text-gray-500">{s.role}</td>
                    <td className="px-5 py-3 text-right">
                      {s.payType === "Salary" ? "Salary" : `$${s.hourlyRate}/hr`}
                    </td>
                    <td className="px-5 py-3 text-right">{s.totalHours > 0 ? `${s.totalHours}h` : "-"}</td>
                    <td className="px-5 py-3 text-right">
                      {s.overtimeHours > 0 ? (
                        <span className="font-medium text-amber-700">{s.overtimeHours}h</span>
                      ) : "-"}
                    </td>
                    <td className="px-5 py-3 text-right font-medium">{formatCurrency(s.periodCost)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Labour Cost Chart */}
        <div className="rounded-xl border border-gray-200 bg-white p-5 lg:col-span-2">
          <h2 className="mb-4 font-semibold text-gray-900">Cost by Staff Member</h2>
          <LabourCostChart data={data.staff} height={350} />
        </div>
      </div>
    </div>
  );
}
