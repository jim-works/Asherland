using Godot;

public partial class AerialCamera : Camera3D
{
    // Camera movement settings
    [Export] public float PanSpeed = 20.0f;
    [Export] public float ZoomSpeed = 1.5f;
    [Export] public float RotationSpeed = 2.0f;

    // Zoom constraints
    [Export] public float MinZoom = 5.0f;
    [Export] public float MaxZoom = 50.0f;
    [Export] public float MaxZoomTiltDelta = 10f;

    // Edge scrolling settings
    [Export] public bool EnableEdgeScrolling = true;
    [Export] public float EdgeScrollThreshold = 20.0f;

    // Internal state
    private Vector3 targetPosition;
    private float targetZoom;
    private float targetRotation;
    private float targetTilt => CalculateTilt(targetZoom) + baseTilt;
    private float baseTilt;
    private float currentZoom;

    // Camera smoothing
    [Export] public float MovementSmoothness = 5.0f;
    [Export] public float ZoomSmoothness = 5.0f;
    [Export] public float RotationSmoothness = 5.0f;

    // Input tracking
    private Vector2 _dragStartPosition;
    private bool _isDragging = false;
    private bool _isRotating = false;

    public override void _Ready()
    {
        targetPosition = GlobalPosition;
        currentZoom = GlobalPosition.Y;
        targetZoom = currentZoom;
        targetRotation = Rotation.Y;
        baseTilt = Rotation.X;
    }

    public override void _Process(double delta)
    {
        float deltaFloat = (float)delta;

        // Apply smoothing to camera movements
        GlobalPosition = GlobalPosition.Lerp(
            new Vector3(targetPosition.X, targetZoom, targetPosition.Z),
            MovementSmoothness * deltaFloat);

        Rotation = new Vector3(
            Mathf.LerpAngle(Rotation.X, targetTilt, RotationSmoothness * deltaFloat),
            Mathf.LerpAngle(Rotation.Y, targetRotation, RotationSmoothness * deltaFloat),
            Rotation.Z);

        // Edge scrolling when enabled
        if (EnableEdgeScrolling)
        {
            HandleEdgeScrolling(deltaFloat);
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Handle camera zoom with mouse wheel
        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseButtonInput(mouseButton);
        }
        // Handle camera panning and rotation with mouse movement
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleMouseMotionInput(mouseMotion);
        }
        // Handle keyboard input for camera movement
        else if (@event is InputEventKey keyEvent)
        {
            HandleKeyboardInput(keyEvent);
        }
    }

    private void HandleMouseButtonInput(InputEventMouseButton mouseButton)
    {
        // Handle mouse wheel for zooming
        if (mouseButton.ButtonIndex == MouseButton.WheelUp || mouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            float zoomChange = (mouseButton.ButtonIndex == MouseButton.WheelUp) ? -ZoomSpeed : ZoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom + zoomChange, MinZoom, MaxZoom);
            return;
        }

        // Middle mouse button for rotating the camera
        if (mouseButton.ButtonIndex == MouseButton.Middle)
        {
            _isRotating = mouseButton.Pressed;
            _dragStartPosition = mouseButton.Position;
            return;
        }

        // Right mouse button for panning the camera
        if (mouseButton.ButtonIndex == MouseButton.Right)
        {
            _isDragging = mouseButton.Pressed;
            _dragStartPosition = mouseButton.Position;
            return;
        }
    }

    private void HandleMouseMotionInput(InputEventMouseMotion mouseMotion)
    {
        // Handle camera rotation with middle mouse button
        if (_isRotating)
        {
            targetRotation += mouseMotion.Relative.X * RotationSpeed * 0.01f;
            return;
        }

        // Handle camera panning with right mouse button
        if (_isDragging)
        {
            // Convert screen movement to world movement based on camera rotation
            Vector2 drag = -mouseMotion.Relative * PanSpeed * 0.01f * (currentZoom / MaxZoom);
            Vector3 forward = Transform.Basis.Z;
            Vector3 right = Transform.Basis.X;

            // Remove Y component to keep movement on the horizontal plane
            forward.Y = 0;
            right.Y = 0;
            forward = forward.Normalized();
            right = right.Normalized();

            targetPosition += right * drag.X + forward * drag.Y;
        }
    }

    private void HandleKeyboardInput(InputEventKey keyEvent)
    {
        if (!keyEvent.Pressed)
            return;

        float moveSpeed = PanSpeed * (currentZoom / MaxZoom);

        // Get camera forward and right vectors for movement relative to camera orientation
        Vector3 forward = -Transform.Basis.Z;
        Vector3 right = Transform.Basis.X;

        // Remove Y component to keep movement on the horizontal plane
        forward.Y = 0;
        right.Y = 0;

        forward = forward.Normalized();
        right = right.Normalized();

        // WASD movement
        if (keyEvent.Keycode == Key.W)
            targetPosition += forward * moveSpeed;
        else if (keyEvent.Keycode == Key.S)
            targetPosition -= forward * moveSpeed;
        else if (keyEvent.Keycode == Key.A)
            targetPosition -= right * moveSpeed;
        else if (keyEvent.Keycode == Key.D)
            targetPosition += right * moveSpeed;

        // QE rotation
        else if (keyEvent.Keycode == Key.Q)
            targetRotation -= 0.1f;
        else if (keyEvent.Keycode == Key.E)
            targetRotation += 0.1f;

        // Arrow keys as alternative movement
        else if (keyEvent.Keycode == Key.Up)
            targetPosition += forward * moveSpeed;
        else if (keyEvent.Keycode == Key.Down)
            targetPosition -= forward * moveSpeed;
        else if (keyEvent.Keycode == Key.Left)
            targetPosition -= right * moveSpeed;
        else if (keyEvent.Keycode == Key.Right)
            targetPosition += right * moveSpeed;
    }

    private float CalculateTilt(float zoom) => Mathf.Lerp(0, Mathf.DegToRad(MaxZoomTiltDelta), Mathf.InverseLerp(MinZoom, MaxZoom, zoom));

    private void HandleEdgeScrolling(float delta)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

        // Calculate movement vector based on mouse position near screen edges
        Vector2 movement = Vector2.Zero;

        if (mousePos.X < EdgeScrollThreshold)
            movement.X = -1;
        else if (mousePos.X > viewportSize.X - EdgeScrollThreshold)
            movement.X = 1;

        if (mousePos.Y < EdgeScrollThreshold)
            movement.Y = -1;
        else if (mousePos.Y > viewportSize.Y - EdgeScrollThreshold)
            movement.Y = 1;

        if (movement != Vector2.Zero)
        {
            float edgeMoveSpeed = PanSpeed * delta * (currentZoom / MaxZoom);

            // Get camera forward and right vectors
            Vector3 forward = Transform.Basis.Z;
            Vector3 right = Transform.Basis.X;

            // Remove Y component to keep movement on the horizontal plane
            forward.Y = 0;
            right.Y = 0;

            forward = forward.Normalized();
            right = right.Normalized();

            // Move camera based on screen edge position
            targetPosition += right * movement.X * edgeMoveSpeed + forward * movement.Y * edgeMoveSpeed;
        }
    }

    // Helper method to restrict camera to a specific world boundary
    public void ClampToBoundary(Vector2 minBounds, Vector2 maxBounds)
    {
        targetPosition = new Vector3(
            Mathf.Clamp(targetPosition.X, minBounds.X, maxBounds.X),
            targetPosition.Y,
            Mathf.Clamp(targetPosition.Z, minBounds.Y, maxBounds.Y)
        );
    }

    // Move camera to focus on a specific hex coordinate
    public void FocusOnHex(HexCoord hexCoord)
    {
        Vector2 worldPos = hexCoord.ToWorld();
        targetPosition = new Vector3(worldPos.X, targetPosition.Y, worldPos.Y);
    }
}
