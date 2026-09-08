using Godot;

public partial class Hazard : TimeArea
{
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.Respawn();
        }
    }
}