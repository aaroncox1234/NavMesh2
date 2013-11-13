using UnityEngine;
using System.Collections;

namespace Rx
{	
	public class DebugPolygon2 : DebugShape2
	{
		public bool CheckPointsInside { get; set; }
		public bool CheckIntersectingSegments { get; set; }
		public bool CheckIntersectingPolygons { get; set; }
		public bool CheckPolygonsInside { get; set; }
	}	
}
