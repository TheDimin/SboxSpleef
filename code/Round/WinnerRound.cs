using MyGame;
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
		[ConVar.Replicated( "spleef_EndOfRoundTimer", Min = 0, Max = 30, Help = "Time in seconds we will stay in the WinnerGameState" )]
		public static int EndOfRoundTime { get; set; } = 5;
		[Net] public TimeUntil countdownFinished { get; private set; }
		[Net] internal IClient WinningClient { get; set; }
		public WinnerRound( IClient winningClient )
		{
			WinningClient = winningClient;
			Log.Warning( $"We got a winner: {winningClient}" );
			SpleefGame.Instance.PlayerWonIncrement( To.Single( winningClient ) );
		}
		public WinnerRound() { }

		public override void OnStateEnter()
		{
			base.OnStateEnter();

			Event.Register( this );
			countdownFinished = EndOfRoundTime;

			SpleefGame.Instance.PushPlayerStats();
			SpleefGame.Instance.BuildLevel();

			foreach ( IClient client in Game.Clients )
			{
				if ( !(client.Pawn != null && client.Pawn.IsValid) )
					SpawnPlayer( client );
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
