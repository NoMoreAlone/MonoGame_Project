using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace Light_Up_Your_Life.Core;

/// <summary>
/// ゲームのアセットを読み込むクラス
/// </summary>
public class AssetLoader
{
	//リソースフォルダーのパス
	private const string RESOURCES_PATH = @"\Data\Resources\";

	//一時的なリソース
	private static Dictionary<string, Texture2D> textures;
	private static Dictionary<string, string> audio;
	//TODO: フォントアセット関連の処理
	private static Dictionary<string, string[]> fonts;

	private AssetLoader() { }

	public static void LoadAssets(SynchronizationContext mainContext, IRegisterEvent registerAction, GraphicsDevice graphicsDevice, string mainGameName)
	{
		//ルートディレクトリを取得
		var rootDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + RESOURCES_PATH);

		//初期化
		textures = new Dictionary<string, Texture2D>();
		audio = new Dictionary<string, string>();
		fonts = new Dictionary<string, string[]>();

		//サブディレクトリを一覧取得
		foreach (var subDirectory in rootDirectory.GetDirectories())
		{
			var directoryName = subDirectory.Name;

			//ファイルを一覧取得
			foreach (var loadingFile in subDirectory.GetFiles("*", SearchOption.AllDirectories))
			{
				//ファイルの読み込み
				LoadingFile(graphicsDevice, mainGameName, directoryName, loadingFile);
			}
		}

		//最後にイベントをレジストリに登録する
		//登録したイベントの実行はMainクラスで行う
		registerAction.ResourcesRegisterEvent -= AddResources;
		registerAction.ResourcesRegisterEvent += AddResources;
	}

	/// <summary>
	/// リソースをレジストリに登録するイベント用メソッド
	/// </summary>
	/// <param name="resourcesRegistry"></param>
	/// <param name="textures"></param>
	/// <param name="audio"></param>
	private static void AddResources(ResourcesRegistry resourcesRegistry)
	{
		//読み込んだテクスチャを一つ一つレジストリに登録していく
		//テクスチャ
		foreach (var _texture in textures)
		{
			resourcesRegistry.AddTexture(_texture.Key, _texture.Value);
		}

		//オーディオ
		foreach (var _audio in audio)
		{
			resourcesRegistry.AddAudio(_audio.Key, _audio.Value);
		}
		//
	}

	/// <summary>
	/// ディレクトリの名前からアセットタイプを判定しそれぞれのメソッドを呼び出す
	/// </summary>
	/// <param name="graphicsDevice"></param>
	/// <param name="modName"></param>
	/// <param name="directoryName"></param>
	/// <param name="loadingFile"></param>
	private static void LoadingFile(GraphicsDevice graphicsDevice, string modName, string directoryName, FileInfo loadingFile)
	{
		//dummyファイルは読み込まない
		if (loadingFile.Extension == ".dummy")
			return;

		//ディレクトリの名前でどのアセットを読み込むか分岐させる
		switch (directoryName)
		{
			//Textures
			case ("Textures"):
				LoadingTexture(graphicsDevice, modName, loadingFile);
				break;
			//Audio
			case ("Audio"):
				LoadingAudio(modName, loadingFile);
				break;
		}
	}

	/// <summary>
	/// テクスチャを読み込む
	/// </summary>
	/// <param name="graphicsDevice"></param>
	/// <param name="modName"></param>
	/// <param name="loadingFile"></param>
	/// <param name="textures"></param>
	private static void LoadingTexture(GraphicsDevice graphicsDevice, string modName, FileInfo loadingFile)
	{
		Texture2D texture = default;
		//テクスチャの読み込み処理だけメインスレッドで行う
		texture = Texture2D.FromFile(graphicsDevice, loadingFile.FullName);
		var textureId = modName + "." + loadingFile.Name.Replace(loadingFile.Extension, "");
		DebugLog.SystemLog("Add Texture: " + textureId);
		textures.Add(textureId, texture);
	}

	/// <summary>
	/// オーディオを読み込む
	/// </summary>
	/// <param name="modName"></param>
	/// <param name="loadingFile"></param>
	/// <param name="audio"></param>
	private static void LoadingAudio(string modName, FileInfo loadingFile)
	{
		var audioId = modName + "." + loadingFile.Name.Replace(loadingFile.Extension, "");
		DebugLog.SystemLog("Add Audio: " + audioId);
		audio.Add(audioId, loadingFile.FullName);
	}
}
