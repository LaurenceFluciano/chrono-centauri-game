
using System;
using Godot;



// Acoplamento-04 : A area de trigger conhece o componente player
// Acoplamento-05 : A area de trigger conhece a ação de interação do player

public partial class ManoplaAltarController : TimeArea
{
    public event EventHandler<GetItemEventArgs> OnGetItem;

    // Mensagem do evento

    [Export]
    public Sprite2D ItemSprite;

    [Export]
    public String NameItem = "Gauntlet of Time";

    [Export]
    public Node ItemComponent;



    // Lógica interna
    protected bool PlayerInside = false;
    private bool isGot = false; 
    private bool isClosed = true;

    private AnimationPlayer _anim;


    // Escuta quem abrir ele

    [Export]
    private OnOffEventAggregator _eventAgreggator;

    [Export]
    public int IdListener;


    
    public override void _Ready()
    {
        base._Ready();


        _anim = GetNode<AnimationPlayer>("AnimationPlayer");


        _anim.Play("Closed");

        if (_eventAgreggator != null)
        {
            _eventAgreggator.OnEvent += HandleInteract;
        }

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        if (PlayerInside && Input.IsActionJustPressed("Interagir") && !isGot && !isClosed)
        {
            _anim.Play("Got");
            ItemEvents.RaiseGetItem(this, new GetItemEventArgs(ItemSprite, NameItem, ItemComponent));
            isGot = true;
        }
    }

    public void HandleInteract(object sender, OnOffEventArgs eventArgs)
    {
        if (isGot)
            return;

        if (eventArgs.Id != IdListener)
            return;
        

        if (eventArgs.State == OnOffEnum.On)
        {
            _anim.Play("Opened");
            isClosed = false;
        } else
        {
            _anim.Play("Closed");
            isClosed = true;
        }


    }


    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            PlayerInside = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player player)
        {
            PlayerInside = false;
        }
    }

    public override void _ExitTree()
    {
        if (_eventAgreggator != null)
            _eventAgreggator.OnEvent -= HandleInteract;
    }
    
}