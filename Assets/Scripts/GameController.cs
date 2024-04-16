using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable enable
public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameController>();
            return instance;
        }
    }

    public bool IsPaused { get; private set; } = true;
    private readonly List<Player> players = new List<Player>();

    [Header("PlayerInteraction")]
    [SerializeField] private int levelToLoadOnGameOver;

    // Start is called before the first frame update
    [ExecuteInEditMode]
    void Awake()
    {
        instance = this;
        IsPaused = false;
        StartCoroutine(FadeIn());
    }

    public Player? GetPlayerByIndex(int index)
    {
        if (index > players.Count) return null;
        return players[index];
    }
    public int RegisterPlayer(Player player)
    {
        players.Add(player);
        return players.Count - 1;
    }

    public bool AllowedToPause { get { return !levelSwitching; } }

    bool levelSwitching = false;
    public void BeginGameOver()
    {
        if (levelSwitching) return;
        levelSwitching = true;
        StartCoroutine(GameOverCoroutine());
    }
    public void BeginLoadingLevel(int level)
    {
        if (levelSwitching) return;
        levelSwitching = true;
        StartCoroutine(FadeToLevel(level));
    }
    public void TriggerVictory()
    {
        UIAgent.Instance.ActivateVictoryScreen();
    }
    private IEnumerator GameOverCoroutine()
    {
        UIAgent.Instance.Animator.SetTrigger("GameOver");
        yield return FadeToLevel(levelToLoadOnGameOver);
    }
    private IEnumerator FadeToLevel(int level)
    {
        IsPaused = true;
        Time.timeScale = 0.25f;
        yield return FadeOut();
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(level);
    }
    bool fadingIn = false;
    private IEnumerator FadeIn()
    {
        if (fadingIn) yield break;
        fadingIn = true;
        UIAgent.Instance.Animator.SetTrigger("FadeIn");
        yield return new WaitForSecondsRealtime(5.0f);
        fadingIn = false;
    }
    bool fadingOut = false;
    private IEnumerator FadeOut()
    {
        if (fadingOut) yield break;
        fadingOut = true;
        UIAgent.Instance.Animator.SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(5.0f);
        fadingOut = false;
    }
}
