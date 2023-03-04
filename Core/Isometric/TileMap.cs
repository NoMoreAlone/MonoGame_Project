using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Light_Up_Your_Life.Core.Isometric;

/// <summary>
/// タイルをまとめて管理、描画するクラス
/// </summary>
public class TileMap
{
	//このタイルマップ上に存在するタイル
	protected Dictionary<Vector3, Tile> _tiles { get; private set; }
	private List<Tile> _drawTiles;
	private List<KeyValuePair<Vector3, Tile>> sortTiles;

	public readonly Texture2D selectTexture;
	public readonly Vector2 gridSize;
	public bool isSelect;
	public Vector3 selectPosition;
	public Vector3 offset;
	public int visibleMaxZ;
	public int tileMapMaxZ { get; private set; }

	public TileMap(Texture2D selectTexture, Vector2 gridSize)
	{
		_tiles = new Dictionary<Vector3, Tile>();
		_drawTiles = new List<Tile>();
		this.selectTexture = selectTexture;
		this.gridSize = gridSize;
		isSelect = false;
		selectPosition = new Vector3();
		offset = new Vector3();
		visibleMaxZ = 0;
		tileMapMaxZ = 0;
	}

	public List<KeyValuePair<Vector3, Tile>> GetAllTiles()
	{
		return sortTiles.ToList();
	}

	//ポジションからタイルを取得する
	public Tile GetTile(Vector3 position)
	{
		return _tiles[position];
	}

	public void RemoveTile(Vector3 position)
	{
		_tiles.Remove(position);
	}

	public void SetColor(Vector3 position, Color color)
	{
		_tiles[position].color = color;
	}

	//タイルを追加する
	public void SetTile(Tile tile)
	{
		var hasTile = _tiles.ContainsKey(tile.position);
		if (hasTile)
			_tiles[tile.position] = tile;//タイルが存在しない場合は追加
		else
			_tiles.Add(tile.position, tile); //タイルが存在する場合は上書き

		bool isModifiedMaxZ = false;

		//追加するタイルのZが一番上ならtileMapMaxZを更新する
		if (tile.position.Z > tileMapMaxZ)
		{
			tileMapMaxZ = (int)tile.position.Z;
			isModifiedMaxZ = true;
		}

		TileModified(isModifiedMaxZ);
	}

	public bool HasTile(Vector3 position)
	{
		return _tiles.ContainsKey(position);
	}

	/// <summary>
	/// マウスの座標と同じグリッドの中心のスクリーン座標を取得する
	/// </summary>
	/// <param name="mousePosition"></param>
	/// <returns></returns>
	public Vector2 GetGridCenter(Vector2 mousePosition)
	{
		//マウス座標をタイル座標に変換してからそのタイル座標をスクリーン座標に変換する

		var offset = new Vector2(this.offset.X, this.offset.Y);
		var gridOffset = new Vector2(this.gridSize.X / 2, (this.gridSize.Y / 2) / 2);
		mousePosition = mousePosition - offset - gridOffset + new Vector2(0, 5);
		var mouseTileX = MathF.Floor((-mousePosition.Y / gridSize.Y) + (mousePosition.X / gridSize.X));
		var mouseTileY = MathF.Floor((mousePosition.X / gridSize.X) + (mousePosition.Y / gridSize.Y));
		mouseTileX += 1;

		//タイル座標からスクリーン座標を計算する
		var x = mouseTileX * gridSize.X / 2 + mouseTileY * gridSize.X / 2 + this.offset.X;
		var y = mouseTileY * gridSize.Y / 2 - mouseTileX * gridSize.Y / 2 + this.offset.Y;

		return new Vector2(x, y) + new Vector2(gridSize.X / 2, gridSize.Y / 2);
	}

	public Vector2 GetGridScreenPosition(Vector3 gridPosition)
	{
		var x = gridPosition.X * gridSize.X / 2 + gridPosition.Y * gridSize.X / 2 + this.offset.X;
		var y = gridPosition.Y * gridSize.Y / 2 - gridPosition.X * gridSize.Y / 2 + this.offset.Y;
		y -= (gridPosition.Z - 1) * (gridSize.Y * 2 / 2);

		return new Vector2(x, y) + new Vector2(gridSize.X, gridSize.Y);
	}

	//描画タイルの座標計算
	public void Update()
	{
		_drawTiles.Clear();

		foreach (var tile in sortTiles)
		{
			//表示制限値を超える高さのタイルは描画しない
			if (tile.Value.position.Z > visibleMaxZ)
				continue;

			var position = tile.Value.position;

			//タイル座標からスクリーン座標を計算する
			var x = position.X * gridSize.X / 2 + position.Y * gridSize.X / 2 + offset.X;
			var y = position.Y * gridSize.Y / 2 - position.X * gridSize.Y / 2 + offset.Y;
			var z = position.Z * gridSize.Y + offset.Z;

			//スクリーンのポジションに合わせて移動させる
			x -= Main.CurrentScene.screenPosition.X;
			y -= Main.CurrentScene.screenPosition.Y;

			var screenPosition = new Vector2(x, y - z);

			//タイルのスクリーン座標をタイルクラスに代入する
			_tiles[tile.Value.position].screenPosition = screenPosition + new Vector2(gridSize.X / 2, gridSize.Y / 2);

			_drawTiles.Add(tile.Value);
		}
	}

	private void TileModified(bool isModifiedMaxZ)
	{
		var maxPosition = _tiles.Max((_tile) => { return _tile.Value.position.X + _tile.Value.position.Y; });
		sortTiles = _tiles.ToList();
		sortTiles.Sort((_tileA, _tileB) => { return (int)(_tileB.Value.position.X - _tileB.Value.position.Y - _tileB.Value.position.Z * maxPosition) - (int)(_tileA.Value.position.X - _tileA.Value.position.Y - _tileA.Value.position.Z * maxPosition); });
		if (isModifiedMaxZ)
			visibleMaxZ = tileMapMaxZ;
	}

	public void Draw()
	{
		foreach (var tile in _drawTiles)
		{
			//描画
			if (tile.position == selectPosition)
			{
				// タイル選択が有効の場合のみ選択中タイルを表示する
				// タイル選択がオフになっている場合は通常通りタイルを表示する
				if (isSelect)
				{
					Main.SpriteBatch.Draw(tile.texture, tile.screenPosition, null, Color.Blue, 0, new Vector2(), new Vector2(1, 1), SpriteEffects.None, 0); //選択中タイル
					continue;
				}
			}

			//選択中タイル以外の場合
			Main.SpriteBatch.Draw(tile.texture, tile.screenPosition, null, tile.color, 0, new Vector2(), new Vector2(1, 1), SpriteEffects.None, 0);
		}
	}
}
