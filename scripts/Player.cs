using Godot;
using System;

public partial class Player : CharacterBody2D
{

    // Controle de estado do player
    public enum PlayerState
    {
        Idle,
        Walking,
        Jumping,
        Falling,
        GettingItem
    }

    private PlayerState currentState = PlayerState.Idle;
    private float direction = 1;

    // Animacao 
    private AnimationPlayer _animPlayer;

    // Desenho / Sprite

    private Sprite2D _sprite;

    private Sprite2D _spriteGettingItem;

    // Coyote config

    private double _coyote = 0.08f;
    private double _coyoteTimer = 0.0;

    // Poderes do player

    [Export] public GauntletTime GauntletTime;



    public static Player Instance { get; private set; }
	[Export] public float Speed = 200.0f;
    [Export] public float JumpVelocity = -350.0f;





	private Vector2 _spawnPosition;

    public bool IsTransition = false;

    public bool IsNearLadder = false;

    // Funcionalidade de subir escadas (acoplado diretamente pq somenteo player faz isso)

    // Temporario
    public Node2D CurrentLadder = null;


	// Pega uma constante da velocidade que foi configurado como 980 px/s² 
	// Para poder ver ela ou alterar:
	// Projeto > Configurações do Projeto > Geral 
	//		Na aba: Física > 2D
	//			No campo: Gravida Padrão
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();


    // Efeitos sonoros do player

    private AudioStreamPlayer _jumpSound;


    // Item que está sendo exibido/pego no momento
    private GetItemEventArgs _pendingItem;

    private const float ItemOffsetY = -24f;


    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        ItemEvents.OnGetItem -= HandleGetItem;
    }

	public override void _Ready()
    {
        var spawnPoint = GetNodeOrNull<Node2D>("../SpawnPoint");

        _jumpSound = GetNode<AudioStreamPlayer>("JumpSound");


        _coyote = 0.08f;

        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite = GetNode<Sprite2D>("PlayerAnimation");
        _spriteGettingItem = GetNode<Sprite2D>("PickItem");

        _spriteGettingItem.Visible = false;

        _animPlayer.Play("idle");

        
        ItemEvents.OnGetItem += HandleGetItem;
        _animPlayer.AnimationFinished += OnAnimationFinished;

        if (spawnPoint != null)
        {
            _spawnPosition = spawnPoint.GlobalPosition;
            GlobalPosition = _spawnPosition;
        }
        else
        {
            _spawnPosition = GlobalPosition;
        }
    }

 

	public void Respawn()
    {
        GlobalPosition = _spawnPosition;
        Velocity = Vector2.Zero;
    }

    public void HandleGetItem(object sender, GetItemEventArgs eventArgs)
    {
        _pendingItem = eventArgs;

        changeState(PlayerState.GettingItem);

        Velocity = Vector2.Zero;
        _animPlayer.SpeedScale = 1;

        _sprite.Visible = false;
        _spriteGettingItem.Visible = true;

        var itemSprite = eventArgs.ItemSprite;
        if (itemSprite != null)
        {
            if (itemSprite.GetParent() != null)
                itemSprite.Reparent(this, false);
            else
                AddChild(itemSprite);

            itemSprite.Position = new Vector2(0, ItemOffsetY);
            itemSprite.Visible = true;
        }

        var component = eventArgs.ItemComponent;
        if (component != null && component.GetParent() != this)
        {
            component.GetParent()?.RemoveChild(component);
            AddChild(component);
        }

        processAnimation();

    }

    private void OnAnimationFinished(StringName animName)
    {
        if (animName != "GettingItem" || _pendingItem == null)
            return;

        if (_pendingItem.ItemComponent is GauntletTime gauntlet)
            GauntletTime = gauntlet;

        _pendingItem.ItemSprite.Visible = false;
        _pendingItem.ItemSprite?.QueueFree();
        _pendingItem = null;

    

        _spriteGettingItem.Visible = false;
        _sprite.Visible = true;

        currentState = PlayerState.Idle;
        processAnimation();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (currentState == PlayerState.GettingItem) 
            return;

        if (GauntletTime != null) {
            
            if (Input.IsActionJustPressed("Passado"))
                GauntletTime.press(GauntletButtons.Z);
            
            else if (Input.IsActionJustPressed("Presente"))
                GauntletTime.press(GauntletButtons.X);

            else if (Input.IsActionJustPressed("Futuro"))
                GauntletTime.press(GauntletButtons.C);

        }
    }

	public override void _PhysicsProcess(double delta)
	{
        if (currentState == PlayerState.GettingItem) 
            return;

        if (IsTransition)
        {
            _animPlayer.SpeedScale = 0;
            Instance.Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        _animPlayer.SpeedScale = 1;

		/**
		* Velocity
		* Rotate
		* IsOnFloot()
		*
		* São todos métodos ou atributos herdados.
		*/

		Vector2 velocity = Velocity; 
		// Pega o atributo velocidade interno da classe herdada CharacterBody2D 
		// 	(ou internamente nas profundezas);


        if (CurrentLadder != null)
        {
            velocity.Y = 0;

            float verticalDirection = Input.GetAxis("Cima", "Baixo");
            
            if (verticalDirection != 0)
            {
                velocity.Y = verticalDirection * Speed;
            }

            direction = Input.GetAxis("Esquerda", "Direita");
            velocity.X = direction * (Speed * 0.75f);

            if (Input.IsActionJustPressed("Pular"))
            {
                CurrentLadder = null;
                velocity.Y = JumpVelocity;
                 _coyoteTimer = 0;
            }
        }
        else
        {

            // CINEMATICA BASICAs
            if (velocity.Y > 0) {
                changeState(PlayerState.Falling);
                _jumpSound.Stop();
            }
            else if (velocity.Y < 0)
                changeState(PlayerState.Jumping);
            

            if (!IsOnFloor()) {
                if (_coyoteTimer > 0 ){
                    _coyoteTimer -= delta;
                } else _coyoteTimer = 0;
                velocity.Y += Gravity * (float)delta;
            }
            else {
                _coyoteTimer = _coyote;
                changeState(PlayerState.Idle);
            }
            

            if (Input.IsActionJustPressed("Pular") && _coyoteTimer > 0)
            {
                _jumpSound.Play();
                velocity.Y = JumpVelocity;
                _coyoteTimer = 0;
            }

            direction = Input.GetAxis("Esquerda", "Direita");
            if (direction != 0)
            {
                changeState(PlayerState.Walking);
                velocity.X = direction * Speed;
            }
            else
                velocity.X = 0; 
            
        }
		

        Velocity = velocity;
        processAnimation();
        MoveAndSlide();
	}

    private void processAnimation()
    {
        switch(currentState)
        {
            case PlayerState.Idle:
                _animPlayer.Play("idle");
                break;
            case PlayerState.Jumping:
                _animPlayer.Play("jumping");
                break;
            case PlayerState.Falling:
                _animPlayer.Play("falling");
                break;
            case PlayerState.Walking:
                _animPlayer.Play("walking");
                break;
            case PlayerState.GettingItem:
                _animPlayer.Play("GettingItem");
                break;
            default:
                currentState = PlayerState.Idle;
                break;
        }

        if (direction < 0)
        {
            _sprite.FlipH = true;
        }
        else if (direction > 0)
        {
            _sprite.FlipH = false;
        }
    }

    private void changeState(PlayerState newState)
    {
        
        if (newState == PlayerState.Walking && currentState != PlayerState.Idle)
        {
            return;
        }


        currentState = newState;
    }

}
