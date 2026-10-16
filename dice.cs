using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

Application
    .Create()
    .Run<MainWindow>()
    .Dispose();

internal sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Dice roller (Esc to quit)";

        Label result = new()
        {
            Text = "Press the button to roll",
            X = Pos.Center(),
            Y = 2
        };

        Button rollButton = new()
        {
            Text = "Roll a six-sided die",
            X = Pos.Center(),
            Y = 4
        };

        rollButton.Accepted += (_, _) =>
            result.Text = $"You rolled: {Random.Shared.Next(1, 7)}";

        Button quitButton = new()
        {
            Text = "_Quit",
            X = Pos.Center(),
            Y = 6
        };

        quitButton.Accepted += (_, _) => App!.RequestStop();

        Add(result, rollButton, quitButton);
    }
}
