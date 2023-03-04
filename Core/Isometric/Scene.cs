using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Light_Up_Your_Life.Core.Isometric;

/// <summary>
/// シーン上のエンティティやタイルマップを管理するクラス
/// </summary>
public class Scene
{
	protected List<Entity> entities;

	public TileMap tileMap;
	//シーン上のカメラの位置
	public Vector2 screenPosition;

	public Scene()
	{
		entities = new List<Entity>();
	}

	public virtual void Update()
	{
		tileMap.Update();
	}

	public virtual void FixedUpdate(float updateTime)
	{

	}

	public virtual void Draw()
	{
		Main.SpriteBatch.Begin();
		tileMap.Draw();
		foreach (var entity in entities)
		{
			entity.Draw();
		}
		// SpriteFontBase font = Main.fontSystem.GetFont(20);
		// Main.SpriteBatch.DrawString(font, tileMap.selectPosition.ToString(), new Vector2(), Color.White);
		Main.SpriteBatch.End();
	}

	public void EntityRegisterEvent(Entity entity)
	{
		entities.Add(entity);
	}
}
