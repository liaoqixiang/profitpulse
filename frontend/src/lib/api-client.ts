import { getSession } from "next-auth/react";

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5175";

async function getAuthHeaders(): Promise<HeadersInit> {
  const session = await getSession();
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };
  if (session?.accessToken) {
    headers["Authorization"] = `Bearer ${session.accessToken}`;
  }
  return headers;
}

async function apiRequest<T>(path: string, options?: RequestInit): Promise<T> {
  const headers = await getAuthHeaders();
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: { ...headers, ...options?.headers },
  });

  if (!res.ok) {
    const error = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(error.error || `API error: ${res.status}`);
  }

  return res.json();
}

export const api = {
  // Auth
  login: (email: string, password: string) =>
    apiRequest<{ token: string; user: { id: string; name: string; email: string; cafeName: string } }>(
      "/api/auth/login",
      { method: "POST", body: JSON.stringify({ email, password }) }
    ),

  // Dashboard
  getDashboard: () => apiRequest<DashboardResponse>("/api/dashboard"),
  getWeeklyBrief: () => apiRequest<WeeklyBriefResponse>("/api/dashboard/brief"),

  // Menu
  getMenuPerformance: (days = 7) =>
    apiRequest<MenuPerformanceResponse>(`/api/menu/performance?days=${days}`),

  // Staff
  getStaffCosts: (days = 7) =>
    apiRequest<StaffCostResponse>(`/api/staff/costs?days=${days}`),

  // Trends
  getTrends: (days = 30) =>
    apiRequest<TrendsResponse>(`/api/trends?days=${days}`),

  // Insights
  generateInsights: () =>
    apiRequest<{ count: number; insights: AIInsight[] }>("/api/insights/generate", { method: "POST" }),
  generateBrief: () =>
    apiRequest<WeeklyBriefResponse>("/api/insights/generate-brief", { method: "POST" }),
  getInsights: (status?: string) =>
    apiRequest<AIInsight[]>(`/api/insights${status ? `?status=${status}` : ""}`),
  updateInsightStatus: (id: string, status: string) =>
    apiRequest<{ status: string }>(`/api/insights/${id}/status`, {
      method: "PUT",
      body: JSON.stringify({ status }),
    }),
};

// Types
export interface DashboardResponse {
  metrics: DashboardMetrics;
  trends: DashboardTrends;
  alerts: DashboardAlert[];
}

export interface DashboardMetrics {
  todayRevenue: number;
  weekRevenue: number;
  monthRevenue: number;
  foodCostPercent: number;
  labourCostPercent: number;
  netProfitMargin: number;
  todayCustomers: number;
  avgTransactionValue: number;
}

export interface DashboardTrends {
  revenueVsLastWeek: number;
  foodCostVsLastWeek: number;
  labourCostVsLastWeek: number;
  customersVsLastWeek: number;
}

export interface DashboardAlert {
  type: string;
  message: string;
  severity: "success" | "warning" | "danger";
}

export interface WeeklyBriefResponse {
  summary: string;
  highlights: string;
  concerns: string;
  recommendations: string;
  weekStarting: string;
}

export interface MenuPerformanceResponse {
  items: MenuItemPerformance[];
  totalRevenue: number;
  averageMargin: number;
  bestPerformer: string;
  worstMargin: string;
  days: number;
}

export interface MenuItemPerformance {
  id: string;
  name: string;
  category: string;
  price: number;
  costToMake: number;
  marginPercent: number;
  totalSold: number;
  revenue: number;
  profit: number;
  revenueShare: number;
}

export interface StaffCostResponse {
  staff: StaffCostBreakdown[];
  totalLabourCost: number;
  labourCostPercent: number;
  totalOvertimeHours: number;
  totalRevenue: number;
  days: number;
}

export interface StaffCostBreakdown {
  id: string;
  name: string;
  role: string;
  payType: string;
  hourlyRate: number;
  totalHours: number;
  overtimeHours: number;
  daysWorked: number;
  periodCost: number;
  hasOvertime: boolean;
  avgHoursPerDay: number;
}

export interface TrendsResponse {
  daily: DailyTrendPoint[];
  weekly: WeeklyTrendPoint[];
  avgDailyRevenue: number;
  avgFoodCostPercent: number;
  avgLabourCostPercent: number;
  days: number;
}

export interface DailyTrendPoint {
  date: string;
  revenue: number;
  foodCostPercent: number;
  labourCostPercent: number;
  netProfit: number;
  customers: number;
  transactions: number;
}

export interface WeeklyTrendPoint {
  weekStart: string;
  revenue: number;
  avgFoodCostPercent: number;
  avgLabourCostPercent: number;
  netProfit: number;
  customers: number;
  daysInWeek: number;
}

export interface AIInsight {
  id: string;
  cafeId: string;
  title: string;
  summary: string;
  detailedAnalysis: string;
  recommendedAction: string | null;
  category: "Menu" | "Staff" | "Revenue" | "Cost" | "Opportunity" | "Warning";
  priority: "Low" | "Medium" | "High" | "Critical";
  status: "New" | "Actioned" | "Dismissed";
  potentialImpact: number | null;
  createdAt: string;
}
