using UnityEngine;

public class ObstacleMapManager : MonoBehaviour
{
    public GameObject[] layouts; // Assign Layout_0/1/2...
    private int currentIndex = 0;

    void Start()
    {
        ActivateLayout(currentIndex);
    }

    // Switch to the next layout
    public void SwitchToNextLayout()
    {
        layouts[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % layouts.Length;
        layouts[currentIndex].SetActive(true);
        Debug.Log("Switched to layout: " + currentIndex);
    }
    // Activate the layout at the specified index
    private void ActivateLayout(int index)
    {
        for (int i = 0; i < layouts.Length; i++)
        {
            layouts[i].SetActive(i == index);
        }
    }
}
