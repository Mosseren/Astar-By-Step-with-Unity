using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridInfo
{
    public int g = 0;
    public int h = 0;
    public bool visited = false;
    public bool inPath = false;
    public int f
    {
        get {
            return g + h;
        }
    }
    public GridInfo(int g, int h, bool visited = false)
    {
        this.g = g;
        this.h = h;
        this.visited = visited;
    }
}

public class Astar
{
    Action displayChessBoardDel;

    public Vector2Int source = new Vector2Int(2, 3);

    public Vector2Int target = new Vector2Int(6, 8);

    bool eightDirEnabled = true;

    int[] deltaXs = {-1,  0,  1,  0, -1,  1,  1, -1};
    int[] deltaYs = { 0, -1,  0,  1, -1, -1,  1,  1};
    int[] costs   = {10, 10, 10, 10, 14, 14, 14, 14};

    int row = 9;
    int col = 11;
    int straightCost = 10;
    int obliqueCost = 14;

    public bool begun = false;
    public bool goNextStep = true;
    public bool finished = false;
    public bool marked = false;

    int frontierNum = 0;

    public HashSet<Vector2Int> bariers = new HashSet<Vector2Int>();

    public Dictionary<Vector2Int, GridInfo> Grids = new Dictionary<Vector2Int, GridInfo>();

    public Dictionary<Vector2Int, Vector2Int> curToPre = new Dictionary<Vector2Int, Vector2Int>();

    public Astar(Vector2Int start, Vector2Int target, int row, int col, bool eightDirEnabled = true)
    {
        this.source = start;
        this.target = target;
        this.row = row;
        this.col = col;
    }

    public bool IsBarier(Vector2Int loc)
    {
        return bariers.Contains(loc);
    }

    public void ShiftBarier(Vector2Int loc)
    {
        if (bariers.Contains(loc)) {
            bariers.Remove(loc);
        } else {
            bariers.Add(loc);
        }
    }


    public void Clear()
    {
        if (begun) {
            Grids.Clear();
            curToPre.Clear();
            begun = false;
            finished = false;
            marked = false;
        }
    }

    private int  GetH(Vector2Int loc)
    {
        int cost;
        int xDis = Mathf.Abs(loc.x - target.x);
        int yDis = Mathf.Abs(loc.y - target.y);
        if (eightDirEnabled) {
            int obliqueDis = Mathf.Min(xDis, yDis);
            int straightDis = Mathf.Max(xDis, yDis) - obliqueDis;
            cost =  obliqueCost * obliqueDis + straightCost * straightDis;
        } else {
            cost = straightCost * (xDis + yDis);
        }
        
        return cost;
    }

    private void GetNeighbours(Vector2Int loc, int currG, ref List<Vector3Int> neibs)
    {
        int dirNum = eightDirEnabled ? 8 : 4;
        for (int i = 0; i < dirNum; ++i) {
            Vector2Int nextLoc = new Vector2Int(loc.x + deltaXs[i], loc.y + deltaYs[i]);
            if (bariers.Contains(nextLoc) || nextLoc.x < 0 || nextLoc.x >= row || 
                nextLoc.y < 0 || nextLoc.y >= col) {
                    continue;
            }
            neibs.Add(new Vector3Int(nextLoc.x, nextLoc.y, currG + costs[i]));
        }
    }

    private Vector2Int FindClosestGrid()
    {
        Debug.Log("找最近");
        Vector2Int ans = new Vector2Int(-1, -1);
        int f = int.MaxValue;
        foreach (var pair in Grids) {
            GridInfo info = pair.Value;
            Vector2Int loc = pair.Key;
            if (info.visited == false && info.f < f) {
                f = info.f;
                ans = loc;
            }
        }
        return ans;
    }

    public void getDel(Action del)
    {
        displayChessBoardDel = del;
    }

    public void FindPath()
    {
        if (!begun) {
            begun = true;
            Grids[source] = new GridInfo(0, GetH(source));
            curToPre[source] = new Vector2Int(-1, -1);
            ++frontierNum;
        }
        if (finished) {
            if (!marked) {
                MarkPath();
                marked = true;
            }
            return;
        }
        List<Vector3Int> neibs = new List<Vector3Int>();

        Debug.Log(frontierNum);
        while (frontierNum > 0) {
            Vector2Int currLoc = FindClosestGrid();
            Grids[currLoc].visited = true;
            --frontierNum;
            if (currLoc == target) {
                break;
            }
            
            GetNeighbours(currLoc, Grids[currLoc].g, ref neibs);
            foreach (Vector3Int neib in neibs) {
                Vector2Int neibLoc = new Vector2Int(neib.x, neib.y);
                int neibG = neib.z;
                if (!Grids.ContainsKey(neibLoc)) {
                    Grids[neibLoc] = new GridInfo(neibG, GetH(neibLoc));
                    curToPre[neibLoc] = currLoc;
                    ++frontierNum;
                } else if (neibG < Grids[neibLoc].g) {
                    Grids[neibLoc].g = neibG;
                    curToPre[neibLoc] = currLoc;
                }
            }
            neibs.Clear();

            displayChessBoardDel();
            return;
        }

        finished = true;
        displayChessBoardDel();
    }

    public void MarkPath()
    {
        Vector2Int loc = target;
        Vector2Int end = new Vector2Int(-1, -1);
        if (!curToPre.ContainsKey(loc)) {
            return;
        }
        while (loc != end) {
            Debug.Log("mark");
            Grids[loc].inPath = true;
            loc = curToPre[loc];
        }
        Grids[source].inPath = true;
        displayChessBoardDel();
    }
}
