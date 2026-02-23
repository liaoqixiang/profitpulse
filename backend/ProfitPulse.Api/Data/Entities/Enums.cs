namespace ProfitPulse.Api.Data.Entities;

public enum MenuCategory
{
    Coffee,
    Breakfast,
    Lunch,
    Cabinet,
    Beverage,
    Dessert
}

public enum StaffRole
{
    Owner,
    Manager,
    HeadChef,
    Chef,
    Barista,
    Waiter,
    KitchenHand
}

public enum PayType
{
    Salary,
    Hourly
}

public enum DayOfWeekType
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

public enum InsightCategory
{
    Menu,
    Staff,
    Revenue,
    Cost,
    Opportunity,
    Warning
}

public enum InsightPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum InsightStatus
{
    New,
    Actioned,
    Dismissed
}
