using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string playerId;
    public bool walls;
    public bool glassWalls;
    public bool advertisements;
    public bool crowd;
    public bool trains;
    public bool pillars;
    public bool vendingMachines;
    public bool signs;
    public bool benches;
    public bool fireboxes;
    public float exposure;
    public int rating;
    public string timestamp;

    public GameData
        (
        string playerId, 
        bool walls, 
        bool glassWalls, 
        bool advertisements, 
        bool crowd, 
        bool trains, 
        bool pillars,
        bool vendingMachines, 
        bool signs, 
        bool benches, 
        bool fireboxes,
        float exposure,
        int rating
        )
    {
        this.playerId = playerId;
        this.walls = walls;
        this.glassWalls = glassWalls;
        this.advertisements = advertisements;
        this.crowd = crowd;
        this.trains = trains;
        this.pillars = pillars;
        this.vendingMachines = vendingMachines;
        this.signs = signs;
        this.benches = benches; 
        this.fireboxes = fireboxes;
        this.exposure = exposure;
        this.rating = rating;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
