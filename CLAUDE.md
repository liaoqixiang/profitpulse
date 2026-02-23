# ProfitPulse — AI-Powered Profit Advisor for NZ Cafes

## Stack
- **Backend**: .NET 10 (net10.0) + EF Core + PostgreSQL
- **Frontend**: Next.js 15.4 + React 19 + Tailwind v4 + Auth.js v5 + Recharts
- **AI**: Claude API (Anthropic)
- **Database**: PostgreSQL on port 5434 (Docker)

## Running Locally
```bash
docker compose up -d                          # PostgreSQL
cd backend/ProfitPulse.Api && dotnet run      # API on :5175
cd frontend && npm run dev                    # Frontend on :3000
```

## Demo Login
- Email: `demo@profitpulse.co.nz`
- Password: `demo123`

## Key Patterns
- Middleware order: CORS → ExceptionHandler → RateLimit → Auth
- JWT auth with cafeId claim for multi-tenant isolation
- AI insights: data → prompt → Claude → parse JSON → store
- NZ-specific: GST 15% inclusive, NZ min wage, NZ suppliers
