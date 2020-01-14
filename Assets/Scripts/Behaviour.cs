﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Vert {
    public Vert(Vector2Int _v, Vert _p) { v = _v; p = _p; }
    public Vert(Vector2Int _v, Vert _p, int val) { v = _v; p = _p; value = val; }
    public Vector2Int v { get; set; }
    public Vert p { get; set; }
    public int value { get; set; }
};

public class Behaviour : MonoBehaviour {

    public bool ignoreDoors = false;
    public MobFactory mobFactory;
    List<Vert> open, closed;
    List<Vector2Int> path = new List<Vector2Int>();
    Vector2 pursuitPoint = new Vector2();

    public GameObject aaaa;
    void Start() {
        //StartCoroutine(changePPoint());

        //StartCoroutine(c_FindTheWay());
        //FindTheWay();

        /*path = FindPath(mobFactory.nav.mesh,
            mobFactory.nav.coordToGridValues(transform.position),
            mobFactory.nav.coordToGridValues(pursuitPoint));*/

        
        foreach (Vector2Int pos in path)
            Instantiate(aaaa, mobFactory.nav.gridToRealCoords(pos), new Quaternion());
    }

    protected virtual void FindTheWay_v2() {
        closed = new List<Vert>();
        open = new List<Vert>();
        Vert start = new Vert(mobFactory.nav.coordToGridValues(transform.position), null), current;
        open.Add(start);
    }

    protected virtual void FindTheWay() {
        closed = new List<Vert>();
        open = new List<Vert>();
        Vert start = new Vert(mobFactory.nav.coordToGridValues(transform.position), null), current;
        open.Add( start );
        while(open.Count > 0) {
            open.Sort(Comparer_v1);
            current = open[0];
            open.RemoveAt(0);
            int pass = 1;
            for(int i = -1; i < 2; i++)
                for(int q = -1; q < 2; q++) {
                    if((i==0)&&(q==0)) continue;
                    try { pass = mobFactory.nav.mesh[current.v.y + i, current.v.x + q]; }
                    catch (System.IndexOutOfRangeException) { continue; }
                    Vector2Int doubt = new Vector2Int(current.v.x+q, current.v.y+i);
                    if(pass==0 && !closed.Exists((Vert v) => { return v.v == doubt; })) {
                        open.Add( new Vert(doubt, current) );
                        if(doubt == mobFactory.nav.coordToGridValues(pursuitPoint)) {
                            path = new List<Vector2Int>();
                            path.Add(current.v);
                            while(current.p != null) {
                                current = current.p;
                                path.Add(current.v);
                            }
                            return;
                        }
                    }
                }
            closed.Add( current );

        }
        throw new System.TimeoutException("Can`t get a way");
    } 

    void FixedUpdate() {
        
    }

    IEnumerator DoorsInactiveByTime() {
        ignoreDoors = true;
        yield return new WaitForSeconds(3);
        ignoreDoors = false;
        yield break;
    }
    IEnumerator changePPoint() {
        while(true) {
            pursuitPoint = mobFactory.rndPoint();
            yield return new WaitForSeconds(8);
        }
    }
    IEnumerator c_safeCall(System.Action func) {
        func();
        yield break;
    }
    IEnumerator c_FindTheWay() {
        Coroutine c = StartCoroutine(c_safeCall(FindTheWay));
        yield return new WaitForSeconds(3);
        if (c != null) { StopCoroutine(c); Debug.Log("Failed to path"); }
    }

    private int Comparer_v1(Vert A, Vert B) {
        Vector2Int dest = mobFactory.nav.coordToGridValues(pursuitPoint);
        Vector2Int v1 = A.v-dest, v2 = B.v-dest;
        int a = v1.sqrMagnitude, b = v2.sqrMagnitude;
        if (a == b) return 0;
        else if (a > b) return 1;
        else return -1;
    }

    // 
    // Some interesting code (don`t works)
    // https://lsreg.ru/realizaciya-algoritma-poiska-a-na-c/
    //
    public class PathNode
    {
        // Координаты точки на карте.
        public Vector2Int Position { get; set; }
        // Длина пути от старта (G).
        public int PathLengthFromStart { get; set; }
        // Точка, из которой пришли в эту точку.
        public PathNode CameFrom { get; set; }
        // Примерное расстояние до цели (H).
        public int HeuristicEstimatePathLength { get; set; }
        // Ожидаемое полное расстояние до цели (F).
        public int EstimateFullPathLength
        {
            get
            {
                return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
            }
        }
    }

    public static List<Vector2Int> FindPath(int[,] field, Vector2Int start, Vector2Int goal)
    {
        // Шаг 1.
        var closedSet = new List<PathNode>();
        var openSet = new List<PathNode>();
        // Шаг 2.
        PathNode startNode = new PathNode()
        {
            Position = start,
            CameFrom = null,
            PathLengthFromStart = 0,
            HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
        };
        openSet.Add(startNode);
        while (openSet.Count > 0)
        {
            // Шаг 3.
            openSet.Sort((node1, node2) => node1.EstimateFullPathLength > node2.EstimateFullPathLength ? 1 :
              (node1.EstimateFullPathLength < node2.EstimateFullPathLength ? -1 : 0) );
            var currentNode = openSet[0];
            // Шаг 4.
            if (currentNode.Position == goal)
                return GetPathForNode(currentNode);
            // Шаг 5.
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            // Шаг 6.
            foreach (var neighbourNode in GetNeighbours(currentNode, goal, field))
            {
                // Шаг 7.
                if (closedSet.FindAll(node => node.Position == neighbourNode.Position).Count > 0)
                    continue;
                var openNode = openSet.Find(node =>
                  node.Position == neighbourNode.Position);
                // Шаг 8.
                if (openNode == null)
                    openSet.Add(neighbourNode);
                else
                  if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                {
                    // Шаг 9.
                    openNode.CameFrom = currentNode;
                    openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                }
            }
        }
        // Шаг 10.
        return null;
    }

    private static int GetDistanceBetweenNeighbours()
    {
        return 1;
    }

    private static int GetHeuristicPathLength(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    private static List<PathNode> GetNeighbours(PathNode pathNode, Vector2Int goal, int[,] field)
    {
        var result = new List<PathNode>();

        // Соседними точками являются соседние по стороне клетки.
        Vector2Int[] neighbourPoints = new Vector2Int[4];
        neighbourPoints[0] = new Vector2Int(pathNode.Position.x + 1, pathNode.Position.y);
        neighbourPoints[1] = new Vector2Int(pathNode.Position.x - 1, pathNode.Position.y);
        neighbourPoints[2] = new Vector2Int(pathNode.Position.x, pathNode.Position.y + 1);
        neighbourPoints[3] = new Vector2Int(pathNode.Position.x, pathNode.Position.y - 1);

        foreach (var point in neighbourPoints)
        {
            // Проверяем, что не вышли за границы карты.
            if (point.x < 0 || point.x >= field.GetLength(0))
                continue;
            if (point.y < 0 || point.y >= field.GetLength(1))
                continue;
            // Проверяем, что по клетке можно ходить.
            if (field[point.y, point.x] == 0)
                continue;
            // Заполняем данные для точки маршрута.
            var neighbourNode = new PathNode()
            {
                Position = point,
                CameFrom = pathNode,
                PathLengthFromStart = pathNode.PathLengthFromStart +
                GetDistanceBetweenNeighbours(),
                HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
            };
            result.Add(neighbourNode);
        }
        return result;
    }

    private static List<Vector2Int> GetPathForNode(PathNode pathNode)
    {
        var result = new List<Vector2Int>();
        var currentNode = pathNode;
        while (currentNode != null)
        {
            result.Add(currentNode.Position);
            currentNode = currentNode.CameFrom;
        }
        result.Reverse();
        return result;
    }
}
