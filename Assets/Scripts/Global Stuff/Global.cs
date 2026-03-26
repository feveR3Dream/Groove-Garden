using System.Collections.Generic;
using UnityEngine;

public class Global
{
    public class SizeValue
    {
        // If the record is child of "Shelf"
        public static float RecordCoverShelfSize { get; private set; } = 0.3f;


        // If the record if child of "Equip Record Transform"
        public static float RecordCoverHandlingSize { get; private set; } = 0.45f;


        // If the record if child of "Equip Record Transform"
        public static float RecordDiskSpawnedSize { get; private set; } = 0.88f;


        // For spawning in record
        public static float RecordDiskHandlingSize { get; private set; } = 0.54f; // IF NOT PARENTED: 0.27f; IF PARENTED: 0.54f
    } 


    public class SortingValue
    {
        // For vinyl cover
        public static int OnShelfSortingOrder { get; private set; } = 0;
        public static int EquippedSortingOrder { get; private set; } = 5;


        // For vinyl record
        public static int SpawnedInSortingOrder { get; private set; } = 4;
        public static int HandlingSortingOrder { get; private set; } = 2;
        public static Dictionary<Sorter, List<GameObject>> SorterDictionary = new Dictionary<Sorter, List<GameObject>>();
    }


    public class RecordPlayingValue
    {
        // For playing records
        public static float MaxInterpolationTime { get; private set; } = 1f; // Make changes to this when needed.

        public static float SpinninRecordSpeed { get; private set; } = 20f;

        public static float SlowedSpinSpeed { get; private set; } = 10f;
        public static float NormalSpinSpeed { get; private set; } = 25f;
        public static float SpedUpSpinSpeed { get; private set; } = 50f;
    }


    public static class DisplayerTextOptions
    {
        public class ToggleText
        {
            // No record on turntable
            private static string emptyTrackText;
            public static string EmptyTrackText
            {
                get => emptyTrackText ?? "Please insert:";
            }


            // Has record but no tonearm
            private static string missingTonearm;
            public static string MissingTonearm
            {
                get => missingTonearm ?? "Please adjust tonearm";
            }


            // Waiting for turntable to read record
            private static string readingRecord;
            public static string ReadingRecord
            {
                get => readingRecord ?? "Please wait!";
            }


            // Has record & tonearm but hasn't play
            private static string pause;
            public static string Pause
            {
                get => pause ?? "Pausing";
            }


            // Has record & tonearm but hasn't play
            private static string play;
            public static string Play
            {
                get => play ?? "Currently Playing:";
            }
        }


        public class DisplayAndPowerText
        {
            // No record on turntable
            private static string emptyTrackText;
            public static string EmptyTrackText
            {
                get => emptyTrackText ?? "Track";
            }


            // Turntable power ON
            private static string powerOn;
            public static string PowerOn
            {
                get => powerOn ?? "On";
            }


            // Turntable power OFF
            private static string powerOff;
            public static string PowerOff
            {
                get => powerOff ?? "Off";
            }
        }
    }
}


#region ENUM SECTION
//------------------


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
    None,
    Slowed, 
    Normal, 
    SpedUp
}
public enum UpdateDisplay
{
    Volume,
    RPM,
    Reverb,
    Tonearm,
    Record
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


//--------
#endregion



#region STRUCT SECTION
//------------------





//--------
#endregion