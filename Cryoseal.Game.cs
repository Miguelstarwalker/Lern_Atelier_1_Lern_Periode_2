using CryoSeal.Core.Characters;
using CryoSeal.Core.Combat;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace CryoSeal.Game.Views;

public sealed class BattleWindow : Window
{
    private readonly CharacterState _vincent;
    private readonly CharacterState _robo;
    private readonly Label _vincentPanel;
    private readonly Label _roboPanel;
    private readonly Label _message;
    private bool _encounterOver;

    public BattleWindow()
    {
        Title = "CryoSeal — First Encounter";
        Width = Dim.Fill();
        Height = Dim.Fill();

        _vincent = new CharacterState(GameSession.Content.GetFirst(CharacterKind.PartyMember));
        _robo = new CharacterState(GameSession.Content.GetFirst(CharacterKind.Enemy));

        var encounterTitle = new Label { Text = "An enemy blocks the frozen passage.", X = Pos.Center(), Y = 1, TextAlignment = Alignment.Center };
        _vincentPanel = new Label { X = 2, Y = 3, Width = 28, Height = 5 };
        _roboPanel = new Label { X = Pos.AnchorEnd(30), Y = 3, Width = 28, Height = 5 };
        _message = new Label { X = 2, Y = 9, Width = Dim.Fill(4), Height = 3, Text = "Vincent steps forward. Choose an action." };

        var frostSeal = new Button { Text = "Frost Seal", X = 2, Y = 13 };
        frostSeal.Accepted += (_, _) => PlayTurn(BattleAction.FrostSeal);
        var strike = new Button { Text = "Strike", X = 18, Y = 13 };
        strike.Accepted += (_, _) => PlayTurn(BattleAction.Strike);
        var quit = new Button { Text = "Quit", X = Pos.AnchorEnd(10), Y = 13 };
        quit.Accepted += (_, _) => App!.RequestStop();

        Add(encounterTitle, _vincentPanel, _roboPanel, _message, frostSeal, strike, quit);
        RefreshPanels();
    }

    private void PlayTurn(BattleAction vincentAction)
    {
        if (_encounterOver) return;

        var vincentTurn = CombatResolver.ResolveTurn(_vincent, _robo, vincentAction);
        if (_robo.IsDefeated)
        {
            Finish($"{vincentTurn.Description}\nRobo is defeated. He may join the party later.");
            return;
        }

        var roboTurn = CombatResolver.ResolveTurn(_robo, _vincent, BattleAction.Strike);
        if (_vincent.IsDefeated)
        {
            Finish($"{vincentTurn.Description}\n{roboTurn.Description}\nVincent was defeated.");
            return;
        }

        _message.Text = $"{vincentTurn.Description}\n{roboTurn.Description}";
        RefreshPanels();
    }

    private void Finish(string text)
    {
        _encounterOver = true;
        _message.Text = text;
        RefreshPanels();
    }

    private void RefreshPanels()
    {
        _vincentPanel.Text = StatusText(_vincent, "PLAYER");
        _roboPanel.Text = StatusText(_robo, "ENEMY");
    }

    private static string StatusText(CharacterState character, string side) =>
        $"[{side}]\n{character.Definition.Name} the {character.Definition.Species}\n" +
        $"HP {character.CurrentHealth}/{character.Definition.MaxHealth}\n{character.Definition.Role} focus";
}
