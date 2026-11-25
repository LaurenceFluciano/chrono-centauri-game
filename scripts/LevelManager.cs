using Godot;
using System;

public partial class LevelManager : Node2D
{
    public static LevelManager Instance { get; private set; }

    public event Action<Era> OnEraChanged;

    public Era CurrentEra { get; private set; } = Era.Present;


    [Export] public Era InitialEra = Era.Present;

    [Export] Camera2D Camera;

    [Export] public float TransactionDuration = 2f;

    [ExportGroup("Static Time Scenes")]
    [Export] public Node2D PastScene { get; set; }
    [Export] public Node2D PresentScene { get; set; }
    [Export] public Node2D FutureScene { get; set; }

    private bool _isTransitioning;

    // Sons de efeito

    private AudioStreamPlayer _fowardSound;
    private AudioStreamPlayer _backwardSound;

    // Agregador de evento principal

    [Export] private OnOffEventAggregator _eventAgreggator;

    [Export] private int PresentId = 11;
    [Export] private int PastId = 10;
    [Export] private int FutureId = 12;


    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (_eventAgreggator != null)
            _eventAgreggator.OnEvent -= HandleInteract;

        if (GauntletTime.Instance != null)
            GauntletTime.Instance.EraChangeRequested -= RequestEraChange;

        if (Instance == this)
            Instance = null;
    }

    public override void _Ready()
    {

        _fowardSound = GetNode<AudioStreamPlayer>("FowardTimeEffect");

        _backwardSound = GetNode<AudioStreamPlayer>("BackwardTimeEffect");


        if (_eventAgreggator != null)
        {
            _eventAgreggator.OnEvent += HandleInteract;
        }

        // Mover no futuro para o MainLevel
        GauntletTime.Instance.EraChangeRequested += RequestEraChange;

        CurrentEra = InitialEra;
        SwitchScene(CurrentEra);
    }

    public void HandleInteract(object sender, OnOffEventArgs eventArgs)
    {
        if (eventArgs.State != OnOffEnum.On)
            return;

        if (eventArgs.Id == PastId)
            SwitchToEra(Era.Past);
        else if (eventArgs.Id == PresentId)
            SwitchToEra(Era.Present);
        else if (eventArgs.Id == FutureId)
            SwitchToEra(Era.Future);
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




        if (warpDir > 0f)
            _fowardSound.Play();
        else
            _backwardSound.Play();

    
        
        var tileTween = CreateTween();
        tileTween.TweenInterval(TransactionDuration * 0.78f);
        tileTween.TweenCallback(Callable.From(() => {
            SwitchScene(CurrentEra);
            OnEraChanged?.Invoke(CurrentEra);
        }));


        CreateTween().TweenCallback(Callable.From(() =>
        {
            if (warpDir > 0f)
                _fowardSound.Stop();
            else
                _backwardSound.Stop();
            
        })).SetDelay(TransactionDuration);


        
        
        
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

    private void SwitchScene(Era era)
    {
        SetSceneActive(PastScene, era == Era.Past);
        SetSceneActive(PresentScene, era == Era.Present);
        SetSceneActive(FutureScene, era == Era.Future);
    }

    private void SetSceneActive(Node2D scene, bool active)
    {
        if (scene == null) return;

        scene.Visible = active;

        scene.ProcessMode = active
            ? ProcessModeEnum.Inherit
            : ProcessModeEnum.Disabled;

        SetTileLayersActive(scene, active);
    }

    private void SetTileLayersActive(Node node, bool active)
    {
        if (node is TileMapLayer layer)
        {
            layer.Enabled = active;
        }

        foreach (Node child in node.GetChildren())
        {
            SetTileLayersActive(child, active);
        }
    }


}