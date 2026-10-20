using System;
using Godot;

// Acoplamento-03 : A porta de fim deveria renderizar a tela de fim?
public partial class EndDoor : TimeArea
{
    private AnimationPlayer _animPlayer;
    private bool _isOpen = false;

    [Export] public int InteractId = 0; 

    [Export] private OnOffEventAggregator _eventAgreggator;

    // Podes exportar a cena da UI para arrastá-la no inspetor (ex: LevelClearUI.tscn)
    [Export] private PackedScene _endGameUiScene;

    public override void _Ready()
    {
        base._Ready();

        if (_eventAgreggator != null)
        {
            _eventAgreggator.OnEvent += HandleInteract;
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
        if (_eventAgreggator != null)
        {
            _eventAgreggator.OnEvent -= HandleInteract;
        }
        BodyEntered -= OnBodyEntered;
    }

    private void HandleInteract(object sender, OnOffEventArgs eventArgs)
    {
        GD.Print($"IdInteracao={InteractId} | Evento=Id:{eventArgs.Id};State:{eventArgs.State};");
        if (eventArgs.Id != InteractId)
            return;
        

        if (eventArgs.State == OnOffEnum.On)
        {
            _isOpen = true;
        } else
        {
            _isOpen = false;
        }
        
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