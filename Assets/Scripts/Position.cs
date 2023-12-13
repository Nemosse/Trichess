using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Position : MonoBehaviour
{

    public static BoardPosition Get(Players colour, int row, int column)
    {
        int index = row + 4 * column;
        if (index >= 0 && index < 32)
        {
            switch (colour)
            {
                case Players.Player1:
                    return (BoardPosition)index;
                case Players.Player2:
                    return (BoardPosition)(index + 32);
                case Players.Player3:
                    return (BoardPosition)(index + 64);
                default:
                    throw new Exception("No such position.");
            }
        }
        throw new ApplicationException("No such position.");
    }


}

public enum BoardPosition
{
    BA1 = 0, BA2, BA3, BA4,
    BB1, BB2, BB3, BB4,
    BC1, BC2, BC3, BC4,
    BD1, BD2, BD3, BD4,
    BE1, BE2, BE3, BE4,
    BF1, BF2, BF3, BF4,
    BG1, BG2, BG3, BG4,
    BH1, BH2, BH3, BH4,
    GA1 = 32, GA2, GA3, GA4,
    GB1, GB2, GB3, GB4,
    GC1, GC2, GC3, GC4,
    GD1, GD2, GD3, GD4,
    GE1, GE2, GE3, GE4,
    GF1, GF2, GF3, GF4,
    GG1, GG2, GG3, GG4,
    GH1, GH2, GH3, GH4,
    RA1 = 64, RA2, RA3, RA4,
    RB1, RB2, RB3, RB4,
    RC1, RC2, RC3, RC4,
    RD1, RD2, RD3, RD4,
    RE1, RE2, RE3, RE4,
    RF1, RF2, RF3, RF4,
    RG1, RG2, RG3, RG4,
    RH1, RH2, RH3, RH4
}

public enum VirtualBoardPosition
{
    BA1 = 0, BA2, BA3, BA4,
    BB1, BB2, BB3, BB4,
    BC1, BC2, BC3, BC4,
    BD1, BD2, BD3, BD4,
    BE1, BE2, BE3, BE4,
    BF1, BF2, BF3, BF4,
    BG1, BG2, BG3, BG4,
    BH1, BH2, BH3, BH4,
    GA1 = 32, GA2, GA3, GA4,
    GB1, GB2, GB3, GB4,
    GC1, GC2, GC3, GC4,
    GD1, GD2, GD3, GD4,
    GE1, GE2, GE3, GE4,
    GF1, GF2, GF3, GF4,
    GG1, GG2, GG3, GG4,
    GH1, GH2, GH3, GH4,
    RA1 = 64, RA2, RA3, RA4,
    RB1, RB2, RB3, RB4,
    RC1, RC2, RC3, RC4,
    RD1, RD2, RD3, RD4,
    RE1, RE2, RE3, RE4,
    RF1, RF2, RF3, RF4,
    RG1, RG2, RG3, RG4,
    RH1, RH2, RH3, RH4
}
