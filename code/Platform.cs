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
	[Title("Spleef Platform"),Category("Platform"),Icon("place")]
	public partial class Platform : ModelEntity, IUse
	{
		[Prefab] public float DestroyTimer { get; set; } = .01f;
		public override void Spawn()
		{
			base.Spawn();
		}

		public bool IsUsable( Entity user )
		{
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
