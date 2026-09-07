using Godot;

public partial class TimeEntity : Node2D
{
    [Export] public EraMask AllowedEras { get; set; } = EraMask.AllEras;

    [Export] public Texture2D TexturePast { get; set; }
    [Export] public Texture2D TexturePresent { get; set; }
    [Export] public Texture2D TextureFuture { get; set; }

    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        LevelManager.Instance.OnEraChanged += OnEraChanged;

        OnEraChanged(LevelManager.Instance.CurrentEra);
    }

    public override void _ExitTree()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnEraChanged -= OnEraChanged;
        }
    }

    private void OnEraChanged(Era newEra)
    {
        bool isVisibleInEra = newEra switch
        {
            Era.Past => AllowedEras.HasFlag(EraMask.Past),
            Era.Present => AllowedEras.HasFlag(EraMask.Present),
            Era.Future => AllowedEras.HasFlag(EraMask.Future),
            _ => false
        };

        Visible = isVisibleInEra;
        ProcessMode = isVisibleInEra ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

        if (isVisibleInEra && _sprite != null)
        {
            UpdateTexture(newEra);
        }
    }

    private void UpdateTexture(Era currentEra)
    {
        Texture2D targetTexture = currentEra switch
        {
            Era.Past => TexturePast,
            Era.Present => TexturePresent,
            Era.Future => TextureFuture,
            _ => null
        };

        if (targetTexture != null)
        {
            _sprite.Texture = targetTexture;
        }
    }
}