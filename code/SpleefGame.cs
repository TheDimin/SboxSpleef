
using Sandbox;
using Spleef;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sandbox.Diagnostics;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Spleef;


/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class SpleefGame : Sandbox.GameManager
{
	public static SpleefGame Instance => GameManager.Current as SpleefGame;

	[Net] internal GameStateBase gamestate { get; private set; }

	//At some point we load this from maps instead of generation code..
	#region LevelGeneration
	static int LevelX { get; set; } = 25;
	static int LevelY { get; set; } = 25;
	static int LevelLayers { get; set; } = 1;

	static float HeightOffsetbetweenLayer = 300;
	static float GroundOffset = 20000;
	public static float KillZoneHeight { get; } = 19800;

	//Cheeky bounds finder, not valid till after a map is generated
	[Net] private BBox bounds { get; set; }
	[Net] private float elementScale { get; set; } = 1;
	#endregion

	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public SpleefGame()
	{
		//GameManager.Current = this;
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}

		if ( Game.IsServer )
			ChangeRound( new LobbyState() );
	}

	~SpleefGame()
	{
		if ( Game.IsClient )
		{
			Sandbox.Services.Stats.Flush();//Maybe we want to save this every few games...
		}
	}

	[ConCmd.Server( "spleef_RestartGame" )]
	public static void RestartGame()
	{
		Log.Warning( "Spleef restart command issued" );
		Instance.ChangeRound( new LobbyState() );
	}

	public static Vector3 SpawnPosition
	{
		get
		{
			return ((Vector3.Forward * (LevelX * Instance.bounds.Size.x)) +// 
			(Vector3.Left * (LevelY * Instance.bounds.Size.y))) * Instance.elementScale * .5f +
			Vector3.Up * (GroundOffset + LevelLayers * HeightOffsetbetweenLayer + Instance.bounds.Size.z * 0.5f);
		}
	}


	public void BuildLevel()
	{
		//Maybe we should rename this.. Its more like Destroy old level.
		Event.Run( SpleefEvent.GameReset );

		//Build map, Ideally we do this a other way, but eyy this works...
		//With a savegame sounds honestly ideal... Allowing us to reload data somehow...
		for ( int layer = 0; layer < LevelLayers; layer++ )
		{
			for ( int x = 0; x < LevelX; x++ )
			{
				for ( int y = 0; y < LevelY; y++ )
				{
					Platform pr = PrefabLibrary.Spawn<Platform>( "untitled.prefab" );
					if ( !pr.IsValid )
					{
						Log.Error( "Failed to load map part...." );
						continue;
					}
					bounds = pr.CollisionBounds;
					elementScale = pr.Scale;
					pr.Position = Vector3.Forward * x * pr.CollisionBounds.Size.x * elementScale
						+ Vector3.Left * y * pr.CollisionBounds.Size.y * elementScale;
					pr.Position += Vector3.Up * (GroundOffset + layer * HeightOffsetbetweenLayer);
				}
			}
		}
	}

	internal void ChangeRound( GameStateBase newState )
	{
		if ( !Game.IsServer )
		{
			Log.Error( "Attempted to change round on client..." );
			return;
		}

		gamestate?.OnStateExit();

		gamestate = newState;
		gamestate.OnStateEnter();
	}

	#region GameEvents
	//Called on server when player died (Falled Of the map)
	internal void OnPlayerDied( IClient client )
	{
		gamestate.OnPlayerDied( client );
		//TODO Push to UI (maybe only while playing and not in the lobby...)
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		gamestate.OnPlayerQuit( cl );

		base.ClientDisconnect( cl, reason );
	}

	public override void ClientJoined( IClient client )
	{
		gamestate.OnPlayerJoin( client );

		base.ClientJoined( client );
	}
	#endregion

	#region Stats
	[ClientRpc]
	//All other stats are replicated differently, but this one has to be send to all clients so lets do it here for now.
	public static void GamesPlayedIncrement()
	{
		Log.Trace( "GamesPlayedIncrement" );
		Sandbox.Services.Stats.Increment( "games_played_v2", 1 );
	}

	[ClientRpc]
	public static void PushPlayerStats()
	{
		Log.Trace( "PushPlayerStats" );
		PushStats();
	}
	private static async void PushStats()
	{
		await Sandbox.Services.Stats.FlushAsync();

		await Sandbox.Services.Stats.LocalPlayer.Refresh();
	}
	#endregion


	public virtual SpectatorComponent MakeSpectator( IClient client, Vector3 pos, Rotation rotation )
	{
		Game.AssertServer();

		client.Pawn?.Delete();
		client.Pawn = null;

		var camera = client.Components.Get<SpectatorComponent>( true );

		if ( camera == null )
		{
			camera = new SpectatorComponent();
			client.Components.Add( camera );

			camera.SetPosition(pos);
			//camera.TargetPos = pos;
			//camera.TargetRot = rotation;

			return camera;
		}

		camera.Enabled = !camera.Enabled;
		camera.SetPosition(pos);
		//camera.TargetPos = pos;
		//camera.TargetRot = rotation;

		return camera;
	}
}

