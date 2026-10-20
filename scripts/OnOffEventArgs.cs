using System;


public enum OnOffEnum
{
    On,
    Off
}

public sealed class OnOffEventArgs : EventArgs
{

    public int Id { get; }
    public OnOffEnum State { get; } 

    public OnOffEventArgs(int id, OnOffEnum state)
    {
        Id = id;
        State = state; 
    }
}