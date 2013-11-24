using System.Collections.Generic;

namespace RxSoft
{
	public class PolygonInfoSet
	{
		public List<PolygonInfo> additivePolygons;
		public List<PolygonInfo> subtractivePolygons;

		public PolygonInfoSet()
		{
			this.additivePolygons = new List<PolygonInfo>();
			this.subtractivePolygons = new List<PolygonInfo>();
		}
	}
}