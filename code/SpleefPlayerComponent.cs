using Sandbox;
using Sandbox.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		public Platform LastDamagedPlatform { get; protected set; } = null;
		public TimeUntil InteractCooldown { get; protected set; } = 0;

		public static float CooldownAfterBlockDamaged { get; set; } = .000f;

		public static float PlatformHitDamage { get; set; } = 100;

		public virtual void ApplyDamageToPlatform( [NotNull] Platform platform )
		{
			if ( !InteractCooldown ) return;

			if ( !SpleefGame.CanDestroyBlocks ) return;

			platform.TakeDamage( DamageInfo.Generic( PlatformHitDamage ).WithAttacker( Entity ) );

			LastDamagedPlatform = platform;
			InteractCooldown = CooldownAfterBlockDamaged;
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
