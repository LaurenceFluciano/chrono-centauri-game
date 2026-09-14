using Godot;
using System;

public partial class Player : CharacterBody2D
{

    public static Player Instance { get; private set; }
	[Export] public float Speed = 200.0f;
    [Export] public float JumpVelocity = -350.0f;

    public event Action<Era> EraChangeRequested;

    [Export] public double EraChangeCooldownTime = 0.5;
    private double _eraChangeCooldownTimer = 0.0;

	private Vector2 _spawnPosition;

    public bool IsTransition = false;


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
            Instance.Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

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
		

        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
        }

		// Configurei isso em Projeto > Configurações do Projeto > Mapeamento de Entrada
		//	Nele eu tenho uma ação e os inputs linkados
		//	Através dos métodos estáticos (IsActionJustPressed, GetAxis) da classe Input
		//	eu consigo ver se essa ação foi chamada
		//	
        if (Input.IsActionJustPressed("Pular") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        float direction = Input.GetAxis("Esquerda", "Direita");
        if (direction != 0)
        {
            velocity.X = direction * Speed;
			// Aqui ele muda o SENTIDO do vetor
			// Nesse caso a velocidade é uma grandeza vetorial  (física e algebra linear)
        }
        else
        {
            velocity.X = 0; 
        }

        Velocity = velocity;
        MoveAndSlide();
	}
}
