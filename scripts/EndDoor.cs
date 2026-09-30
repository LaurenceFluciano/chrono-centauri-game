using System;
using Godot;

public partial class EndDoor : TimeArea
{
    private AnimationPlayer _animPlayer;
    private bool _isOpen = false;

    [Export] private Lever _targetLever;

    // Podes exportar a cena da UI para arrastá-la no inspetor (ex: LevelClearUI.tscn)
    [Export] private PackedScene _endGameUiScene;

    public override void _Ready()
    {
        base._Ready();

        if (_targetLever != null)
        {
            _targetLever.OnLeverPull += HandleLeverPull;
        }

        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animPlayer.Play("RESET");
        _animPlayer.Seek(0, true);

        // Subscrevemos o evento de colisão da área para detetar o player
        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_targetLever != null)
        {
            _targetLever.OnLeverPull -= HandleLeverPull;
        }
        BodyEntered -= OnBodyEntered;
    }

    private void HandleLeverPull(bool isActivated)
    {
        _isOpen = isActivated;
        
        if (_isOpen)
        {
            _animPlayer.Play("Aberto");
        }
        else
        {
            _animPlayer.Play("RESET");
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player && _isOpen)
        {
            ShowEndGameUI();
        }
    }

    private void ShowEndGameUI()
    {
        if (_endGameUiScene != null)
        {
            var uiInstance = _endGameUiScene.Instantiate<CanvasLayer>();
            
            GetTree().Root.AddChild(uiInstance);

            GetTree().Paused = true;
        }
        else
        {
            GD.Print("Fim da fase. Obrigado por jogar!");
        }
    }
}