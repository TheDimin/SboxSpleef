using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class WinnerRound : LobbyRound
	{
		[ConVar.Replicated( "spleef_EndOfRoundTimer", Min = 3, Max = 30, Help = "Time in seconds we will stay in the WinnerGameState" )]
		public static int EndOfRoundTime { get; set; } = 15;
		[Net] public TimeUntil countdownFinished { get; private set; } = 999999;
		[Net] internal IClient WinningClient { get; set; }
		public WinnerRound( IClient winningClient )
		{
			WinningClient = winningClient;
			Log.Warning( $"We got a winner: {winningClient}" );
			SpleefGame.PlayerWonIncrement( To.Single( winningClient ) );
		}
		public WinnerRound() { }

		public override void OnStateEnter()
		{
			//Important that countdown is set before Base.OnStateEnter 
			// As it will call ExitConditionCheck which would pass as timer has not been set yet. 
			countdownFinished = EndOfRoundTime;

			base.OnStateEnter();

			Event.Register( this );

			SpleefGame.Instance.PushPlayerStats();
			SpleefGame.Instance.BuildLevel();

			foreach ( IClient client in Game.Clients )
			{
				if ( !(client.Pawn != null && client.Pawn.IsValid) )
					SpawnPlayer( client );
				else
					//Prevent client from getting stuck in the ground.
					client.Pawn.Position += Vector3.Up * 5;
			}
		}

		public override void OnStateExit()
		{
			base.OnStateExit();

			Event.Unregister( this );
		}

		public override void ExitConditionCheck()
		{
			if ( countdownFinished )
			{
				SpleefGame.Instance.ChangeRound( new LobbyRound() );
			}
		}


		[GameEvent.Tick.Server]
		public void CountdownTick()
		{
			ExitConditionCheck();
		}
	}
}
