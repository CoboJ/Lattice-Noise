using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Noise;
public class NoiseVisualization : Visualization
{
    static int noiseId = Shader.PropertyToID("_Noise");

    [SerializeField] int seed;
    [SerializeField] SpaceTRS domain = new SpaceTRS {
        scale = 8f
    };
    [SerializeField] Shape shape;
    [SerializeField, Range(0.1f, 10f)] float instanceScale = 2f;
    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material material;
    [SerializeField, Range(1, 512)]int resolution = 16;
	[SerializeField, Range(-0.5f, 0.5f)]float displacement = 0.1f;

    
    NativeArray<float4> noise;
    ComputeBuffer noiseBuffer;

    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) 
    {
        noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
        //positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        //normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
        propertyBlock.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void DisableVisualization() 
    {
        noise.Dispose();
        noiseBuffer.Release();
        noiseBuffer = null;
    }

    protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle) 
    {
        Job<Lattice1D>.ScheduleParallel(
            positions, noise, seed, domain, resolution, handle
        ).Complete();
        noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
    }
}
