using Battle;
using Unity.Mathematics;
using UnityEngine;

public class GameApp : MonoBehaviour
{

    public int2 CellGrid = 50;
    public float CellSize = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BattleConfig.CELL_WIDTH = CellGrid.x;
        BattleConfig.CELL_WIDTH = CellGrid.y;
    }
}
