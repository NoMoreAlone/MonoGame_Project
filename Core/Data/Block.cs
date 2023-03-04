using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life.Core.Data;

public class Block
{
	public string name;
	public readonly string id;
	public string description;
	public Texture2D texture;
	public bool _isPassable = true;
	public SlopeDirection slopeDirection = SlopeDirection.NotSlope;
	public bool isGlowing;
	public int glowLevel;

	public enum SlopeDirection
	{
		Forward,
		Left,
		Right,
		Back,
		NotSlope
		// 斜めも追加するかも
	}

	public Block(string id, Texture2D texture, string gameName)
	{
		this.id = gameName + "." + id;
		this.texture = texture;
	}
}
