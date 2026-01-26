using UnityEngine;


[CreateAssetMenu(fileName = "NewRecord", menuName = "MusicProject/Record")]
public class RecordSO : ScriptableObject
{
    public Sprite picture;
    public AudioClip track;
}