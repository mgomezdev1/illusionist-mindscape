using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HiveMind : MonoBehaviour
{
    private static HiveMind instance;
    public static HiveMind Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<HiveMind>();
            return instance;
        }
    }
    private List<Enemy> subjects = new List<Enemy>();
    public List<Enemy> Subjects => subjects;
    public float searchTime;
    [SerializeField] private TileGrid associatedGrid;

    private readonly Dictionary<Enemy, bool> visionEnlightenmentCache = new();
    private readonly Dictionary<TileGrid, GridMap<CellVision>> hiveVision = new();

    float lastRefreshedHiveVision = 0.0f;
    float hiveVisionRefreshInterval = 0.5f;

    public Color hiveVisionRenderColor = Color.blue;
    public Color[] altVisionRenderColors = new Color[]
    {
        Color.red, Color.yellow, Color.green
    };

    // Start is called before the first frame update
    [ExecuteInEditMode]
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlayerSpotted(Enemy source, Vector3 position)
    {
        foreach (var enemy in subjects)
        {
            if (enemy == source) continue;
            enemy.OnHiveMindPlayerDetected(position);
        }
    }
    private int nextId = 0;
    public int RegisterEnemy(Enemy enemy)
    {
        subjects.Add(enemy);
        return nextId++;
    }
    public void UnregisterEnemy(Enemy enemy)
    {
        subjects.Remove(enemy);
    }
    public Color GetVisionRenderColor(Enemy enemy)
    {
        if (enemy.IsEnlightened()) return hiveVisionRenderColor;
        int buzzedIndex = 0;
        foreach (Enemy subject in subjects)
        {
            if (subject.IsEnlightened()) continue;
            if (subject.Id == enemy.Id) 
                return altVisionRenderColors[buzzedIndex % altVisionRenderColors.Length];
            buzzedIndex++;
        }
        // Should never reach here if enemy is registered
        throw new Exception($"Enemy {enemy.gameObject.name} (ID {enemy.Id}) is not registered as a subject of the hive mind!");
    }

    public GridMap<CellVision> GetHiveVision(TileGrid grid)
    {
        // TONS of logic just for efficient caching!
        if (lastRefreshedHiveVision + hiveVisionRefreshInterval < Time.time || !hiveVision.ContainsKey(grid))
        {
            RecalculateHiveVision(grid);
        }
        else
        {
            bool enlightenmentChanges = false;
            foreach (var e in Subjects)
            {
                bool isEnemyEnlightened = e.IsEnlightened();
                // Check for added subjects and subjects who have been buzzed/enlightened
                if (!visionEnlightenmentCache.ContainsKey(e))
                {
                    visionEnlightenmentCache[e] = isEnemyEnlightened;
                    if (isEnemyEnlightened) enlightenmentChanges = true;
                } else if (visionEnlightenmentCache[e] != isEnemyEnlightened) {
                    visionEnlightenmentCache[e] = isEnemyEnlightened;
                    enlightenmentChanges = true;
                }
            }
            // Check for "removed" subjects
            if (Subjects.Count != visionEnlightenmentCache.Count)
            {
                foreach (var e in visionEnlightenmentCache.Keys.ToArray())
                {
                    if (!Subjects.Contains(e))
                    {
                        visionEnlightenmentCache.Remove(e);
                        if (e.IsEnlightened()) enlightenmentChanges = true;
                    }
                }
            }            

            if (enlightenmentChanges)
            {
                RecalculateHiveVision(grid);
            }
        }

        return hiveVision[grid];
    }

    public void RecalculateHiveVision(TileGrid grid)
    {
        lastRefreshedHiveVision = Time.time;
        IEnumerable<Enemy> enlightenedEnemies = Subjects.Where(e => e.IsEnlightened());
        GridMap<CellVision> result = grid.GetVisibilityGridMap(enlightenedEnemies);
        hiveVision[grid] = result;
    }
}
