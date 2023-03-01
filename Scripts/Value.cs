using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Value : MonoBehaviour
{
    private Grid.Values pieceValue;
    public Grid.Values PieceValue
    {
        get{return pieceValue;}
        set{pieceValue = value;}
    }
}

