using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string participantId;
    public bool walls;
    public bool glassWalls;
    public bool advertisements;
    public bool crowd;
    public bool trains;
    public bool pillars;
    public bool vendingMachines;
    public bool signs;
    public bool benches;
    public float exposure;
    public int rating;
    public string timestamp;

    public GameData
        (
        string participantId, 
        bool walls, 
        bool glassWalls, 
        bool advertisements, 
        bool crowd, 
        bool trains, 
        bool pillars,
        bool vendingMachines, 
        bool signs, 
        bool benches, 
        float exposure,
        int rating
        )
    {
        this.participantId = participantId;
        this.walls = walls;
        this.glassWalls = glassWalls;
        this.advertisements = advertisements;
        this.crowd = crowd;
        this.trains = trains;
        this.pillars = pillars;
        this.vendingMachines = vendingMachines;
        this.signs = signs;
        this.benches = benches; 
        this.exposure = exposure;
        this.rating = rating;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
