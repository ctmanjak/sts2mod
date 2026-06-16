using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;

namespace IntegratedStrategyEvents.TreeHoles;

internal sealed class IntegratedStrategyTreeHoleActMap : ActMap
{
	private const int GridWidth = 7;
	private const int MapLength = 5;
	private const int RouteCount = 3;
	private const int CenterColumn = 3;
	private const int StartRow = 0;
	private const int FirstRow = 1;
	private const int BranchStartRow = 2;
	private const int BranchEndRow = MapLength - 1;
	private const int TerminalRow = MapLength;
	private const int BossRow = MapLength + 1;
	private static readonly int[] RouteColumns = [1, CenterColumn, 5];

	private readonly MapCoord _terminalCoord;

	protected override MapPoint?[,] Grid { get; }

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	public MapCoord TerminalCoord => _terminalCoord;

	public static IntegratedStrategyTreeHoleActMap Create(Rng rng)
	{
		return new IntegratedStrategyTreeHoleActMap(rng);
	}

	public bool IsTerminalCoord(MapCoord? coord)
	{
		return coord is { } value && value.Equals(_terminalCoord);
	}

	public bool HasVisitedTerminal(IEnumerable<MapCoord> visitedCoords)
	{
		return visitedCoords.Any(coord => coord.Equals(_terminalCoord));
	}

	private IntegratedStrategyTreeHoleActMap(Rng rng)
	{
		Grid = new MapPoint[GridWidth, MapLength + 1];
		StartingMapPoint = CreateSpecialPoint(CenterColumn, StartRow, MapPointType.Ancient);
		BossMapPoint = CreateSpecialPoint(CenterColumn, BossRow, MapPointType.Boss);

		bool hasShop = false;
		int eliteFreeLane = rng.NextInt(RouteCount);

		MapPoint first = CreatePathPoint(
			CenterColumn,
			FirstRow,
			RollPointType(rng, isFirstNode: true, allowElite: false, ref hasShop));
		StartingMapPoint.AddChildPoint(first);
		startMapPoints.Add(first);

		MapPoint[] previousRow = CreateBranchRow(rng, BranchStartRow, eliteFreeLane, ref hasShop);
		foreach (MapPoint point in previousRow)
		{
			first.AddChildPoint(point);
		}

		for (int row = BranchStartRow + 1; row <= BranchEndRow; row++)
		{
			MapPoint[] currentRow = CreateBranchRow(rng, row, eliteFreeLane, ref hasShop);
			for (int lane = 0; lane < RouteCount; lane++)
			{
				previousRow[lane].AddChildPoint(currentRow[lane]);
			}

			previousRow = currentRow;
		}

		MapPoint terminal = CreatePathPoint(
			CenterColumn,
			TerminalRow,
			RollTerminalPointType(rng));
		foreach (MapPoint point in previousRow)
		{
			point.AddChildPoint(terminal);
		}

		_terminalCoord = terminal.coord;
	}

	private MapPoint[] CreateBranchRow(Rng rng, int row, int eliteFreeLane, ref bool hasShop)
	{
		MapPoint[] points = new MapPoint[RouteCount];
		for (int lane = 0; lane < RouteCount; lane++)
		{
			points[lane] = CreatePathPoint(
				RouteColumns[lane],
				row,
				RollPointType(rng, isFirstNode: false, allowElite: lane != eliteFreeLane, ref hasShop));
		}

		return points;
	}

	private static MapPointType RollPointType(Rng rng, bool isFirstNode, bool allowElite, ref bool hasShop)
	{
		List<MapPointType> pool = isFirstNode
			? [MapPointType.Unknown, MapPointType.Treasure, MapPointType.Treasure, MapPointType.RestSite]
			:
			[
				MapPointType.Monster,
				MapPointType.Monster,
				MapPointType.Unknown,
				MapPointType.Treasure,
				MapPointType.Treasure,
				MapPointType.RestSite
			];

		if (!hasShop)
		{
			pool.Add(MapPointType.Shop);
		}

		if (!isFirstNode && allowElite)
		{
			pool.Add(MapPointType.Elite);
		}

		MapPointType pointType = pool[rng.NextInt(pool.Count)];
		hasShop |= pointType == MapPointType.Shop;
		return pointType;
	}

	private static MapPointType RollTerminalPointType(Rng rng)
	{
		return rng.NextInt(2) == 0 ? MapPointType.Treasure : MapPointType.Elite;
	}

	private MapPoint CreatePathPoint(int col, int row, MapPointType pointType)
	{
		MapPoint point = new(col, row)
		{
			PointType = pointType,
			CanBeModified = false
		};
		Grid[col, row] = point;
		return point;
	}

	private static MapPoint CreateSpecialPoint(int col, int row, MapPointType pointType)
	{
		return new MapPoint(col, row)
		{
			PointType = pointType,
			CanBeModified = false
		};
	}
}
