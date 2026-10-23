using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

Application
    .Create()
    .Run<GuessWindow>()
    .Dispose();

internal sealed class GuessWindow : Window
{
    private int _secretNumber = Random.Shared.Next(1, 101);
    private int _attempts;
    private readonly TextField _guess;
    private readonly Label _message;

    public GuessWindow()
    {
        Title = "Guess the Number (1–100)";

        Add(new Label
        {
            Text = "I picked a number from 1 to 100.",
            X = Pos.Center(),
            Y = 2
        });

        _guess = new TextField
        {
            X = Pos.Center(),
            Y = 5,
            Width = 10,
            Text = ""
        };
        Add(_guess);

        var check = new Button
        {
            Text = "Check guess",
            X = Pos.Center(),
            Y = 7
        };
        check.Accepted += (_, _) => CheckGuess();

        var restart = new Button
        {
            Text = "New number",
            X = Pos.Center(),
            Y = 9
        };
        restart.Accepted += (_, _) => Restart();

        var quit = new Button
        {
            Text = "_Quit",
            X = Pos.Center(),
            Y = 11
        };
        quit.Accepted += (_, _) => App!.RequestStop();

        _message = new Label
        {
            Text = "Type a number, then choose Check guess.",
            X = Pos.Center(),
            Y = 14,
            TextAlignment = Alignment.Center
        };
        Add(_guess, check, restart, quit, _message);
    }

    private void CheckGuess()
    {
        if (!int.TryParse(_guess.Text.ToString(), out var guess) || guess is < 1 or > 100)
        {
            _message.Text = "Please enter a whole number from 1 to 100.";
            return;
        }

        _attempts++;
        _message.Text = guess == _secretNumber
            ? $"You got it! {_secretNumber} was the number. Attempts: {_attempts}."
            : guess < _secretNumber
                ? "Too low. Try a bigger number."
                : "Too high. Try a smaller number.";
    }

    private void Restart()
    {
        _secretNumber = Random.Shared.Next(1, 101);
        _attempts = 0;
        _guess.Text = "";
        _message.Text = "New number chosen. Good luck!";
    }
}
