using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Light_Up_Your_Life.Core.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life.Core.Isometric;

/// <summary>
/// マップ上に存在する一つのブロック(タイル)のクラス
/// </summary>
public class Tile
{
	public Texture2D texture;
	public Vector3 position;
	public Vector2 screenPosition;
	public Color color;
	public bool isGlowing;
	public int glowLevel;
	public Block block;

	public Tile(Block block, Vector3 position, Color color)
	{
		texture = block.texture;
		this.block = block;
		this.position = position;
		this.color = color;
	}
}
