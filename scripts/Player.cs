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
        Falling
    }

    private PlayerState currentState = PlayerState.Idle;
    private float direction = 1;

    // Animacao 
    private AnimationPlayer _animPlayer;

    // Desenho / Sprite

    private Sprite2D _sprite;

    // Coyote config

    private double _coyote = 0.08f;
    private double _coyoteTimer = 0.0;


    public static Player Instance { get; private set; }
	[Export] public float Speed = 200.0f;
    [Export] public float JumpVelocity = -350.0f;

    public event Action<Era> EraChangeRequested;

    [Export] public double EraChangeCooldownTime = 0.5;
    private double _eraChangeCooldownTimer = 0.0;

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

    public override void _EnterTree()
    {
        Instance = this;
    }

	public override void _Ready()
    {
        var spawnPoint = GetNodeOrNull<Node2D>("../SpawnPoint");

        _coyote = 0.08f;

        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _sprite = GetNode<Sprite2D>("PlayerAnimation");

        _animPlayer.Play("idle");

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

    public override void _Process(double delta)
    {
        if (_eraChangeCooldownTimer > 0)
        {
            _eraChangeCooldownTimer -= delta;
        }
    }

	public void Respawn()
    {
        GlobalPosition = _spawnPosition;
        Velocity = Vector2.Zero;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        Era? requestedEra = null;

        if (Input.IsActionJustPressed("Passado"))
        {
            requestedEra = Era.Past;
        }
        else if (Input.IsActionJustPressed("Presente"))
        {
            requestedEra = Era.Present;
        }
        else if (Input.IsActionJustPressed("Futuro"))
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

	public override void _PhysicsProcess(double delta)
	{
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
            if (velocity.Y > 0)
                changeState(PlayerState.Falling);
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
