
using HabitManager.Domain.Enums;

namespace HabitManager.Mobile.Pages;

public sealed class SchedulePickerPage : ContentPage
{
    private readonly Dictionary<DayOfWeek, CheckBox> _dayCheckBoxes = [];

    public (string RepeatType, List<string> DaysOfWeek)? SelectedSchedule
    {
        get;
        private set;
    }

    public SchedulePickerPage(
        string currentRepeatType,
        List<string> currentDays)
    {
        Title = "Choose days";
        BackgroundColor = Color.FromArgb("#F5F6FA");

        var selectedDays = currentRepeatType == "Weekly"
            ? currentDays
            : [];

        var content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 14
        };

        content.Children.Add(new Label
        {
            Text = "Choose days of the week",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#20243A")
        });

        content.Children.Add(new Label
        {
            Text = "Select one or more days.",
            FontSize = 14,
            TextColor = Color.FromArgb("#73788C")
        });

        var days = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        foreach (var day in days)
        {
            var checkBox = new CheckBox
            {
                IsChecked = selectedDays.Contains(day.ToString()),
                Color = Color.FromArgb("#5965D8"),
                VerticalOptions = LayoutOptions.Center
            };

            _dayCheckBoxes[day] = checkBox;

            var row = new HorizontalStackLayout
            {
                Spacing = 12,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    checkBox,
                    new Label
                    {
                        Text = day.ToString(),
                        FontSize = 16,
                        TextColor = Color.FromArgb("#20243A"),
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            };

            var border = new Border
            {
                Padding = new Thickness(12, 4),
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E5E7F0"),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 10
                },
                Content = row
            };

            content.Children.Add(border);
        }

        var saveButton = new Button
        {
            Text = "Save schedule",
            BackgroundColor = Color.FromArgb("#5965D8"),
            TextColor = Colors.White,
            CornerRadius = 12,
            HeightRequest = 48
        };

        saveButton.Clicked += OnSaveClicked;

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = Color.FromArgb("#E8EAFB"),
            TextColor = Color.FromArgb("#3944A8"),
            CornerRadius = 12,
            HeightRequest = 48
        };

        cancelButton.Clicked += async (_, _) =>
        {
            await Navigation.PopModalAsync();
        };

        content.Children.Add(saveButton);
        content.Children.Add(cancelButton);

        Content = new ScrollView
        {
            Content = content
        };
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var selectedDays = _dayCheckBoxes
            .Where(pair => pair.Value.IsChecked)
            .Select(pair => pair.Key.ToString())
            .ToList();

        if (selectedDays.Count == 0)
        {
            await DisplayAlert(
                "Invalid schedule",
                "Select at least one day of the week.",
                "OK");

            return;
        }

        if (selectedDays.Count == 7)
        {
            SelectedSchedule = ("Daily", []);
        }
        else
        {
            SelectedSchedule = ("Weekly", selectedDays);
        }

        await Navigation.PopModalAsync();
    }
}
