
using Nebukam.Common;
using Nebukam.Common.Editor;
#if UNITY_EDITOR
using Nebukam.ORCA;
#endif
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;


public class OrcaManagerSetup : MonoBehaviour
{
    public class CAgent : Agent
    {
        public int Id;
    }

    public static OrcaManagerSetup Instance;

    private AgentGroup<CAgent> agents;
    private ObstacleGroup obstacles;
    private ObstacleGroup dynObstacles;
    private RaycastGroup raycasts;
    private ORCA simulation;

    [Header("Settings")]
    public int seed = 12345;
    public Transform target;
    public GameObject ObstacleGameObject;
    public Transform ObstacleGameObjectParent;
    public AxisPair axis = AxisPair.XY;

    [Header("Agents")]
    public int agentCount = 50;
    public float maxAgentRadius = 2f;
    public float maxSpeed = 1f;
    public float minSpeed = 1f;

    [Header("Raycasts")]
    public int raycastCount = 50;
    public float raycastDistance = 10f;

    public void FetchAgentsPosition(ref NativeHashMap<int, float3> maps)
    {
        maps.Clear();
        for (int i = 0; i < agents.Count; i++)
        {
            var a = agents[i];
            maps[a.Id] = a.pos;
        }
    }

    /// <summary>
    /// 增加一个Orca Agent
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="initialPosition"></param>
    public void AddAgent(int agentId, float3 initialPosition)
    {
        var agent = new CAgent() { Id = agentId, pos = initialPosition };
        CAgent a = agents.Add(agent as IVertex) as CAgent;
        a.radius = 2.5f;
        a.radiusObst = a.radius + 0.05f;
        //a.prefVelocity = float3(0f);
        //a.maxSpeed = 2;
    }

    private void AddObstacle(Vector3 position, Vector3 scale)
    {
        float halfx = scale.x * 0.5f;
        float halfy = scale.y * 0.5f;
        List<float3> vList = new List<float3>();
        vList.Add(position + new Vector3(-halfx, halfy, 0));
        vList.Add(position + new Vector3(-halfx, -halfy, 0));
        vList.Add(position + new Vector3(halfx, -halfy, 0));
        vList.Add(position + new Vector3(halfx, halfy, 0));

        obstacles.Add(vList, false);
    }

    private void Awake()
    {
        Instance = this;

        agents = new AgentGroup<CAgent>();

        obstacles = new ObstacleGroup();
        dynObstacles = new ObstacleGroup();
        raycasts = new RaycastGroup();

        simulation = new ORCA();
        simulation.plane = axis;
        simulation.agents = agents;
        simulation.staticObstacles = obstacles;
        simulation.dynamicObstacles = dynObstacles;
        simulation.raycasts = raycasts;

    }

    private void Start()
    {


    }

    private void Update()
    {
        //Schedule the simulation job. 
        simulation.Schedule(Time.deltaTime);

        if (Input.GetMouseButtonDown(0))  // 检测鼠标左键点击
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // 从摄像机发射一条射线到鼠标点击的位置
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitPoint = hit.point;
                hitPoint.z = 0;

                float halfRadius = Random.Range(0.5f, 2f);
                GameObject obstacleInstance = GameObject.Instantiate(ObstacleGameObject);
                obstacleInstance.transform.SetParent(ObstacleGameObjectParent.transform, false);
                obstacleInstance.transform.position = hitPoint;
                obstacleInstance.transform.localScale = obstacleInstance.transform.localScale * halfRadius;
                obstacleInstance.transform.localRotation = Quaternion.identity;
                AddObstacle(hitPoint, obstacleInstance.transform.lossyScale);
            }
        }

        //Store "target" position
        float3 tr = target.position;

        //Draw agents debug
        IAgent agent;
        float3 agentPos;
        for (int i = 0, count = agents.Count; i < count; i++)
        {
            agent = agents[i] as IAgent;
            agentPos = agent.pos;

#if UNITY_EDITOR
            //Agent body
            if (axis == AxisPair.XY)
            {
                Draw.Circle2D(agentPos, agent.radius, Color.green, 12);
                Draw.Circle2D(agentPos, agent.radiusObst, Color.cyan.A(0.15f), 12);
            }
            else
            {
                Draw.Circle(agentPos, agent.radius, Color.green, 12);
                Draw.Circle(agentPos, agent.radiusObst, Color.cyan.A(0.15f), 12);

            }
            //Agent simulated velocity (ORCA compliant)
            Draw.Line(agentPos, agentPos + (normalize(agent.velocity) * agent.radius), Color.green);
            //Agent goal vector
            Draw.Line(agentPos, agentPos + (normalize(agent.prefVelocity) * agent.radius), Color.grey);
#endif

            agent.prefVelocity = normalize(tr - agent.pos) * 3;

        }
    }

    private void LateUpdate()
    {
        if (simulation.TryComplete())
        {
            int oCount = dynObstacles.Count;
            float delta = Time.deltaTime * 50f;

            for (int i = 0; i < oCount; i++)
                dynObstacles[i].Offset(float3(Random.Range(-delta, delta), Random.Range(-delta, delta), 0f));

        }
    }

    private void OnApplicationQuit()
    {
        //Make sure to clean-up the jobs
        simulation.DisposeAll();
    }

}