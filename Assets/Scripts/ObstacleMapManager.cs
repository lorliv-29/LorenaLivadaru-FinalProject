using UnityEngine;

public class ObstacleMapManager : MonoBehaviour
{
    public GameObject[] layouts; // Assign Layout Prefabs
    private GameObject currentLayoutInstance;
    private int currentIndex = 0;

    void Start()
    {
        LoadLayout(currentIndex);
    }

    // Switch to the next layout and reset it fully
    public void SwitchToNextLayout()
    {
        // Destroy current layout if exists
        if (currentLayoutInstance != null)
        {
            Destroy(currentLayoutInstance);
        }

        // Move to next layout index
        currentIndex = (currentIndex + 1) % layouts.Length;

        // Instantiate new layout from prefab
        LoadLayout(currentIndex);

        Debug.Log("Switched to layout: " + currentIndex);
    }

    // Instantiate layout prefab
    private void LoadLayout(int index)
    {
        if (index >= 0 && index < layouts.Length)
        {
            currentLayoutInstance = Instantiate(layouts[index], transform);
        }
        else
        {
            Debug.LogWarning("Layout index out of range: " + index);
        }
    }
}
