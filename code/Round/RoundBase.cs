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

		public virtual void OnPlayerDied( IClient client )
		{
			StartSpectating( client );
		}

		public virtual void OnStateEnter()
		{
			Log.Trace( $"[GAMESTATE] Enterd :{this.ClassName}" );
		}
		public virtual void OnStateExit() { }


		protected virtual void SpawnPlayer( IClient client )
		{
			client.Pawn?.Delete();

			client.Components.RemoveAny<DevCamera>();

			var pawn = new Pawn();
			client.Pawn = pawn;
			pawn.Respawn();
			pawn.DressFromClient( client );

			pawn.Position = SpleefGame.SpawnPosition;

		}

		protected virtual void StartSpectating( IClient client )
		{
			client.Pawn?.Delete();
			client.Pawn = null;

			//TODO call this on the client not server...
			Camera.Position = SpleefGame.SpawnPosition + Vector3.Backward * 100;
			SpleefGame.Current.DoPlayerDevCam( client );

		}
		#endregion
	}
}
