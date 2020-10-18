using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Boids : MonoBehaviour
{
    [Serializable]
    private struct BoidData
    {
        public Vector3 Velocity;
        public Vector3 Position;
    }

    const int SIMULATION_BLOCK_SIZE = 256;

    [Range(256, 32768)]
    public int MaxObjectCount = 16384;

    [SerializeField]
    private float CohesionNeighborhoodRadius = 2.0f;

    [SerializeField]
    private float AlignmentNeighborhoodRadius = 2.0f;

    [SerializeField]
    private float SeparateNeighborhoodRadius = 1.0f;

    [SerializeField]
    private float MaxSpeed = 5.0f;

    [SerializeField]
    private float MaxSteerForce = 0.5f;

    [SerializeField]
    private float CohesionWeight = 1.0f;

    [SerializeField]
    private float AlignmentWeight = 1.0f;

    [SerializeField]
    private float SeparateWeight = 3.0f;

    [SerializeField]
    private float AvoidWallWeight = 10.0f;

    [SerializeField]
    private Vector3 WallCenter = Vector3.zero;

    [SerializeField]
    private Vector3 WallSize = new Vector3(32f, 32f, 32f);

    [SerializeField]
    ComputeShader BoidsCS;


    private ComputeBuffer BoidForceBuffer;
    private ComputeBuffer BoidDataBuffer;

    public ComputeBuffer GetBoidDataBuffer()
    {
        return BoidDataBuffer;
    }

    public int GetMaxObjectCount()
    {
        return MaxObjectCount;
    }

    public Vector3 GetSimulationAreaCenter()
    {
        return WallCenter;
    }

    public Vector3 GetSimulationAreaSize()
    {
        return WallSize;
    }


    private void InitBuffer()
    {
        BoidDataBuffer = new ComputeBuffer(MaxObjectCount,
            Marshal.SizeOf(typeof(BoidData)));
        BoidForceBuffer = new ComputeBuffer(MaxObjectCount,
            Marshal.SizeOf(typeof(Vector3)));

        var boidDataArr = new BoidData[MaxObjectCount];
        var forceArr = new Vector3[MaxObjectCount];

        for (int i = 0; i < MaxObjectCount; ++i)
        {
            forceArr[i] = Vector3.zero;
            boidDataArr[i].Position = UnityEngine.Random.insideUnitSphere * 1f;
            boidDataArr[i].Velocity = UnityEngine.Random.insideUnitSphere * 0.1f;
        }

        BoidDataBuffer.SetData(boidDataArr);
        BoidForceBuffer.SetData(forceArr);

        boidDataArr = null;
        forceArr = null;
    }

    private void Simulation()
    {
        ComputeShader cs = BoidsCS;
        int id = -1;

        var threadGroupSize = Mathf.CeilToInt(MaxObjectCount / SIMULATION_BLOCK_SIZE);

        id = cs.FindKernel("ForceCS");
        cs.SetInt("MaxBoidObjectNum", MaxObjectCount);
        cs.SetFloat("CohesionNeighborhoodRadius", CohesionNeighborhoodRadius);
        cs.SetFloat("AlignmentNeighborhoodRadius", AlignmentNeighborhoodRadius);
        cs.SetFloat("SeparateNeighborhoodRadius", SeparateNeighborhoodRadius);
        cs.SetFloat("MaxSpeed", MaxSpeed);
        cs.SetFloat("MaxSteerForce", MaxSteerForce);
        cs.SetFloat("SeparateWeight", SeparateWeight);
        cs.SetFloat("CohesionWeight", CohesionWeight);
        cs.SetFloat("AlignmentWeight", AlignmentWeight);
        cs.SetVector("WallCenter", WallCenter);
        cs.SetVector("WallSize", WallSize);
        cs.SetFloat("AvoidWallWeight", AvoidWallWeight);
        cs.SetBuffer(id, "BoidDataBufferRead", BoidDataBuffer);
        cs.SetBuffer(id, "BoidForceBufferWrite", BoidForceBuffer);
        cs.Dispatch(id, threadGroupSize, 1, 1); 

        id = cs.FindKernel("IntegrateCS"); 
        cs.SetFloat("DeltaTime", Time.deltaTime);
        cs.SetBuffer(id, "BoidForceBufferRead", BoidForceBuffer);
        cs.SetBuffer(id, "BoidDataBufferWrite", BoidDataBuffer);
        cs.Dispatch(id, threadGroupSize, 1, 1);

    }


    void Start()
    {
        InitBuffer();
    }

    void Update()
    {
        Simulation();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(WallCenter, WallSize);
    }
}
