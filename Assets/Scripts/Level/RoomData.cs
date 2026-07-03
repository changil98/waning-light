using UnityEngine;

[CreateAssetMenu(menuName = "Waning Light/Room Data")]
public class RoomData : ScriptableObject
{
    public int minWidth = 6;
    public int maxWidth = 14;
    public int minHeight = 6;
    public int maxHeight = 14;
}