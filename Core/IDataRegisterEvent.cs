using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Light_Up_Your_Life.Core;

public interface IDataRegisterEvent
{
	public delegate void RegisterEventHandler(DataRegistry dataRegistry);
	public event RegisterEventHandler RegisterEvent;
}
