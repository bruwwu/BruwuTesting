using UnityEngine;

public class WaterSim : MonoBehaviour
{
    public float speed;
    public float lenght;

    public float offset;
    public float amplitude = 1f;
    public static WaterSim instance;

    
    private void Awake() {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }
    }
    private void Update()
    {
        offset += Time.deltaTime * speed;
    }
    public float GetWaveHeight(float _x)
    {
        return amplitude * Mathf.Sin(_x / lenght + offset);
    }
}
