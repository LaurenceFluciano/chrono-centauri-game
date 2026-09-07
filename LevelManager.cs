using Godot;
using System;


public partial class LevelManager : Node2D
{
    [Export] public TileMapLayer TilesPast { get; set; }
    [Export] public TileMapLayer TilesPresent { get; set; }
    [Export] public TileMapLayer TilesFuture { get; set; }

    public override void _Ready()
    {
        SwitchToEra("Presente");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("Passado")) SwitchToEra("Passado");
        else if (Input.IsActionJustPressed("Presente")) SwitchToEra("Presente");
        else if (Input.IsActionJustPressed("Futuro")) SwitchToEra("Futuro");
    }

    private void SwitchToEra(string era)
    {
		var targetTiles = era switch
		{
			"Passado"  => TilesPast,
			"Presente" => TilesPresent,
			"Futuro"   => TilesFuture,
			_          => null
		};

		if (targetTiles != null)
		{
			SetEra(targetTiles);
		}
    }


	private void SetEra(TileMapLayer activeLayer)
	{
		DisableLayer(TilesPresent);
		DisableLayer(TilesPast);
		DisableLayer(TilesFuture);

		EnableLayer(activeLayer);
	}

	private void EnableLayer(TileMapLayer layer)
	{
		if (layer == null) return;
		layer.Visible = true;
		layer.ProcessMode = ProcessModeEnum.Inherit;
		layer.Enabled = true;
	}

	private void DisableLayer(TileMapLayer layer)
	{
		if (layer == null) return;
		layer.Visible = false;
		layer.ProcessMode = ProcessModeEnum.Disabled; // Desliga a física completamente
		layer.Enabled = false;
	}


}