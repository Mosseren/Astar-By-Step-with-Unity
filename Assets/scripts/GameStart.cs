using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameStart : MonoBehaviour
{
    Transform canvas;
    Transform chessBoard;
    Transform controlPanel;
    Transform pawn;
    Transform bishop;


    GameObject initGrid;

    Button changeSource;
    Button changeTarget;
    Button changeBarier;
    Button findPath;
    Button clear;

    [SerializeField]
    Vector2Int source = new Vector2Int(2, 3);
    [SerializeField]
    Vector2Int target = new Vector2Int(6, 8);

    int row = 9;
    int col = 11;

    Astar astar;

    Color initColor = Color.white;
    Color visitedColor = Color.blue;
    Color frontierColor = Color.green;
    Color pathColor = Color.yellow;
    Color barierColor = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>().transform;
        chessBoard = canvas.Find("ChessBoard").transform;
        controlPanel = canvas.Find("ControlPanel").transform;
        pawn = canvas.Find("Pawn").transform;
        bishop = canvas.Find("Bishop").transform;

        changeSource = controlPanel.Find("ChangeSource").GetComponent<Button>();
        changeTarget = controlPanel.Find("ChangeTarget").GetComponent<Button>();
        changeBarier = controlPanel.Find("ChangeBarier").GetComponent<Button>();
        findPath = controlPanel.Find("FindPath").GetComponent<Button>();
        clear = controlPanel.Find("Clear").GetComponent<Button>();

        changeSource.onClick.AddListener(ChangeSource);
        changeTarget.onClick.AddListener(ChangeTarget);
        changeBarier.onClick.AddListener(ChangeBarier);
        findPath.onClick.AddListener(FindPath);
        clear.onClick.AddListener(Clear);

        initGrid = chessBoard.Find("Grid").gameObject;
        astar = new Astar(source, target, row, col);

        CreateGrids();
        astar.getDel(DisplayChessBoard);
    }

    Vector2Int NameToLoc(string name)
    {
        Vector2Int loc = new Vector2Int();
        string[] nums = name.Split('_');
        loc.x = int.Parse(nums[1]);
        loc.y = int.Parse(nums[2]);
        return loc;
    }

    private void Choosen()
    {
        var buttonSelf = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Vector2Int loc = NameToLoc(buttonSelf.name);
        if (changeSource.interactable == false) {
            if (loc != target && !astar.IsBarier(loc)) {
                source = loc;
                if (astar != null) {
                    astar.source = loc;
                }
                pawn.SetParent(buttonSelf.transform, false);
            }
            changeSource.interactable = true;
        }

        if (changeTarget.interactable == false) {
            if (loc != source && !astar.IsBarier(loc)) {
                target = loc;
                if (astar != null) {
                    astar.target = loc;
                }
                bishop.SetParent(buttonSelf.transform, false);
            }
            changeTarget.interactable = true;
        }

        if (changeBarier.interactable == false) {
            if (loc != source && loc != target && astar != null) {
                astar.ShiftBarier(loc);
                Color color = astar.IsBarier(loc) ? barierColor : initColor;
                DisplayGrid(loc, color);
            }
        }
    }

    private void ChangeSource()
    {
        Clear();
        changeSource.interactable = false;
        changeTarget.interactable = true;
        changeBarier.interactable = true;
    }

    private void ChangeTarget()
    {
        Clear();
        changeTarget.interactable = false;
        changeSource.interactable = true;
        changeBarier.interactable = true;
    }

    private void ChangeBarier()
    {
        Clear();
        changeBarier.interactable = false;
        changeTarget.interactable = true;
        changeSource.interactable = true;
    }

    void CreateGrids()
    {
        for (int i = 0; i < row; ++i) {
            for (int j = 0; j < col; ++j) {
                GameObject o = Instantiate(initGrid, chessBoard);
                o.name = string.Format("Grid_{0}_{1}", i, j);
                o.GetComponent<Button>().onClick.AddListener(Choosen);
            }
        }
        initGrid.SetActive(false);
        pawn.SetParent(FindGrid(source));
        pawn.position = Vector3.zero;
        bishop.SetParent(FindGrid(target));
        bishop.position = Vector3.zero;
    }

    Transform FindGrid(Vector2Int loc)
    {
        name = string.Format("Grid_{0}_{1}", loc.x, loc.y);
        return chessBoard.Find(name).transform;
    }

    void DisplayGrid(Vector2Int loc, Color color)
    {
        Button grid = FindGrid(loc).GetComponent<Button>();
        grid.image.color = color;
    }

    void DisplayGrid(Vector2Int loc, Color color, int g, int h)
    {
        DisplayGrid(loc, color);
        Transform grid = FindGrid(loc);
        grid.Find("G").GetComponent<TextMeshProUGUI>().text = g == 0 ? "" : g.ToString();
        grid.Find("H").GetComponent<TextMeshProUGUI>().text = h == 0 ? "" : h.ToString();
        grid.Find("F").GetComponent<TextMeshProUGUI>().text = (g + h) == 0 ? "" : (g + h).ToString();
    }

    void DisplayChessBoard()
    {
        foreach (var v in astar.Grids) {
            Vector2Int loc = v.Key;
            GridInfo info = v.Value;
            Color color = info.visited ? visitedColor : frontierColor;
            if (info.inPath) {
                color = pathColor;
            }
            DisplayGrid(loc, color, info.g, info.h);
        }
    }

    void ClearChessBoard()
    {
        foreach (var v in astar.Grids) {
            Vector2Int loc = v.Key;
            DisplayGrid(loc, initColor, 0, 0);
        }
    }

    void FindPath()
    {
        changeSource.interactable = true;
        changeTarget.interactable = true;
        changeBarier.interactable = true;

        astar.FindPath();
    }

    void Clear()
    {
        changeSource.interactable = true;
        changeTarget.interactable = true;
        changeBarier.interactable = true;

        ClearChessBoard();
        astar.Clear();
    }
}
