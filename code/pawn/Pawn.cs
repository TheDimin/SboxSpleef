using Sandbox;
using Sandbox.Component;
using Sandbox.Internal.Globals;
using Spleef;
using System.ComponentModel;

namespace MyGame;

public partial class Pawn : AnimatedEntity
{
	[ClientInput]
	public Vector3 InputDirection { get; set; }

	[ClientInput]
	public Angles ViewAngles { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}

	[BindComponent] public PawnController Controller { get; }
	[BindComponent] public PawnAnimator Animator { get; }
	[BindComponent] public SpleefPlayerComponent spleefPlayerComponent { get; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public void Respawn()
	{
		Components.Create<PawnController>();
		Components.Create<PawnAnimator>();
		Components.Create<SpleefPlayerComponent>();
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		Controller?.Simulate( cl );
		Animator?.Simulate();
		EyeLocalPosition = Vector3.Up * (64f * Scale);
		TickPlayerUse();
	}

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;


		if ( Input.StopProcessing )
			return;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}

	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		Camera.FirstPersonViewer = this;
		Camera.Position = EyePosition;
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}



	/// <summary>
	/// This should be called somewhere in your player's tick to allow them to use entities,
	/// for example the Simulate callback method of the player pawn entity.
	/// </summary>
	protected virtual void TickPlayerUse()
	{
		// This is serverside only
		if ( !Game.IsServer ) return;

		// Turn prediction off
		using ( Prediction.Off() )
		{
			Entity Using = null;

			if ( Input.Down( "Use" ) )
			{
				Using = FindUsable();

				if ( Using == null )
				{
					UseFail();
					return;
				}

				spleefPlayerComponent.OnInteractHold( Using );
			}
		}
	}

	/// <summary>
	/// Player tried to use something but there was nothing there.
	/// Tradition is to give a disappointed boop.
	/// </summary>
	protected virtual void UseFail()
	{
		PlaySound( "player_use_fail" );
	}

	/// <summary>
	/// Returns if the entity is a valid usable entity
	/// </summary>
	protected bool IsValidUseEntity( Entity e )
	{
		if ( e == null ) return false;
		if ( e is not IUse use ) return false;
		if ( !use.IsUsable( this ) ) return false;

		return true;
	}
	DebugOverlay l { get; set; }


	/// <summary>
	/// Find a usable entity for this player to use
	/// </summary>
	protected Entity FindUsable()
	{
		// First try a direct 0 width line
		var tr = Trace.Ray( AimRay, 185 )
			.Ignore( this )
			.Run();

		if ( l == null )
			l = new DebugOverlay();
		Sandbox.Internal.GlobalGameNamespace.DebugOverlay.TraceResult( tr, 10 );
		// See if any of the parent entities are usable if we ain't.
		var ent = tr.Entity;
		while ( ent.IsValid() && !IsValidUseEntity( ent ) )
		{
			ent = ent.Parent;
		}

		// Nothing found, try a wider search
		if ( !IsValidUseEntity( ent ) )
		{
			tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 85 )
			.Radius( 2 )
			.Ignore( this )
			.Run();

			// See if any of the parent entities are usable if we ain't.
			ent = tr.Entity;
			while ( ent.IsValid() && !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		// Still no good? Bail.
		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}
}
