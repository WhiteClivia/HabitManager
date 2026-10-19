using HabitManager.Contracts;
using HabitManager.Domain.Entities;
using HabitManager.Domain.Enums;
using HabitManager.Domain.TestData;

Console.WriteLine("========================================");
Console.WriteLine("       HABIT MANAGER - DEMO");
Console.WriteLine("========================================");
Console.WriteLine();

var habits = TestDataGenerator.GenerateHabits();

var today = DateOnly.FromDateTime(DateTime.Today);

// --------------------------------------------------
// 1. Total habit count
// --------------------------------------------------

Console.WriteLine($"Test habit count in total: {habits.Count}");
Console.WriteLine();

// --------------------------------------------------
// 2. Users
// --------------------------------------------------

var userIds = habits
    .Select(habit => habit.UserId)
    .Distinct()
    .Order()
    .ToList();

Console.WriteLine("Users:");

foreach (var userId in userIds)
{
    Console.WriteLine($"  - {userId}");
}

Console.WriteLine();

// --------------------------------------------------
// 3. Each user's habits
// --------------------------------------------------

foreach (var userId in userIds)
{
    Console.WriteLine($"User: {userId}");
    Console.WriteLine(new string('-', 40));

    var userHabits = habits
        .Where(habit => habit.UserId == userId)
        .ToList();

    foreach (var habit in userHabits)
    {
        Console.WriteLine($"  Name: {habit.Name}");
        Console.WriteLine($"  Description: {habit.Description}");
        Console.WriteLine($"  Status: {GetStatusText(habit.Status)}");
        Console.WriteLine($"  Schedule: {habit.Schedule}");
        Console.WriteLine($"  Completed today: {YesNo(habit.IsCompletedOn(today))}");
        Console.WriteLine($"  Total completions: {habit.Completions.Count}");
        Console.WriteLine();
    }

    Console.WriteLine();
}

// --------------------------------------------------
// 4. Checking domain logic
// --------------------------------------------------

Console.WriteLine("========================================");
Console.WriteLine("Checking domain logic");
Console.WriteLine("========================================");
Console.WriteLine();

var testHabit = habits
    .First(habit =>
        habit.UserId == "demo-user-1" &&
        habit.Name == "Morning exercise");

Console.WriteLine($"Habit: {testHabit.Name}");
Console.WriteLine($"Completed today before action: {testHabit.IsCompletedOn(today)}");

Console.WriteLine();
Console.WriteLine("Marking habit as completed...");

testHabit.MarkAsCompleted(today);

Console.WriteLine($"Completed today after action: {testHabit.IsCompletedOn(today)}");

Console.WriteLine();

// --------------------------------------------------
// 5. Checking protection against repeated completion
// --------------------------------------------------

Console.WriteLine("Checking protection against repeated completion...");

try
{
    testHabit.MarkAsCompleted(today);
}
catch (InvalidOperationException exception)
{
    Console.WriteLine($"Error correctly detected: {exception.Message}");
}

Console.WriteLine();

// --------------------------------------------------
// 6. Checking archive
// --------------------------------------------------

var archivedHabit = habits
    .First(habit => habit.Status == HabitStatus.Archived);

Console.WriteLine($"Archived habit: {archivedHabit.Name}");
Console.WriteLine("Trying to mark it as completed...");

try
{
    archivedHabit.MarkAsCompleted(today);
}
catch (InvalidOperationException exception)
{
    Console.WriteLine($"Error correctly detected: {exception.Message}");
}

Console.WriteLine();

// --------------------------------------------------
// 7. Example of a shared data model
// --------------------------------------------------

Console.WriteLine("========================================");
Console.WriteLine("Example of a shared data model");
Console.WriteLine("========================================");
Console.WriteLine();

var dto = ToDto(testHabit);

Console.WriteLine($"DTO:");
Console.WriteLine($"  Id: {dto.Id}");
Console.WriteLine($"  UserId: {dto.UserId}");
Console.WriteLine($"  Name: {dto.Name}");
Console.WriteLine($"  Status: {dto.Status}");
Console.WriteLine($"  Schedule: {dto.Schedule}");

Console.WriteLine();
Console.WriteLine("========================================");
Console.WriteLine("Demo over.");
Console.WriteLine("========================================");

static HabitDto ToDto(Habit habit)
{
    return new HabitDto(
        habit.Id,
        habit.UserId,
        habit.Name,
        habit.Description,
        habit.Status.ToString(),
        habit.Schedule.ToString(),
        habit.CreatedAt,
        habit.UpdatedAt);
}

static string GetStatusText(HabitStatus status)
{
    return status switch
    {
        HabitStatus.Active => "Active",
        HabitStatus.Archived => "Archived",
        _ => "Unknown"
    };
}

static string YesNo(bool value)
{
    return value ? "Yes" : "No";
}