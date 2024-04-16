using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public int level;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SwitchToLevel);
    }

    public void SwitchToLevel()
    {
        UIAgent.Instance.HideAllMenus();
        GameController.Instance.BeginLoadingLevel(level);
    }
}
