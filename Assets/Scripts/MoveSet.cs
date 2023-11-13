using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSet : MonoBehaviour
{
    private static Direction[][] pawnMoveSet()
    {
        return new Direction[][]
        {
            new Direction[] { Direction.Forward },
            new Direction[] { Direction.Forward, Direction.Forward },
            new Direction[] { Direction.Forward, Direction.Left },
            new Direction[] { Direction.Left, Direction.Forward },
            new Direction[] { Direction.Forward, Direction.Right },
            new Direction[] { Direction.Right, Direction.Forward }
        };
    }

    private static Direction[][] knightMoveSet()
    {
        return new Direction[][]
        {
            new Direction[] { Direction.Forward, Direction.Forward, Direction.Left },
            new Direction[] { Direction.Forward, Direction.Forward, Direction.Right },
            new Direction[] { Direction.Forward, Direction.Left, Direction.Left },
            new Direction[] { Direction.Forward, Direction.Right, Direction.Right },
            new Direction[] { Direction.Backward, Direction.Backward, Direction.Left },
            new Direction[] { Direction.Backward, Direction.Backward, Direction.Right },
            new Direction[] { Direction.Backward, Direction.Left, Direction.Left },
            new Direction[] { Direction.Backward, Direction.Right, Direction.Right },
            new Direction[] { Direction.Left, Direction.Left, Direction.Forward },
            new Direction[] { Direction.Left, Direction.Left, Direction.Backward },
            new Direction[] { Direction.Left, Direction.Forward, Direction.Forward },
            new Direction[] { Direction.Left, Direction.Backward, Direction.Backward },
            new Direction[] { Direction.Right, Direction.Right, Direction.Forward },
            new Direction[] { Direction.Right, Direction.Right, Direction.Backward },
            new Direction[] { Direction.Right, Direction.Forward, Direction.Forward },
            new Direction[] { Direction.Right, Direction.Backward, Direction.Backward }
        };
    }

    private static Direction[][] BishopMoveSet()
    {
        return new Direction[][]
        {
            new Direction[] { Direction.Forward, Direction.Left },
            new Direction[] { Direction.Forward, Direction.Right },
            new Direction[] { Direction.Left, Direction.Forward },
            new Direction[] { Direction.Right, Direction.Forward },
            new Direction[] { Direction.Backward, Direction.Left },
            new Direction[] { Direction.Backward, Direction.Right },
            new Direction[] { Direction.Left, Direction.Backward },
            new Direction[] { Direction.Right, Direction.Backward }
        };
    }

    private static Direction[][] RookMoveSet()
    {
        return new Direction[][]
        {
            new Direction[] { Direction.Forward },
            new Direction[] { Direction.Backward },
            new Direction[] { Direction.Left },
            new Direction[] { Direction.Right }
        };
    }

    private static Direction[][] KingMoveSet()
    {
        return new Direction[][]
        {
            new Direction[] { Direction.Forward, Direction.Left },
            new Direction[] { Direction.Forward, Direction.Right },
            new Direction[] { Direction.Left, Direction.Forward },
            new Direction[] { Direction.Right, Direction.Forward },
            new Direction[] { Direction.Backward, Direction.Left },
            new Direction[] { Direction.Backward, Direction.Right },
            new Direction[] { Direction.Left, Direction.Backward },
            new Direction[] { Direction.Right, Direction.Backward },
            new Direction[] { Direction.Forward },
            new Direction[] { Direction.Backward },
            new Direction[] { Direction.Left },
            new Direction[] { Direction.Right }
        };
    }

    public static Direction[][] GetMoveSet(PieceTypes pieceType)
    {
        switch (pieceType)
        {
            case PieceTypes.Pawn:
                return pawnMoveSet();
            case PieceTypes.Knight:
                return knightMoveSet();
            case PieceTypes.Bishop:
                return BishopMoveSet();
            case PieceTypes.Rook:
                return RookMoveSet();
            default:
                return KingMoveSet();
        }
    }

    // public int GetStepReps(PieceTypes pieceType)
    // {
    //     switch (pieceType)
    //     {
    //         case PieceTypes.Rook:
    //         case PieceTypes.Queen:
    //         case PieceTypes.Bishop:
    //             return 8;
    //         default:
    //             // Kings, pawns, and knights cannot repeat their moves.
    //             return 1;
    //     }

    // }
}

public enum PieceMoveSets
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}
