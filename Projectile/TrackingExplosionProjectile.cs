#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits;
using OpenRA;
using System;
using System.Linq;
using OpenRA.Primitives;
using OpenRA.Mods.Common.Scripting;
using OpenRA.Scripting;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.RA2A.Effects;

namespace OpenRA.Mods.RA2A.Projectile
{
	// -------------- Disclaimer --------------
	// This thing is horrible mess and it's going
	// to be recoded soon. Result of doing this
	// in hurry.
	[Desc("Projectile that renders image over the target and explodes after certain duration")]
	public class TrackingExplosionProjectileInfo : IProjectileInfo
	{
		[Desc("Time untill exploding counted after firing weapon")]
		public readonly int Delay = 100;

		[Desc("Image that will be shown above the target untill the delay passes")]
		public readonly string TrackingImage = "explosion";

		[Desc("Image that will be shown above the target untill the delay passes")]
		public readonly string TrackingImagePalette = "effect";

		[Desc("List of explosion sequences that can be used.")]
		[SequenceReference("TrackingImage")]
		public readonly string[] Explosions = new string[0];

		[Desc("TrackingImage offset relative to Actor's center position")]
		public readonly WVec LocalOffset = WVec.Zero;

		[Desc("Animation scale in float")]
		public float Scale = 1f;

		[Desc("Does this have to be scaled dependant on zoom?")]
		public bool ScaleSizeWithZoom = false;

		public IProjectile Create(ProjectileArgs args) { return new TrackingExplosionProjectile(this, args); }
	}
	class TrackingExplosionProjectile : IProjectile
	{
		readonly ProjectileArgs args;

		string trackingimage;
		string trackingimagepalette;
		string[] explosions = new string[0];
		int delay;
		WVec localoffset;
		readonly Animation anim;
		float scale;
		bool scaleSizeWithZoom;
		bool imagealreadyexists = false;

		public TrackingExplosionProjectile(TrackingExplosionProjectileInfo info, ProjectileArgs args)
		{
			this.args = args;
			trackingimage = info.TrackingImage;
			trackingimagepalette = info.TrackingImagePalette;
			explosions = info.Explosions;
			delay = info.Delay;
			localoffset = info.LocalOffset;
			scale = info.Scale;
			scaleSizeWithZoom = info.ScaleSizeWithZoom;
			imagealreadyexists = false;
			anim = new Animation(args.SourceActor.World, trackingimage, () => 0);
		}

		public void Tick(World world)
		{
			string explosion = null;
			// transport.Trait<Cargo>().Passengers.Contains(myPassenger)
			// args.GuidedTarget.Actor.Trait<Passenger>().Transport != null
			if (explosions != null)
			{
				explosion = explosions.RandomOrDefault(Game.CosmeticRandom);// picks random explosion from array 
			}
			if (trackingimage != null && explosions != null) // if they exist render
			{
				if (imagealreadyexists == false)
				{
					if (args.GuidedTarget.IsValidFor(args.SourceActor) && args.GuidedTarget.Type == TargetType.Actor)
						world.AddFrameEndTask(w => w.Add(new TrackingImage(args.GuidedTarget.CenterPosition, args.GuidedTarget.Actor, localoffset, world, trackingimage, explosion, trackingimagepalette, false, scaleSizeWithZoom)));
					else
						world.AddFrameEndTask(w => w.Add(new TrackingImage(args.PassiveTarget., null, localoffset, world, trackingimage, explosion, trackingimagepalette, false, scaleSizeWithZoom)));
					imagealreadyexists = true;
				}
			}
			if (--delay <= 0)
			{
				// TODO: implement ra2 transport + bomb logic on bool by freezing delay if in cargo
				WPos target;
				if (args.GuidedTarget.IsValidFor(args.SourceActor) && args.GuidedTarget.Type == TargetType.Actor)
					target = args.GuidedTarget.CenterPosition + localoffset;
				else
					target = args.PassiveTarget + localoffset;
				world.AddFrameEndTask(w => w.Remove(this));
				args.Weapon.Impact(Target.FromPos(target), args.SourceActor, args.DamageModifiers);
			}
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			yield break;
		}
	}
}
