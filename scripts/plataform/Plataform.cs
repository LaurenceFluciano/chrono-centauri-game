using Godot;

[Tool]
public partial class Plataform : StaticBody2D
{
    private CollisionShape2D _collision;
    private NinePatchRect _ninePatch;

    public override void _Ready()
    {
        _collision = GetNode<CollisionShape2D>("Collision");
        _ninePatch = GetNode<NinePatchRect>("NinePatchRect");
        UpdatePlatform();
    }

    public override void _Process(double delta)
    {
        // Roda no editor em tempo real sempre que mexes no NinePatchRect
        if (Engine.IsEditorHint())
        {
            UpdatePlatform();
        }
    }

    private void UpdatePlatform()
    {
        if (_collision == null || _ninePatch == null) return;

        // Pega o tamanho atual que esticaste no NinePatchRect
        Vector2 currentSize = _ninePatch.Size;

        // Garante que o CollisionShape2D tem um RectangleShape2D atribuído
        if (_collision.Shape == null)
        {
            _collision.Shape = new RectangleShape2D();
        }

        if (_collision.Shape is RectangleShape2D rectShape)
        {
            // Atualiza o tamanho do retângulo para o mesmo tamanho do gráfico
            rectShape.Size = currentSize;
        }

        // Ativa o One-Way Collision automaticamente
        _collision.OneWayCollision = true;

        // Centraliza a colisão exatamente no meio do NinePatchRect
        _collision.Position = new Vector2(currentSize.X / 2f, currentSize.Y / 2f);
    }
}