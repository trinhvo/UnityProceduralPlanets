﻿using UnityEngine;

public partial class Planet
{
	public int NumberOfVerticesNeededTotal { get { return numberOfVerticesOnEdge * numberOfVerticesOnEdge; } }




	int subdivisionMaxRecurisonDepthCached = -1;
	public int SubdivisionMaxRecurisonDepth
	{
		get
		{
			if (subdivisionMaxRecurisonDepthCached == -1)
			{
				var planetCircumference = 2 * Mathf.PI * radiusMin;
				var oneRootChunkCircumference = planetCircumference / 6.0f;

				subdivisionMaxRecurisonDepthCached = 0;
				while (oneRootChunkCircumference > stopSegmentRecursionAtWorldSize)
				{
					oneRootChunkCircumference /= 2;
					subdivisionMaxRecurisonDepthCached++;
				}
			}
			return subdivisionMaxRecurisonDepthCached;
		}
	}

}