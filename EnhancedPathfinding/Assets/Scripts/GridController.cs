using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject targetPrefab;

    private Dictionary<Vector2,GameObject> squares;

    private Vector2 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        squares = new Dictionary<Vector2,GameObject>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetGrid()
    {
        foreach (var value in squares.Values)
        {
            Destroy(value);
        }
        squares = new Dictionary<Vector2, GameObject>();
    }
    private Vector2 GetGridPositionFromPosition(Vector2 position)
    {
        return new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
    }

    public void ChangeSquare(bool squareEnabled, Vector2 position)
    {
        Vector2 gridPosition = GetGridPositionFromPosition(position);
        if(squares.ContainsKey(gridPosition) && !squareEnabled)
        {
            Destroy(squares[gridPosition]);
            squares.Remove(gridPosition);
        }
        if(!squares.ContainsKey(gridPosition) && squareEnabled)
        {
            GameObject newSquare = Instantiate(squarePrefab, transform);
            newSquare.transform.position = gridPosition;
            squares[gridPosition] = newSquare;
        }
    }

    public void SetGridTarget(Vector2 position)
    {
        Vector2 gridPosition = GetGridPositionFromPosition(position);
        if (squares.ContainsKey(targetPosition)) Destroy(squares[targetPosition]);
        targetPosition = gridPosition;
        if(squares.ContainsKey(gridPosition)) Destroy(squares[gridPosition]);
        GameObject newSquare = Instantiate(targetPrefab, transform);
        newSquare.transform.position = gridPosition;
        squares[gridPosition] = newSquare;
    }
}
