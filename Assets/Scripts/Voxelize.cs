using UnityEngine;
using System.Collections;


public class Voxelize : MonoBehaviour {

	public Shader voxelizeShader;
   
 

	// Use this for initialization
	void Start () {
		
        RenderTexture sizeTex = new RenderTexture(128, 128, 0);
        camera.targetTexture = sizeTex;
        camera.aspect = 1;
        

		Matrix4x4 P = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
		Matrix4x4 V = camera.worldToCameraMatrix;
		Matrix4x4 MVP = P * V;
		Shader.SetGlobalMatrix("zMVP", MVP);


       
	}


    void FixedUpdate()
    {
        Graphics.SetRenderTarget(Fluid.media);
        GL.Clear(true, true, new Color(0, 0, 0, 0));

        Graphics.SetRandomWriteTarget(1, Fluid.media);
        camera.RenderWithShader(voxelizeShader, "");
        Graphics.ClearRandomWriteTargets();

    }


}
