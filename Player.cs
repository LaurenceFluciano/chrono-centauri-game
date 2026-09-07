using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200.0f;
    [Export] public float JumpVelocity = -350.0f;


	// Pega uma constante da velocidade que foi configurado como 980 px/s² 
	// Para poder ver ela ou alterar:
	// Projeto > Configurações do Projeto > Geral 
	//		Na aba: Física > 2D
	//			No campo: Gravida Padrão
	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();


	public override void _PhysicsProcess(double delta)
	{
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
