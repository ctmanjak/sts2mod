using MegaCrit.Sts2.Core.Map;

namespace IntegratedStrategyEvents.TreeHoles;

internal sealed class IntegratedStrategyEndlessFinaleActMap : ActMap
{
	private const int GridWidth = 7;
	private const int GridRows = 2;
	private const int CenterColumn = 3;
	private const int StartRow = 0;
	private const int BossRow = GridRows;

	protected override MapPoint?[,] Grid { get; }

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	public IntegratedStrategyEndlessFinaleActMap()
	{
		Grid = new MapPoint[GridWidth, GridRows];
		StartingMapPoint = CreateSpecialPoint(CenterColumn, StartRow, MapPointType.Ancient);
		BossMapPoint = CreateSpecialPoint(CenterColumn, BossRow, MapPointType.Boss);
		StartingMapPoint.AddChildPoint(BossMapPoint);
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
