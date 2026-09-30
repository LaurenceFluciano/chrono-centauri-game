using Godot;

public partial class Ladder : TimeArea
{
    public bool PlayerInside = false;

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
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
            if (player.CurrentLadder == this)
            {
                player.CurrentLadder = null;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (PlayerInside && Input.IsActionPressed("Cima"))
        {
            Player.Instance.CurrentLadder = this; 
        }
    }
}