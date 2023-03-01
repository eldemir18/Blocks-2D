using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] GameObject filledPiecePrefab;
    [SerializeField] GameObject topLeftPrefab;
    [SerializeField] GameObject topRightPrefab;
    [SerializeField] GameObject bottomLeftPiecePrefab;
    [SerializeField] GameObject bottomRightPrefab;

    private Dictionary<Tuple<int, int>, Grid.Values> pieceInfo;
    public Dictionary<Tuple<int, int>, Grid.Values> PieceInfo 
    { 
        get { return pieceInfo;} 
        set { pieceInfo = value;} 
    }

    private Vector3 dragOffset;
    private Camera mainCamera;
    private Grid gridRef;

    void Awake()
    {
        mainCamera = Camera.main;
        gridRef = FindObjectOfType<Grid>();
    }

    public void SetPieceGameObject(Dictionary<Tuple<int, int>, Grid.Values> _pieceInfo, int sortingOrder)
    {
        pieceInfo = _pieceInfo;
        List<Transform> childTransforms = new List<Transform>();
        Color color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        // Loop through all coordinates in the piece
        foreach (Tuple<int, int> coordinate in pieceInfo.Keys)
        {
            // Get the value at the current coordinate
            Grid.Values value = pieceInfo[coordinate];

            // Determine which prefab to use based on the value
            GameObject prefab;
            switch (value)
            {
                case Grid.Values.BOTTOMLEFT:
                    prefab = bottomLeftPiecePrefab;
                    break;
                case Grid.Values.BOTTOMRIGHT:
                    prefab = bottomRightPrefab;
                    break;
                case Grid.Values.TOPLEFT:
                    prefab = topLeftPrefab;
                    break;
                case Grid.Values.TOPRIGTH:
                    prefab = topRightPrefab;
                    break;
                default:
                    prefab = filledPiecePrefab;
                    break;
            }

            // Instantiate the prefab at the current coordinate
            SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();

            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = sortingOrder;
            
            Vector2 prefabPos = gridRef.GetWorldPosition(coordinate.Item1, coordinate.Item2, -3);
            
            GameObject gameObject = Instantiate
            (
                prefab,
                prefabPos,
                Quaternion.Euler(0, 0, prefab.transform.rotation.eulerAngles.z)
            );
            
            gameObject.transform.parent = transform;
            
            gameObject.GetComponent<Value>().PieceValue = value;
            
            childTransforms.Add(gameObject.transform);
        }
        
        Vector2 averagePosition = Vector2.zero;
        foreach (Transform childTransform in childTransforms)
        {
            averagePosition += (Vector2)childTransform.position;
        }
        averagePosition /= childTransforms.Count;

        float randomX = UnityEngine.Random.Range(-2.5f, 2.5f);
        float randomY = UnityEngine.Random.Range(-4f, -3f);

        Vector2 newPos = new Vector2(randomX, randomY);

        transform.position += (Vector3)(newPos - averagePosition);
    }


    private void OnMouseDown()
    {
        dragOffset = transform.position - GetMousePos();
        gridRef.SelectGamePiece(this);
        gridRef.DeletePiece();
    }

    private void OnMouseDrag()
    {
        transform.position = dragOffset + GetMousePos();
    }

    private void OnMouseUp()
    {
        gridRef.ReleasePiece();
    }

    private Vector3 GetMousePos()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }

}
    

