using UnityEngine;

[CreateAssetMenu(fileName = "NewVinylCover", menuName = "MusicProject/Vinyl Cover")]
public class VinylCoverSO : ScriptableObject
{
    public string vinylName;
    public string vinylArtist;

    public Sprite frontCover;
    public Sprite backCover;
}