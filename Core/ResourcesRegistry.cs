using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life.Core;

/// <summary>
/// レジストリ用のリソースと外部からアクセスする際に使用するメソッドを提供するアブストラクトクラス
/// </summary>
public abstract class ResourcesRegistry
{
	protected abstract Dictionary<string, Texture2D> Textures { get; set; }
	protected abstract Dictionary<string, string> Audio { get; set; }

	public abstract void AddTexture(string id, Texture2D texture);
	public abstract void AddAudio(string id, string audio);
}
