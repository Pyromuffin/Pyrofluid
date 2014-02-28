using UnityEditor;
using UnityEngine;
using System.Collections;

public class Raymarching : MonoBehaviour
{
	// Fields
	private Vector3 LL;
    private Vector3 LR;
    private Vector3 corner;
    private Camera mainCamera;
	private Material rayMat;
    private Vector2 size;
	public float stepSize;
    public int stepCount;
    private Vector3 UL;
    public Vector4 intensityMask;
    public Shader raymarchingShader;

	// Methods
	private void Start()
	{
        GetComponent<MeshRenderer>().enabled = true;
		rayMat = new Material(raymarchingShader);
	    renderer.material = rayMat;


	}

   
	void Update()
	{
        if (Camera.current != null)
        {
            Vector3 up = Camera.current.transform.up;
            Vector3 right = Camera.current.transform.right;

            this.LL = Camera.current.ViewportToWorldPoint(new Vector3(0f, 0f, Camera.current.nearClipPlane));
            this.UL = Camera.current.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.current.nearClipPlane));
            this.LR = Camera.current.ViewportToWorldPoint(new Vector3(1f, 0f, Camera.current.nearClipPlane));
            Vector3 vector = this.LL - this.LR;
            Vector3 vector2 = this.LL - this.UL;
            this.size = new Vector2(vector.magnitude, vector2.magnitude);
            rayMat.SetVector("cameraWorldSize", new Vector4(this.size.x, this.size.y));


            rayMat.SetVector("cameraWorldPosition", Camera.current.transform.position);
            rayMat.SetVector("cameraUp", new Vector4(up.x, up.y, up.z));
            rayMat.SetVector("cameraRight", new Vector4(right.x, right.y, right.z));
            rayMat.SetInt("stepCount", stepCount);
            rayMat.SetFloat("StepSize", this.stepSize);
            rayMat.SetVector("intensityMask", intensityMask);
            
            rayMat.SetFloat("worldSize", transform.localScale.x);

            corner = Camera.current.ViewportToWorldPoint(new Vector3(0f, 0f, Camera.current.nearClipPlane));
            rayMat.SetVector("screenCorner", new Vector4(corner.x, corner.y, corner.z));
        }
    }
    
}



