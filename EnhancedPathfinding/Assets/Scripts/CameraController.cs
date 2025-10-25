using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float maxZoom;

    private Vector2 moveInput;
    private bool leftMouseClicked;
    private bool rightMouseClicked;

    private GridController grid;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<GridController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        TryPlaceSquare();
    }

    private void Move()
    {
        transform.position += (Vector3)(moveInput.normalized * Time.deltaTime * moveSpeed * GetComponent<Camera>().orthographicSize);
    }

    private void TryPlaceSquare()
    {
        if (leftMouseClicked)
        {
            grid.ChangeSquare(true, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        }

        if (rightMouseClicked)
        {
            grid.ChangeSquare(false, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        }
    }



    public void UpdateMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void UpdateZoom(InputAction.CallbackContext context)
    {
        GetComponent<Camera>().orthographicSize += context.ReadValue<float>() * zoomSpeed;
        if (GetComponent<Camera>().orthographicSize < maxZoom) GetComponent<Camera>().orthographicSize = maxZoom;
    }

    public void AddSquare(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            leftMouseClicked = true;
        }
        else if(context.canceled)
        {
            leftMouseClicked = false;
        }
    }

    public void RemoveSquare(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            rightMouseClicked = true;
        }
        else if(context.canceled)
        {
            rightMouseClicked = false;
        }
    }

    public void SetTarget(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            grid.SetGridTarget(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        }
    }

    public void PlaceAgent(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            grid.PlaceAgent(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        }
    }
}
