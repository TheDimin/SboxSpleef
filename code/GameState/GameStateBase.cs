using Sandbox;

namespace Spleef
{
	internal partial class GameStateBase : BaseNetworkable
	{
		public virtual bool CanDestroyBlocks => false;


		#region Server
		public virtual void OnPlayerJoin( IClient client ) { }

		public virtual void OnPlayerQuit( IClient client ) { }

		public virtual void OnPlayerDied( IClient client )
		{
			SpleefGame.Instance.MakeSpectator( client);
		}

		public virtual void OnStateEnter()
		{
			Log.Trace( $"[GAMESTATE] Enterd :{this.ClassName}" );
		}
		public virtual void OnStateExit() { }


		protected virtual void SpawnPlayer( IClient client )
		{
			client.Pawn?.Delete();

			client.Components.RemoveAny<SpectatorComponent>();

			var pawn = new Pawn();
			client.Pawn = pawn;
			pawn.Respawn();
			pawn.DressFromClient( client );

			pawn.Position = SpleefGame.SpawnPosition;

		}
		#endregion
	}
}
