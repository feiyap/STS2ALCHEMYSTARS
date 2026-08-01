using MegaCrit.Sts2.Core.Map;

namespace Valencina.ValencinaCode.Acts;

public sealed class ValencinaAct4Map : ActMap
{
	private const int Column = 3;

	private readonly MapPoint?[,] _grid = new MapPoint[7, 7];

	public override MapPoint BossMapPoint { get; }

	public override MapPoint StartingMapPoint { get; }

	protected override MapPoint?[,] Grid => _grid;

	public ValencinaAct4Map()
	{
		StartingMapPoint = FixedPoint(3, 0, (MapPointType)8);
		MapPoint val = Put(3, 1, (MapPointType)4);
		MapPoint val2 = Put(3, 2, (MapPointType)2);
		MapPoint val3 = Put(3, 3, (MapPointType)6);
		MapPoint val4 = Put(3, 4, (MapPointType)1);
		BossMapPoint = FixedPoint(3, 6, (MapPointType)7);
		((ActMap)this).StartingMapPoint.AddChildPoint(val);
		val.AddChildPoint(val2);
		val2.AddChildPoint(val3);
		val3.AddChildPoint(val4);
		val4.AddChildPoint(((ActMap)this).BossMapPoint);
		base.startMapPoints.Add(val);
	}

	private MapPoint Put(int col, int row, MapPointType pointType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		MapPoint val = FixedPoint(col, row, pointType);
		_grid[col, row] = val;
		return val;
	}

	private static MapPoint FixedPoint(int col, int row, MapPointType pointType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		return new MapPoint(col, row)
		{
			PointType = pointType,
			CanBeModified = false
		};
	}
}
