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

    // ����ģʽ��ȷ��ֻ��һ�����Ի�����ʵ��
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
            // ���Ƹ��ӵı߽�
            Gizmos.color = Color.white;
            DrawGridCell(position, CellRadius); // ������ӵĴ�СΪ1x1��λ�����Ը���ʵ���������

            // ���Ʒ�����
            if (debugData.BestCost < ushort.MaxValue)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(position, position + debugData.Direction * CellRadius);
            }

            // ���� BestCost ֵ
            Handles.Label(position + Vector3.up * 0.5f, debugData.BestCost.ToString());

            // ��ʾ����
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

    // �������������Ƹ��ӵ�������
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
