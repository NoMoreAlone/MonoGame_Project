using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Light_Up_Your_Life.Core.Isometric;

public class Lighting
{
	public int maxLightingLevel = 255;

	private TileMap tileMap;
	private Dictionary<Vector3, Tile> glowBlocks = new Dictionary<Vector3, Tile>();
	Dictionary<Vector3, float> glowLevels = new Dictionary<Vector3, float>();
	private List<Vector3> open = new List<Vector3>();
	private List<Vector3> closed = new List<Vector3>();
	private List<Vector3> temp = new List<Vector3>();
	private Color defaultColor;
	private int maxShadow;
	private int maxLight;
	private Vector3[] directions1 =
	{
			new Vector3(-1, 1, 0),
			new Vector3(1, 1, 0),
			new Vector3(-1, -1, 0),
			new Vector3(1, -1, 0),
			new Vector3(0, 1, 0),
			new Vector3(-1, 0, 0),
			new Vector3(1, 0, 0),
			new Vector3(0, -1, 0),
	};
	private Vector3[] directions2 =
	{
			new Vector3(0, 1, 0),
			new Vector3(-1, 0, 0),
			new Vector3(1, 0, 0),
			new Vector3(0, -1, 0),
	};
	private CancellationToken _token;

	public Lighting(TileMap updateTileMap, Color defaultTileColor, int maxShadow, int maxLight)
	{
		tileMap = updateTileMap;
		defaultColor = defaultTileColor;
		this.maxShadow = maxShadow;
		this.maxLight = maxLight;
	}

	public bool Update(CancellationToken token)
	{
		_token = token;
		glowLevels.Clear();
		glowBlocks.Clear();
		open.Clear();
		closed.Clear();
		temp.Clear();
		return update().GetAwaiter().GetResult();
	}

	private async Task<bool> update()
	{
		return await Task.Run(async () =>
		{
			await LightingInit();
			await LightingUpdate();
			await ShadowUpdate();
			return true;
		});
	}

	private async Task LightingInit()
	{
		if (_token.IsCancellationRequested)
			return;

		foreach (var tile in tileMap.GetAllTiles())
		{
			if (_token.IsCancellationRequested)
				break;

			await Task.Run(() =>
			{
				if (tile.Value.isGlowing)
				{

					glowBlocks.Add(tile.Value.position, tile.Value);
					tileMap.SetColor(tile.Value.position, new Color((int)maxLightingLevel, (int)maxLightingLevel, (int)maxLightingLevel));
					tileMap.SetColor(tile.Value.position + new Vector3(0, 0, -1), new Color((int)maxLightingLevel, (int)maxLightingLevel, (int)maxLightingLevel));
				}
				else
				{
					glowLevels.Add(tile.Value.position, defaultColor.R);
					tileMap.SetColor(tile.Value.position, defaultColor);
				}
			});
		}
	}

	private async Task LightingUpdate()
	{
		if (_token.IsCancellationRequested)
			return;

		foreach (var block in glowBlocks)
		{
			if (_token.IsCancellationRequested)
				break;

			await Task.Run(() =>
			{
				var startPos = block.Key + new Vector3(0, 0, -1);
				temp.Add(startPos);
				var glowingValue = block.Value.glowLevel;
				for (int i = 0; i < glowingValue; i++)
				{
					foreach (var tempPos in temp)
					{
						Vector3[] directions = directions2;
						// if (i == 0)
						// {
						//     directions = directions1;
						// }
						foreach (var direction in directions)
						{
							for (int z = 0; z <= maxLight; z++)
							{
								var blockPos = tempPos + direction + new Vector3(0, 0, z);
								if (blockPos.Z > block.Key.Z - 1 + maxLight)
									continue;

								if (tileMap.HasTile(blockPos + new Vector3(0, 0, 1)))
									continue;

								if (!tileMap.HasTile(blockPos))
									continue;

								if (temp.FindIndex((v) => v == blockPos) != -1)
									continue;

								if (open.FindIndex((v) => v == blockPos) != -1)
									continue;

								if (closed.FindIndex((v) => v == blockPos) != -1)
									continue;

								if (glowBlocks.ContainsKey(blockPos))
									continue;

								var currentColor = tileMap.GetTile(blockPos).color;
								var lightingLevel = (float)maxLightingLevel * (1 / (Vector3.Distance(block.Key, blockPos))) + glowLevels[blockPos];

								if (currentColor.R != defaultColor.R)
								{
									lightingLevel /= 2;
								}

								glowLevels[blockPos] += lightingLevel;
								glowLevels[blockPos] = Math.Min(glowLevels[blockPos], maxLightingLevel);

								tileMap.SetColor(blockPos, new Color((int)lightingLevel, (int)lightingLevel, (int)lightingLevel));
								open.Add(blockPos);
							}
						}
					}
					foreach (var tempPos in temp)
					{
						closed.Add(tempPos);
					}
					temp.Clear();
					foreach (var openPos in open)
					{
						temp.Add(openPos);
					}
					open.Clear();
				}
				open.Clear();
				closed.Clear();
				temp.Clear();
			});
		}
	}

	private async Task ShadowUpdate()
	{
		if (_token.IsCancellationRequested)
			return;

		foreach (var block in tileMap.GetAllTiles())
		{
			if (_token.IsCancellationRequested)
				break;

			await Task.Run(() =>
			{
				for (int i = 1; i <= maxShadow; i++)
				{
					var position = block.Value.position + new Vector3(0, 0, -i);
					if (tileMap.HasTile(position))
					{
						var currentColor = tileMap.GetTile(position).color;
						tileMap.SetColor(position, new Color((int)(currentColor.R * 0.4f), (int)(currentColor.G * 0.4f), (int)(currentColor.B * 0.4f)));
						break;
					}
				}
			});
		}
	}
}
