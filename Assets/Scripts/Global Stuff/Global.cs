using UnityEngine;

public class Global
{
    // If the record is child of "Shelf"
    public static float RecordShelfSize = 0.3f;
    //public static float RecordHandlingSize = 0.9f;

    // If the record if child of "Equip Record Transform"
    public static float RecordHandlingSize = 0.45f;

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
    Hide,
    Show
}

public enum Direction // CHECK FOR "CURRENT HOVER DIRECTION"
{
    Left,
    Right,
    None
}
#endregion


#region Event Dispatcher
public struct EquipStatus { public bool equipped; }
public struct ToggleRecord { public bool hide; }
public struct RemoveRecord { };
#endregion