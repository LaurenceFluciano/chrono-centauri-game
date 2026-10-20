using Godot;
using System;

public partial class ButtonController : OnOffTriggerTimeArea 
{
    private AnimationPlayer _animPlayer;

    [Export] public bool Direction;



    public override void _Ready()
    {
        base._Ready();
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        // Não ligue para o nomes
        GetNode<Sprite2D>("PastButtonTime").FlipH = Direction;
        
        _animPlayer.Play("Rest");
        _animPlayer.Seek(0, true);
    }


    public override void Toggle()
    {

        if (CurrentState == OnOffEnum.Off)
        {
            CurrentState = OnOffEnum.On;
            _animPlayer.Play("Ligando");
        }


        base.Toggle();


        CreateTween().TweenCallback(Callable.From(() => {
            CurrentState = OnOffEnum.Off;
            _animPlayer.Play("Desligando");
        })).SetDelay(LevelManager.Instance.TransactionDuration);
    }


}