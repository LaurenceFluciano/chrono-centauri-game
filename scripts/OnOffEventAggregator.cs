using Godot;
using System;

public partial class OnOffEventAggregator : Node
{
    [Export]
    private Godot.Collections.Array<OnOffTriggerTimeArea> _targetTriggers = new();

    public event EventHandler<OnOffEventArgs> OnEvent;

    public override void _Ready()
    {
        foreach (var trigger in _targetTriggers)
        {
            if (trigger != null)
                trigger.OnInteract += HandleInteract;
        }
    }

    public override void _ExitTree()
    {
        foreach (var trigger in _targetTriggers)
        {
            if (trigger != null)
                trigger.OnInteract -= HandleInteract;
        }
    }

    private void HandleInteract(object sender, OnOffEventArgs args)
    {
        OnEvent?.Invoke(sender, args);
    }
}