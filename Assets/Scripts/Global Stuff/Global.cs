using UnityEngine;

public class Global
{
    // If the record is child of "Shelf"
    public static float RecordShelfSize { get; private set; } = 0.3f;

    // If the record if child of "Equip Record Transform"
    public static float RecordHandlingSize { get; private set; } = 0.45f;

    // For spawning in record
    public static float RecordDraggingSize { get; private set; } = 0.27f;

    // If scrolling is needed
    public static void ScrollMovement(Transform anything, float scrollSpeed)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float movementAmount = scroll * scrollSpeed * Time.deltaTime;

        anything.Translate(Vector3.up * movementAmount);
    }
}

#region For UI in General
public enum Screen // CHECK FOR "CURRENT SCREEN"
{
    Turntable,
    Selection
}
#endregion


#region For UI Buttons
public enum Opacity
{
    Transparent,
    Visible
}

public enum Direction // CHECK FOR "CURRENT HOVER DIRECTION"
{
    Left,
    Right,
    None
}
#endregion


#region For Record UI
public enum RecordToggle      // UNCOMMENT IF NEEDED TO USE
{
    Show,
    Hide
}

public enum RecordExamine
{
    None,
    Default,
    Info,
    Unsheathe,
    Return
}

public enum RecordSide
{
    Front,
    Back
}

public struct RecordToDrag
{
    public GameObject recordGO;
}
#endregion