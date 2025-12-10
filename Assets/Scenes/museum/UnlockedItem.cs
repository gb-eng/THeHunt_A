using UnityEngine;

public class UnlockedItem
{
    public string id;
    public string name;
    public string type; // "2D" or "3D"
    public string thumbnailPath;
    public bool isLocked;

    // ✅ Constructor with 5 parameters
    public UnlockedItem(string id, string name, string type, string thumbnailPath, bool isLocked)
    {
        this.id = id;
        this.name = name;
        this.type = type;
        this.thumbnailPath = thumbnailPath;
        this.isLocked = isLocked;
    }
}