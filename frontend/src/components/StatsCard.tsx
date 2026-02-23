import clsx from "clsx";

interface StatsCardProps {
  title: string;
  value: string;
  trend?: number;
  trendLabel?: string;
  icon?: string;
  variant?: "default" | "success" | "warning" | "danger";
}

export default function StatsCard({
  title,
  value,
  trend,
  trendLabel,
  variant = "default",
}: StatsCardProps) {
  const borderColor = {
    default: "border-l-teal-500",
    success: "border-l-green-500",
    warning: "border-l-amber-500",
    danger: "border-l-red-500",
  }[variant];

  return (
    <div className={clsx("rounded-xl border border-gray-200 border-l-4 bg-white p-5", borderColor)}>
      <p className="text-sm font-medium text-gray-500">{title}</p>
      <p className="mt-1 text-2xl font-bold text-gray-900">{value}</p>
      {trend !== undefined && (
        <div className="mt-2 flex items-center gap-1">
          <TrendBadge value={trend} />
          {trendLabel && (
            <span className="text-xs text-gray-500">{trendLabel}</span>
          )}
        </div>
      )}
    </div>
  );
}

export function TrendBadge({ value }: { value: number }) {
  const isPositive = value > 0;
  const isNeutral = value === 0;

  return (
    <span
      className={clsx(
        "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium",
        isNeutral && "bg-gray-100 text-gray-600",
        isPositive && "bg-green-50 text-green-700",
        !isPositive && !isNeutral && "bg-red-50 text-red-700"
      )}
    >
      {isPositive ? "\u2191" : isNeutral ? "\u2192" : "\u2193"}
      {Math.abs(value).toFixed(1)}%
    </span>
  );
}
