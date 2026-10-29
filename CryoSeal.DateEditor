using CryoSeal.Core.Characters;
using CryoSeal.Data.Content;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

EditorSession.ContentPath = FindSourceContentFile();
Application.Create().Run<CharacterEditor>().Dispose();

static string FindSourceContentFile()
{
    for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
    {
        var candidate = Path.Combine(folder.FullName, "Content", "characters.json");
        if (File.Exists(candidate) && File.Exists(Path.Combine(folder.FullName, "CryoSeal.DataEditor.csproj")))
        {
            return candidate;
        }
    }

    throw new FileNotFoundException("Could not find the Data Editor character file.");
}

internal static class EditorSession
{
    public static string ContentPath { get; set; } = null!;
}

internal sealed class CharacterEditor : Window
{
    private readonly List<CharacterData> _characters;
    private readonly Label _selected;
    private readonly Label _moves;
    private readonly Label _status;
    private readonly TextField _id;
    private readonly TextField _name;
    private readonly TextField _species;
    private readonly TextField _maxHealth;
    private readonly TextField _attack;
    private readonly TextField _defense;
    private readonly TextField _magic;
    private readonly TextField _magicDefense;
    private readonly TextField _moveName;
    private readonly TextField _movePower;
    private CharacterData _current;

    public CharacterEditor()
    {
        _characters = JsonGameContentLoader.LoadCharacterData(EditorSession.ContentPath);
        _current = _characters[0];
        Title = "CryoSeal Data Editor";
        Width = Dim.Fill();
        Height = Dim.Fill();

        Add(new Label { Text = "CHARACTERS", X = 2, Y = 1 });
        var newHero = new Button { Text = "New hero", X = 2, Y = 3 };
        newHero.Accepted += (_, _) => CreateCharacter(CharacterKind.PartyMember);
        var newEnemy = new Button { Text = "New enemy", X = 2, Y = 5 };
        newEnemy.Accepted += (_, _) => CreateCharacter(CharacterKind.Enemy);
        Add(newHero, newEnemy);

        var choiceY = 8;
        foreach (var character in _characters)
        {
            var characterId = character.Id;
            var choose = new Button { Text = character.Name, X = 2, Y = choiceY++ };
            choose.Accepted += (_, _) => Select(characterId);
            Add(choose);
        }

        _selected = new Label { X = 24, Y = 1, Width = Dim.Fill(26) };
        Add(_selected);
        _id = AddField("Id", 24, 3, 20);
        _name = AddField("Name", 24, 5, 20);
        _species = AddField("Species", 24, 7, 20);
        _maxHealth = AddField("Max HP", 24, 9, 8);
        _attack = AddField("Attack", 36, 9, 8);
        _defense = AddField("Defense", 48, 9, 8);
        _magic = AddField("Magic", 24, 12, 8);
        _magicDefense = AddField("Magic Defense", 36, 12, 8);

        Add(new Label { Text = "MOVES", X = 24, Y = 15 });
        _moves = new Label { X = 24, Y = 16, Width = Dim.Fill(26), Height = 3 };
        Add(_moves);
        _moveName = AddField("Move name", 24, 20, 16);
        _movePower = AddField("Power", 44, 20, 8);
        var addMove = new Button { Text = "Add magic move", X = 24, Y = 23 };
        addMove.Accepted += (_, _) => AddMove(CombatRole.Magic);
        var addPhysicalMove = new Button { Text = "Add physical move", X = 43, Y = 23 };
        addPhysicalMove.Accepted += (_, _) => AddMove(CombatRole.Physical);
        var removeMove = new Button { Text = "Remove last move", X = 24, Y = 25 };
        removeMove.Accepted += (_, _) => RemoveLastMove();
        var save = new Button { Text = "Save all changes", X = 43, Y = 25 };
        save.Accepted += (_, _) => Save();
        var quit = new Button { Text = "Quit", X = Pos.AnchorEnd(10), Y = 25 };
        quit.Accepted += (_, _) => App!.RequestStop();
        _status = new Label { X = 24, Y = 27, Width = Dim.Fill(26), Height = 3 };
        Add(addMove, addPhysicalMove, removeMove, save, quit, _status);
        ShowCurrent();
    }

    private TextField AddField(string label, int x, int y, int width)
    {
        Add(new Label { Text = label, X = x, Y = y });
        var field = new TextField { X = x, Y = y + 1, Width = width };
        Add(field);
        return field;
    }

    private void CreateCharacter(CharacterKind kind)
    {
        if (!SaveCurrentInMemory()) return;
        var number = _characters.Count + 1;
        _current = new CharacterData
        {
            Id = kind == CharacterKind.Enemy ? $"enemy_{number}" : $"hero_{number}",
            Name = kind == CharacterKind.Enemy ? "New Enemy" : "New Hero",
            Species = "Unknown",
            Kind = kind,
            Role = CombatRole.Mixed,
            MaxHealth = 30,
            Attack = 5,
            Defense = 3,
            Magic = 5,
            MagicDefense = 3
        };
        _characters.Add(_current);
        ShowCurrent();
        _status.Text = "New character created. Edit it, add moves, save, then restart to see its menu button.";
    }

    private void Select(string id)
    {
        if (!SaveCurrentInMemory()) return;
        _current = _characters.Single(character => character.Id == id);
        ShowCurrent();
        _status.Text = "";
    }

    private void ShowCurrent()
    {
        _selected.Text = $"EDITING: {_current.Name} — {_current.Kind}, {_current.Role}";
        _id.Text = _current.Id;
        _name.Text = _current.Name;
        _species.Text = _current.Species;
        _maxHealth.Text = _current.MaxHealth.ToString();
        _attack.Text = _current.Attack.ToString();
        _defense.Text = _current.Defense.ToString();
        _magic.Text = _current.Magic.ToString();
        _magicDefense.Text = _current.MagicDefense.ToString();
        ShowMoves();
    }

    private void AddMove(CombatRole role)
    {
        if (!SaveCurrentInMemory()) return;
        if (!int.TryParse(_movePower.Text.ToString(), out var power) || power < 1 || string.IsNullOrWhiteSpace(_moveName.Text.ToString()))
        {
            _status.Text = "Give the move a name and a power of 1 or more.";
            return;
        }

        var name = _moveName.Text.ToString().Trim();
        _current.Moves.Add(new MoveData { Id = $"{_current.Id}_{_current.Moves.Count + 1}", Name = name, Role = role, Power = power });
        _moveName.Text = "";
        _movePower.Text = "";
        ShowMoves();
        _status.Text = "Move added. Choose Save all changes to keep it.";
    }

    private void RemoveLastMove()
    {
        if (!SaveCurrentInMemory() || _current.Moves.Count == 0) return;
        _current.Moves.RemoveAt(_current.Moves.Count - 1);
        ShowMoves();
    }

    private void ShowMoves() => _moves.Text = _current.Moves.Count == 0
        ? "No moves yet. Add one below."
        : string.Join("\n", _current.Moves.Select(move => $"• {move.Name} ({move.Role}, power {move.Power})"));

    private void Save()
    {
        if (!SaveCurrentInMemory()) return;
        try
        {
            JsonGameContentLoader.SaveCharacterData(EditorSession.ContentPath, _characters);
            _status.Text = "Saved to the project character file. You do not need to edit JSON.";
        }
        catch (Exception exception) { _status.Text = $"Could not save: {exception.Message}"; }
    }

    private bool SaveCurrentInMemory()
    {
        if (!TryReadStats(out var hp, out var attack, out var defense, out var magic, out var magicDefense))
        {
            _status.Text = "Stats must be whole numbers; Max HP must be above zero.";
            return false;
        }

        var index = _characters.IndexOf(_current);
        _current = new CharacterData
        {
            Id = _id.Text.ToString().Trim(), Name = _name.Text.ToString().Trim(), Species = _species.Text.ToString().Trim(),
            Kind = _current.Kind, Role = _current.Role, MaxHealth = hp, Attack = attack, Defense = defense,
            Magic = magic, MagicDefense = magicDefense, Moves = _current.Moves
        };
        _characters[index] = _current;
        return true;
    }

    private bool TryReadStats(out int hp, out int attack, out int defense, out int magic, out int magicDefense)
    {
        hp = attack = defense = magic = magicDefense = 0;
        return int.TryParse(_maxHealth.Text.ToString(), out hp) && hp > 0 &&
               int.TryParse(_attack.Text.ToString(), out attack) && attack >= 0 &&
               int.TryParse(_defense.Text.ToString(), out defense) && defense >= 0 &&
               int.TryParse(_magic.Text.ToString(), out magic) && magic >= 0 &&
               int.TryParse(_magicDefense.Text.ToString(), out magicDefense) && magicDefense >= 0;
    }
}
