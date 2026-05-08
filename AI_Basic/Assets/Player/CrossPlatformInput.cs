using UnityEngine;

public static class CrossPlatformInput
{
    private static Vector2 _virtualMove;
    private static Vector2 _virtualLook;

    public static void SetVirtualMove(Vector2 input)
    {
        _virtualMove = Vector2.ClampMagnitude(input, 1f);
    }

    public static void SetVirtualLook(Vector2 delta)
    {
        _virtualLook += delta;
    }

    public static void ClearVirtualMove()
    {
        _virtualMove = Vector2.zero;
    }

    public static void ClearVirtualLook()
    {
        _virtualLook = Vector2.zero;
    }

    public static void EndFrame()
    {
        _virtualLook = Vector2.zero;
    }

    public static Vector2 GetMoveInput()
    {
        Vector2 keyboardInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        if (_virtualMove.sqrMagnitude > 0.0001f)
        {
            return Vector2.ClampMagnitude(_virtualMove + keyboardInput, 1f);
        }

        return Vector2.ClampMagnitude(keyboardInput, 1f);
    }

    public static float GetAxis(string axisName)
    {
        if (axisName == "Horizontal")
        {
            return GetMoveInput().x;
        }

        if (axisName == "Vertical")
        {
            return GetMoveInput().y;
        }

        if (axisName == "Mouse X")
        {
            return Input.GetAxis("Mouse X") + _virtualLook.x;
        }

        if (axisName == "Mouse Y")
        {
            return Input.GetAxis("Mouse Y") + _virtualLook.y;
        }

        return Input.GetAxis(axisName);
    }
}
