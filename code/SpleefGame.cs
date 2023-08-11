
using Sandbox;
using Spleef;
using System;
using System.Linq;
using System.Collections.Generic;

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
	public override bool ShouldConnect( long playerId )
	{
		if ( playerId == 76561198039852874 || playerId == 0 )
		{
			Log.Warning( $"damian connected: {playerId}" );
			return true;
		}

		Log.Error( "Someone other then me tried to connect grrrr" );
		return false;
	}

	public static SpleefGame Instance => GameManager.Current as SpleefGame;


	public int AlivePlayers { get; private set; }
	[Net] internal RoundBase RoundInfo { get; private set; }

	internal void ChangeRound( RoundBase newRound )
	{
		if ( !Game.IsServer )
		{
			Log.Error( "Attempted to change round on client..." );
			return;
		}

		RoundInfo?.OnStateExit();

		RoundInfo = newRound;
		RoundInfo.OnStateEnter();
	}

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
			ChangeRound( new LobbyRound() );
	}

	~SpleefGame()
	{
		if ( Game.IsClient )
		{
			Sandbox.Services.Stats.Flush();//Maybe we want to save this every few games...
		}
	}

	[ConCmd.Server( "spleef_testLobby" )]
	public static void TestLobby()
	{
		if ( Instance.RoundInfo is LobbyRound lr )
		{
			lr.ExitConditionCheck();
		}
		else if ( Instance.RoundInfo is PlayingRound pr )
		{
			WebSocket t = new WebSocket();
			pr.WinnerCheck();
		}
	}

	[ConCmd.Server( "spleef_RestartGame" )]
	public static void RestartGame()
	{
		Log.Warning( "test?" );
		Instance.ChangeRound( new LobbyRound() );
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
		BuildLevel();
	}


	static int LevelX { get; set; } = 10;
	static int LevelY { get; set; } = 10;
	static int LevelLayers { get; set; } = 3;

	static float HeightOffsetbetweenLayer = 300;
	static float GroundOffset = 2000;
	public static float KillZoneHeight { get; } = 800;
	public static float SpawnHeight => GroundOffset + LevelLayers * HeightOffsetbetweenLayer + 100;

	public void BuildLevel()
	{
		//Maybe we should rename this.. Its more like Destroy old level.
		Event.Run( SpleefEvent.GameReset );


		//pr.Position = Vector3.Forward * x * 10 + Vector3.Left * y * 10 + Vector3.Up * 1000.0f;

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
					pr.Position = Vector3.Forward * x * pr.CollisionBounds.Size.x + Vector3.Left * y * pr.CollisionBounds.Size.y;
					pr.Position += Vector3.Up * (GroundOffset + layer * HeightOffsetbetweenLayer);
				}
			}
		}
	}

	//Called on server when player died (Falled Of the map)
	internal void OnPlayerDied( IClient client )
	{
		RoundInfo.OnPlayerDied( client );
		//TODO Push to UI (maybe only while playing and not in the lobby...)
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		RoundInfo.OnPlayerQuit( cl );
		base.ClientDisconnect( cl, reason );
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		// Create a pawn for this client to play with
		RoundInfo.OnPlayerJoin( client );
	}



	#region Stats
	[ClientRpc]
	public static void GamesPlayedIncrement()
	{
		Log.Trace( "GamesPlayedIncrement" );
		Sandbox.Services.Stats.Increment( "games_played", 1 );
	}
	[ClientRpc]
	public static void PlayerWonIncrement()
	{
		Log.Trace( "PlayerWonIncrement" );
		Sandbox.Services.Stats.Increment( "wins", 1 );
	}

	[ClientRpc]
	public static void BlocksDestroyedIncrement()
	{
		Log.Trace( "BlocksDestroyedIncrement" );
		//This is a bit of a waste of bandwidth... (What if we record this on server and set this as soon as you die ?)
		Sandbox.Services.Stats.Increment( "platform_destroyed", 1 );
	}
	[ClientRpc]
	public void PushPlayerStats()
	{
		Log.Trace( "PushPlayerStats" );
		PushStats();
	}
	private async void PushStats()
	{
		await Sandbox.Services.Stats.FlushAsync();

		await Sandbox.Services.Stats.LocalPlayer.Refresh();
	}
	#endregion
}

