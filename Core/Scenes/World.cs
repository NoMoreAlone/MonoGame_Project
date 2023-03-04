using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Light_Up_Your_Life.Core.Isometric;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Light_Up_Your_Life.Core.Scenes;

public class World : Scene
{
	private readonly float SCREEN_MOVE_SPEED = 10;
	private int _scrollWheelValueCache;
	private bool _isPressed_LeftMouse;
	private bool _isPressed_RightMouse;
	private Lighting _lighting;
	private CancellationTokenSource cts;
	private bool result = true;

	public void LightingInit()
	{
		_lighting = new Lighting(tileMap, new Color(40, 40, 40), 1, 2);
		cts = new CancellationTokenSource();
		result = false;
		result = _lighting.Update(cts.Token);
	}

	public void LightingUpdate()
	{
		if (!result)
			cts.Cancel();

		result = false;
		result = _lighting.Update(cts.Token);
	}

	public enum TileSide
	{
		Left,
		Right
	}
	public bool isInputEnabled;

	public override void Update()
	{
		base.Update();

		if (!isInputEnabled)
			return;

		MouseInput();
	}

	public override void FixedUpdate(float updateTime)
	{
		base.FixedUpdate(updateTime);

		//入力を受け付けている間のみ実行
		//入力を受け付けている間のみ実行
		//入力を受け付けていな場合はtileMapのタイル選択をオフにする
		if (!isInputEnabled)
			return;

		KeyboardInput();
		MouseWheelInput();
	}

	/// <summary>
	/// キーボード入力
	/// </summary>
	private void KeyboardInput()
	{
		//WASDキーでスクリーンポジション(カメラの位置)を動かす
		if (Keyboard.GetState().IsKeyDown(Keys.W))
		{
			screenPosition.Y -= SCREEN_MOVE_SPEED;
		}
		if (Keyboard.GetState().IsKeyDown(Keys.A))
		{
			screenPosition.X -= SCREEN_MOVE_SPEED;
		}
		if (Keyboard.GetState().IsKeyDown(Keys.S))
		{
			screenPosition.Y += SCREEN_MOVE_SPEED;
		}
		if (Keyboard.GetState().IsKeyDown(Keys.D))
		{
			screenPosition.X += SCREEN_MOVE_SPEED;
		}
	}

	/// <summary>
	/// マウス入力
	/// </summary>
	private void MouseInput()
	{
		//マウスと重なっているタイルで一番手前のタイルを取得する
		var mousePosition = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
		var selectTileResult = GetSelectTile(mousePosition);

		tileMap.isSelect = selectTileResult.isSuccess;

		//タイルマップの選択座標に代入
		tileMap.selectPosition = selectTileResult.position;

		//光源ブロック設置処理
		if (Mouse.GetState().RightButton == ButtonState.Pressed)
		{
			if (_isPressed_RightMouse)
				return;

			var tile = new Tile(Registry.GetData.GetBlock("Template.Grid_Block"), tileMap.selectPosition + new Vector3(0, 0, 1), Color.White);
			tile.isGlowing = true;
			tile.glowLevel = 6;
			tileMap.SetTile(tile);
			LightingUpdate();
			_isPressed_RightMouse = true;
		}
		else
		{
			_isPressed_RightMouse = false;
		}

		//ブロック設置/破壊
		if (Mouse.GetState().LeftButton == ButtonState.Pressed)
		{
			if (_isPressed_LeftMouse)
				return;

			if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
			{
				tileMap.RemoveTile(tileMap.selectPosition);
				LightingUpdate();
			}
			else
			{
				var tile = new Tile(Registry.GetData.GetBlock("Template.Grid_Block_Black"), tileMap.selectPosition + new Vector3(0, 0, 1), Color.White);
				tileMap.SetTile(tile);
				LightingUpdate();
			}
			_isPressed_LeftMouse = true;
		}
		else
		{
			_isPressed_LeftMouse = false;
		}
	}

	/// <summary>
	/// マウスホイール入力
	/// </summary>
	private void MouseWheelInput()
	{
		var scrollValue = Mouse.GetState().ScrollWheelValue;

		//マウスホイールを上に回した場合
		if (scrollValue > _scrollWheelValueCache)
		{
			tileMap.visibleMaxZ += 1;

			//上限
			tileMap.visibleMaxZ = (int)MathF.Min(tileMap.visibleMaxZ, tileMap.tileMapMaxZ);
		}

		//マウスホイールを下に回した場合
		if (scrollValue < _scrollWheelValueCache)
		{
			tileMap.visibleMaxZ -= 1;

			//下限
			tileMap.visibleMaxZ = (int)MathF.Max(tileMap.visibleMaxZ, 0);
		}

		//scrollValueは現在までのスクロールの蓄積値なのでキャッシュを取っておく
		//そのキャッシュを使って新しくマウスホイールを回したか判定する
		_scrollWheelValueCache = scrollValue;
	}

	//選択中タイル判定メソッド
	private SelectTileResult GetSelectTile(Vector2 mousePosition)
	{
		var overlapTiles = GetOverlapTiles(mousePosition);
		return GetResult(overlapTiles, mousePosition);
	}

	//マウスカーソルと重なっている縦列のタイルを抽出
	private Tile[] GetOverlapTiles(Vector2 mousePosition)
	{
		List<Tile> positions = new List<Tile>();

		foreach (var tile in tileMap.GetAllTiles())
		{
			var tileSize = new Vector2(tileMap.gridSize.X, tileMap.gridSize.Y * 2);
			//タイルのXサイズの中にマウスカーソルが含まれている場合は戻り地として返す
			if (mousePosition.X >= tile.Value.screenPosition.X && mousePosition.X <= tile.Value.screenPosition.X + tileMap.gridSize.X)
			{
				positions.Add(tile.Value);
			}
		}

		return positions.ToArray();
	}

	//抽出した縦列のタイルからマウスカーソルと完全に重なっているタイルを返す
	private SelectTileResult GetResult(Tile[] overlapTiles, Vector2 mousePosition)
	{
		//2Dでタイルに高さがあるゲームなのでマウスと完全に重なっているタイルが複数ある場合がある
		//タイルは下に行くほど最後に、高くなるほど更に最後になるようにリストに保持しているので
		//重なったタイルを全て順番に判定(古いものは新しいもので上書き)すれば自然と手前のタイルを選択したようにできる。
		var overlap = new Vector3();
		var isSuccess = false;

		foreach (var tile in overlapTiles)
		{
			var tileSize = new Vector2(tileMap.gridSize.X, tileMap.gridSize.Y * 2);
			var tileRect = new Rectangle(new Point((int)tile.screenPosition.X, (int)tile.screenPosition.Y), new Point((int)tileSize.X, (int)tileSize.Y));
			//正方形でタイルとマウスが重なっているか判定
			if (!tileRect.Contains(mousePosition))
				continue;

			//ひし形でタイルとマウスが重なっているか判定
			var pa = new Vector2(tile.screenPosition.X, tile.screenPosition.Y + tileMap.gridSize.Y / 2);
			var pb = new Vector2(tile.screenPosition.X + tileMap.gridSize.X / 2, tile.screenPosition.Y);
			var pc = new Vector2(tile.screenPosition.X + tileMap.gridSize.X, tile.screenPosition.Y + tileMap.gridSize.Y / 2);
			var pd = new Vector2(tile.screenPosition.X + tileMap.gridSize.X / 2, tile.screenPosition.Y + tileMap.gridSize.Y);

			//マウスがタイルの上部だった場合はそのまま、下部だった場合は下にずらす
			if (mousePosition.Y >= tile.screenPosition.Y + tileMap.gridSize.Y)
			{
				pa.Y += tileMap.gridSize.Y;
				pb.Y += tileMap.gridSize.Y;
				pc.Y += tileMap.gridSize.Y;
				pd.Y += tileMap.gridSize.Y;
			}

			if (!IsInSquare(pa, pb, pc, pd, mousePosition))
			{
				var y1 = tile.screenPosition.Y + tileMap.gridSize.Y / 2;
				var y2 = y1 + tileMap.gridSize.Y;
				//正方形で重なったけど、ひし形で重ならなかった場合は、タイルと重なっているけど判定外だった位置なのかを判定する
				if (!(y1 <= mousePosition.Y && y2 >= mousePosition.Y))
					continue;
			}

			//全ての条件を達成できたら返す
			overlap = tile.position;
			isSuccess = true;
		}

		return new SelectTileResult(overlap, isSuccess);
	}

	//ひし形の当たり判定メソッド---
	private bool IsInSquare(Vector2 pa, Vector2 pb, Vector2 pc, Vector2 pd, Vector2 p)
	{
		float a = CalcExteriorProduct(pa, pb, p);
		float b = CalcExteriorProduct(pb, pc, p);
		float c = CalcExteriorProduct(pc, pd, p);
		float d = CalcExteriorProduct(pd, pa, p);

		bool res = a > 0 && b > 0 && c > 0 && d > 0;

		return res;
	}

	private float CalcExteriorProduct(Vector2 a, Vector2 b, Vector2 p)
	{
		var vecab = new Vector2(a.X - b.X, a.Y - b.Y);
		var vecpa = new Vector2(a.X - p.X, a.Y - p.Y);

		float ext = vecab.X * vecpa.Y - vecpa.X * vecab.Y;

		return ext;
	}
	//---

	private class SelectTileResult
	{
		public readonly Vector3 position;
		public readonly bool isSuccess;

		public SelectTileResult(Vector3 position, bool isSuccess)
		{
			this.position = position;
			this.isSuccess = isSuccess;
		}
	}
}
