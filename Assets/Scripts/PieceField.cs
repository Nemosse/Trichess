using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PieceField : MonoBehaviour
{
    [SerializeField] private Pieces piece;
    [SerializeField] private Players color;
    [SerializeField] private BoardPosition boardPosition;
    // private Position position;
    [SerializeField] private int row; //0-3
    [SerializeField] private int column; //0-7
    [SerializeField] private PieceField forwardField = null;
    [SerializeField] private PieceField LeftField = null;
    [SerializeField] private PieceField RightField = null;
    [SerializeField] private PieceField BackwardField = null;



    // [SerializeField] private BoardColumn boardColumn;
    // [SerializeField] private BoardRow boardRow;

    void Awake()
    {
        // position = new Position(color, row, column);
    }

    public PieceField(Players color, int row, int column)
    {
        this.color = color;
        this.row = row;
        this.column = column;
    }

    public int GetRow() => row;
    public int GetColumn() => column;


    public Pieces GetPiece()
    {
        return piece;
    }

    public void SetPiece(Pieces _piece)
    {
        piece = _piece;
    }

    public Players GetColor()
    {
        return color;
    }

    // public void SetColor(Players player)
    // {
    //     color = player;
    // }

    public BoardPosition GetBoardPosition()
    {
        return boardPosition;
    }

    // public Position GetPosition()
    // {
    //     return position;
    // }



    public PieceField MovableSpaceCheck(Direction direction)
    {
        switch (direction)
        {
            case Direction.Forward:
                // if (row < 3) return Position.Get(color, row + 1, column);
                // if (column < 4 && color == Players.Player1) return Position.Get(Players.Player2, 3, 7 - column);
                // if (color == Players.Player1) return Position.Get(Players.Player3, 3, 7 - column);
                // if (column < 4 && color == Players.Player2) return Position.Get(Players.Player3, 3, 7 - column);
                // if (color == Players.Player2) return Position.Get(Players.Player1, 3, 7 - column);
                // if (column < 4 && color == Players.Player3) return Position.Get(Players.Player1, 3, 7 - column);
                // if (color == Players.Player3) return Position.Get(Players.Player2, 3, 7 - column);
                // return Position.Get(color, row + 1, column);
                return GetNextPieceFieldInDirection(Direction.Forward);
            case Direction.Backward:
                // if (row == 0) throw new Exception("Moved off board");
                // return Position.Get(color, row - 1, column);
                return GetNextPieceFieldInDirection(Direction.Backward);
            case Direction.Left:
                // if (column == 0) throw new Exception("Moved off board");
                // return Position.Get(color, row, column - 1);
                return GetNextPieceFieldInDirection(Direction.Left);
            case Direction.Right:
                // if (column == 7) throw new Exception("Moved off board");
                // return Position.Get(color, row, column + 1);
                return GetNextPieceFieldInDirection(Direction.Right);
        }
        throw new Exception("Unreachable code?");
    }

    public PieceField GetNextPieceFieldInDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Forward:
                return forwardField;
            case Direction.Left:
                return LeftField;
            case Direction.Right:
                return RightField;
            case Direction.Backward:
                return BackwardField;
            default:
                return null;

        }
    }

}
