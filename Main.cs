using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FontStashSharp;
using Light_Up_Your_Life.Core;
using Light_Up_Your_Life.Core.Data;
using Light_Up_Your_Life.Core.Isometric;
using Light_Up_Your_Life.Core.Isometric.Entities;
using Light_Up_Your_Life.Core.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life;

public class Main : Game
{
	private float timer = 0;

	public static GraphicsDeviceManager Graphics { get; private set; }
	public static SpriteBatch SpriteBatch { get; private set; }
	public static Registry Registry { get; private set; }
	//ゲームのタイトル
	public static string GameName { get; private set; } = "Template";
	public static Scene CurrentScene { get; set; }
	public static List<Scene> Scenes { get; set; }
	public static FontSystem fontSystem;

	public Main()
	{
		Graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";

		//グラフィック初期化
		IsFixedTimeStep = false;
		Graphics.SynchronizeWithVerticalRetrace = false;
		Graphics.PreferredBackBufferWidth = 1280;
		Graphics.PreferredBackBufferHeight = 720;
		// _graphics.ToggleFullScreen();
		Graphics.ApplyChanges();

		Window.AllowUserResizing = true;
		IsMouseVisible = true;
	}

	protected override void Initialize()
	{
		try
		{
			//初期化
			Scenes = new List<Scene>();
			Registry = new Registry();
			Registry.GetDataRegisterEvent.RegisterEvent += RegisterData;
			Registry.GetAccessRegister.Register();

			// TODO: Add your initialization logic here
			TestInit();
		}
		catch (Exception exception)
		{
			DebugLog.ErrorLog(exception.Message, exception);
		}

		base.Initialize();
	}

	protected override void LoadContent()
	{
		SpriteBatch = new SpriteBatch(GraphicsDevice);

		fontSystem = new FontSystem();
		// fontSystem.AddFont(File.ReadAllBytes(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Data\Resources\Fonts\Main\NotoSansJP-Medium.otf"));

		// TODO: use this.Content to load your game content here
	}

	protected override void Update(GameTime gameTime)
	{
		try
		{
			// TODO: Add your update logic here
			timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
			float updateTime = 1f / 60;

			while (timer >= updateTime)
			{
				CurrentScene.FixedUpdate(updateTime);
				timer -= updateTime;
			}
			CurrentScene.Update();
		}
		catch (Exception exception)
		{
			DebugLog.ErrorLog(exception.Message, exception);
		}

		base.Update(gameTime);
	}

	private DateTime prevTime;
	private int framePerSec;

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		try
		{
			// TODO: Add your drawing code here
			CurrentScene.Draw();
		}
		catch (Exception exception)
		{
			DebugLog.ErrorLog(exception.Message, exception);
		}

		framePerSec++;
		DateTime now = DateTime.Now;

		TimeSpan t = now - prevTime;
		if (t.TotalMilliseconds >= 1000)
		{
			Window.Title = framePerSec + "fps";
			framePerSec = 0;
			prevTime = now;
		}

		base.Draw(gameTime);
	}

	private void RegisterData(DataRegistry dataRegistry)
	{
		var gridBlock = new Block("Grid_Block", Registry.GetResources.GetTexture("Template.Grid_Block"), GameName);
		gridBlock.isGlowing = true;
		gridBlock.glowLevel = 6;
		dataRegistry.AddBlock(gridBlock.id, gridBlock);

		var gridBlockBlack = new Block("Grid_Block_Black", Registry.GetResources.GetTexture("Template.Grid_Block_Black"), GameName);
		dataRegistry.AddBlock(gridBlockBlack.id, gridBlockBlack);

		var GridStairsDown = new Block("Grid_Stairs_Down", Registry.GetResources.GetTexture("Template.Grid_Stairs_Down"), GameName);
		GridStairsDown.slopeDirection = Block.SlopeDirection.Back;
		dataRegistry.AddBlock(GridStairsDown.id, GridStairsDown);

		var GridStairsLeft = new Block("Grid_Stairs_Left", Registry.GetResources.GetTexture("Template.Grid_Stairs_Left"), GameName);
		GridStairsLeft.slopeDirection = Block.SlopeDirection.Left;
		dataRegistry.AddBlock(GridStairsLeft.id, GridStairsLeft);

		var GridStairsRight = new Block("Grid_Stairs_Right", Registry.GetResources.GetTexture("Template.Grid_Stairs_Right"), GameName);
		GridStairsRight.slopeDirection = Block.SlopeDirection.Right;
		dataRegistry.AddBlock(GridStairsRight.id, GridStairsRight);

		var GridStairsTop = new Block("Grid_Stairs_Top", Registry.GetResources.GetTexture("Template.Grid_Stairs_Top"), GameName);
		GridStairsTop.slopeDirection = Block.SlopeDirection.Forward;
		dataRegistry.AddBlock(GridStairsTop.id, GridStairsTop);
	}

	private void TestInit()
	{
		var scene = new World();
		scene.isInputEnabled = true;
		scene.tileMap = new TileMap(Registry.GetResources.GetTexture("Template.Select_Grid"), new Vector2(32, 16));
		Scenes.Add(scene);
		CurrentScene = scene;
		for (int x = 0; x < 50; x++)
		{
			for (int y = 0; y < 50; y++)
			{
				CurrentScene.tileMap.SetTile(new Tile(Registry.GetData.GetBlock("Template.Grid_Block_Black"), new Vector3(x, y, 0), Color.White));
			}
		}

		for (int y = 0; y < 10; y++)
		{
			for (int z = 1; z < 6; z++)
			{
				CurrentScene.tileMap.SetTile(new Tile(Registry.GetData.GetBlock("Template.Grid_Block_Black"), new Vector3(0, y, z), Color.White));
			}
		}

		scene.LightingInit();

		var entity = new Player(Registry.GetResources.GetTexture("Template.Dummy"), new Vector3(10, 10, 1));
	}
}
