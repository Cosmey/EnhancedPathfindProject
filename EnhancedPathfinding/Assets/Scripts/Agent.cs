
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Agent : MonoBehaviour
{
    [SerializeField] private int maxSearch;
    [SerializeField] private float reachDistance;
    [SerializeField] private float speed;
    [SerializeField] private float distanceFromTargetBias;

    [SerializeField] private GameObject pathLinePrefab;
    List<GameObject> pathLines;


    List<GameObject> considerationBoxes;
    [SerializeField] private GameObject considerationBox;

    [SerializeField] private float pathfindVisualizationDelay;
    [SerializeField] private Color frontierColor;
    [SerializeField] private Color doneColor;

    [SerializeField] private GameObject optimizerVisualLinePrefab;
    [SerializeField] private GameObject optimizerCheckPositionVisualizerPrefab;
    [SerializeField] private float optimizerVisualizationDelay;
    bool pathOptimized;

    private List<Vector2> currentPath;
    private int currentPathPoint;
    private bool atTarget;

    private GridController grid;

    private Rigidbody2D rb;

    [SerializeField] private LayerMask wallsLayer;

    private bool running;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        considerationBoxes = new List<GameObject>();
        pathLines = new List<GameObject>();
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
        if(currentPath != null && !atTarget && pathOptimized)
        {

            rb.linearVelocity += (currentPath[currentPathPoint] - (Vector2)transform.position).normalized * speed * Time.deltaTime;
            if (Vector2.Distance(currentPath[currentPathPoint], (Vector2)transform.position) < reachDistance)
            {
                currentPathPoint++;
            }
            if (currentPathPoint >= currentPath.Count)
            {
                currentPathPoint--;
                if (Vector2.Distance(transform.position, currentPath[currentPathPoint]) < 0.1f)
                {
                    transform.position = currentPath[currentPathPoint];
                    rb.linearVelocity = Vector2.zero;
                    atTarget = true;
                }
            }
            
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    public void SetPathfindBias(float bias)
    {
        distanceFromTargetBias = bias;
    }

    public void ClearPathLines()
    {
        for(int i = 0;i < pathLines.Count;i++)
        {
            Destroy(pathLines[i]);
        }
    }
    public void ClearConsiderationBoxes()
    {
        for (int i = 0; i < considerationBoxes.Count; i++)
        {
            Destroy(considerationBoxes[i]);
        }
        considerationBoxes = new List<GameObject>();
    }

    public void GridUpdated()
    {
        currentPathPoint = 0;
        currentPath = null;
        StopAllCoroutines();
        if(running) StartCoroutine(Pathfind());
    }
    public void Activate()
    {
        running = true;
    }
    public void Deactivate()
    {
        running = false;
    }
    private GameObject DrawPath(List<Vector2> path,Color lineColor)
    {
        GameObject line = Instantiate(pathLinePrefab);
        pathLinePrefab.transform.position = Vector2.zero;
        LineRenderer lRenderer = line.GetComponent<LineRenderer>();
        lRenderer.startColor = lineColor;
        lRenderer.endColor = lineColor;
        lRenderer.positionCount = path.Count;
        lRenderer.SetPosition(0, path[0]);
        for (int i = 1; i < path.Count; i++)
        {
            lRenderer.SetPosition(i, path[i]);
        }
        pathLines.Add(line);
        return line;
    }
    public IEnumerator<WaitForSeconds> Pathfind()
    {
        pathOptimized = false;
        ClearPathLines();
        ClearConsiderationBoxes();
        atTarget = false;
        Vector2 startingPoint = grid.GetGridPositionFromPosition(transform.position);
        Vector2 targetPosition = grid.GetTargetPosition();
        int numSearches = 0;
        PathfindingPoint currentPoint = null;
        PriorityQueue<PathfindingPoint> queue = new PriorityQueue<PathfindingPoint>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        HashSet<Vector2> nonVisitables = new HashSet<Vector2>();
        bool reachedTarget = false;

        considerationBoxes.Add(Instantiate(considerationBox));
        considerationBoxes[considerationBoxes.Count - 1].GetComponent<SpriteRenderer>().color = frontierColor;
        considerationBoxes[considerationBoxes.Count - 1].transform.position = startingPoint;
        queue.Enqueue(new PathfindingPoint(startingPoint, 0, considerationBoxes[considerationBoxes.Count - 1]),0);

        while(!queue.IsEmpty() && !reachedTarget)
        {
            currentPoint = queue.Dequeue();
            currentPoint.visualBox.GetComponent<SpriteRenderer>().color = doneColor;
            numSearches++;
            if (numSearches > maxSearch) break;
            if(currentPoint.point == targetPosition)
            {
                reachedTarget = true;
                break;
            }

            List<Vector2> neighbors = grid.GetNeighbors(currentPoint.point, nonVisitables);
            foreach (Vector2 neighbor in neighbors)
            {
                cameFrom[neighbor] = currentPoint.point;
                float distanceTravelled = currentPoint.distanceTravelled + Vector2.Distance(currentPoint.point, neighbor);
                considerationBoxes.Add(Instantiate(considerationBox));
                considerationBoxes[considerationBoxes.Count - 1].GetComponent<SpriteRenderer>().color = frontierColor;
                considerationBoxes[considerationBoxes.Count-1].transform.position = neighbor;
                queue.Enqueue(new PathfindingPoint(neighbor, distanceTravelled, considerationBoxes[considerationBoxes.Count - 1]), (int)CalculateHeuristic(targetPosition, neighbor, distanceTravelled));
                nonVisitables.Add(neighbor);
            }
            yield return new WaitForSeconds(pathfindVisualizationDelay);
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
            currentPath.Add(startingPoint);
            currentPath.Reverse();
            currentPathPoint = 0;
            DrawPath(currentPath,Color.blue);
            StartCoroutine(OptimizePath());

            
        }
        else
        {
            currentPath = null;
            ClearPathLines();
        }



        
    }
    private IEnumerator<WaitForSeconds> OptimizePath()
    {
        if(currentPath.Count > 1)
        {
            GameObject prevPathVisual = null;

            GameObject startCircle = Instantiate(optimizerCheckPositionVisualizerPrefab);
            GameObject endCircle = Instantiate(optimizerCheckPositionVisualizerPrefab);
            GameObject visualLine = Instantiate(optimizerVisualLinePrefab);
            pathLines.Add(startCircle);
            pathLines.Add(endCircle);
            visualLine.transform.position = Vector2.zero;
            visualLine.GetComponent<LineRenderer>().positionCount = 2;
            pathLines.Add(visualLine);


            int nextPositionIndex = 1;
            List<Vector2> newPath = new List<Vector2>();
            Vector2 currentPosition = currentPath[0];
            newPath.Add(currentPosition);
            Vector2 moveDirection = Vector2.zero;
            while (true)
            {
                for (int i = currentPath.Count - 1; i > nextPositionIndex; i--)
                {
                    if (CheckPointReachable(currentPosition, currentPath[i]))
                    {
                        nextPositionIndex = i;
                        newPath.Add(currentPosition);
                        break;
                    }
                    startCircle.transform.position = currentPosition;
                    endCircle.transform.position = currentPath[i];
                    visualLine.GetComponent<LineRenderer>().SetPosition(0, currentPosition);
                    visualLine.GetComponent<LineRenderer>().SetPosition(1, currentPath[i]);
                    yield return new WaitForSeconds(optimizerVisualizationDelay);
                }
                Destroy(prevPathVisual);
                prevPathVisual = DrawPath(newPath, Color.green);

                //check here in case we went straight to the end
                if (nextPositionIndex >= currentPath.Count-1)
                {
                    newPath.Add(currentPath[nextPositionIndex]);
                    break;
                }


                moveDirection = (currentPath[nextPositionIndex] - currentPosition).normalized;
                currentPosition += moveDirection;

                //if the direction changes we've moved past the point
                if ((currentPath[nextPositionIndex] - currentPosition).normalized != moveDirection)
                {
                    currentPosition = currentPath[nextPositionIndex];
                    newPath.Add(currentPosition);
                    nextPositionIndex++;
                }

                //check here if we went to the end after incrementing
                if (nextPositionIndex >= currentPath.Count-1)
                {
                    newPath.Add(currentPath[nextPositionIndex]);
                    break;
                }
            }
            DrawPath(newPath, Color.green);
            pathOptimized = true;
            pathLines.Remove(visualLine);
            Destroy(visualLine);
            Destroy(startCircle);
            Destroy(endCircle);

            currentPath = newPath;
        }
    }
    private float CalculateHeuristic(Vector2 targetPosition, Vector2 position, float distanceTravelled)
    {
        Vector2 distance = targetPosition - position;
        return (distance.magnitude + (distanceTravelled * distanceFromTargetBias)) * 100;
    }
    private bool CheckPointReachable(Vector2 start, Vector2 end)
    {
        float halfSizeY = transform.localScale.y / 2;
        Vector2 topRightVector = new Vector2(1, 1).normalized * halfSizeY;
        Vector2 topLeftVector = new Vector2(-1, 1).normalized * halfSizeY;
        Vector2 bottomRightVector = new Vector2(1, -1).normalized * halfSizeY;
        Vector2 bottomLeftVector = new Vector2(-1, -1).normalized * halfSizeY;
        Vector2 originTopRight = start + topRightVector;
        Vector2 originTopLeft = start + topLeftVector;
        Vector2 originBottomRight = start + bottomRightVector;
        Vector2 originBottomLeft = start + bottomLeftVector;
        Vector2 pathVector = end - start;
        return !((Physics2D.Raycast(originTopLeft, pathVector.normalized, pathVector.magnitude, wallsLayer)) || (Physics2D.Raycast(originTopRight, pathVector.normalized, pathVector.magnitude, wallsLayer)) || (Physics2D.Raycast(originBottomLeft, pathVector.normalized, pathVector.magnitude, wallsLayer)) || (Physics2D.Raycast(originBottomRight, pathVector.normalized, pathVector.magnitude, wallsLayer)));
    }
}



public class PathfindingPoint
{
    public PathfindingPoint(Vector2 point, float distanceTravelled,GameObject visualBox)
    {
        this.point = point;
        this.distanceTravelled = distanceTravelled;
        this.visualBox = visualBox;
    }
    public Vector2 point;
    public float distanceTravelled;
    public GameObject visualBox;
}