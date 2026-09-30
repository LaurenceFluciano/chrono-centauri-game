using Godot;
using System;

public partial class Lever : TimeArea // ou Area2D
{
    private AnimationPlayer _animPlayer;
    private bool IsActivated = false;
    private bool PlayerInside = false; // Guarda se o player está na zona

    public event Action<bool> OnLeverPull;

    public override void _Ready()
    {
        base._Ready();
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
        _animPlayer.Play("RESET");
        _animPlayer.Seek(0, true);

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        // OBSERVE, é resposabilidade da alavanca saber o input de interação?!
        if (PlayerInside && Input.IsActionJustPressed("Interagir"))
        {
            ToggleLever();
        }
    }

    public void ToggleLever()
    {
        if (!IsActivated)
        {
            _animPlayer.Play("Ativar");
            IsActivated = true;
        }
        else
        {
            _animPlayer.Play("Desativar");
            IsActivated = false;
        }

        OnLeverPull?.Invoke(IsActivated);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            PlayerInside = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player player)
        {
            PlayerInside = false;
        }
    }
}