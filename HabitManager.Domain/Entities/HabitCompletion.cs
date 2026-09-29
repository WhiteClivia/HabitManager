using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager.Domain.Entities;

public sealed class HabitCompletion
{
    public Guid Id { get; }

    public Guid HabitId { get; }

    public DateOnly Date { get; }

    public HabitCompletion(Guid habitId, DateOnly date)
    {
        if (habitId == Guid.Empty)
        {
            throw new ArgumentException(
                "The HabitID cannot be empty.",
                nameof(habitId));
        }

        Id = Guid.NewGuid();
        HabitId = habitId;
        Date = date;
    }
}
