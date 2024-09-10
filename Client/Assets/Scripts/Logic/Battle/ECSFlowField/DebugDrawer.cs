using TMG.FlowFieldECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class DebugDrawer : MonoBehaviour
{
    public float CellRadius = 0;
    public Texture2D DirectionTex;
    public Texture2D DestinationTex;
    public Texture2D ImpossibleTex;

    private Renderer[] _texRenderers;

    // 单例模式以确保只有一个调试绘制器实例
    public static DebugDrawer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _texRenderers = GetComponentsInChildren<Renderer>();
    }

    private void OnDrawGizmos()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(DebugData));
        if (query.IsEmpty) return;

        var debugEntity = query.GetSingletonEntity();
        DebugData flowFieldDebugData = entityManager.GetComponentData<DebugData>(debugEntity);

        var debugDataList = flowFieldDebugData.debugCellDatas;
        for (int i = 0; i < debugDataList.Length; i++)
        {
            var debugData = debugDataList[i];
            var renderer = _texRenderers[i];
            Vector3 position = new Vector3(debugData.GridIndex.x + 0.5f, 0, debugData.GridIndex.y + 0.5f) * CellRadius * 2;
            // 绘制格子的边界
            Gizmos.color = Color.white;
            DrawGridCell(position, CellRadius); // 假设格子的大小为1x1单位，可以根据实际情况调整

            // 绘制方向线
            if (debugData.BestCost < ushort.MaxValue)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(position, position + debugData.Direction * CellRadius);
            }

            // 绘制 BestCost 值
            Handles.Label(position + Vector3.up * 0.5f, debugData.BestCost.ToString());

            // 显示方向
            if (debugData.IsDestination)
            {
                renderer.material.SetTexture("_BaseMap", DestinationTex);
            }
            else
            {
                if (debugData.BestCost < ushort.MaxValue)
                {
                    renderer.material.SetTexture("_BaseMap", DirectionTex);
                }
                else
                {
                    renderer.material.SetTexture("_BaseMap", ImpossibleTex);
                }
            }

            Vector3 direction = debugData.Direction;
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            renderer.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 辅助方法：绘制格子的四条边
    private void DrawGridCell(Vector3 center, float size)
    {
        Vector3 halfSize = new Vector3(size, 0, size);

        Vector3 topLeft = center + new Vector3(-halfSize.x, 0, halfSize.z);
        Vector3 topRight = center + new Vector3(halfSize.x, 0, halfSize.z);
        Vector3 bottomLeft = center + new Vector3(-halfSize.x, 0, -halfSize.z);
        Vector3 bottomRight = center + new Vector3(halfSize.x, 0, -halfSize.z);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
