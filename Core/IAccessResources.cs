using Microsoft.Xna.Framework.Graphics;

namespace Light_Up_Your_Life.Core;

/// <summary>
/// Resourcesクラスへのアクセス用インターフェース
/// </summary>
public interface IAccessResources
{
	public Texture2D GetTexture(string id);
	public string GetAudio(string id);
}
