using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class WaterController : MonoBehaviour
{
    public static WaterController current;
    private float time;

    public float scale = 1.0f;
    public float speed = 1.0f;
    public float length = 10;
    public float phase = 0;

    void Start()
    {
        current = this;
        time = Time.time;
        
    }

    // Update is called once per frame
    void Update()
    {
        time = Time.time;
        
        Shader.SetGlobalFloat("_WaterScale", scale);
        Shader.SetGlobalFloat("_WaterSpeed", speed);
        Shader.SetGlobalFloat("_WaterTime", time);
        Shader.SetGlobalFloat("_WaterLength", length);
        Shader.SetGlobalFloat("_Phase", phase);
    }

    public float GetWaveYPos(Vector3 position)
    {
        //역푸리에 변환
        float WaveHeight = scale* Mathf.Sin((time / speed - position.x / length) + phase);
        WaveHeight += scale * Mathf.Sin((time / speed * 2f - (position.x + position.z) / length * 0.5f) + 5.0f);
        WaveHeight += scale * Mathf.Sin((time / speed / 5f + (position.x + 0.3f * position.z) / length * 0.2f) + 15.0f);
        WaveHeight += scale* Mathf.Sin((time / speed - (2f * position.x - position.z) / length) + 7.0f);
        WaveHeight += 1.5f * scale * Mathf.Sin((time / speed - (position.x + 5.0f * position.z) / length) + 10.0f);
        WaveHeight += scale * Mathf.Sin((time / speed - (position.x - position.z) / length / 2) + 1.0f);
        return WaveHeight;
    }

    public float DistanceToWater(Vector3 position)
    {
        float waterHeight = GetWaveYPos(position);
        float distanceToWater = position.y - waterHeight;
        
        return distanceToWater;
    }
}
