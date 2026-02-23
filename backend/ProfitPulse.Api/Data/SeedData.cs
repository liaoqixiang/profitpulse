using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data.Entities;

namespace ProfitPulse.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Cafes.AnyAsync()) return;

        var cafeId = Guid.NewGuid();
        var cafe = new Cafe
        {
            Id = cafeId,
            Name = "Flat White & Co",
            Location = "Ponsonby, Auckland",
            GstRate = 0.15m,
            TargetFoodCostPercent = 35m,
            TargetLabourCostPercent = 30m
        };
        db.Cafes.Add(cafe);

        // Demo user
        db.Users.Add(new DemoUser
        {
            Id = Guid.NewGuid(),
            CafeId = cafeId,
            Email = "demo@profitpulse.co.nz",
            Name = "Sam Mitchell",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("demo123")
        });

        // Menu items: (name, category, price, costToMake)
        var menuDefs = new (string Name, MenuCategory Cat, decimal Price, decimal Cost)[]
        {
            // Coffee (high margin)
            ("Flat White", MenuCategory.Coffee, 5.50m, 1.10m),
            ("Long Black", MenuCategory.Coffee, 5.00m, 0.90m),
            ("Cappuccino", MenuCategory.Coffee, 5.50m, 1.15m),
            ("Latte", MenuCategory.Coffee, 5.50m, 1.20m),
            ("Mocha", MenuCategory.Coffee, 6.00m, 1.50m),
            // Breakfast
            ("Big Breakfast", MenuCategory.Breakfast, 25.00m, 8.50m),
            ("Eggs Benedict", MenuCategory.Breakfast, 23.00m, 7.80m),
            ("Avocado on Toast", MenuCategory.Breakfast, 19.00m, 6.50m),
            ("Acai Bowl", MenuCategory.Breakfast, 19.00m, 6.80m),
            // Lunch
            ("Chicken Caesar Wrap", MenuCategory.Lunch, 18.00m, 5.50m),
            ("Beef Burger", MenuCategory.Lunch, 19.00m, 7.20m),
            ("Fish & Chips", MenuCategory.Lunch, 17.00m, 6.50m),
            ("Vegan Buddha Bowl", MenuCategory.Lunch, 18.50m, 5.80m),
            // Cabinet
            ("Banana Bread", MenuCategory.Cabinet, 6.50m, 1.60m),
            ("Blueberry Muffin", MenuCategory.Cabinet, 6.00m, 1.40m),
            ("Scone with Jam", MenuCategory.Cabinet, 6.00m, 2.20m),
            // Deliberately unprofitable
            ("Fresh Orange Juice", MenuCategory.Beverage, 7.00m, 3.90m),
            ("Smoothie Bowl", MenuCategory.Breakfast, 18.00m, 10.60m),
        };

        var menuItems = new List<MenuItem>();
        foreach (var (name, cat, price, cost) in menuDefs)
        {
            var item = new MenuItem
            {
                Id = Guid.NewGuid(),
                CafeId = cafeId,
                Name = name,
                Category = cat,
                Price = price,
                CostToMake = cost
            };
            menuItems.Add(item);
            db.MenuItems.Add(item);
        }

        // Staff
        var staffDefs = new (string Name, StaffRole Role, PayType Pay, decimal Rate, decimal? Salary)[]
        {
            ("Sam Mitchell", StaffRole.Owner, PayType.Salary, 0m, 85000m),
            ("Jordan Taylor", StaffRole.Manager, PayType.Hourly, 32.00m, null),
            ("Aroha Patel", StaffRole.HeadChef, PayType.Hourly, 30.00m, null),
            ("Liam Chen", StaffRole.Chef, PayType.Hourly, 26.00m, null),
            ("Maia Johnson", StaffRole.Barista, PayType.Hourly, 24.50m, null),
            ("Ethan Williams", StaffRole.Barista, PayType.Hourly, 23.00m, null),
            ("Sophie Brown", StaffRole.Waiter, PayType.Hourly, 23.00m, null),
            ("Te Reo Nikau", StaffRole.Waiter, PayType.Hourly, 22.70m, null),
            ("Jake Kumar", StaffRole.KitchenHand, PayType.Hourly, 22.70m, null),
        };

        var staffMembers = new List<StaffMember>();
        foreach (var (name, role, pay, rate, salary) in staffDefs)
        {
            var staff = new StaffMember
            {
                Id = Guid.NewGuid(),
                CafeId = cafeId,
                Name = name,
                Role = role,
                PayType = pay,
                HourlyRate = rate,
                AnnualSalary = salary,
                StartDate = DateTime.UtcNow.AddMonths(-Random.Shared.Next(6, 36))
            };
            staffMembers.Add(staff);
            db.StaffMembers.Add(staff);
        }

        // Generate 90 days of data
        var rng = new Random(42); // deterministic seed for reproducibility
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-89);

        for (var day = 0; day < 90; day++)
        {
            var date = startDate.AddDays(day);
            var dow = date.DayOfWeek;

            // Base revenue varies by day of week
            var baseRevenue = dow switch
            {
                DayOfWeek.Monday => rng.Next(2000, 2800),
                DayOfWeek.Saturday => rng.Next(3800, 5200),
                DayOfWeek.Sunday => rng.Next(3800, 5200),
                _ => rng.Next(2500, 3500)
            };

            // Slight upward trend over 90 days (+15%)
            var trendMultiplier = 1.0 + (day / 90.0) * 0.15;
            // Random variation +-10%
            var variation = 0.9 + rng.NextDouble() * 0.2;
            var revenue = (decimal)(baseRevenue * trendMultiplier * variation);

            // Food cost 30-38% of revenue
            var foodCostPct = 0.30 + rng.NextDouble() * 0.08;
            var foodCost = revenue * (decimal)foodCostPct;

            // Labour cost 28-34% of revenue
            var labourCostPct = 0.28 + rng.NextDouble() * 0.06;
            var labourCost = revenue * (decimal)labourCostPct;

            // Other costs (rent, utilities etc) ~8-12%
            var otherPct = 0.08 + rng.NextDouble() * 0.04;
            var otherCosts = revenue * (decimal)otherPct;

            // Customers = roughly revenue / average spend ($15-20)
            var avgSpend = 15 + rng.NextDouble() * 5;
            var customers = (int)(double)(revenue / (decimal)avgSpend);
            var transactions = (int)(customers * (0.7 + rng.NextDouble() * 0.3));

            db.DailySummaries.Add(new DailySummary
            {
                Id = Guid.NewGuid(),
                CafeId = cafeId,
                Date = date,
                TotalRevenue = Math.Round(revenue, 2),
                FoodCost = Math.Round(foodCost, 2),
                LabourCost = Math.Round(labourCost, 2),
                OtherCosts = Math.Round(otherCosts, 2),
                CustomerCount = customers,
                TransactionCount = transactions
            });

            // Menu item sales for this day
            foreach (var item in menuItems)
            {
                // Coffee sells more, cabinet moderate, meals less
                var baseSales = item.Category switch
                {
                    MenuCategory.Coffee => rng.Next(20, 60),
                    MenuCategory.Breakfast => rng.Next(5, 20),
                    MenuCategory.Lunch => rng.Next(5, 15),
                    MenuCategory.Cabinet => rng.Next(8, 25),
                    MenuCategory.Beverage => rng.Next(3, 12),
                    _ => rng.Next(3, 10)
                };

                // Weekend bump
                if (dow is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    baseSales = (int)(baseSales * 1.4);

                // Monday dip
                if (dow is DayOfWeek.Monday)
                    baseSales = (int)(baseSales * 0.7);

                if (baseSales > 0)
                {
                    db.MenuItemSales.Add(new MenuItemSales
                    {
                        Id = Guid.NewGuid(),
                        MenuItemId = item.Id,
                        Date = date,
                        QuantitySold = baseSales
                    });
                }
            }

            // Staff shifts for this day
            foreach (var staff in staffMembers)
            {
                if (staff.PayType == PayType.Salary) continue; // Owner doesn't log shifts

                // Not everyone works every day
                var worksToday = staff.Role switch
                {
                    StaffRole.Manager => dow != DayOfWeek.Sunday,
                    StaffRole.HeadChef => dow != DayOfWeek.Monday,
                    StaffRole.Chef => dow is not (DayOfWeek.Sunday or DayOfWeek.Monday),
                    StaffRole.Barista => rng.NextDouble() > 0.2, // 80% chance
                    StaffRole.Waiter => rng.NextDouble() > 0.25,
                    StaffRole.KitchenHand => dow is not (DayOfWeek.Monday or DayOfWeek.Tuesday),
                    _ => true
                };

                if (!worksToday) continue;

                // Base hours 6-9, weekends often longer
                var hours = 6.0m + (decimal)(rng.NextDouble() * 3);
                if (dow is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    hours += 1.5m;

                hours = Math.Round(hours * 2) / 2; // round to half hours

                // Overtime if >8 hours
                var overtime = Math.Max(0, hours - 8);
                var regularHours = hours - overtime;
                var cost = (regularHours * staff.HourlyRate) + (overtime * staff.HourlyRate * 1.5m);

                db.StaffShifts.Add(new StaffShift
                {
                    Id = Guid.NewGuid(),
                    StaffMemberId = staff.Id,
                    Date = date,
                    HoursWorked = hours,
                    OvertimeHours = overtime,
                    TotalCost = Math.Round(cost, 2)
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
