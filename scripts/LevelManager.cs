using Godot;
using System;

public partial class LevelManager : Node2D
{
    public static LevelManager Instance { get; private set; }

    public event Action<Era> OnEraChanged;

    public Era CurrentEra { get; private set; } = Era.Present;

    [Export] Camera2D Camera;

    [Export] public float TransactionDuration = 2f;

    [Export] public TileMapLayer TilesPast { get; set; }
    [Export] public TileMapLayer TilesPresent { get; set; }
    [Export] public TileMapLayer TilesFuture { get; set; }

    private bool _isTransitioning;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        SwitchToEra(CurrentEra, forceUpdate: true);

        // Mover no futuro para o MainLevel
        Player.Instance.EraChangeRequested += RequestEraChange;


    }

    public void RequestEraChange(Era requestedEra)
    {
        SwitchToEra(requestedEra);
    }

    private void SwitchToEra(Era newEra, bool forceUpdate = false)
    { 
        if (
            (newEra == CurrentEra && !forceUpdate) || 
            (_isTransitioning && !forceUpdate)
            ) return;
        
        _isTransitioning = true;

        float warpDir = (newEra > CurrentEra) ? 1f : -1f;

        CurrentEra = newEra;


        ShakeCamera(Camera);
        TriggerVisualJuice(warpDir);

        // Criar evento e Mover no futuro para o MainLevel

        Player.Instance.IsTransition = _isTransitioning;
        
        var tileTween = CreateTween();
        tileTween.TweenInterval(TransactionDuration * 0.78f);
        tileTween.TweenCallback(Callable.From(() => {
            SwitchTiles(CurrentEra);
            OnEraChanged?.Invoke(CurrentEra);
        }));


        
        
        
        CreateTween().TweenCallback(Callable.From(() => {
            _isTransitioning = false;
            Player.Instance.IsTransition = _isTransitioning;
        })).SetDelay(TransactionDuration);
    }

    
    //
    // !!!! Separar no futuro em algo que é responsavel pela parte responsavel por efeitos de tela !!!!
    //

    [Export] private ColorRect _travelScreen;
    private void TriggerVisualJuice(float direction)
    {
        var travelMaterial = _travelScreen?.Material as ShaderMaterial;
        if (travelMaterial == null) return;

        travelMaterial.SetShaderParameter("direction", direction);

        var tween = CreateTween();

        tween.TweenProperty(travelMaterial, "shader_parameter/travel_intensity", 0.2f, TransactionDuration * 0.3f);
        tween.Parallel().TweenProperty(travelMaterial, "shader_parameter/distortion_intensity", 0.5f, TransactionDuration * 0.3f);

      
        tween.TweenProperty(travelMaterial, "shader_parameter/spark_alpha", 1.0f, TransactionDuration * 0.45f)
             .SetEase(Tween.EaseType.In)
             .SetTrans(Tween.TransitionType.Quad);

       
        tween.TweenInterval(TransactionDuration * 0.15f);

        tween.TweenProperty(travelMaterial, "shader_parameter/spark_alpha", 0.0f, TransactionDuration * 0.2f)
             .SetEase(Tween.EaseType.Out)
             .SetTrans(Tween.TransitionType.Sine);
        
        tween.Parallel().TweenProperty(travelMaterial, "shader_parameter/distortion_intensity", 0.0f, TransactionDuration * 0.2f);
        tween.Parallel().TweenProperty(travelMaterial, "shader_parameter/travel_intensity", 0.0f, TransactionDuration * 0.2f);
    }

    //
    // !!!! Separar no futuro em algo que é responsavel pela camera !!!!
    //
    private async void ShakeCamera(Camera2D camera, float duration = 0.2f, float intensity = 8.0f)
    {
        if (camera == null) return;

        Vector2 originalOffset = camera.Offset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = (float)GD.RandRange(-intensity, intensity);
            float offsetY = (float)GD.RandRange(-intensity, intensity);
            camera.Offset = originalOffset + new Vector2(offsetX, offsetY);

            elapsed += (float)GetProcessDeltaTime();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        camera.Offset = originalOffset;
    }

    // FIM DO CODIGO INTRUSO DE MEXER A CAMERA ------

    private void SwitchTiles(Era era)
    {
        var targetTiles = era switch
        {
            Era.Past    => TilesPast,
            Era.Present => TilesPresent,
            Era.Future  => TilesFuture,
            _           => null
        };

        SetEra(targetTiles);
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