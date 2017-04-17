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

using System;
using System.Collections.Generic;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA;

namespace OpenRA.Mods.RA2A.Effects
{
	class TrackingImage : IEffect
	{
		readonly World world;
		readonly string palette;
		readonly Animation anim;
		readonly bool visibleThroughFog;
		readonly bool scaleSizeWithZoom;
		readonly WVec localoffset;

		WPos pos;
		Actor targetactor;
		public TrackingImage(WPos pos, Actor targetactor, WVec localoffset, World world, string image, string sequence, string palette, bool visibleThroughFog = false, bool scaleSizeWithZoom = false)
		{
			this.world = world;
			this.pos = pos;
			this.targetactor = targetactor;
			this.localoffset = localoffset;
			this.palette = palette;
			this.scaleSizeWithZoom = scaleSizeWithZoom;
			this.visibleThroughFog = visibleThroughFog;
			anim = new Animation(world, image, () => 0);
			anim.PlayThen(sequence, () => world.AddFrameEndTask(w => w.Remove(this)));
		}

		public void Tick(World world)
		{
			anim.Tick();
			WVec offset = localoffset;
			if (targetactor != null)
			{
				var temppos = targetactor.CenterPosition;
				temppos += localoffset;
				pos = temppos;
			}
			else
			{
				var temppos = pos;
				temppos += offset;
				pos = temppos;
			}
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (world.FogObscures(pos) && !visibleThroughFog)
				return SpriteRenderable.None;

			var zoom = scaleSizeWithZoom ? 1f / wr.Viewport.Zoom : 1f;
			return anim.Render(pos, WVec.Zero, 0, wr.Palette(palette), zoom);
		}
	}
}