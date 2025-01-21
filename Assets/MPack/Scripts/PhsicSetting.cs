using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PhsicSetting2D
{
    // [SerializeField]
    // pri
    public Vector2 InitialSpeed;
    public Vector2 Gravity;
    public Vector2 Drag;
}

public class Physic2DSimulator
{
    PhsicSetting2D setting;
    Vector2 position;

    Vector2 speed;

    public Vector2 Position
    {
        get
        {
            return position;
        }
    }

    public Physic2DSimulator(PhsicSetting2D _setting)
    {
        position = Vector3.zero;
        setting = _setting;
        speed = _setting.InitialSpeed;
    }

    public Physic2DSimulator(Vector3 _position, PhsicSetting2D _setting)
    {
        position = _position;
        setting = _setting;
        speed = _setting.InitialSpeed;
    }

    public void Update(float deltaTime)
    {
        speed += setting.Gravity * deltaTime;
        speed.x = Mathf.MoveTowards(speed.x, 0, setting.Drag.x);
        speed.y = Mathf.MoveTowards(speed.y, 0, setting.Drag.y);
        Vector2 delta = speed * deltaTime;
        // del
        position += delta;
    }

    public void SetPosition(Vector2 pos)
    {
        position = pos;
    }
}
