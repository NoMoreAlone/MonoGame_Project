using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Light_Up_Your_Life.Core.Data;

namespace Light_Up_Your_Life.Core;

public abstract class DataRegistry
{
	protected abstract Dictionary<string, Block> blocks { get; set; }
	protected abstract Dictionary<string, Item> items { get; set; }

	public abstract void AddBlock(string id, Block block);
	public abstract void AddItem(string id, Item item);
}
