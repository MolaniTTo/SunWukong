using UnityEngine;

public class DeletePlayerPrefs : MonoBehaviour
{
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    
}
