using Editor;
using Sandbox;
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


	[Prefab, HammerEntity, Library( "thedimin_spleef" ),]
	public partial class Platform : ModelEntity, IUse
	{
		[Prefab] public float DestroyTimer { get; set; } = .01f;
		public override void Spawn()
		{
			base.Spawn();
			Log.Info( "Test" );
		}

		public bool IsUsable( Entity user )
		{
			Log.Warning( "IsUsable" );
			return true;
		}

		public bool OnUse( Entity user )
		{
			Delete();
			return false;
		}

		[SpleefEvent.OnGameReset]
		public void OnGameReset()
		{
			Delete();
		}
	}
}
