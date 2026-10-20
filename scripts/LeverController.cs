using Godot;
using System;

public partial class LeverController : OnOffTriggerTimeArea 
{
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        base._Ready();
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
        _animPlayer.Play("RESET");
        _animPlayer.Seek(0, true);
    }


    public override void Toggle()
    {
        if (CurrentState == OnOffEnum.On)
        {
            CurrentState = OnOffEnum.Off;
            _animPlayer.Play("Desativar");
        } else
        {
            CurrentState = OnOffEnum.On;
            _animPlayer.Play("Ativar");
        }


        base.Toggle();
    }


}