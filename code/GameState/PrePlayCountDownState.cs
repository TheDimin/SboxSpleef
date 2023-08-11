using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class PrePlayCountdownState : LobbyState
	{
		[Net] public TimeUntil countdownFinished { get; private set; } = 999999;

		//TODO CONFIG VARIABLE...
		static float StartCountdown = 3;
		public override void OnStateEnter()
		{
			//Important that countdown is set before Base.OnStateEnter 
			// As it will call ExitConditionCheck which would pass as timer has not been set yet. 
			countdownFinished = StartCountdown + 1;

			Event.Register( this );
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
				SpleefGame.Instance.ChangeRound( new PlayingState() );
			}
		}

		[GameEvent.Tick.Server]
		public void CountdownTick()
		{
			ExitConditionCheck();
		}
	}
}
