using Godot;
using Godot.Collections;

public class Player : Entity
{
	private Sprite _interactionSprite;
	private RemoteTransform2D _remoteTransform;
	private RayCast2D _interactionRayCast;

	private readonly Dictionary<string, Vector2> _acceptedInputs = new Dictionary<string, Vector2>()
	{
		{ "ui_up", Vector2.Up },
		{ "ui_down", Vector2.Down },
		{ "ui_right", Vector2.Right },
		{ "ui_left", Vector2.Left },
	};

	public override void _Ready()
	{
		base._Ready();

		_remoteTransform = GetNode<RemoteTransform2D>("CameraRemote");
		_interactionRayCast = GetNode<RayCast2D>("InteractionRayCast2D");
		_interactionSprite = GetNode<Sprite>("InteractionSprite");

		Global.InteractionManager.Connect("InteractionTriggered", this, nameof(OnInteractionTriggered));
		Global.InteractionManager.Connect("InteractionEnded", this, nameof(OnInteractionEnded));
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);

		_interactionRayCast.CastTo = _rayCast.CastTo;
		_interactionRayCast.ForceRaycastUpdate();
		var interactable = _interactionRayCast.GetCollider() as InteractableArea;

		_interactionSprite.Texture = null;
		if (interactable == null || !interactable.CanTrigger() || !CanMove) return;
		_interactionSprite.Texture = interactable.GetIcon();

		if (!Input.IsActionJustPressed("ui_interact")) return;
		interactable.Interact();
	}

	protected override Vector2 CheckForInput()
	{
		foreach (var item in _acceptedInputs)
		{
			if (Input.IsActionPressed(item.Key)) return item.Value;
		}

		return Vector2.Zero;
	}

	public void SetCamera(MainCamera camera)
	{
		_remoteTransform.RemotePath = _remoteTransform.GetPathTo(camera);
	}

	private void OnInteractionTriggered(Interaction interaction)
	{
		CanMove = false;
	}

	private void OnInteractionEnded(Interaction interaction)
	{
		CanMove = true;
	}
}
