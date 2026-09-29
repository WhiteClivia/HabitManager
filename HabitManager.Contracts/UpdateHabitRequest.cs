using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager.Contracts;

public sealed record UpdateHabitRequest(
    string Name,
    string Description,
    string RepeatType,
    IReadOnlyCollection<DayOfWeek>? DaysOfWeek);