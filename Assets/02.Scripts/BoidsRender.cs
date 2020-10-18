using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsRender : MonoBehaviour
{

    [SerializeField]
    private Vector3 ObjectScale = new Vector3(0.1f, 0.2f, 0.5f);

    [SerializeField]
    private Boids Boids;

    [SerializeField]
    private Mesh MeshRes;

    [SerializeField]
    private Material RenderMat;

    private uint[] Arguments = new uint[5] { 0, 0, 0, 0, 0 };
    private ComputeBuffer ArgsBuffer;


    private void Start()
    {
        ArgsBuffer = new ComputeBuffer(1, Arguments.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    private void Update()
    {
        RenderInstancedMesh();
    }

    private void RenderInstancedMesh()
    {
        if (RenderMat == null || Boids == null ||
            SystemInfo.supportsInstancing == false)
            return;

        uint indexCount = MeshRes != null ?
            (uint)MeshRes.GetIndexCount(0) : 0;

        Arguments[0] = indexCount;
        Arguments[1] = (uint)Boids.GetMaxObjectCount();
        ArgsBuffer.SetData(Arguments);

        RenderMat.SetBuffer("_BoidDataBuffer", Boids.GetBoidDataBuffer());
        RenderMat.SetVector("_ObjectScale", ObjectScale);

        var bounds = new Bounds
            (
                Boids.GetSimulationAreaCenter(),
                Boids.GetSimulationAreaSize()
            );

        Graphics.DrawMeshInstancedIndirect(MeshRes, 0, RenderMat, bounds, ArgsBuffer);
    }

}
