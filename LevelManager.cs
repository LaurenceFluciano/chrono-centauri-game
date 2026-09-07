using Godot;
using System;


public partial class LevelManager : Node2D
{
	public static LevelManager Instance {get; private set; }

	public event Action<Era> OnEraChanged;

	public Era CurrentEra { get; private set; } = Era.Present;

	// talvez no futuro separar a responsabilidade de mudar de tiles de mudar de sprites

    [Export] public TileMapLayer TilesPast { get; set; }
    [Export] public TileMapLayer TilesPresent { get; set; }
    [Export] public TileMapLayer TilesFuture { get; set; }

	public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        SwitchToEra(CurrentEra);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("Passado")) SwitchToEra(Era.Past);
        else if (Input.IsActionJustPressed("Presente")) SwitchToEra(Era.Present);
        else if (Input.IsActionJustPressed("Futuro")) SwitchToEra(Era.Future);
    }

    private void SwitchToEra(Era newEra)
    {
		if (newEra == CurrentEra) return;

		CurrentEra = newEra;

		SwitchTiles(CurrentEra);

		OnEraChanged?.Invoke(CurrentEra);
    }

	private void SwitchTiles(Era era)
	{
		var targetTiles = era switch
		{
			Era.Past  => TilesPast,
			Era.Present => TilesPresent,
			Era.Future   => TilesFuture,
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
		layer.ProcessMode = ProcessModeEnum.Disabled;
		layer.Enabled = false;
	}


}