using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Piece", fileName = "Piece")]
public class Pieces : ScriptableObject
{
    [SerializeField] private PieceTypes pieceType;
    // [SerializeField] private PieceMoveSets pieceMoveSet;
    [SerializeField] private Players playerOwner;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool castlable = true;

    public Pieces getChess()
    {
        return this;
    }

    public bool GetCastlable() => castlable;

    public PieceTypes GetPieceType()
    {
        return pieceType;
    }

    // public PieceMoveSets GetPieceMoveSets()
    // {
    //     return pieceMoveSet;
    // }


    public Players GetPlayerOwner()
    {
        return playerOwner;
    }

    public void SetPlayerOwner(Players player)
    {
        playerOwner = player;
    }

    public void SetCastlable(bool castlable)
    {
        this.castlable = castlable;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

}
