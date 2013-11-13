using UnityEngine;
using System.Collections.Generic;

namespace Rx
{	
	public class DebugLine2 : DebugShape2
	{
		public bool CheckIntersectingSegments { get; set; }
		public bool ShowPointsOfIntersectionWithSegments { get; set; }
		public bool CheckIntersectingPolygons { get; set; }
	}	
}
