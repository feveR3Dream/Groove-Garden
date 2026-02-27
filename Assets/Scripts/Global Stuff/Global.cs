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
    public static float RecordDiskHandlingSize { get; private set; } = 0.54f; // IF NOT PARENTED: 0.27f; IF PARENTED: 0.54f


    // For vinyl cover
    public static int OnShelfSortingOrder { get; private set; } = 0;
    public static int EquippedSortingOrder { get; private set; } = 5;


    // For vinyl record
    public static int SpawnedInSortingOrder { get; private set; } = 4;
    public static int HandlingSortingOrder { get; private set; } = 2;
    public static Dictionary<Sorter, List<GameObject>> SorterDictionary = new Dictionary<Sorter, List<GameObject>>();


    // For playing records
    public static float MaxInterpolationTime { get; private set; } = 1f; // Make changes to this when needed.
    
    public static float SpinninRecordSpeed { get; private set; } = 20f; 
    
    public static float SlowedSpinSpeed { get; private set; } = 10f;
    public static float NormalSpinSpeed { get; private set; } = 25f;
    public static float SpedUpSpinSpeed { get; private set; } = 50f;

}


#region For General UI
public enum Screen // CHECK FOR "CURRENT SCREEN"
{
    Turntable,
    Selection
}
#endregion


#region For Button UI
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

public enum RecordMoveTo
{
    To_Mouse,
    To_Turntable,
    To_Spawned_Pos
}
#endregion


#region For Turntable Settings
public enum Power
{
    On,
    Off
}
public enum RPM
{
    Slowed, 
    Normal, 
    SpedUp
}
#endregion


#region For Cursor Interaction Mode
public enum MouseButton
{
    Down,
    Hold,
    Up,
} 
#endregion