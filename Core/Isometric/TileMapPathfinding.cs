using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Light_Up_Your_Life.Core.Isometric;

public class TileMapPathfinding
{
	private TileMap _tileMap;
	private Vector3 _startPosition;
	private Vector3 _endPosition;
	private CancellationToken _token;

	private Node _currentNode;
	private int _passableHeight;

	private List<Node> _openNode = new List<Node>();
	private List<Node> _closedNode = new List<Node>();
	private List<TileProperties> _tileProperties = new List<TileProperties>();

	private Vector3[] _searchDirection = new Vector3[4]
	{
			new Vector3(0, 1, 0),
			new Vector3(-1, 0, 0),
			new Vector3(1, 0, 0),
			new Vector3(0, -1, 0)
	};

	public TileMapPathfinding(TileMap tilemap, int passableHeight)
	{
		_tileMap = tilemap;
		_passableHeight = passableHeight;
	}

	public Vector3[] StartPathfinding(Vector3 start, Vector3 end, CancellationToken token)
	{
		_token = token;
		if (!StartingCheck(start, end))
		{
			return null;
		}

		Init(start, end);

		return Task.Run(async () =>
		{
			return await Pathfinding();
		}).GetAwaiter().GetResult();
	}

	private bool StartingCheck(Vector3 start, Vector3 end)
	{
		if (!_tileMap.HasTile(start))
		{
			return false;
		}
		if (!_tileMap.HasTile(end))
		{
			return false;
		}

		return true;
	}

	private void Init(Vector3 start, Vector3 end)
	{
		_startPosition = start;
		_endPosition = end;
		_currentNode = new Node(start, null, 0, 0);

		_openNode.Clear();
		_closedNode.Clear();
		_tileProperties.Clear();

		foreach (var tile in _tileMap.GetAllTiles())
		{
			var position = tile.Value.position;
			if (!_tileMap.HasTile(position))
			{
				continue;
			}
			var hasTileHeight = false;
			for (int z = 0; z < _passableHeight + 1; z++)
			{
				if (_tileMap.HasTile(position + new Vector3(0, 0, z + 1)))
				{
					hasTileHeight = true;
					break;
				}
			}
			if (!hasTileHeight)
			{
				var hasTile = Registry.GetData.HasBlock(_tileMap.GetTile(position).block);
				var matchBlock = Registry.GetData.GetBlock(_tileMap.GetTile(position).block.id);
				if (!hasTile)
					return;

				if (matchBlock.slopeDirection == Data.Block.SlopeDirection.NotSlope)
				{
					_tileProperties.Add(new TileProperties(position, true, TileProperties.SlopeDirection.NotSlope));
				}
				else if (matchBlock.slopeDirection != Data.Block.SlopeDirection.NotSlope)
				{
					int direction = (int)matchBlock.slopeDirection;
					_tileProperties.Add(new TileProperties(position, true, (TileProperties.SlopeDirection)direction));
				}
			}
		}
	}

	private async Task<Vector3[]> Pathfinding()
	{
		return await Task.Run(() =>
		{
			for (int i = 0; i < _tileProperties.Count; i++)
			{
				if (_currentNode == null)
				{
					return null;
				}
				if (CheckGoal())
				{
					return GetPath();
				}
				AddOpenNode();
				AddClosedNode();
				if (!SetCurrent())
				{
					return null;
				}
			}
			if (_token.IsCancellationRequested)
			{
				return null;
			}

			return null;
		});
	}

	private bool CheckGoal()
	{
		if (_currentNode.GetPosition == _endPosition)
		{
			return true;
		}

		return false;
	}

	private Vector3[] GetPath()
	{
		var pathList = new List<Vector3>();
		pathList.Add(_currentNode.GetPosition);
		var currentNode = _currentNode;
		while (true)
		{
			if (currentNode.GetParentNode == null)
			{
				pathList.Reverse();
				break;
			}
			if (currentNode.GetParentNode.GetParentNode != null)
			{
				var current = currentNode.GetPosition - currentNode.GetParentNode.GetPosition;
				var next = currentNode.GetParentNode.GetParentNode.GetPosition - currentNode.GetParentNode.GetPosition;
				if (current.X == next.Y || current.Y == next.X)
				{
					pathList.Add(currentNode.GetParentNode.GetParentNode.GetPosition);
					currentNode = currentNode.GetParentNode.GetParentNode;
					continue;
				}
			}
			pathList.Add(currentNode.GetParentNode.GetPosition);
			currentNode = currentNode.GetParentNode;
		}
		var path = pathList.ToArray();
		foreach (var pos in path)
		{
			DebugLog.SystemLog("Pathfinding Route: " + pos);
		}
		return path;
	}

	private void AddOpenNode()
	{
		for (int z = -1; z < 2; z++)
		{
			foreach (var direction in _searchDirection)
			{
				AddOpenNodeList(GetSearchPosition(direction, z), z);
			}

			if (z != 0)
			{
				AddOpenNodeList(GetSearchPosition(new Vector3(0, 0, 0), z), z);
			}
		}
	}

	private void AddClosedNode()
	{
		_closedNode.Add(_currentNode);
		var removeNode = _openNode.Find(_ => _.GetPosition == _currentNode.GetPosition);
		if (removeNode != null)
		{
			_openNode.Remove(removeNode);
		}
	}

	private bool SetCurrent()
	{
		var minNodeAll = _openNode.FindAll(_ => Math.Truncate(_.GetRoute + _.GetDirectRoute * 100) / 100 == _openNode.Min(_ => Math.Truncate(_.GetRoute + _.GetDirectRoute * 100) / 100));
		var minNode = minNodeAll.Find(_ => Math.Truncate(_.GetDirectRoute * 100) / 100 == (minNodeAll.Min(_ => (Math.Truncate(_.GetDirectRoute * 100) / 100))));
		_currentNode = minNode;
		return true;
	}

	private Vector3 GetSearchPosition(Vector3 direction, int z)
	{
		var searchX = _currentNode.GetPosition.X + direction.X;
		var searchY = _currentNode.GetPosition.Y + direction.Y;
		var searchZ = _currentNode.GetPosition.Z + direction.Z + z;
		return new Vector3(searchX, searchY, searchZ);
	}

	private void AddOpenNodeList(Vector3 searchPosition, int z)
	{
		if (_closedNode.Find(_ => _.GetPosition == searchPosition) != null)
			return;

		var tileProperties = _tileProperties.Find(_ => _.GetPosition == searchPosition);
		var currentTileProperties = _tileProperties.Find(_ => _.GetPosition == _currentNode.GetPosition);
		if (tileProperties == null)
			return;

		if (tileProperties.IsPassable == false)
			return;

		if (_openNode.Find(_ => _.GetPosition == searchPosition) != null)
			return;

		if (z == 1)
		{
			if (tileProperties.GetSlopeDirection == TileProperties.SlopeDirection.NotSlope)
				return;

			var searchDirection = searchPosition - (_searchDirection[(int)tileProperties.GetSlopeDirection] + new Vector3(0, 0, 1));
			if (_currentNode.GetPosition != searchDirection)
				return;
		}
		if (z == -1)
		{
			if (currentTileProperties.GetSlopeDirection == TileProperties.SlopeDirection.NotSlope)
				return;

			var searchDirection = _currentNode.GetPosition - (_searchDirection[(int)currentTileProperties.GetSlopeDirection] + new Vector3(0, 0, 1));
			if (searchPosition != searchDirection)
				return;

		}

		_openNode.Add(new Node(searchPosition, _currentNode, _currentNode.GetRoute + 1, Vector3.Distance(_endPosition, searchPosition)));
	}
}

public class Node
{
	private Vector3 _position;
	private Node _parentNode;
	private float _route;
	private float _directRoute;

	public Vector3 GetPosition { get { return _position; } }
	public Node GetParentNode { get { return _parentNode; } }
	public float GetRoute { get { return _route; } }
	public float GetDirectRoute { get { return _directRoute; } }

	public float SetRoute { set { _route = value; } }
	public Node SetParentNode { set { _parentNode = value; } }

	public Node(Vector3 position, Node parentNode, float route, float directRoute)
	{
		_position = position;
		_parentNode = parentNode;
		_route = route;
		_directRoute = directRoute;
	}
}

public class TileProperties
{
	public enum SlopeDirection
	{
		Forward,
		Left,
		Right,
		Back,
		NotSlope
		// 斜めも追加するかも
	}

	private Vector3 _position;
	private bool _isPassable;
	private SlopeDirection _slopeDirection;

	public Vector3 GetPosition { get { return _position; } }
	public bool IsPassable { get { return _isPassable; } }
	public SlopeDirection GetSlopeDirection { get { return _slopeDirection; } }

	public TileProperties(Vector3 position, bool isPassable, SlopeDirection slopeDirection)
	{
		_position = position;
		_isPassable = isPassable;
		_slopeDirection = slopeDirection;
	}
}
