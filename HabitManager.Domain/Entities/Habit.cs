using System;
using System.Collections.Generic;
using System.Text;

using HabitManager.Domain.Enums;
using HabitManager.Domain.ValueObjects;

namespace HabitManager.Domain.Entities;

public sealed class Habit
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 1000;

    private readonly List<HabitCompletion> _completions = [];

    public Guid Id { get; }

    public string UserId { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public HabitStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public HabitSchedule Schedule { get; private set; }

    public IReadOnlyCollection<HabitCompletion> Completions =>
        _completions.AsReadOnly();

    public Habit(
        string userId,
        string name,
        string description,
        HabitSchedule schedule)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "The habit must belong to a user.",
                nameof(userId));
        }

        ValidateName(name);
        ValidateDescription(description);

        ArgumentNullException.ThrowIfNull(schedule);

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name.Trim();
        Description = description.Trim();
        Schedule = schedule;

        Status = HabitStatus.Active;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Update(
        string name,
        string description,
        HabitSchedule schedule)
    {
        if (Status == HabitStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived habits cannot be edited.");
        }

        ValidateName(name);
        ValidateDescription(description);

        ArgumentNullException.ThrowIfNull(schedule);

        Name = name.Trim();
        Description = description.Trim();
        Schedule = schedule;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == HabitStatus.Archived)
        {
            return;
        }

        Status = HabitStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (Status == HabitStatus.Active)
        {
            return;
        }

        Status = HabitStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public HabitCompletion MarkAsCompleted(DateOnly date)
    {
        if (Status == HabitStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived habits cannot be marked as completed.");
        }

        if (!Schedule.IsScheduledFor(date))
        {
            throw new InvalidOperationException(
                $"The habit is not scheduled for {date}.");
        }

        if (IsCompletedOn(date))
        {
            throw new InvalidOperationException(
                $"The habit is already marked as completed for {date}.");
        }

        var completion = new HabitCompletion(Id, date);

        _completions.Add(completion);
        UpdatedAt = DateTime.UtcNow;

        return completion;
    }

    public bool IsCompletedOn(DateOnly date)
    {
        return _completions.Any(completion => completion.Date == date);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The habit name cannot be empty.",
                nameof(name));
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"The habit name cannot be longer than {MaxNameLength} characters.",
                nameof(name));
        }
    }

    private static void ValidateDescription(string description)
    {
        if (description is null)
        {
            throw new ArgumentNullException(nameof(description));
        }

        if (description.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"The habit description cannot be longer than {MaxDescriptionLength} characters.",
                nameof(description));
        }
    }
}
