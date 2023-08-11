using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class WinnerState : GameStateBase
	{
		[ConVar.Replicated( "spleef_EndOfRoundTimer", Min = 3, Max = 30, Help = "Time in seconds we will stay in the WinnerGameState" )]
		public static int EndOfRoundTime { get; set; } = 15;
		//TODO break out the finished countdown into a seperate gamestate
		[Net] public TimeUntil countdownFinished { get; private set; } = 999999;
		[Net] internal IClient WinningClient { get; set; }
		public WinnerState( IClient winningClient )
		{
			WinningClient = winningClient;
			Log.Warning( $"We got a winner: {winningClient}" );
			SpleefGame.PlayerWonIncrement( To.Single( winningClient ) );
		}
		public WinnerState() { }

		public override void OnStateEnter()
		{
			//Important that countdown is set before Base.OnStateEnter 
			// As it will call ExitConditionCheck which would pass as timer has not been set yet. 
			countdownFinished = EndOfRoundTime;

			Event.Register( this );

			SpleefGame.PushPlayerStats();
		}

		public override void OnStateExit()
		{
			base.OnStateExit();

			Event.Unregister( this );
		}

		public void ExitConditionCheck()
		{
			if ( countdownFinished )
			{
				SpleefGame.Instance.ChangeRound( new LobbyState() );
			}
		}

		[GameEvent.Tick.Server]
		public void CountdownTick()
		{
			ExitConditionCheck();
		}
	}
}
