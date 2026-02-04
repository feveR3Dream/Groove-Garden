using System.Collections.Generic;
using UnityEngine;

public class Global
{
    // If the record is child of "Shelf"
    public static float RecordCoverShelfSize { get; private set; } = 0.3f;


    // If the record if child of "Equip Record Transform"
    public static float RecordCoverHandlingSize { get; private set; } = 0.45f;


    // If the record if child of "Equip Record Transform"
    public static float RecordDiskSpawnedSize { get; private set; } = 0.88f;


    // For spawning in record
    public static float RecordDiskDraggingSize { get; private set; } = 0.54f; // IF NOT PARENTED: 0.27f; IF PARENTED: 0.54f


    // For vinyl cover
    public static int OnShelfSortingOrder { get; private set; } = 0;
    public static int EquippedSortingOrder { get; private set; } = 5;


    // For vinyl record
    public static int SpawnedInSortingOrder { get; private set; } = 4;
    public static int PlaySortingOrder { get; private set; } = 2;
    public static Dictionary<Sorter, List<GameObject>> SorterDictionary = new Dictionary<Sorter, List<GameObject>>();   


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

#region For Cursor Interaction Mode
public enum CursorInteraction
{
    Record_Interaction,
    Turntable_Interaction
} 
#endregion