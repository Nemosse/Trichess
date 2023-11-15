using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class History : MonoBehaviour
{
    public static List<Record> history = new List<Record>();

    public static void Push(Record record)
    {
        history.Add(record);
    }

    public void Awake()
    {
        if (history.Count > 0)
        {
            history = new List<Record>();
        }
    }
}

[System.Serializable]
public struct Record
{
    private PieceField oldPieceMoverField;
    private Pieces moverPiece;
    private PieceField newPieceMoverField;
    private Pieces takenPiece;
    private PieceField oldRookCastleField;
    private PieceField newRookCastleField;
    private Pieces pawnPromotionPiece;
    private bool firstMove;

    public Record(
        PieceField oldPieceMoverField,
        Pieces moverPiece,
        PieceField newPieceMoverField,
        Pieces takenPiece,
        bool firstMove = false,
        PieceField oldRookCastleField = null,
        PieceField newRookCastleField = null,
        Pieces pawnPromotionPiece = null
    )
    {
        this.oldPieceMoverField = oldPieceMoverField;
        this.moverPiece = moverPiece;
        this.newPieceMoverField = newPieceMoverField;
        this.takenPiece = takenPiece;
        this.oldRookCastleField = oldRookCastleField;
        this.newRookCastleField = newRookCastleField;
        this.firstMove = firstMove;
        this.pawnPromotionPiece = pawnPromotionPiece;
    }

    public PieceField GetVariablePieceField(int i)
    {
        switch (i)
        {
            case 0:
                return oldPieceMoverField;
            case 1:
                return newPieceMoverField;
            case 2:
                return oldRookCastleField;
            case 3:
                return newRookCastleField;
            default:
                return null;
        }
    }

    public bool GetFirstMove()
    {
        return firstMove;
    }

    public Pieces GetVariablePiece(int i)
    {
        switch (i)
        {
            case 0:
                return moverPiece;
            case 1:
                return takenPiece;
            case 2:
                return pawnPromotionPiece;
            default:
                return null;
        }
    }

    public void SetPromotionPawnNewPiece(Pieces newPiece)
    {
        Debug.Log("Setting promotion pawn new piece");
        pawnPromotionPiece = newPiece;
        Debug.Log("Promotion pawn new piece set: " + newPiece);
    }

}