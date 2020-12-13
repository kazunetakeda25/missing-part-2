using UnityEngine;
using System.Collections;

public class LightFader : MonoBehaviour {

    public Light lightSource;
    public float high = 4;
    public float low = 0;
    public float diff;
    public bool raise;
    public float speed;
    public float current;

	// Use this for initialization
	void Start () {
        diff = high - low;

        if (raise)
            current = 0.0f;
        else
            current = diff;
	}
	
	// Update is called once per frame
	void Update () {
        if (!raise)
        {
            current -= speed * Time.deltaTime;
            if (current < 0)
            {
                raise = true;
                current = 0;
            }
        }
        else
        {
            current += speed * Time.deltaTime;
            if (current >= diff)
            {
                current = diff;
				raise = false;
            }
        }
        float percentage = current / diff;
        lightSource.intensity = Mathf.Lerp(low, high, percentage);
	}
}
