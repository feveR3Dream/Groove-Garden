using UnityEngine;

public class Global
{
    // If the record is child of shelf
    public static float RecordShelfSize = 0.3f; 
    public static float RecordNormalSize = 0.9f;

    // If scrolling is needed
    public static void ScrollMovement(Transform anything, float scrollSpeed)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float movementAmount = scroll * scrollSpeed * Time.deltaTime;

        anything.Translate(Vector3.up * movementAmount);
    }
}


#region For UI in General
public enum Screen
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

public enum Direction
{
    Left,
    Right,
    None
}
#endregion