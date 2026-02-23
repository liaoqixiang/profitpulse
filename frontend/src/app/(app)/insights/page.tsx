"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { api } from "@/lib/api-client";
import type { AIInsight } from "@/lib/api-client";
import InsightCard from "@/components/InsightCard";
import clsx from "clsx";

const filterTabs = ["All", "New", "Actioned", "Dismissed"] as const;

export default function InsightsPage() {
  const { data: session } = useSession();
  const [insights, setInsights] = useState<AIInsight[]>([]);
  const [filter, setFilter] = useState<string>("All");
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);

  useEffect(() => {
    if (!session?.accessToken) return;
    loadInsights();
  }, [session?.accessToken]);

  async function loadInsights() {
    try {
      setLoading(true);
      const data = await api.getInsights();
      setInsights(data);
    } catch (err) {
      console.error("Failed to load insights:", err);
    } finally {
      setLoading(false);
    }
  }

  async function handleGenerate() {
    setGenerating(true);
    try {
      await api.generateInsights();
      await loadInsights();
    } catch (err) {
      console.error("Failed to generate insights:", err);
      alert("Failed to generate insights. Make sure Claude API key is configured.");
    } finally {
      setGenerating(false);
    }
  }

  async function handleAction(id: string) {
    await api.updateInsightStatus(id, "Actioned");
    setInsights((prev) =>
      prev.map((i) => (i.id === id ? { ...i, status: "Actioned" as const } : i))
    );
  }

  async function handleDismiss(id: string) {
    await api.updateInsightStatus(id, "Dismissed");
    setInsights((prev) =>
      prev.map((i) => (i.id === id ? { ...i, status: "Dismissed" as const } : i))
    );
  }

  const filtered =
    filter === "All" ? insights : insights.filter((i) => i.status === filter);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">AI Insights</h1>
          <p className="text-gray-500">AI-powered profit optimization recommendations</p>
        </div>
        <button
          onClick={handleGenerate}
          disabled={generating}
          className="flex items-center gap-2 rounded-lg bg-teal-600 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-teal-700 disabled:opacity-50"
        >
          {generating ? (
            <>
              <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
              Generating...
            </>
          ) : (
            <>
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
              </svg>
              Generate Insights
            </>
          )}
        </button>
      </div>

      {/* Filter Tabs */}
      <div className="flex gap-2 border-b border-gray-200 pb-2">
        {filterTabs.map((tab) => {
          const count =
            tab === "All"
              ? insights.length
              : insights.filter((i) => i.status === tab).length;
          return (
            <button
              key={tab}
              onClick={() => setFilter(tab)}
              className={clsx(
                "rounded-lg px-3 py-1.5 text-sm font-medium transition",
                filter === tab
                  ? "bg-teal-100 text-teal-700"
                  : "text-gray-500 hover:text-gray-700"
              )}
            >
              {tab}
              <span className="ml-1.5 text-xs text-gray-400">({count})</span>
            </button>
          );
        })}
      </div>

      {/* Insights List */}
      {loading ? (
        <div className="flex h-32 items-center justify-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-teal-600 border-t-transparent" />
        </div>
      ) : filtered.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <svg className="mb-4 h-12 w-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
          </svg>
          <p className="text-gray-500">No insights yet. Click &quot;Generate Insights&quot; to get AI recommendations.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {filtered.map((insight) => (
            <InsightCard
              key={insight.id}
              insight={insight}
              onAction={handleAction}
              onDismiss={handleDismiss}
            />
          ))}
        </div>
      )}
    </div>
  );
}
