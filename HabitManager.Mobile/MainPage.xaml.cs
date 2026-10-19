
using HabitManager.Domain.Enums;
using HabitManager.Mobile.Services;
using Microsoft.Maui.Layouts;

namespace HabitManager.Mobile;

public partial class MainPage : ContentPage
{
    private readonly LocalHabitStorage _storage = new();
    private List<LocalHabit> _habits = [];
    private bool _isLoading;

    public MainPage()
    {
        InitializeComponent();

        UserPicker.ItemsSource = new List<string>
        {
            "demo-user-1",
            "demo-user-2"
        };

        UserPicker.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            _habits = await _storage.GetHabitsAsync();
            RefreshHabits();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnUserChanged(object? sender, EventArgs e)
    {
        RefreshHabits();
    }

    private void RefreshHabits()
    {
        HabitsContainer.Children.Clear();

        if (UserPicker.SelectedItem is not string userId)
            return;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var userHabits = _habits
            .Where(habit => habit.UserId == userId && !habit.IsArchived)
            .OrderBy(habit => habit.Name)
            .ToList();

        var completedCount = userHabits.Count(habit =>
            habit.IsCompletedOn(today));

        ProgressLabel.Text = $"{completedCount}/{userHabits.Count} completed";

        if (userHabits.Count == 0)
        {
            HabitsContainer.Children.Add(new Label
            {
                Text = "No active habits yet. Add your first habit!",
                TextColor = Color.FromArgb("#73788C"),
                Margin = new Thickness(4, 12)
            });

            return;
        }

        foreach (var habit in userHabits)
        {
            HabitsContainer.Children.Add(CreateHabitCard(habit, today));
        }
    }

    private View CreateHabitCard(LocalHabit habit, DateOnly today)
    {
        var isCompleted = habit.IsCompletedOn(today);

        var title = new Label
        {
            Text = habit.Name,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#20243A")
        };

        var description = new Label
        {
            Text = string.IsNullOrWhiteSpace(habit.Description)
                ? "No description"
                : habit.Description,
            FontSize = 14,
            TextColor = Color.FromArgb("#73788C")
        };

        var schedule = new Label
        {
            Text = GetScheduleText(habit),
            FontSize = 12,
            TextColor = Color.FromArgb("#73788C")
        };

        var status = new Label
        {
            Text = isCompleted ? "Completed today" : "Not completed today",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = isCompleted
                ? Color.FromArgb("#21875A")
                : Color.FromArgb("#B36B18")
        };

        var completeButton = new Button
        {
            Text = isCompleted ? "Completed" : "Mark complete",
            IsEnabled = !isCompleted,
            BackgroundColor = Color.FromArgb("#5965D8"),
            TextColor = Colors.White,
            CornerRadius = 10,
            FontSize = 13,
            Padding = new Thickness(10, 6)
        };

        completeButton.Clicked += async (_, _) =>
        {
            if (isCompleted)
                return;

            if (!IsScheduledForToday(habit, today))
            {
                await DisplayAlert(
                    "Not scheduled",
                    "This habit is not scheduled for today.",
                    "OK");
                return;
            }

            habit.CompletedDates.Add(today);
            habit.UpdatedAt = DateTime.UtcNow;

            await SaveAndRefreshAsync();
        };

        var detailsButton = new Button
        {
            Text = "Details",
            BackgroundColor = Color.FromArgb("#E8EAFB"),
            TextColor = Color.FromArgb("#3944A8"),
            CornerRadius = 10,
            FontSize = 13,
            Padding = new Thickness(10, 6)
        };

        detailsButton.Clicked += async (_, _) =>
        {
            var completionHistory = habit.CompletedDates.Count == 0
                ? "No completions yet."
                : string.Join(
                    Environment.NewLine,
                    habit.CompletedDates
                        .OrderByDescending(date => date)
                        .Select(date => date.ToString("dd MMM yyyy")));

            await DisplayAlert(
                habit.Name,
                $"{habit.Description}\n\n" +
                $"Schedule: {GetScheduleText(habit)}\n" +
                $"Status: Active\n" +
                $"Created: {habit.CreatedAt:g}\n\n" +
                $"Completion history:\n{completionHistory}",
                "Close");
        };

        var editButton = new Button
        {
            Text = "Edit",
            BackgroundColor = Color.FromArgb("#E8EAFB"),
            TextColor = Color.FromArgb("#3944A8"),
            CornerRadius = 10,
            FontSize = 13,
            Padding = new Thickness(10, 6)
        };

        editButton.Clicked += async (_, _) =>
        {
            var newName = await DisplayPromptAsync(
                "Edit habit",
                "Enter a habit name:",
                initialValue: habit.Name);

            if (newName is null)
                return;

            if (string.IsNullOrWhiteSpace(newName) ||
                newName.Trim().Length > 100)
            {
                await DisplayAlert(
                    "Invalid name",
                    "The name must contain between 1 and 100 characters.",
                    "OK");
                return;
            }

            var newDescription = await DisplayPromptAsync(
                "Edit habit",
                "Enter a description:",
                initialValue: habit.Description);

            if (newDescription is null)
                return;

            if (newDescription.Length > 1000)
            {
                await DisplayAlert(
                    "Invalid description",
                    "The description cannot exceed 1000 characters.",
                    "OK");
                return;
            }

            var schedule = await ChooseScheduleAsync();

            if (schedule is null)
                return;

            habit.Name = newName.Trim();
            habit.Description = newDescription.Trim();
            habit.RepeatType = schedule.Value.RepeatType;
            habit.DaysOfWeek = schedule.Value.DaysOfWeek;
            habit.UpdatedAt = DateTime.UtcNow;

            await SaveAndRefreshAsync();
        };

        var archiveButton = new Button
        {
            Text = "Archive",
            BackgroundColor = Color.FromArgb("#FBE9E7"),
            TextColor = Color.FromArgb("#A33A32"),
            CornerRadius = 10,
            FontSize = 13,
            Padding = new Thickness(10, 6)
        };

        archiveButton.Clicked += async (_, _) =>
        {
            var confirm = await DisplayAlert(
                "Archive habit",
                $"Are you sure you want to archive \"{habit.Name}\"?",
                "Archive",
                "Cancel");

            if (!confirm)
                return;

            habit.IsArchived = true;
            habit.UpdatedAt = DateTime.UtcNow;

            await SaveAndRefreshAsync();
        };

        var buttons = new FlexLayout
        {
            Wrap = FlexWrap.Wrap,
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustify.Start,
            AlignItems = FlexAlignItems.Center,
            Children =
            {
                completeButton,
                detailsButton,
                editButton,
                archiveButton
            }
        };

        var content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                title,
                description,
                schedule,
                status,
                buttons
            }
        };

        return new Border
        {
            Padding = 16,
            Stroke = Color.FromArgb("#E5E7F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 16
            },
            Content = content
        };
    }

    private static string GetScheduleText(LocalHabit habit)
    {
        if (habit.RepeatType == "Daily")
            return "Every day";

        if (habit.RepeatType == "Weekly")
        {
            var days = string.Join(", ", habit.DaysOfWeek);
            return $"Weekly: {days}";
        }

        return habit.RepeatType;
    }

    private static bool IsScheduledForToday(
        LocalHabit habit,
        DateOnly today)
    {
        if (habit.RepeatType == "Daily")
            return true;

        if (habit.RepeatType == "Weekly")
            return habit.DaysOfWeek.Contains(today.DayOfWeek.ToString());

        return false;
    }


    private async Task<(string RepeatType, List<string> DaysOfWeek)?>
        ChooseScheduleAsync()
    {
        var choice = await DisplayActionSheet(
            "Choose a schedule",
            "Cancel",
            null,
            "Every day",
            "Selected days");

        if (choice == "Cancel" || choice is null)
            return null;

        if (choice == "Every day")
            return ("Daily", []);

        if (choice != "Selected days")
            return null;

        var selectedDays = new List<string>();

        var weekdays = Enum.GetValues<DayOfWeek>();

        foreach (var day in weekdays)
        {
            var answer = await DisplayActionSheet(
                $"Include {day}?",
                "Cancel",
                null,
                "Yes",
                "No");

            if (answer == "Cancel" || answer is null)
                return null;

            if (answer == "Yes")
                selectedDays.Add(day.ToString());
        }

        if (selectedDays.Count == 0)
        {
            await DisplayAlert(
                "Invalid schedule",
                "Select at least one day of the week.",
                "OK");

            return null;
        }

        return ("Weekly", selectedDays);
    }


    private async Task SaveAndRefreshAsync()
    {
        try
        {
            await _storage.SaveHabitsAsync(_habits);
            RefreshHabits();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save error", ex.Message, "OK");
        }
    }

    private async void OnAddHabitClicked(object? sender, EventArgs e)
    {
        if (UserPicker.SelectedItem is not string userId)
            return;

        var name = await DisplayPromptAsync(
            "New habit",
            "Enter a habit name:");

        if (name is null)
            return;

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
        {
            await DisplayAlert(
                "Invalid name",
                "The name must contain between 1 and 100 characters.",
                "OK");
            return;
        }

        var description = await DisplayPromptAsync(
            "New habit",
            "Enter a description:");

        if (description is null)
            return;

        if (description.Length > 1000)
        {
            await DisplayAlert(
                "Invalid description",
                "The description cannot exceed 1000 characters.",
                "OK");
            return;
        }

        var schedule = await ChooseScheduleAsync();

        if (schedule is null)
            return;

        var habit = new LocalHabit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Description = description.Trim(),
            IsArchived = false,
            RepeatType = schedule.Value.RepeatType,
            DaysOfWeek = schedule.Value.DaysOfWeek,
            CompletedDates = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _habits.Add(habit);
        await SaveAndRefreshAsync();
    }
}
