using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int playerId;
    public int rating;
    public string timestamp;

    public GameData(int playerId, int rating)
    {
        this.playerId = playerId;
        this.rating = rating;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
