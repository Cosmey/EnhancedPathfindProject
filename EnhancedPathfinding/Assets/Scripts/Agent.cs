
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Agent : MonoBehaviour
{
    [SerializeField] private int maxSearch;
    [SerializeField] private float reachDistance;
    [SerializeField] private float speed;

    [SerializeField] private GameObject pathLinePrefab;
    private GameObject currentPathLine;

    private List<Vector2> currentPath;
    private int currentPathPoint;
    private bool atTarget;

    private GridController grid;

    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<GridController>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        if(currentPath != null && !atTarget)
        {

            rb.linearVelocity += (currentPath[currentPathPoint] - (Vector2)transform.position).normalized * speed * Time.deltaTime;
            if (Vector2.Distance(currentPath[currentPathPoint], (Vector2)transform.position) < reachDistance)
            {
                currentPathPoint++;
            }
            if (currentPathPoint >= currentPath.Count) currentPathPoint--; //atTarget = true;
        }
    }

    public void ClearPathLine()
    {
        if (currentPathLine != null) Destroy(currentPathLine);
    }

    public void Pathfind(Vector2 targetPosition)
    {
        atTarget = false;
        Vector2 startingPoint = grid.GetGridPositionFromPosition(transform.position);
        int numSearches = 0;
        PathfindingPoint currentPoint = null;
        PriorityQueue<PathfindingPoint> queue = new PriorityQueue<PathfindingPoint>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        HashSet<Vector2> nonVisitables = new HashSet<Vector2>();
        bool reachedTarget = false;

        queue.Enqueue(new PathfindingPoint(startingPoint, 0),0);

        while(!queue.IsEmpty() && !reachedTarget)
        {
            currentPoint = queue.Dequeue();
            numSearches++;
            if (numSearches > maxSearch) break;
            if(currentPoint.point == targetPosition)
            {
                reachedTarget = true;
                break;
            }
            List<Vector2> neighbors = grid.GetNeighbors(currentPoint.point, nonVisitables);
            foreach(Vector2 neighbor in neighbors)
            {
                cameFrom[neighbor] = currentPoint.point;
                float distanceTravelled = currentPoint.distanceTravelled + Vector2.Distance(currentPoint.point, neighbor);
                queue.Enqueue(new PathfindingPoint(neighbor, distanceTravelled), (int)CalculateHeuristic(targetPosition, neighbor, distanceTravelled));
                nonVisitables.Add(neighbor);
            }
        }

        if(reachedTarget)
        {
            currentPath = new List<Vector2>();
            Vector2 pathPoint = currentPoint.point;
            while(startingPoint != pathPoint)
            {
                currentPath.Add(pathPoint);
                pathPoint = cameFrom[pathPoint];
            }
            currentPath.Reverse();
            currentPathPoint = 0;

            if (currentPathLine != null) Destroy(currentPathLine);
            currentPathLine = Instantiate(pathLinePrefab);
            pathLinePrefab.transform.position = Vector2.zero;
            LineRenderer lRenderer = currentPathLine.GetComponent<LineRenderer>();
            lRenderer.positionCount = currentPath.Count;
            lRenderer.SetPosition(0, startingPoint);
            for (int i = 1; i < currentPath.Count; i++)
            {
                lRenderer.SetPosition(i, currentPath[i]);
            }
        }


        

    }
    private float CalculateHeuristic(Vector2 targetPosition, Vector2 position, float distanceTravelled)
    {
        return (Vector2.Distance(position, targetPosition) * 1.001f) + distanceTravelled;
    }
}



public class PathfindingPoint
{
    public PathfindingPoint(Vector2 point, float distanceTravelled)
    {
        this.point = point;
        this.distanceTravelled = distanceTravelled;
    }
    public Vector2 point;
    public float distanceTravelled;
}