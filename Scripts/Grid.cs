using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Grid : MonoBehaviour
{
    public enum DifficultyLevel
    {
        EASY,
        MEDIUM,
        HARD,
        COUNT
    }

    public enum Values
    {
        EMPTY,
        TOPLEFT,
        TOPRIGTH,
        BOTTOMLEFT,
        BOTTOMRIGHT,
        FILLED
    };

    private int gridSize = 6;
    private int pieceCount;
    private int crossCount;
    
    private Dictionary<Vector2, Values> gridVals;
    
    [SerializeField] GameObject backgroundPrefab;
    [SerializeField] GamePiece gamePiecePrefab;
    [SerializeField] GameObject nextButton;
    [SerializeField] TextMeshProUGUI diffucultyText;

    private DifficultyLevel currentDifficultyLevel;
    private bool gameOver;
    
    private GamePiece selectedGamePiece;
    private List<Dictionary<Tuple<int, int>, Values>> pieceList; 
    public List<Dictionary<Tuple<int, int>, Values>> PieceList
    {
        get{return pieceList;}
        set{pieceList = value;}
    }

    void Start()
    {
        CreateBackGroundGrid();
        // Create pieces
        CreatePieces();
    }

    // Creates a background grid and initializes a dictionary to store values for each cell
    private void CreateBackGroundGrid()
    {
        gridVals = new Dictionary<Vector2, Values>();

        // Loop through each row and column of the grid
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Instantiate a background prefab at the given coordinates and add its position and an EMPTY value to the gridVals dictionary
                GameObject background = Instantiate(backgroundPrefab, GetWorldPosition(x, y, 3), Quaternion.identity);
                gridVals.Add(background.transform.position, Values.EMPTY);

                // Set the background as a child of the current object
                background.transform.parent = transform;

                // Check if the background is on the border and enable the appropriate border game object
                if (x == 0) // left border
                {
                    background.transform.Find("BorderLeft").gameObject.SetActive(true);
                }
                else if (x == gridSize - 1) // right border
                {
                    background.transform.Find("BorderRight").gameObject.SetActive(true);
                }

                if (y == 0) // bottom border
                {
                    background.transform.Find("BorderBottom").gameObject.SetActive(true);
                }
                else if (y == gridSize - 1) // top border
                {
                    background.transform.Find("BorderTop").gameObject.SetActive(true);
                }

                // Check if the background is at an even intersection of the grid and enable the circle game object
                if (x % 2 == 0 && y % 2 == 0)
                {
                    background.transform.Find("Circle").gameObject.SetActive(true);
                }
            }
        }
    }


    private void CreatePieces()
    {
        // Set the difficulty level for the game
        SetDifficulty();

        // Assign grid coordinates
        List<List<Tuple<int, int>>> pieceCoordinates = AssignPieceCoordinates();

        // Randomly split the coordinates of pieces
        SplitGridCoordinates(pieceCoordinates);

        // Fill the piece list with the split coordinates
        FillPieceList(pieceCoordinates);

        // Randomly assign diagonal lines to some of the pieces
        AssignDiagnols();

        // Set the pieces on the game board
        SetPieces();
    }

    // Set the difficulty level for the game
    private void SetDifficulty()
    {
        // Choose a random difficulty level from the available options
        int randomIndex = UnityEngine.Random.Range(0, (int)DifficultyLevel.COUNT);
        currentDifficultyLevel = (DifficultyLevel)randomIndex;

        // Update the UI text to display the chosen difficulty level
        SetDifficultyText(currentDifficultyLevel);

        // Set the game parameters based on the chosen difficulty level
        if (currentDifficultyLevel == DifficultyLevel.EASY)
        {
            pieceCount = UnityEngine.Random.Range(5, 7);
            crossCount = UnityEngine.Random.Range(0, 4);
        }
        else if (currentDifficultyLevel == DifficultyLevel.MEDIUM)
        {
            pieceCount = UnityEngine.Random.Range(7, 9);
            crossCount = UnityEngine.Random.Range(4, 6);
        }
        else // DifficultyLevel.HARD
        {
            pieceCount = UnityEngine.Random.Range(9, 12);
            crossCount = UnityEngine.Random.Range(6, 8);
        }
    }


    public void SetDifficultyText(DifficultyLevel difficultyLevel)
    {
        diffucultyText.text = difficultyLevel.ToString();
    }

    private void AssignDiagnols()
    {
        int currenCrossCount = 0;
        while (currenCrossCount < crossCount)
        {
            // Select a random index for the pieceList
            int randomIndex = UnityEngine.Random.Range(0, pieceList.Count);

            // Get the keys of the dictionary
            Tuple<int, int>[] keys = pieceList[randomIndex].Keys.Cast<Tuple<int, int>>().ToArray();

            // Select a random key from the keys array
            Tuple<int, int> randomKey = keys[UnityEngine.Random.Range(0, keys.Length)];

            // Get a random value from the BL, BR, TL, and TR values
            Values randomValue = (Values)UnityEngine.Random.Range(1, 5);

            // Update the value of the random coordinate in the dictionary
            if (pieceList[randomIndex][randomKey] != Values.FILLED) continue;

            // Get the x and y values from the random key
            int x = randomKey.Item1;
            int y = randomKey.Item2;

            // Calculate the coordinates for the adjacent cells
            Tuple<int, int> topCoord = new Tuple<int, int>(x, y + 1);
            Tuple<int, int> bottomCoord = new Tuple<int, int>(x, y - 1);
            Tuple<int, int> leftCoord = new Tuple<int, int>(x - 1, y);
            Tuple<int, int> rightCoord = new Tuple<int, int>(x + 1, y);

            // If the random value is TOPLEFT or BOTTOMRIGHT, try to add the key-value pairs to the adjacent cells
            if (randomValue == Values.TOPLEFT || randomValue == Values.BOTTOMRIGHT)
            {
                // Check if the adjacent cells are fits corners
                if ((y + 1 >= gridSize && x - 1 < 0) || (y - 1 < 0 && x + 1 >= gridSize)) continue;

                // Remove the key-value pair from the current piece
                pieceList[randomIndex].Remove(randomKey);

                // Try to add the key-value pairs to the adjacent cells
                if (y + 1 < gridSize) AddToPiece(randomKey, Values.TOPLEFT, topCoord, "Top");
                else if (x - 1 >= 0) AddToPiece(randomKey, Values.TOPLEFT, leftCoord, "Left");

                if (y - 1 >= 0) AddToPiece(randomKey, Values.BOTTOMRIGHT, bottomCoord, "Bottom");
                else if (x + 1 < gridSize) AddToPiece(randomKey, Values.BOTTOMRIGHT, rightCoord, "Right");
            }
            // If the random value is TOPRIGHT or BOTTOMLEFT, try to add the key-value pairs to the adjacent cells
            else if (randomValue == Values.TOPRIGTH || randomValue == Values.BOTTOMLEFT)
            {
                // Check if the adjacent cells are fits corners
                if ((y + 1 >= gridSize && x + 1 >= gridSize) || (y - 1 < 0 && x - 1 < 0)) continue;

                // Remove the key-value pair from the current piece
                pieceList[randomIndex].Remove(randomKey);

                // Try to add the key-value pairs to the adjacent cells
                if (y + 1 < gridSize) AddToPiece(randomKey, Values.TOPRIGTH, topCoord, "Top");
                else if (x + 1 < gridSize) AddToPiece(randomKey, Values.TOPRIGTH, rightCoord, "Right");

                if (y - 1 >= 0) AddToPiece(randomKey, Values.BOTTOMLEFT, bottomCoord, "Bottom");
                else if (x - 1 >= 0) AddToPiece(randomKey, Values.BOTTOMLEFT, leftCoord, "Left");
            }

            // Increase currentCrossCount
            currenCrossCount++;
        }
    }

    public void SetPieces()
    {
        for (int i = 0; i < pieceList.Count; i++)
        {
            Dictionary<Tuple<int, int>, Values> currentPiece = pieceList[i];
            GamePiece gamePiece = Instantiate(gamePiecePrefab, Vector3.zero, Quaternion.identity);
            gamePiece.SetPieceGameObject(currentPiece, i);
        }
    }

    private void AddToPiece(Tuple<int, int> randomKey, Values randomValue, Tuple<int, int> newKey, string direction)
    {
        foreach (Dictionary<Tuple<int, int>, Values> pieceDict in pieceList)
        {
            if (pieceDict.ContainsKey(newKey))
            {
                switch (direction)
                {
                    case "Top":
                        if(pieceDict[newKey] == Values.TOPLEFT || pieceDict[newKey] == Values.TOPRIGTH) continue;
                        break;
                    case "Bottom":
                        if(pieceDict[newKey] == Values.BOTTOMLEFT || pieceDict[newKey] == Values.BOTTOMRIGHT) continue;
                        break;
                    case "Left":
                        if(pieceDict[newKey] == Values.TOPLEFT || pieceDict[newKey] == Values.BOTTOMLEFT) continue;
                        break;
                    case "Right":
                        if(pieceDict[newKey] == Values.TOPRIGTH || pieceDict[newKey] == Values.BOTTOMRIGHT) continue;
                        break;
                    default:
                        Debug.LogError("Direction not valid");
                        break;
                }

                // If it is in same piece
                if(pieceDict.ContainsKey(randomKey))
                {
                    pieceDict[randomKey] = Values.FILLED;
                }
                else
                {
                    pieceDict.Add(randomKey, randomValue);
                }

                return;
            }
        }
    }

    private void FillPieceList(List<List<Tuple<int, int>>> pieces)
    {
        pieceList = new List<Dictionary<Tuple<int, int>, Values>>();
        // Loop through each piece
        for (int i = 0; i < pieces.Count; i++)
        {
            // Create a new dictionary for the current piece
            Dictionary<Tuple<int, int>, Values> currentPiece = new Dictionary<Tuple<int, int>, Values>();

            // Loop through each coordinate in the current piece
            for (int j = 0; j < pieces[i].Count; j++)
            {
                // Set the value of the current coordinate to FILLED
                Tuple<int, int> coord = pieces[i][j];
                currentPiece.Add(coord, Values.FILLED);
            }

            // Add the current piece to the list of pieces
            pieceList.Add(currentPiece);
        }
    }

    private void SplitGridCoordinates(List<List<Tuple<int, int>>> pieces)
    {
        while (pieces.Count < pieceCount)
        {
            int maxIndex = 0;
            int maxCoordinateCount = 0;

            // Find the index of the piece with the most coordinates
            foreach (List<Tuple<int, int>> piece in pieces)
            {
                int coordinateCount = piece.Count;
                if (coordinateCount > maxCoordinateCount)
                {
                    maxCoordinateCount = coordinateCount;
                    maxIndex = pieces.IndexOf(piece);
                }
            }

            // Get the largest piece and split it
            List<Tuple<int, int>> largestPiece = pieces[maxIndex];
            pieces.RemoveAt(maxIndex);

            int largestPieceWidth  = largestPiece.Max(coord => coord.Item1) - largestPiece.Min(coord => coord.Item1);
            int largestPieceHeight = largestPiece.Max(coord => coord.Item2) - largestPiece.Min(coord => coord.Item2);

            // Create 2 new pieces
            List<Tuple<int, int>> newPiece1 = new List<Tuple<int, int>>();
            List<Tuple<int, int>> newPiece2 = new List<Tuple<int, int>>();

            if (largestPieceWidth > largestPieceHeight)
            {
                // Split the piece horizontally
                int splitX = largestPiece.Min(coord => coord.Item1) + UnityEngine.Random.Range(0, largestPieceWidth);
                foreach (Tuple<int, int> coord in largestPiece)
                {
                    if (coord.Item1 < splitX)
                    {
                        newPiece1.Add(coord);
                    }
                    else
                    {
                        newPiece2.Add(coord);
                    }
                }
            }
            else
            {
                // Split the piece vertically
                int splitY = largestPiece.Min(coord => coord.Item2) + UnityEngine.Random.Range(0, largestPieceHeight);
                foreach (Tuple<int, int> coord in largestPiece)
                {
                    if (coord.Item2 < splitY)
                    {
                        newPiece1.Add(coord);
                    }
                    else
                    {
                        newPiece2.Add(coord);
                    }
                }
            }

            if (newPiece1.Count > 0) // Only add non-empty pieces
            {
                pieces.Add(newPiece1);
            }
            if (newPiece2.Count > 0) // Only add non-empty pieces
            {
                pieces.Add(newPiece2);
            }
        }
    }

    // This function assigns piece coordinates to a two-dimensional board game.
    private List<List<Tuple<int, int>>> AssignPieceCoordinates()
    {
        // Create a new list to hold all of the piece coordinates
        List<List<Tuple<int, int>>> pieceCoordinates = new List<List<Tuple<int, int>>>();
        
        // Create a new list to hold all of the grid coordinates
        List<Tuple<int, int>> gridCoordinates = new List<Tuple<int, int>>();

        // Loop over all rows and columns in the board
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Create a new tuple representing the current piece coordinate
                Tuple<int, int> key = new Tuple<int, int>(x, y);
                
                // Add the tuple to the list of piece coordinates
                gridCoordinates.Add(key);
            }
        }

        // Add the list of grid coordinates to the list of piece coordinates
        pieceCoordinates.Add(gridCoordinates);
        
        // Return the list of piece coordinates with their assigned grid coordinates
        return pieceCoordinates;
    }


    public Vector2 GetWorldPosition (int x, int y, int offsetY)
    {
        // Set offsets if x or y is odd
        float offsetX = 0.5f;
        if (gridSize % 2 == 1) offsetX = 0f;

        // Center grid
        return new Vector2
        (
            transform.position.x - gridSize / 2 + x + offsetX, 
            transform.position.y - gridSize / 2 + y + offsetX + offsetY
        );
    } 


    public bool IsGameFinished()
    {
        // Check all grid values
        foreach(KeyValuePair<Vector2, Values> gridPiece in gridVals)
        {
            if(gridPiece.Value != Values.FILLED) return false;
        }

        return true;
    }

    public void ReleasePiece()
    {
        float snapDistance = 0.5f;
        Vector2 possibleDifference = Vector2.zero;
        bool possibleSnap = false;
        
        // Get the number of child objects
        int numChildren = selectedGamePiece.transform.childCount;

        // Create an array to hold the child objects
        Transform[] children = new Transform[numChildren];

        // Iterate over the children and add them to the array
        for (int i = 0; i < numChildren; i++)
        {
            children[i] = selectedGamePiece.transform.GetChild(i);
        }

        // Check first child is snappable
        Vector2 pieceChildPos = children[0].position;
        Values pieceChildValue = children[0].gameObject.GetComponent<Value>().PieceValue;
        foreach(KeyValuePair<Vector2, Values> gridPiece in gridVals)
        {
            Vector2 gridPiecePos = gridPiece.Key;
            Values gridPieceValue = gridPiece.Value;
            float distance = Vector2.Distance(pieceChildPos, gridPiecePos);
            
            if(distance < snapDistance)
            {
                possibleDifference = gridPiecePos - pieceChildPos;
                possibleSnap = true; 
                break;
            }
        }

        if(!possibleSnap)
        {
            return;
        } 

        // Iterate over the children and check it is fit
        for (int i = 0; i < numChildren; i++)
        {
            //Vector2 targetTransform = (Vector2)children[i].position + possibleDifference;
            Vector2 targetTransform = new Vector2
            (
                Mathf.Round((children[i].position.x + possibleDifference.x) * 10f) / 10f,
                Mathf.Round((children[i].position.y + possibleDifference.y) * 10f) / 10f
            );

            if(!(gridVals.ContainsKey(targetTransform)))
            {
                return;
            }

            if(((int)gridVals[targetTransform] + (int)children[i].GetComponent<Value>().PieceValue != (int)Values.FILLED) &&
                gridVals[targetTransform] != Values.EMPTY)
            {
                return;
            } 
        }

        // Assign Values to grid
        for (int i = 0; i < numChildren; i++)
        {
            Vector2 targetTransform = new Vector2
            (
                Mathf.Round((children[i].position.x + possibleDifference.x) * 10f) / 10f,
                Mathf.Round((children[i].position.y + possibleDifference.y) * 10f) / 10f
            );

            gridVals[targetTransform] += (int)children[i].gameObject.GetComponent<Value>().PieceValue;
        }

        // Snap operation
        selectedGamePiece.transform.position += (Vector3)possibleDifference;
        
        if(IsGameFinished()) nextButton.SetActive(true);
    }

    public void SelectGamePiece(GamePiece gamePiece)
    {
        selectedGamePiece = gamePiece;
    }

    public void DeletePiece()
    {
        foreach (Transform pieceChild in selectedGamePiece.transform)
        {
            Vector2 targetTransform = new Vector2
            (
                Mathf.Round((pieceChild.position.x) * 10f) / 10f,
                Mathf.Round((pieceChild.position.y) * 10f) / 10f
            );

            if (gridVals.ContainsKey(targetTransform))
            {
                gridVals[targetTransform] = Values.EMPTY;
            }
        }
    }

    public void ResetGrid()
    {
        // Destroy pieces
        DestroyCurrentPieces();

        // Reset grid values
        ResetGridValues();

        CreatePieces();
    }

    public void ResetGridValues()
    {
        foreach (Vector2 key in gridVals.Keys.ToList())
        {
            gridVals[key] = Values.EMPTY;
        }
    }

    public void DestroyCurrentPieces()
    {
        GamePiece[] objectsWithTag = FindObjectsOfType<GamePiece>();
        foreach (GamePiece obj in objectsWithTag)
        {
            Destroy(obj.gameObject);
        }
    }
}

