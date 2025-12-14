using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SwipeDetector : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;

    public float minSwipeDistance = 20f;

    public event Action<SwipeData> OnSwipe;

    private InputAction touchPress;
    private InputAction touchPosition;
    private InputAction mousePress;
    private InputAction mousePosition;

    private Vector2 startPos;
    private float startTime;
    private bool isPressed;

    private void Awake()
    {
        var map = inputActions.FindActionMap("TouchControls", true);

        touchPress = map.FindAction("PrimaryContact", true);
        touchPosition = map.FindAction("PrimaryPosition", true);

        mousePress = map.FindAction("MouseContact", true);
        mousePosition = map.FindAction("MousePosition", true);
    }

    private void OnEnable()
    {
        inputActions.Enable();

        touchPress.started += OnPressStartedTouch;
        touchPress.canceled += OnPressEndedTouch;

        mousePress.started += OnPressStartedMouse;
        mousePress.canceled += OnPressEndedMouse;
    }

    private void OnDisable()
    {
        inputActions.Disable();

        touchPress.started -= OnPressStartedTouch;
        touchPress.canceled -= OnPressEndedTouch;

        mousePress.started -= OnPressStartedMouse;
        mousePress.canceled -= OnPressEndedMouse;
    }

    private void OnPressStartedTouch(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Touch Press Started");

        isPressed = true;
        startTime = Time.time;
        startPos = touchPosition.ReadValue<Vector2>();
    }

    private void OnPressEndedTouch(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Touch Press Ended");

        HandleSwipe(touchPosition.ReadValue<Vector2>());
    }

    private void OnPressStartedMouse(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Mouse Press Started");

        isPressed = true;
        startTime = Time.time;
        startPos = mousePosition.ReadValue<Vector2>();
    }

    private void OnPressEndedMouse(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Mouse Press Ended");

        HandleSwipe(mousePosition.ReadValue<Vector2>());
    }

    private void HandleSwipe(Vector2 endPos)
    {
        if (!isPressed) return;
        isPressed = false;

        float duration = Time.time - startTime;

        Vector2 delta = endPos - startPos;
        float distance = delta.magnitude;

        if (distance < minSwipeDistance)
            return;

        SwipeData swipe = new SwipeData
        {
            StartPos = startPos,
            EndPos = endPos,
            Direction = delta.normalized,
            Distance = distance,
            Duration = duration,
            Speed = distance / duration,
            Angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
        };

        if (swipe.Angle < 0)
            swipe.Angle += 360f;

        Debug.Log("Swipe detected!");
        OnSwipe?.Invoke(swipe);
    }
}
