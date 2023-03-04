using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life.Core.Isometric;

public class Entity
{
	protected float speed;
	private Vector3 position;

	public Texture2D texture;
	public Vector2 screenPosition;
	public Vector2 scale = new Vector2(0.5f, 0.5f);

	public float GetSpeed { get { return speed; } }
	public Vector3 Position { get { return position; } set { position = value; TilePositionToScreenPosition(); } }

	public Entity(Texture2D texture, Vector3 position)
	{
		this.texture = texture;
		this.position = position;
		TilePositionToScreenPosition();
		Main.CurrentScene.EntityRegisterEvent(this);
	}

	protected void TilePositionToScreenPosition()
	{
		screenPosition = Main.CurrentScene.tileMap.GetGridScreenPosition(position);

	}

	public void Draw()
	{
		Main.SpriteBatch.Draw(texture, screenPosition + -Main.CurrentScene.screenPosition - new Vector2(texture.Width * scale.X / 2, texture.Height * scale.Y), null, Color.White, 0, new Vector2(), scale, SpriteEffects.None, 0);
	}
}
