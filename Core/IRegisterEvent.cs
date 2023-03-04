namespace Light_Up_Your_Life.Core;

/// <summary>
/// リソースをレジストリに登録するイベント
/// </summary>
public interface IRegisterEvent
{
	public delegate void ResourcesRegisterEventHandler(ResourcesRegistry addResources);
	public event ResourcesRegisterEventHandler ResourcesRegisterEvent;
}
