﻿using Sandbox;
using Spleef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class PlayingState : GameStateBase
	{
		public override bool CanDestroyBlocks => true;

		List<IClient> AliveClients = new List<IClient>();

		public override void OnPlayerJoin( IClient client )
		{
			base.OnPlayerJoin( client );
			SpleefGame.Instance.MakeSpectator( client );
		}
		public override void OnPlayerQuit( IClient client )
		{
			base.OnPlayerQuit( client );

			//Maybe we don't have to call this as we have to destroy the pawn anyway...
			AliveClients.Remove( client );

			WinnerCheck();
		}

		public override void OnPlayerDied( IClient client )
		{
			base.OnPlayerDied( client );
			AliveClients.Remove( client );

			WinnerCheck();
		}

		public override void OnStateEnter()
		{
			SpleefGame.GamesPlayedIncrement();

			base.OnStateEnter();
			foreach ( var client in Game.Clients )
			{
				if ( client.Pawn != null ) //In a rare case the client is Connected but hasn't spawned in yet. Ignore those...
					AliveClients.Add( client );
			}
		}

		public void WinnerCheck()
		{
			if ( AliveClients.Count <= 1 )
			{
				SpleefGame.Instance.ChangeRound( new WinnerState( AliveClients[0] ) );
			}
		}
	}
}
