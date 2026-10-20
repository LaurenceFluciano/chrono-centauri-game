using Godot;
using System;


// Acoplamento-01 : A area de trigger conhece o componente player
// Acoplamento-02 : A area de trigger conhece a ação de interação do player
public partial class OnOffTriggerTimeArea : TimeArea
{
    
    public event EventHandler<OnOffEventArgs> OnInteract;

    protected bool PlayerInside = false;

    [Export]
    public OnOffEnum CurrentState = OnOffEnum.Off;

    [Export]
    public int Id = 0; 


    
    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        if (PlayerInside && Input.IsActionJustPressed("Interagir"))
        {
            Toggle();
        }
    }


    public virtual void Toggle()
    {
        /*
        *
        * Component Contrller handles with Toggle Logic
        *
        */

        OnInteract?.Invoke(this, new OnOffEventArgs(Id, CurrentState));
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