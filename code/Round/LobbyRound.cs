using Sandbox;
using Spleef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class LobbyRound : RoundBase
	{
		[ConVar.Replicated( "spleef_MinPlayerCount", Min = 2 )] public static int RequiredPlayerCount { get; set; } = 2;

		public override void OnStateEnter()
		{
			foreach ( IClient client in Game.Clients )
			{
				if ( client.Pawn == null )
					SpawnPlayer( client );
			}

			ExitConditionCheck();
		}

		public override void OnPlayerJoin( IClient client )
		{
			base.OnPlayerJoin( client );

			SpawnPlayer( client );

			ExitConditionCheck();
		}

		public virtual void ExitConditionCheck()
		{
			if ( RequiredPlayerCount <= Game.Clients.Count )
				SpleefGame.Instance.ChangeRound( new CountDownRound() );
		}

		public override void OnPlayerDied( IClient client )
		{
			base.OnPlayerDied( client );

			SpawnPlayer( client );
		}
	}
}
