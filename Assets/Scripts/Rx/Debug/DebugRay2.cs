using UnityEngine;
using System.Collections.Generic;

namespace Rx
{	
	public class DebugRay2 : DebugShape2
	{
		public bool CheckIntersectingSegments { get; set; }
		public bool ShowPointsOfIntersectionWithSegments { get; set; }
		public bool CheckIntersectingPolygons { get; set; }
	}	
}
