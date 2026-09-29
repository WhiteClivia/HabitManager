using System;
using System.Collections.Generic;
using System.Text;

using HabitManager.Domain.Entities;
using HabitManager.Domain.Enums;
using HabitManager.Domain.ValueObjects;

namespace HabitManager.Domain.TestData;

public static class TestDataGenerator
{
    public static List<Habit> GenerateHabits()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var habits = new List<Habit>
        {
            // User 1
            new Habit(
                "demo-user-1",
                "Read a book",
                "Read at least 20 pages.",
                HabitSchedule.Daily()),

            new Habit(
                "demo-user-1",
                "Morning exercise",
                "Do a short workout after waking up.",
                HabitSchedule.Daily()),

            new Habit(
                "demo-user-1",
                "Learn C#",
                "Spend time programming.",
                HabitSchedule.Weekly(
                    DayOfWeek.Monday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Friday)),

            // User 2
            new Habit(
                "demo-user-2",
                "Learn English words",
                "Review at least 10 new words.",
                HabitSchedule.Daily()),

            new Habit(
                "demo-user-2",
                "Morning run",
                "A light run outdoors.",
                HabitSchedule.Weekly(
                    DayOfWeek.Tuesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Saturday)),

            // Archived habit
            new Habit(
                "demo-user-2",
                "Old project",
                "This project is no longer in use.",
                HabitSchedule.Daily())
        };

        // Архивируем последнюю привычку.
        habits[^1].Archive();

        // Прошлые выполнения первого пользователя.
        habits[0].MarkAsCompleted(today.AddDays(-2));
        habits[0].MarkAsCompleted(today.AddDays(-1));
        habits[0].MarkAsCompleted(today);

        habits[1].MarkAsCompleted(today.AddDays(-1));

        // Прошлые выполнения второго пользователя.
        habits[3].MarkAsCompleted(today.AddDays(-3));
        habits[3].MarkAsCompleted(today.AddDays(-1));

        return habits;
    }
}
