using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField] StickMovementFinal stickMovement;
    [SerializeField] Button playButton;
    [SerializeField] GameObject UI;
    void Start()
    {
        Time.timeScale = 0;
        stickMovement.enabled = false;
        playButton.onClick.AddListener(start);
    }

    void start()
    {
        Time.timeScale = 1;
        stickMovement.enabled = true;
        Destroy(UI);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
