
using System.Text.Json;
using HabitManager.Domain.Entities;
using HabitManager.Domain.Enums;
using HabitManager.Domain.TestData;

namespace HabitManager.Mobile.Services;

public sealed class LocalHabitStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public LocalHabitStorage()
    {
        _filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "habits.json");
    }

    public async Task<List<LocalHabit>> GetHabitsAsync()
    {
        if (!File.Exists(_filePath))
        {
            var initialHabits = CreateInitialHabits();
            await SaveHabitsAsync(initialHabits);
            return initialHabits;
        }

        await using var stream = File.OpenRead(_filePath);

        var habits = await JsonSerializer.DeserializeAsync<List<LocalHabit>>(
            stream,
            _jsonOptions);

        return habits ?? [];
    }

    public async Task SaveHabitsAsync(List<LocalHabit> habits)
    {
        await using var stream = File.Create(_filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            habits,
            _jsonOptions);
    }

    private static List<LocalHabit> CreateInitialHabits()
    {
        return TestDataGenerator.GenerateHabits()
            .Select(habit => new LocalHabit
            {
                Id = habit.Id,
                UserId = habit.UserId,
                Name = habit.Name,
                Description = habit.Description,
                IsArchived = habit.Status == HabitStatus.Archived,
                RepeatType = habit.Schedule.RepeatType.ToString(),
                DaysOfWeek = habit.Schedule.DaysOfWeek
                    .Select(day => day.ToString())
                    .ToList(),
                CompletedDates = habit.Completions
                    .Select(completion => completion.Date)
                    .ToList(),
                CreatedAt = habit.CreatedAt,
                UpdatedAt = habit.UpdatedAt
            })
            .ToList();
    }
}

public sealed class LocalHabit
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "demo-user-1";
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public string RepeatType { get; set; } = "Daily";
    public List<string> DaysOfWeek { get; set; } = [];
    public List<DateOnly> CompletedDates { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsCompletedOn(DateOnly date)
    {
        return CompletedDates.Contains(date);
    }
}
