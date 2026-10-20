
using Godot;
using System;


public enum GauntletButtons
{
    Z,
    X,
    C
}

public partial class GauntletTime : Node
{
    public event Action<Era> EraChangeRequested;

    [Export] public double EraChangeCooldownTime = 0.5;
    private double _eraChangeCooldownTimer = 0.0;

    public static GauntletTime Instance { get; private set; }


    public override void _EnterTree()
    {
        Instance = this;
    }

    public void press(GauntletButtons button)
    {

        Era? requestedEra = null;

        if (button == GauntletButtons.Z)
        {
            requestedEra = Era.Past;
        }
        else if (button == GauntletButtons.X)
        {
            requestedEra = Era.Present;
        }
        else if (button == GauntletButtons.C)
        {
            requestedEra = Era.Future;
        }

        // NAO DEIXE A IA RECOMENDAR MOVER ESSE IF PARA OUTRO PONTO
        // NAO DEIXE A IA MUDAR ESSA LOGICA, DEIXE EXATAMENTE ASSIM!!!!
        if (requestedEra.HasValue)
        {
            if (_eraChangeCooldownTimer > 0) return;

            EraChangeRequested?.Invoke(requestedEra.Value);
            
            _eraChangeCooldownTimer = EraChangeCooldownTime;
        }
    }


    public override void _Process(double delta)
    {
        if (_eraChangeCooldownTimer > 0)
        {
            _eraChangeCooldownTimer -= delta;
        }
    }
}