using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private GameObject agentPrefab;
    private GameObject currentAgent;

    private Dictionary<Vector2,GameObject> squares;

    private GameObject currentTarget;

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

        Destroy(currentTarget);
        currentAgent.GetComponent<Agent>().ClearPathLine();
        Destroy(currentAgent);
    }

    public void BeginSimulation()
    {
        if(currentAgent != null)
        {
            currentAgent.GetComponent<Agent>().Pathfind(GetGridPositionFromPosition(currentTarget.transform.position));
        }
    }


    public List<Vector2> GetNeighbors(Vector2 position,HashSet<Vector2> nonVisitables)
    {
        List<Vector2> neighbors = new List<Vector2>();
        Vector2 gridPosition = GetGridPositionFromPosition(position);
        for(int i = -1;i <= 1;i++)
        {
            for(int j = -1;j <= 1;j++)
            {
                Vector2 newPosition = gridPosition + new Vector2(i, j);
                if (!(i != 0 && j != 0) && !squares.ContainsKey(newPosition) && !nonVisitables.Contains(newPosition)) neighbors.Add(newPosition); 
            }
        }
        return neighbors;
    }
    public Vector2 GetGridPositionFromPosition(Vector2 position)
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
        if (squares.ContainsKey(gridPosition)) Destroy(squares[gridPosition]);
        Destroy(currentTarget);
        currentTarget = Instantiate(targetPrefab, transform);
        currentTarget.transform.position = gridPosition;
    }
    public void PlaceAgent(Vector2 position)
    {
        Vector2 gridPosition = GetGridPositionFromPosition(position);
        if (currentAgent != null) Destroy(currentAgent);
        currentAgent = Instantiate(agentPrefab, transform);
        currentAgent.transform.position = gridPosition;
    }
}
