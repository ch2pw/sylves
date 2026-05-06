using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Periodic 2d grid of 12 sided greek crosses.
    /// This is a specialization of <see cref="PeriodicPlanarMeshGrid"/>.
    /// </summary>
    public class GreekCrossGrid : PeriodicPlanarMeshGrid
    {
        public GreekCrossGrid() : base(GreekCrossMeshData(), new Vector2(2.0f, 1.0f), new Vector2(-1.0f, 2.0f))
        {

        }

        private static MeshData GreekCrossMeshData()
        {
            var meshData = new MeshData();
            meshData.vertices = new Vector3[]
            {
                new Vector3(-0.5f,  1.5f, 0),
                new Vector3( 0.5f,  1.5f, 0),
                new Vector3( 0.5f,  0.5f, 0),
                new Vector3( 1.5f,  0.5f, 0),
                new Vector3( 1.5f, -0.5f, 0),
                new Vector3( 0.5f, -0.5f, 0),
                new Vector3( 0.5f, -1.5f, 0),
                new Vector3(-0.5f, -1.5f, 0),
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(-1.5f, -0.5f, 0),
                new Vector3(-1.5f,  0.5f, 0),
                new Vector3(-0.5f,  0.5f, 0),
            };
            meshData.indices = new[] { new[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, ~11,
            } };
            meshData.topologies = new[] { MeshTopology.NGon };
            return meshData;
        }
    }
}
