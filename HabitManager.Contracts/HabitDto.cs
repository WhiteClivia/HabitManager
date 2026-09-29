using System;
using System.Collections.Generic;
using System.Text;

namespace HabitManager.Contracts;

public sealed record HabitDto(
    Guid Id,
    string UserId,
    string Name,
    string Description,
    string Status,
    string Schedule,
    DateTime CreatedAt,
    DateTime UpdatedAt);
