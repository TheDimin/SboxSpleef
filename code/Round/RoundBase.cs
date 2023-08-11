using Sandbox;
using Sandbox.UI.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	internal partial class RoundBase : BaseNetworkable
	{
		public virtual bool CanDestroyBlocks => false;


		#region Server
		public virtual void OnPlayerJoin( IClient client ) { }

		public virtual void OnPlayerQuit( IClient client ) { }

		public virtual void OnPlayerDied( IClient client ) { }

		public virtual void OnStateEnter()
		{
			Log.Trace( $"[GAMESTATE] Enterd :{this.ClassName}" );
		}
		public virtual void OnStateExit() { }


		protected virtual void SpawnPlayer( IClient client )
		{
			//Reset pawn if its already valid...
			Log.Warning( "Spawning player..." );
			if ( client.Pawn != null )
			{
				client.Pawn.Delete();
			}

			client.Components.RemoveAny<DevCamera>();

			var pawn = new Pawn();
			client.Pawn = pawn;
			pawn.Respawn();
			pawn.DressFromClient( client );

			//We don't use respawn points just force pos for now...
			pawn.Position = Vector3.Up * 1200.0f; // raise it up

		}
		#endregion
	}
}
