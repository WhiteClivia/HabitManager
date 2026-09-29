using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager.Contracts;

public sealed record CompleteHabitRequest(
    Guid HabitId,
    DateOnly Date);