using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class CountDownRound : LobbyRound
	{
		[Net] public TimeUntil countdownFinished { get; private set; }= 999999;

		//TODO CONFIG VARIABLE...
		float StartCountdown = 3;
		public override void OnStateEnter()
		{
			//Important that countdown is set before Base.OnStateEnter 
			// As it will call ExitConditionCheck which would pass as timer has not been set yet. 
			countdownFinished = 3;

			base.OnStateEnter();

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
				SpleefGame.Instance.ChangeRound( new PlayingRound() );
			}
		}

		[GameEvent.Tick.Server]
		public void CountdownTick()
		{
			ExitConditionCheck();
		}
	}
}
