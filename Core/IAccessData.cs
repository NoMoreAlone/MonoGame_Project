using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Light_Up_Your_Life.Core.Data;
using Light_Up_Your_Life.Core.Isometric;

namespace Light_Up_Your_Life.Core;

public interface IAccessData
{
	public Block GetBlock(string id);
	public bool HasBlock(Block block);
	public Item GetItem(string id);
}
