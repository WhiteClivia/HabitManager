using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager.Contracts;

public sealed record CreateHabitRequest(
    string UserId,
    string Name,
    string Description,
    string RepeatType,
    IReadOnlyCollection<DayOfWeek>? DaysOfWeek);