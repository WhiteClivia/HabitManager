using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager.Contracts;

public sealed record HabitCompletionDto(
    Guid Id,
    Guid HabitId,
    DateOnly Date);
