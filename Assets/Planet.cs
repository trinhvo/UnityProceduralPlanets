﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public partial class Planet : MonoBehaviour
{
	public float weightNeededToSubdivide = 0.3f;

	public float radiusMin = 100;
	public float radiusVariation = 10;
	public float seaLevel01 = 0.5f;

	public float stopSegmentRecursionAtWorldSize = 5;

	public Material segmentMaterial;
	public ComputeShader generateVertices;
	public Texture2D biomesControlMap;
	public ComputeShader generateDiffuseMap;
	public ComputeShader generateNormapMap;

	public ulong id;

	public List<Chunk> rootChildren;


	public bool useSkirts = false;
	public int numberOfVerticesOnEdge = 10;

	public static HashSet<Planet> allPlanets = new HashSet<Planet>();

	public Vector3 Center { get { return transform.position; } }

	public void SetParams(ComputeShader c)
	{
		c.SetInt("param_numberOfVerticesOnEdge", numberOfVerticesOnEdge);
		c.SetFloat("param_radiusMin", radiusMin);
		c.SetFloat("param_radiusVariation", radiusVariation);
		c.SetFloat("param_seaLevel01", seaLevel01);

		c.SetTexture(0, "param_biomesControlMap", biomesControlMap);
	}



	// Use this for initialization
	void Start()
	{
		allPlanets.Add(this);
		InitializeRootChildren();
	}


	// Update is called once per frame
	void LateUpdate()
	{
		TrySubdivideOver(new SubdivisionData()
		{
			pos = Camera.main.transform.position,
			fieldOfView = Camera.main.fieldOfView,
		});

		var sw = Stopwatch.StartNew();
		foreach (var s in toGenerate.GetWeighted())
		{
			s.GenerateMesh();
			s.GenerateDiffuseMap();
			s.GenerateNormalMap();
			//if (sw.Elapsed.TotalMilliseconds > 5f)
				break;
		}
	}

	private void OnGUI()
	{
		GUILayout.Button("segments to generate: " + toGenerate.Count);
	}




	void AddRootChunk(ulong id, Vector3[] vertices, int a, int b, int c, int d)
	{
		var range = new Range()
		{
			a = vertices[a],
			b = vertices[b],
			c = vertices[c],
			d = vertices[d],
		};

		var child = Chunk.Create(
			planet: this,
			parent: null,
			generation: 0,
			range: range,
			id: id
		);

		rootChildren.Add(child);
	}

	private void InitializeRootChildren()
	{
		if (rootChildren != null && rootChildren.Count > 0) return;
		if (rootChildren == null)
			rootChildren = new List<Chunk>(6);

		var indicies = new List<uint>();

		var halfSIze = Mathf.Sqrt((radiusMin * radiusMin) / 3.0f);

		/* 3----------0
		  /|         /|
		 / |        / |
		2----------1  |       y
		|  |       |  |       |
		|  7-------|--4       |  z
		| /        | /        | /
		|/         |/         |/
		6----------5          0----------x  */

		var vertices = new[] {
			// top 4
			new Vector3(halfSIze, halfSIze, halfSIze),
			new Vector3(halfSIze, halfSIze, -halfSIze),
			new Vector3(-halfSIze, halfSIze, -halfSIze),
			new Vector3(-halfSIze, halfSIze, halfSIze),
			// bottom 4
			new Vector3(halfSIze, -halfSIze, halfSIze),
			new Vector3(halfSIze, -halfSIze, -halfSIze),
			new Vector3(-halfSIze, -halfSIze, -halfSIze),
			new Vector3(-halfSIze, -halfSIze, halfSIze)
		};

		AddRootChunk(0, vertices, 0, 1, 2, 3); // top
		AddRootChunk(1, vertices, 5, 4, 7, 6); // bottom
		AddRootChunk(2, vertices, 1, 5, 6, 2); // front
		AddRootChunk(3, vertices, 3, 7, 4, 0); // back
		AddRootChunk(4, vertices, 0, 4, 5, 1); // right
		AddRootChunk(5, vertices, 2, 6, 7, 3); // left
	}


	private void OnDrawGizmos()
	{
		if (rootChildren == null || rootChildren.Count == 0)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(this.transform.position, this.radiusMin);
		}
	}
}
