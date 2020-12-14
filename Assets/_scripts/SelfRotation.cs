using UnityEngine;
using System.Collections;

public class SelfRotation : MonoBehaviour
{
	public float rotationSpeedx;
	public float rotationSpeedy;
	public float rotationSpeedz;
    private float initrotationSpeedx;
    private float initrotationSpeedy;
    private float initrotationSpeedz;
	private delegate void rotationDelegate();
	private rotationDelegate Rotate;
	
	void Start ()
	{
        initrotationSpeedx = Mathf.Abs(rotationSpeedx);
        initrotationSpeedy = Mathf.Abs(rotationSpeedy);
        initrotationSpeedz = Mathf.Abs(rotationSpeedz);
		if (rotationSpeedy != 0 && rotationSpeedx == 0 && rotationSpeedz == 0)
		{
			Rotate = RotateY;
		}
		else if (rotationSpeedy == 0 && rotationSpeedx == 0 && rotationSpeedz != 0)
		{
			Rotate = RotateZ;
		}
		else
		{
			Rotate = RotateAll;
		}
	}

	// Update is called once per frame
	void Update ()
	{
        if (initrotationSpeedx < rotationSpeedx)
            rotationSpeedx -= Time.deltaTime;
        else if (-initrotationSpeedx > rotationSpeedx)
            rotationSpeedx += Time.deltaTime;

        if (initrotationSpeedy < rotationSpeedy)
        {
            rotationSpeedy -=  Time.deltaTime;
        }
        else if (-initrotationSpeedy > rotationSpeedy)
        {
            rotationSpeedy += Time.deltaTime;
        }
        if (initrotationSpeedz < rotationSpeedz)
            rotationSpeedz -= 3 * Time.deltaTime;
		Rotate();
	
	}
	
	private void RotateY()
	{
		transform.RotateAroundLocal(Vector3.up, rotationSpeedy * Time.deltaTime);
	}
	
	private void RotateZ()
	{
		transform.RotateAroundLocal(Vector3.forward, rotationSpeedz * Time.deltaTime);
	}
	
	private void RotateAll()
	{
		transform.RotateAroundLocal(Vector3.right, rotationSpeedx * Time.deltaTime);
		transform.RotateAroundLocal(Vector3.up, rotationSpeedy * Time.deltaTime);
		transform.RotateAroundLocal(Vector3.forward, rotationSpeedz * Time.deltaTime);		
	}

    public void RotateY(float speed)
    {
        rotationSpeedy = speed;
    }
    public void RotateZ(float speed)
    {
        rotationSpeedy = speed;
    }
}

