using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CollisionAvoidanceJob : IJobParallelFor
{
    // INPUT DATA (ReadOnly)
    [ReadOnly] public NativeArray<float3> agentPositions;
    [ReadOnly] public NativeArray<float3> preferredVelocities;
    [ReadOnly] public NativeArray<bool> doneFlags;
    [ReadOnly] public NativeArray<float> walkingSpeeds;

    [ReadOnly] public NativeMultiHashMap<int, int> spatialGrid;

    // System Parameters
    public float ringDiameter;
    public float lenOfBin;
    public int neighbourBins;
    public float3 xMinMax;
    public float3 zMinMax;

    // OUTPUT DATA
    [WriteOnly] public NativeArray<float3> outCollisionAvoidanceVelocity;

    public void Execute(int index)
    {
        float3 posA = agentPositions[index];
        float speedA = walkingSpeeds[index];
        float3 totalForce = float3.zero;

        // Determine spatial grid bin and clamp to grid boundaries (matching original SimulationGrid)
        int currentBinRow = (int)((posA.z - zMinMax.x) / lenOfBin);
        int currentBinCol = (int)((posA.x - xMinMax.x) / lenOfBin);
        currentBinRow = math.clamp(currentBinRow, 0, neighbourBins - 1);
        currentBinCol = math.clamp(currentBinCol, 0, neighbourBins - 1);

        // Search 3x3 surrounding spatial bins
        for (int rOffset = -1; rOffset <= 1; rOffset++)
        {
            for (int cOffset = -1; cOffset <= 1; cOffset++)
            {
                int targetRow = currentBinRow + rOffset;
                int targetCol = currentBinCol + cOffset;

                if (targetRow >= 0 && targetRow < neighbourBins && targetCol >= 0 && targetCol < neighbourBins)
                {
                    int binKey = targetRow * neighbourBins + targetCol;

                    if (spatialGrid.TryGetFirstValue(binKey, out int otherAgentIndex, out var iterator))
                    {
                        do
                        {
                            if (index == otherAgentIndex) continue; // Skip self

                            float3 posB = agentPositions[otherAgentIndex];
                            float3 disVector = posA - posB; // Vector pointing from B to A
                            float distance = math.length(disVector);

                            if (distance <= 0.001f) continue;

                            if (distance < ringDiameter)
                            {
                                totalForce += math.normalize(disVector) * (ringDiameter - distance) * speedA;
                            }

                        } while (spatialGrid.TryGetNextValue(out otherAgentIndex, ref iterator));
                    }
                }
            }
        }

        outCollisionAvoidanceVelocity[index] = totalForce;
    }
}