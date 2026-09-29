using System;
using System.Collections.Generic;
using System.Text;

using HabitManager.Domain.Enums;

namespace HabitManager.Domain.ValueObjects;

public sealed class HabitSchedule
{
    public RepeatType RepeatType { get; }

    public IReadOnlyCollection<DayOfWeek> DaysOfWeek { get; }

    private HabitSchedule(
        RepeatType repeatType,
        IEnumerable<DayOfWeek>? daysOfWeek = null)
    {
        RepeatType = repeatType;
        DaysOfWeek = (daysOfWeek ?? [])
            .Distinct()
            .Order()
            .ToList()
            .AsReadOnly();

        Validate();
    }

    public static HabitSchedule Once()
    {
        return new HabitSchedule(RepeatType.Once);
    }

    public static HabitSchedule Daily()
    {
        return new HabitSchedule(RepeatType.Daily);
    }

    public static HabitSchedule Weekly(params DayOfWeek[] days)
    {
        if (days.Length == 0)
        {
            throw new ArgumentException(
                "For a weekly habit, you must specify at least one day.",
                nameof(days));
        }

        return new HabitSchedule(RepeatType.Weekly, days);
    }

    public bool IsScheduledFor(DateOnly date)
    {
        return RepeatType switch
        {
            RepeatType.Once => false,
            RepeatType.Daily => true,
            RepeatType.Weekly => DaysOfWeek.Contains(date.DayOfWeek),
            _ => false
        };
    }

    private void Validate()
    {
        if (RepeatType != RepeatType.Weekly && DaysOfWeek.Count > 0)
        {
            throw new ArgumentException(
                "Days of the week can only be specified for a weekly schedule.");
        }

        if (RepeatType == RepeatType.Weekly && DaysOfWeek.Count == 0)
        {
            throw new ArgumentException(
                "For a weekly habit, you must specify at least one day.");
        }
    }

    public override string ToString()
    {
        return RepeatType switch
        {
            RepeatType.Once => "Once",
            RepeatType.Daily => "Daily",
            RepeatType.Weekly =>
                $"Weekly: {string.Join(", ", DaysOfWeek)}",
            _ => "Unknown"
        };
    }
}