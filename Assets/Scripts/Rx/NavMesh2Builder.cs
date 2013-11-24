using UnityEngine;
using System.Collections.Generic;

// Hole clipping
//	Either support Merging and Clipping in the same process, or document that they can't be done together (and assert that subtractive is empty on Merge)
//		This could help performance, as we'd only generate VertexProcessingInfo once
//		a. merge boundaries
//		b. merge holes
//		c. perform cutting
// Commit to git
// Organize all TODOs
// Clean up and document everything
// Hole integration
// Triangulation
// Triangle reduction
// Navmesh generation
// Fix scene being constantly changed by test editor
// Remove unused test cases
// Center all test cases at the origin
// Profile



// --Next--
// Fuse holes (reject holes within holes)
// Cut boundaries by overlapping holes
// Add contained holes through the boundary method (fix up indices so there aren't duplicate points in the resulting triangles)

// Test holes within holes. Test holes that create holes after merging.
// Test hole with all vertices surrounded by another hole.
// Look for edge cases where, after fusion, one polygon doesn't contain all the other polygons

// TODO
// Early Approach
//  Assume no overlap
//	Handle single boundary with multiple holes
//	Fix the issue of multiple vertices in the outline after merging holes
//	Stick to this approach until you have a searchable, path generating navmesh
//  Detect and remove colinear points
//	Be able to chose which MeshProcessInfo to draw via the NewNavMesh2Editor
//  Remove boundaries that are completely contained by other boundaries (maybe warn too)
//  Warn about holes not contained by any boundary
//  Have to group holes, since on might intersect another, thus extending it as intersecting a boundary.
//  Why do scenes keep changing themselves?
// Better Approach
//	Make everything convex
//	Merge overlapping boundaries
//	Merge overlapping holes
//		On each merge, run the convexity process again on the result
//	Combine holes with boundaries
//		If the boundary is inside the hole, delete it
//		If the boundary contains the hole, integrate the hole and run the convexity process again on the result
//		If the boundary and the hole intersect, cut the boundary up by the hole
//  Optimization (profile): At the very start, merge all boundaries and holes that have no overlap

// TODO: deal with colinearity in everything. It will happen.. Same with degeneracies
//	Create test cases
//		Ray test that lines up perfectly with a segment on the navmesh
//		Overlapping segment intersection
//		Ray passing right through a vertex (e.g. (0,0)-(1,0)t against (5,0)-(5,5)

namespace Rx
{
	// Pipeline for building a set of navmeshes from a set of polygons.
	// Polygons are defined by unclosed lists of vertices. If the pipeline receives a closed list, it will 
	// convert the list for internal processing.
	// Whenever appropriate, members are declared public to provide easy access for debugging the pipeline.
	// The result of each stage of the pipeline is stored for demonstration and debugging purposes.
	// After a call to Build, all public data members will be populated with information from the build.
	public class NavMesh2Builder
	{
		#region Types
		
		public class PolygonProcessingInfo
		{
			public List<Vector2> vertices;

			public bool markedForDeletion;
			
			public PolygonProcessingInfo( List<Vector2> vertices )
			{
				this.vertices = vertices;

				this.markedForDeletion = false;
			}
		}
//		public class PolygonProcessingInfo
//		{
//			public List<Vector2> vertices;
//			public List<int> outline;
//			
//			public PolygonProcessingInfo( List<Vector2> inputVertices )
//			{
//				vertices = new List<Vector2>( inputVertices );
//				
//				outline = new List<int>( inputVertices.Count );				
//				for ( int index = 0; index < inputVertices.Count; ++index )
//				{
//					outline.Add( index );
//				}
//			}
//		}
//
//		public class MeshProcessingInfo
//		{
//			public List< List<Vector2> > boundaries;
//			public List< List<Vector2> > holes;
//			
//			public List< List<int> > triangulatedBoundaries;
//			public List< List<int> > triangulatedHoles;
//			
//			public List<Vector2> vertices;
//			
//			// Contains indices into the vertices list. This may reference the same vertex multiple times because
//			// of how holes are integrated with the fused boundary in GenerateOutline().
//			public List<int> outline;
//			
//			public MeshProcessingInfo()
//			{
//				boundaries = new List< List<Vector2> >();
//				holes = new List< List<Vector2> >();
//				
//				triangulatedBoundaries = new List< List<int> >();
//				triangulatedHoles = new List< List<int> >();
//				
//				vertices = new List<Vector2>();				
//			}
//		}
		
		#endregion
		
		#region Data members
		
		public List<PolygonProcessingInfo> boundaries;
		public List<PolygonProcessingInfo> holes;
		
		private Polygon2Clipper clipper;
		
//		public List<MeshProcessingInfo> meshData;
//		
//		public List<PolygonProcessingInfo> boundaryInfos;
		
		#endregion
		
		#region Initialization
		
		public NavMesh2Builder()
		{
			boundaries = new List<PolygonProcessingInfo>();
			holes = new List<PolygonProcessingInfo>();
			
			clipper = new Polygon2Clipper();
			
//			meshData = new List<MeshProcessingInfo>();
//			
//			boundaryInfos = new List<PolygonProcessingInfo>();
		}
		
		// The input list is consumed and will be altered. It is up to the caller to pass in a copy of the data if the original needs to be kept.
		public void AddBoundary( List<Vector2> boundary )
		{
			AddPolygonToList( boundary, boundaries );
			
//			boundaryInfos.Add( new PolygonProcessingInfo( boundary ) );
		}
		
		// The input list is consumed and will be altered. It is up to the caller to pass in a copy of the data if the original needs to be kept.
		public void AddHole( List<Vector2> hole )
		{
			AddPolygonToList( hole, holes );
		}
		
		private void AddPolygonToList( List<Vector2> polygon, List<PolygonProcessingInfo> list )
		{
			if ( VerticesOverlap( polygon[0], polygon[polygon.Count-1] ) )
			{
				polygon.RemoveAt( polygon.Count-1 );
			}
			
			PolygonProcessingInfo info = new PolygonProcessingInfo( polygon );			
			ForceCCW( info );			
			list.Add( info );
		}
		
		public void Clear()
		{
			boundaries.Clear();
			holes.Clear();
			
//		 	meshData.Clear();
		}
		
		#endregion
		
		#region Build
		
//		public void Build()
//		{
//			EnforceWinding();
//			
//			GroupOverlappingBoundaries();
//			AssignHolesToBoundaries();
//			
//			TriangulateBoundaries();
//			TriangulateHoles();
//		}
		
		public void Build( int maxStep )
		{
			// Step 0
			
			int step = 0;
			if ( step == maxStep ) { return; }
			
			// Step 1
		
			FuseOverlappingBoundaries();
			
			++step;
			if ( step == maxStep ) { return; }
			
			// Step 2
			
			FuseOverlappingHoles();
			
			++step;
			if ( step == maxStep ) { return; }
			
			// Step 3
			
			ClipBoundariesByOverlappingHoles();
			
			++step;
			if ( step == maxStep ) { return; }
		}
		
		// TODO: should we do this before constructing all the PolygonProcessingInfo objects? We create a bunch, fuse, then recreate. Every boundary will construct twice.
		private void FuseOverlappingBoundaries()
		{
			clipper.Reset();
			
			foreach ( PolygonProcessingInfo boundary in boundaries )
			{
				clipper.AddAdditivePolygon( boundary.vertices );
			}		
			
			List< List<Vector2> > fuseResults = clipper.FusePolygons(); // TODO: rename Fuse to Merge
			
			boundaries.Clear();
			
			// Figure out which results are boundaries and which are holes.
			for ( int indexA = 0; indexA < fuseResults.Count; ++indexA )
			{
				bool isContained = false;
				
				for ( int indexB = 0; indexB < fuseResults.Count; ++indexB )
				{
					if ( ( indexA != indexB ) && Geometry2.PolygonContainsPolygon( fuseResults[indexB], fuseResults[indexA] ) )
					{
						isContained = true;
						break;
					}
				}
				
				if ( isContained )
				{
					holes.Add( new PolygonProcessingInfo( fuseResults[indexA] ) );
				}
				else
				{
					boundaries.Add( new PolygonProcessingInfo( fuseResults[indexA] ) );
				}
			}
		}
		
		private void FuseOverlappingHoles()
		{
			clipper.Reset();
			
			foreach ( PolygonProcessingInfo hole in holes )
			{
				clipper.AddAdditivePolygon( hole.vertices );
			}		
			
			List< List<Vector2> > fuseResults = clipper.FusePolygons(); // TODO: rename Fuse to Merge
			
			holes.Clear();
			
			// Reject any hole that's completely contained by another hole.
			for ( int indexA = 0; indexA < fuseResults.Count; ++indexA )
			{
				bool isContained = false;
				
				for ( int indexB = 0; indexB < fuseResults.Count; ++indexB )
				{
					if ( ( indexA != indexB ) && Geometry2.PolygonContainsPolygon( fuseResults[indexB], fuseResults[indexA] ) )
					{
						isContained = true;
						break;
					}
				}
				
				if ( !isContained )
				{
					holes.Add( new PolygonProcessingInfo( fuseResults[indexA] ) );
				}
			}
		}
		
		private void ClipBoundariesByOverlappingHoles()
		{
			clipper.Reset();
			
			foreach ( PolygonProcessingInfo boundary in boundaries )
			{
				clipper.AddAdditivePolygon( boundary.vertices );
			}
			
			foreach ( PolygonProcessingInfo hole in holes )
			{
				clipper.AddSubtractivePolygon( hole.vertices );
			}
			
			List< List<Vector2> > clipResults = clipper.ClipPolygons();
			
			boundaries.Clear();
			holes.Clear();

			// TODO: at the very least, put this in a function
			// Figure out which results are boundaries and which are holes.
			for ( int indexA = 0; indexA < clipResults.Count; ++indexA )
			{
				bool isContained = false;
				
				for ( int indexB = 0; indexB < clipResults.Count; ++indexB )
				{
					if ( ( indexA != indexB ) && Geometry2.PolygonContainsPolygon( clipResults[indexB], clipResults[indexA] ) )
					{
						isContained = true;
						break;
					}
				}

				// TODO: This is buggy. It will turn orphaned holes into boundaries. Stop storing polygons as lists of vertices and start passing them around with data.
				if ( isContained )
				{
					holes.Add( new PolygonProcessingInfo( clipResults[indexA] ) );
				}
				else
				{
					boundaries.Add( new PolygonProcessingInfo( clipResults[indexA] ) );
				}
			}
		}
		
//		private void FuseOverlappingBoundariesXXX()
//		{
//			for ( int indexA = 0; indexA < boundaries.Count; ++indexA )
//			{
//				if ( boundaries[indexA].markedForDeletion )
//				{
//					continue;
//				}
//				
//				bool boundaryWasFused = false;
//				List<Vector2> fusedBoundary = boundaries[indexA].vertices;
//				
//				for ( int indexB = indexA + 1; indexB < boundaries.Count; ++indexB )
//				{
//					if ( boundaries[indexB].markedForDeletion )
//					{
//						continue;
//					}
//					
//					if ( Geometry2.PolygonsIntersect( fusedBoundary, boundaries[indexB].vertices ) )
//					{
//						List<Vector2> newBoundary;
//						List< List<Vector2> > newHoles;
//						FuseBoundaries( fusedBoundary, boundaries[indexB].vertices, out newBoundary, out newHoles );
//							
//						fusedBoundary = newBoundary;
//							
//						foreach ( List<Vector2> newHole in newHoles )
//						{
//							holes.Add( new PolygonProcessingInfo( newHole ) );
//						}
//							
//						boundaries[indexA].markedForDeletion = true;
//						boundaries[indexB].markedForDeletion = true;
//						
//						boundaryWasFused = true;
//					}
//				}
//				
//				if ( boundaryWasFused )
//				{
//					boundaries.Add( new PolygonProcessingInfo( fusedBoundary ) );
//				}
//			}
//			
//			boundaries.RemoveAll( boundary => boundary.markedForDeletion );
//			/*if ( boundaries.Count >= 4 )
//			{
//				FuseBoundaries( boundaries[1].vertices, boundaries[2].vertices, out newBoundary, out newHoles );
//				
//				boundaries.RemoveAt(2);
//				boundaries.RemoveAt(1);
//				
//				boundaries.Add( new PolygonProcessingInfo( newBoundary ) );
//				
//				foreach ( List<Vector2> newHole in newHoles )
//				{
//					holes.Add( new PolygonProcessingInfo( newHole ) );
//				}
//			}*/
//		}
//		
//		// Helper that fuses two boundaries and filters the results.
//		private void FuseBoundaries( List<Vector2> boundary1, List<Vector2> boundary2, out List<Vector2> newBoundary, out List< List<Vector2> > newHoles )
//		{
//			newBoundary = new List<Vector2>();
//			newHoles = new List< List<Vector2> >();
//			
//			// TODO: feed in every polygon and see what happens (i.e. let the clipper decide who clips...profile against: generate a list of intersecting polygons and clip in groups)
//			clipper.Reset();
//			clipper.AddPolygon( boundary1 );
//			clipper.AddPolygon( boundary2 );
//			
//			List< List<Vector2> > fuseResults = clipper.FusePolygons();
//			
//			int boundaryIndex = 0;
//			
//			for ( int index = 1; index < fuseResults.Count; ++index )
//			{
//				if ( Geometry2.PolygonContainsPolygon(  fuseResults[index], fuseResults[boundaryIndex] ) )
//				{
//					boundaryIndex = index;
//				}
//			}
//			
//			newBoundary = fuseResults[boundaryIndex];
//			
//			for ( int index = 0; index < fuseResults.Count; ++index )
//			{
//				if ( index != boundaryIndex )
//				{
//					newHoles.Add( fuseResults[index] );
//				}
//			}
//		}
		
		// Group overlapping boundaries together. Each group of overlapping boundaries forms a single navmesh.
//		public void GroupOverlappingBoundaries()
//		{
//			// Assign a group id to each input boundary.
//			
//			List<int> boundaryGroupIds = new List<int>( inputBoundaries.Count );
//			for ( int i = 0; i < inputBoundaries.Count; ++i )
//			{
//				boundaryGroupIds.Add( -1 );
//			}			
//			
//			int currentBoundaryGroupId = 0;			
//			for ( int inputBoundaryIndex = 0; inputBoundaryIndex < inputBoundaries.Count; ++inputBoundaryIndex )
//			{
//				// Assign the current boundary to a group if it doesn't already belong to one.
//				if ( boundaryGroupIds[inputBoundaryIndex] == -1 )
//				{
//					boundaryGroupIds[inputBoundaryIndex] = currentBoundaryGroupId;
//				}
//				
//				// Find other boundaries that intersect the current boundary.
//				for ( int otherInputBoundaryIndex = inputBoundaryIndex + 1; otherInputBoundaryIndex < inputBoundaries.Count; ++otherInputBoundaryIndex )
//				{
//					// Ignore boundaries that belong to the same group.
//					if ( boundaryGroupIds[inputBoundaryIndex] == boundaryGroupIds[otherInputBoundaryIndex] )
//					{
//						continue;
//					}
//					
//					if ( Geometry2.PolygonsIntersect( inputBoundaries[inputBoundaryIndex], inputBoundaries[otherInputBoundaryIndex] ) )
//					{
//						// If the other boundary doesn't already belong to a group, just add it to the current group.
//						if ( boundaryGroupIds[otherInputBoundaryIndex] == -1 )
//						{
//							boundaryGroupIds[otherInputBoundaryIndex] = boundaryGroupIds[inputBoundaryIndex];
//						}
//						// If the other boundary already belongs to a group, change every group member to belong to the current group.
//						else
//						{
//							int otherGroupId = boundaryGroupIds[otherInputBoundaryIndex];
//							for ( int i = 0; i < boundaryGroupIds.Count; ++i )
//							{
//								if ( boundaryGroupIds[i] == otherGroupId )
//								{
//									boundaryGroupIds[i] = boundaryGroupIds[inputBoundaryIndex];
//								}
//							}
//						}
//					}
//				}
//				
//				++currentBoundaryGroupId;
//			}
//			
//			// Create a MeshProcessingInfo for each group.
//			
//			List<int> meshProcessingInfoGroupIds = new List<int>();
//			
//			for ( int inputBoundaryIndex = 0; inputBoundaryIndex < inputBoundaries.Count; ++inputBoundaryIndex )
//			{
//				int currentGroupId = boundaryGroupIds[inputBoundaryIndex];
//				
//				int meshDataIndex = meshProcessingInfoGroupIds.IndexOf( currentGroupId );
//				
//				// If a MeshProcessingInfo already exists for this group, add the boundary to it.
//				if ( meshDataIndex != -1 )
//				{
//					meshData[meshDataIndex].boundaries.Add( inputBoundaries[inputBoundaryIndex] );
//				}
//				// Otherwise, create a new MeshProcessingInfo.
//				else
//				{
//					MeshProcessingInfo newMeshInfo = new MeshProcessingInfo();
//					newMeshInfo.boundaries.Add( inputBoundaries[inputBoundaryIndex] );
//					
//					meshData.Add( newMeshInfo );
//					meshProcessingInfoGroupIds.Add( currentGroupId );
//				}
//			}
//		}
		
		// Assign each hole to any MeshProcessingInfo with a boundary that intersects or contains the hole.
		// Note that any hole that intersects multiple boundaries will belong to multiple MeshProcessingInfos.
//		public void	AssignHolesToBoundaries()
//		{
//			foreach ( List<Vector2> hole in inputHoles )
//			{
//				foreach ( MeshProcessingInfo mesh in meshData )
//				{
//					foreach ( List<Vector2> boundary in mesh.boundaries )
//					{
//						if ( Geometry2.PolygonsIntersect( boundary, hole ) || Geometry2.PolygonContainsPolygon( boundary, hole ) )
//						{
//							mesh.holes.Add( hole );
//							
//							break;
//						}
//					}
//				}
//			}
//		}
//		
//		public void TriangulateBoundaries()
//		{
//			foreach ( MeshProcessingInfo meshInfo in meshData )
//			{
//				for ( int boundaryIndex = 0; boundaryIndex < meshInfo.boundaries.Count; ++boundaryIndex )
//				{
//					List<int> triangles = Geometry2.TriangulatePolygon( meshInfo.boundaries[boundaryIndex] );
//					meshInfo.triangulatedBoundaries.Add( triangles );
//				}
//			}
//		}
//
//		public void TriangulateHoles()
//		{
//			foreach ( MeshProcessingInfo meshInfo in meshData )
//			{
//				for ( int holeIndex = 0; holeIndex < meshInfo.holes.Count; ++holeIndex )
//				{
//					List<int> triangles = Geometry2.TriangulatePolygon( meshInfo.holes[holeIndex] );
//					meshInfo.triangulatedHoles.Add( triangles );
//				}
//			}
//		}
		
		#endregion
		
		#region Private helpers
		
		private bool VerticesOverlap( Vector2 a, Vector2 b )
		{
			return (a - b).SqrMagnitude() < 0.00001f ;
		}
		
		private void ForceCCW( PolygonProcessingInfo polygon )
		{
			if ( !Geometry2.PolygonIsCCW( polygon.vertices ) )
			{
				polygon.vertices.Reverse();
			}
		}
		
		#endregion
	}
}

