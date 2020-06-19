using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public int ConnectionID;
    public string Name;
    public Color Color = new Color();

    public int HP = 10;
    public Vector2Int Position = Vector2Int.zero;
    public int Gold;

    public Player(int _connectionID, string _name, uint _color)
    {
        HP = 10;

        ConnectionID = _connectionID;
        Name = _name;

        Color.r = (byte)((_color >> 24) & 0xFF);
        Color.g = (byte)((_color >> 16) & 0xFF);
        Color.b = (byte)((_color >> 8) & 0xFF);
        Color.a = (byte)((_color) & 0xFF);
    }

    public void TakeDamage(int _amount)
    {
        HP = HP - _amount > 0 ? HP - _amount : 0;
    }

    public void Heal(int _amount)
    {
        HP = HP > 0 ? HP + _amount : 0;
    }
}
