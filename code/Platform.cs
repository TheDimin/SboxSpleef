using Editor;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spleef
{
	public static partial class SpleefEvent
	{
		public const string GameReset = "SpleefReset";
		public class OnGameResetAttribute : EventAttribute
		{
			public OnGameResetAttribute() : base( GameReset ) { }
		}
	}


	[Prefab, HammerEntity, Library( "thedimin_spleef" )]
	[Title( "Spleef Platform" ), Category( "Platform" ), Icon( "place" )]
	public partial class Platform : ModelEntity, IUse
	{
		[Prefab] public float MaxHP { get; set; } = 100;

		[Prefab] public Color defaultColor { get; set; } = Color.White;
		[Prefab] public Color deathColor { get; set; } = Color.Black;


		public override void Spawn()
		{
			base.Spawn();

			Respawn();
		}

		public override void OnKilled()
		{
			Log.Warning( "Onkilled called" );
			if ( LifeState == LifeState.Alive )
			{
				Log.Warning( "Object was alive" );
				if ( !Game.IsEditor )
					Sandbox.Services.Stats.Increment( LastAttacker.Client, "platforms_destroyed_v2", 1 );

				LifeState = LifeState.Respawnable;

				EnableDrawing = false;
				PhysicsBody.Enabled = false;
			}
		}
		[SpleefEvent.OnGameReset]
		public virtual void Respawn()
		{
			LifeState = LifeState.Respawning;
			EnableDrawing = true;
			PhysicsBody.Enabled = true;
			Health = MaxHP;
			VisualizeHealth();
			LifeState = LifeState.Alive;
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );

			Log.Warning( $"TakeDamage called HP:{Health} :: {info.Damage} :: {LifeState}" );
		}

		public virtual void StopInteraction( SpleefPlayerComponent playerComponent )
		{
			//This doens't make much sense anymore heal platform instead ?
			Respawn();
		}

		protected virtual void VisualizeHealth()
		{
			RenderColor = Color.Lerp( Color.Black, Color.White, Health / MaxHP );
		}

		//IUseable is just used to limit quary results, we don't use it otherwise. We use the Apply damage events now !
		public bool IsUsable( Entity user )
		{
			return true;
		}

		public bool OnUse( Entity user )
		{
			return false;

		}
	}
}
