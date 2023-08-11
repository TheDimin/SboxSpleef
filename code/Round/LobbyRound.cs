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
			base.OnStateEnter();
			SpleefGame.Instance.BuildLevel();

			foreach ( IClient client in Game.Clients )
			{
				if ( !(client.Pawn != null && client.Pawn.IsValid) )
					SpawnPlayer( client );
				else
					//Prevent client from getting stuck in the ground.
					client.Pawn.Position += Vector3.Up * 5;
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
			SpawnPlayer( client );
		}
	}
}
