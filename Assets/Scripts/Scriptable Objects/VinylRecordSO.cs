using UnityEngine;


[CreateAssetMenu(fileName = "NewVinylRecord", menuName = "MusicProject/Vinyl Record")]
public class VinylRecordSO : ScriptableObject
{
    public Sprite cover;
    public AudioClip track;
}