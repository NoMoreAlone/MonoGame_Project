using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Light_Up_Your_Life.Core.Data;
using Light_Up_Your_Life.Core.Isometric;
using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life.Core;

/// <summary>
/// リソースやアイテムなどの登録場所。
/// 構造的にはRegistryクラスの中にResourcesクラスやDataクラスがあり、
/// インターフェースを介してそれらにアクセスする
/// </summary>
public class Registry
{
	private static IAccessResources _resources = new Resources();
	public static IAccessResources GetResources { get { return _resources; } }

	private readonly RegisterAction _registryAction = new RegisterAction();
	public IAccessRegister GetAccessRegister { get { return _registryAction; } }
	public IRegisterEvent GetResourcesRegisterEvent { get { return _registryAction; } }

	private static Data _data = new Data();
	public static IAccessData GetData { get { return _data; } }
	public static IDataRegisterEvent GetDataRegisterEvent { get { return _data; } }

	/// <summary>
	/// レジストリの登録を行うクラス
	/// </summary>
	protected class RegisterAction : IAccessRegister, IRegisterEvent
	{
		private static SynchronizationContext _mainContext;

		//イベント
		public event IRegisterEvent.ResourcesRegisterEventHandler ResourcesRegisterEvent;

		/// <summary>
		/// 全データの登録(再登録)
		/// </summary>
		public void Register()
		{
			//初期化
			_resources = new Resources();
			_mainContext = new SynchronizationContext();
			//アセット読み込み
			AssetLoader.LoadAssets(_mainContext, Main.Registry.GetResourcesRegisterEvent, Main.Graphics.GraphicsDevice, Main.GameName);
			//登録イベント実行
			ResourcesRegisterEvent.Invoke((ResourcesRegistry)_resources);

			_data.Register();
		}
	}

	/// <summary>
	/// リソース関連の保管、動作を行う
	/// </summary>
	protected class Resources : ResourcesRegistry, IAccessResources
	{
		//リソース(テクスチャなど)
		protected override Dictionary<string, Texture2D> Textures { get; set; } = new Dictionary<string, Texture2D>();
		protected override Dictionary<string, string> Audio { get; set; } = new Dictionary<string, string>();

		//Get
		public Texture2D GetTexture(string id)
		{
			return Textures[id];
		}

		public string GetAudio(string id)
		{
			return Audio[id];
		}

		//Add
		public override void AddTexture(string id, Texture2D texture)
		{
			Textures.Add(id, texture);
		}

		public override void AddAudio(string id, string audio)
		{
			Audio.Add(id, audio);
		}
	}

	protected class Data : DataRegistry, IAccessData, IDataRegisterEvent
	{
		protected override Dictionary<string, Block> blocks { get; set; } = new Dictionary<string, Block>();
		protected override Dictionary<string, Item> items { get; set; } = new Dictionary<string, Item>();

		public event IDataRegisterEvent.RegisterEventHandler RegisterEvent;

		public void Register()
		{
			RegisterEvent.Invoke(this);
		}

		public Block GetBlock(string id)
		{
			return blocks[id];
		}

		public Item GetItem(string id)
		{
			return items[id];
		}

		public override void AddBlock(string id, Block block)
		{
			blocks.Add(id, block);
			DebugLog.SystemLog("Register Block: " + id);
		}

		public override void AddItem(string id, Item item)
		{
			items.Add(id, item);
			DebugLog.SystemLog("Register Item: " + id);
		}

		public bool HasBlock(Block block)
		{
			return blocks.ContainsValue(block);
		}
	}
}
