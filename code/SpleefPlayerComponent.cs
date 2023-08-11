using Sandbox;
using Sandbox.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	/// <summary>
	/// Component responsible for interaction with platforms, Has to be done outside platform.cs as multiple players can interact with same platform.
	/// We are not responsible for the interaction code itself, This has to be handeld in the pawn. allowing multiple types of pawns to be used for spleef.
	/// Think about the generic first person one , or maybe unicyle frenzy pawn.
	/// </summary>
	public partial class SpleefPlayerComponent : EntityComponent
	{
		float activeTimeTillDestroy { get; set; } = 0;
		public Platform activeInteractPlatform { get; private set; } = null;
		public void OnInteractHold( Entity e )
		{
			if ( SpleefGame.Instance != null )
			{
				if ( !SpleefGame.Instance.gamestate.CanDestroyBlocks ) return;
			}
			else
			{
				Log.Error( "Spleef game is null ????" );
			}

			if ( !(e is Platform) ) return;

			Platform interactPlatform = (Platform)e;

			if ( interactPlatform != activeInteractPlatform )
			{
				if ( activeInteractPlatform != null )
					ClearActiveInteract();

				activeInteractPlatform = interactPlatform;
				activeTimeTillDestroy = activeInteractPlatform.DestroyTimer;
			}

			activeTimeTillDestroy -= Time.Delta;

			activeInteractPlatform.RenderColor = Color.Lerp( Color.Black, Color.White, activeTimeTillDestroy / activeInteractPlatform.DestroyTimer );

			if ( !activeInteractPlatform.IsValid )
			{
				activeInteractPlatform = null;
				activeTimeTillDestroy = 0;
				return;
			}

			if ( activeTimeTillDestroy < 0 )
			{
				if ( !activeInteractPlatform.OnUse( Entity ) )
				{
					SpleefGame.BlocksDestroyedIncrement( To.Single( Entity ) );
				}
			}
		}


		public void ClearActiveInteract()
		{
			activeInteractPlatform.RenderColor = Color.White;
			activeInteractPlatform = null;
		}

		[GameEvent.Tick.Server]
		void DeathCheck()
		{
			if ( Entity.Position.z < SpleefGame.KillZoneHeight )
			{
				SpleefGame.Instance.OnPlayerDied( Entity.Client );
			}
		}

	}
}
