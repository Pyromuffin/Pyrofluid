using UnityEditor;
using UnityEngine;
using System.Collections;

public class Raymarching : MonoBehaviour
{
	// Fields
	public Vector3 LL;
	public Vector3 LR;
    public Vector3 corner;
	public Camera mainCamera;
	public static Material rayMat;
	public Vector2 size;
	public float stepSize;
	public Vector3 UL;
	public static RenderTexture volume;
	
	// Methods
	private void Start()
	{
        GetComponent<MeshRenderer>().enabled = true;
		rayMat = base.gameObject.renderer.material;
		
		
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, transform.localScale);
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
            rayMat.SetFloat("StepSize", this.stepSize);
            rayMat.SetFloat("worldSize", transform.localScale.x);

            corner = Camera.current.ViewportToWorldPoint(new Vector3(0f, 0f, Camera.current.nearClipPlane));
            rayMat.SetVector("screenCorner", new Vector4(corner.x, corner.y, corner.z));
        }
    }
    
}



