using CryoSeal.Core.Characters;
using CryoSeal.Core.Combat;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace CryoSeal.Game.Views;

public sealed class BattleWindow : Window
{
    private static readonly Scheme DamageScheme = new(new Terminal.Gui.Drawing.Attribute(new Color("Red"), new Color("Black")));
    private readonly CharacterState _player;
    private readonly CharacterState _enemy;
    private readonly FrameView _playerFrame;
    private readonly FrameView _enemyFrame;
    private readonly Label _playerStatus;
    private readonly Label _enemyStatus;
    private readonly Label _message;
    private bool _encounterOver;

    public BattleWindow()
    {
        Title = "CryoSeal — Battle Simulator";
        Width = Dim.Fill();
        Height = Dim.Fill();

        _player = new CharacterState(GameSession.Content.GetFirst(CharacterKind.PartyMember));
        _enemy = new CharacterState(GameSession.Content.GetFirst(CharacterKind.Enemy));

        Add(new Label
        {
            Text = "An enemy blocks the frozen passage.",
            X = Pos.Center(),
            Y = 1,
            TextAlignment = Alignment.Center
        });

        _playerFrame = CreateStatusFrame(2, 3, "PLAYER", out _playerStatus);
        _enemyFrame = CreateStatusFrame(Pos.AnchorEnd(34), 3, "ENEMY", out _enemyStatus);
        Add(_playerFrame, _enemyFrame);

        var dialogueFrame = new FrameView
        {
            Title = "DIALOGUE",
            X = 2,
            Y = 10,
            Width = Dim.Fill(4),
            Height = 6
        };
        dialogueFrame.Add(new Label
        {
            Text = "Beta dialogue to be added.\nThis frame is reserved for character names and story text.",
            X = 1,
            Y = 1,
            Width = Dim.Fill(2)
        });
        _message = new Label { X = 1, Y = 3, Width = Dim.Fill(2) };
        dialogueFrame.Add(_message);
        Add(dialogueFrame);

        var frostSeal = new Button { Text = "Frost Seal", X = 2, Y = 18 };
        frostSeal.Accepted += (_, _) => PlayTurn(BattleAction.FrostSeal);
        var strike = new Button { Text = "Strike", X = 18, Y = 18 };
        strike.Accepted += (_, _) => PlayTurn(BattleAction.Strike);
        var quit = new Button { Text = "Quit", X = Pos.AnchorEnd(10), Y = 18 };
        quit.Accepted += (_, _) => App!.RequestStop();
        Add(frostSeal, strike, quit);

        _message.Text = "Choose an action.";
        RefreshFrames();
    }

    private static FrameView CreateStatusFrame(Pos x, int y, string title, out Label status)
    {
        var frame = new FrameView { Title = title, X = x, Y = y, Width = 32, Height = 6 };
        status = new Label { X = 1, Y = 1, Width = Dim.Fill(2), Height = 4 };
        frame.Add(status);
        return frame;
    }

    private void PlayTurn(BattleAction action)
    {
        if (_encounterOver) return;

        var playerTurn = CombatResolver.ResolveTurn(_player, _enemy, action);
        _message.Text = playerTurn.Description;
        AnimateDamage(_enemyFrame, isPlayer: false, () =>
        {
            if (_enemy.IsDefeated)
            {
                Finish($"{playerTurn.Description}\n{_enemy.Definition.Name} is defeated.");
                return;
            }

            var enemyTurn = CombatResolver.ResolveTurn(_enemy, _player, BattleAction.Strike);
            _message.Text = $"{playerTurn.Description}\n{enemyTurn.Description}";
            AnimateDamage(_playerFrame, isPlayer: true, () =>
            {
                if (_player.IsDefeated)
                {
                    Finish("You were defeated.");
                    return;
                }

                RefreshFrames();
            });
        });
    }

    private void AnimateDamage(FrameView frame, bool isPlayer, Action afterAnimation)
    {
        var step = 0;
        App!.AddTimeout(TimeSpan.FromMilliseconds(80), () =>
        {
            step++;
            frame.SetScheme(DamageScheme);
            frame.X = isPlayer
                ? (step % 2 == 0 ? 1 : 3)
                : (step % 2 == 0 ? Pos.AnchorEnd(35) : Pos.AnchorEnd(33));

            if (step < 6) return true;

            frame.X = isPlayer ? 2 : Pos.AnchorEnd(34);
            RefreshFrames();
            afterAnimation();
            return false;
        });
    }

    private void Finish(string text)
    {
        _encounterOver = true;
        _message.Text = text;
        RefreshFrames();
    }

    private void RefreshFrames()
    {
        _playerFrame.SetScheme(HealthScheme(_player));
        _enemyFrame.SetScheme(HealthScheme(_enemy));
        _playerStatus.Text = StatusText(_player);
        _enemyStatus.Text = StatusText(_enemy);
    }

    private static Scheme HealthScheme(CharacterState character)
    {
        var healthPercent = (double)character.CurrentHealth / character.Definition.MaxHealth;
        var colour = healthPercent > .66 ? "Lime" : healthPercent > .33 ? "DodgerBlue" : "Purple";
        return new Scheme(new Terminal.Gui.Drawing.Attribute(new Color(colour), new Color("Black")));
    }

    private static string StatusText(CharacterState character) =>
        $"{character.Definition.Name} the {character.Definition.Species}\n" +
        $"HP {character.CurrentHealth}/{character.Definition.MaxHealth}\n" +
        $"{character.Definition.Role} focus";
}
