using System.Collections.Generic;

namespace RxSoft
{
	public class rxProcessingSet
	{
		public List<rxProcessingPolygon> additivePolygons;
		public List<rxProcessingPolygon> subtractivePolygons;

		public rxProcessingSet()
		{
			this.additivePolygons = new List<rxProcessingPolygon>();
			this.subtractivePolygons = new List<rxProcessingPolygon>();
		}
	}
}