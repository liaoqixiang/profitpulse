import clsx from "clsx";
import type { AIInsight } from "@/lib/api-client";
import { formatCurrency } from "@/lib/format";

interface InsightCardProps {
  insight: AIInsight;
  onAction?: (id: string) => void;
  onDismiss?: (id: string) => void;
}

const priorityColors = {
  Critical: "border-l-red-500 bg-red-50",
  High: "border-l-amber-500 bg-amber-50",
  Medium: "border-l-teal-500 bg-white",
  Low: "border-l-gray-400 bg-white",
};

const categoryIcons: Record<string, string> = {
  Menu: "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2",
  Staff: "M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z",
  Revenue: "M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z",
  Cost: "M9 14l6-6m-5.5.5h.01m4.99 5h.01M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16l3.5-2 3.5 2 3.5-2 3.5 2z",
  Opportunity: "M13 10V3L4 14h7v7l9-11h-7z",
  Warning: "M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z",
};

export default function InsightCard({ insight, onAction, onDismiss }: InsightCardProps) {
  return (
    <div className={clsx("rounded-xl border border-l-4 p-5", priorityColors[insight.priority])}>
      <div className="flex items-start justify-between">
        <div className="flex items-start gap-3">
          <svg className="mt-0.5 h-5 w-5 shrink-0 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d={categoryIcons[insight.category] || categoryIcons.Revenue} />
          </svg>
          <div>
            <h3 className="font-semibold text-gray-900">{insight.title}</h3>
            <p className="mt-1 text-sm text-gray-600">{insight.summary}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className={clsx(
            "rounded-full px-2 py-0.5 text-xs font-medium",
            insight.priority === "Critical" && "bg-red-100 text-red-700",
            insight.priority === "High" && "bg-amber-100 text-amber-700",
            insight.priority === "Medium" && "bg-teal-100 text-teal-700",
            insight.priority === "Low" && "bg-gray-100 text-gray-600",
          )}>
            {insight.priority}
          </span>
        </div>
      </div>

      {insight.recommendedAction && (
        <div className="mt-3 rounded-lg bg-white/60 p-3 text-sm text-gray-700">
          <span className="font-medium">Recommended: </span>
          {insight.recommendedAction}
        </div>
      )}

      <div className="mt-3 flex items-center justify-between">
        <div className="flex items-center gap-3 text-xs text-gray-500">
          <span>{insight.category}</span>
          {insight.potentialImpact && (
            <span className="font-medium text-teal-700">
              Potential: {formatCurrency(insight.potentialImpact)}/week
            </span>
          )}
        </div>
        {insight.status === "New" && (
          <div className="flex gap-2">
            {onAction && (
              <button
                onClick={() => onAction(insight.id)}
                className="rounded-lg bg-teal-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-teal-700"
              >
                Mark Actioned
              </button>
            )}
            {onDismiss && (
              <button
                onClick={() => onDismiss(insight.id)}
                className="rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-600 transition hover:bg-gray-50"
              >
                Dismiss
              </button>
            )}
          </div>
        )}
        {insight.status !== "New" && (
          <span className={clsx(
            "rounded-full px-2 py-0.5 text-xs font-medium",
            insight.status === "Actioned" ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"
          )}>
            {insight.status}
          </span>
        )}
      </div>
    </div>
  );
}
