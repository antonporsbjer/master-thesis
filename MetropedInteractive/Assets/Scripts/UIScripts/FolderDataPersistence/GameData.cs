using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string participantId;
    public int presetScenarioId;
    public bool walls;
    public bool glassWalls;
    public bool advertisements;
    public bool crowd;
    public bool trains;
    public bool pillars;
    public bool vendingMachines;
    public bool trashCans;
    public bool benches;
    public float exposure;
    public int ratingQuestion1;
    public int ratingQuestion2;
    public int ratingQuestion3;
    public string timestamp;

    public GameData
        (
        string participantId,
        int presetScenarioId, 
        bool walls, 
        bool glassWalls, 
        bool advertisements, 
        bool crowd, 
        bool trains, 
        bool pillars,
        bool vendingMachines, 
        bool trashCans, 
        bool benches, 
        float exposure,
        int ratingQuestion1,
        int ratingQuestion2,
        int ratingQuestion3
        )
    {
        this.participantId = participantId;
        this.presetScenarioId = presetScenarioId;
        this.walls = walls;
        this.glassWalls = glassWalls;
        this.advertisements = advertisements;
        this.crowd = crowd;
        this.trains = trains;
        this.pillars = pillars;
        this.vendingMachines = vendingMachines;
        this.trashCans = trashCans;
        this.benches = benches; 
        this.exposure = exposure;
        this.ratingQuestion1 = ratingQuestion1;
        this.ratingQuestion2 = ratingQuestion2;
        this.ratingQuestion3 = ratingQuestion3;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
