using Godot;

public partial class StartButton : Button
{
    public override void _Ready()
    {
        Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://levels/lvl_1.tscn");
    }
}