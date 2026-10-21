using System;
using Godot;

public class GetItemEventArgs : EventArgs
{
    public Sprite2D ItemSprite { get; }
    public String Name { get; }
    public Node ItemComponent { get; }

    public GetItemEventArgs(Sprite2D itemSprite, String name, Node itemComponent)
    {
        ItemSprite = itemSprite;
        Name = name;
        ItemComponent = itemComponent;
    }
}


public static class ItemEvents
{
    public static event EventHandler<GetItemEventArgs> OnGetItem;

    public static void RaiseGetItem(object sender, GetItemEventArgs args)
    {
        OnGetItem?.Invoke(sender, args);
    }
}