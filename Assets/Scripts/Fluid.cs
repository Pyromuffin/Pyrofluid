using System;
using UnityEngine;
using System.Collections;

public class Fluid : MonoBehaviour
{
    public ComputeShader fluidCompute;
    public const int size = 128;

    private RenderTexture velocity, nextVelocity, pressure, nextPressure, divergenceTexture, density, nextDensity;
    public static RenderTexture media;
    public  float diffusionConstant = 1.76e-5f;
    public bool calculateDiffusion = true;
  

    public float distanceStep, timeStep;
    public Vector3 sourceVelocity, sourceDensity;
    public Vector3 textureSize;
    private int advect, jacobi, project, divergence, jacobiPressure, jacobiVector;
    public int jacobiCount;

    
    public float ambientTemperature, buoyancyConstant;

    [SerializeField] 
    private viewTexture view;

    enum viewTexture { pressure, nextPressure, velocity, nextVelocity, divergence, media, density, nextDensity}

	// Use this for initialization
	void Start ()
	{
	    advect = fluidCompute.FindKernel("Advect");
        jacobi = fluidCompute.FindKernel("Jacobi");
        project = fluidCompute.FindKernel("Project");
	    divergence = fluidCompute.FindKernel("Divergence");
	    jacobiPressure = fluidCompute.FindKernel("JacobiPressure");
	    jacobiVector = fluidCompute.FindKernel("JacobiVector");


        setupTexture(ref density, RenderTextureFormat.RGHalf);
        setupTexture(ref nextDensity, RenderTextureFormat.RGHalf);
        setupTexture(ref media, RenderTextureFormat.RHalf);
    	setupTexture(ref velocity, RenderTextureFormat.ARGBHalf);
        setupTexture(ref nextVelocity, RenderTextureFormat.ARGBHalf);
        setupTexture(ref pressure, RenderTextureFormat.RHalf);
        setupTexture(ref nextPressure, RenderTextureFormat.RHalf);
        setupTexture(ref divergenceTexture, RenderTextureFormat.RHalf);

     
        fluidCompute.SetVector("textureSize", textureSize);

        //maybe
    

      
	}


    void JacobiIteration(RenderTexture x, RenderTexture nextX, float diffusionConstant, float timeStep, float distanceStep)
    {
        float alpha = (diffusionConstant * timeStep) / (Mathf.Pow(distanceStep, 3));
        float beta = 1 + (6 * alpha);

   
        fluidCompute.SetFloat("alpha", alpha);
        fluidCompute.SetFloat("rBeta", 1.0f/beta);

        fluidCompute.SetTexture(jacobi, "media", media);
        fluidCompute.SetTexture(jacobi, "x", x);
        fluidCompute.SetTexture(jacobi, "nextX", nextX);
        fluidCompute.Dispatch(jacobi, 16, 16, 16);



    }
    void JacobiIterationVector(RenderTexture x, RenderTexture nextX, float diffusionConstant, float timeStep, float distanceStep)
    {
        float alpha = (diffusionConstant * timeStep) / (Mathf.Pow(distanceStep, 3));
        float beta = 1 + (6 * alpha);


        fluidCompute.SetFloat("alpha", alpha);
        fluidCompute.SetFloat("rBeta", 1.0f / beta);

        fluidCompute.SetTexture(jacobiVector, "media", media);
        fluidCompute.SetTexture(jacobiVector, "x3D", x);
        fluidCompute.SetTexture(jacobiVector, "nextX3D", nextX);
        fluidCompute.Dispatch(jacobiVector, 16, 16, 16);

    }

    void JacobiPressure(int iterations)
    {

        fluidCompute.SetFloat("alpha", 1);
        fluidCompute.SetFloat("rBeta", 1.0f / 6.0f);
        fluidCompute.SetTexture(jacobiPressure, "b", divergenceTexture);
        fluidCompute.SetTexture(jacobiPressure, "media", media);

        for (int i = 0; i < iterations; i++)
        {
            fluidCompute.SetTexture(jacobiPressure, "x", pressure);
            fluidCompute.SetTexture(jacobiPressure, "nextX", nextPressure);

            fluidCompute.Dispatch(jacobiPressure, 16, 16, 16);

            fluidCompute.SetTexture(jacobiPressure, "x", nextPressure);
            fluidCompute.SetTexture(jacobiPressure, "nextX", pressure);

            fluidCompute.Dispatch(jacobiPressure, 16, 16, 16);
        }
       
    }



	// Update is called once per frame
	void FixedUpdate () {

        //set compute constants
        fluidCompute.SetFloat("distanceStep", distanceStep);
        fluidCompute.SetFloat("timeStep", timeStep);
        fluidCompute.SetVector("sourceVelocity", sourceVelocity);
        fluidCompute.SetVector("sourceDensity", sourceDensity);
        fluidCompute.SetFloat("buoyancyConstant", buoyancyConstant);
        fluidCompute.SetFloat("ambientTemperature", ambientTemperature);
    


        fluidCompute.SetTexture(advect, "density", density);
        fluidCompute.SetTexture(advect, "nextDensity", nextDensity);
        fluidCompute.SetTexture(advect, "velocity", velocity);
        fluidCompute.SetTexture(advect, "nextVelocity", nextVelocity);
        fluidCompute.SetTexture(advect, "media", media);
        
        fluidCompute.Dispatch(advect, 16, 16, 16);

	    if (calculateDiffusion)
	    {
	        for (int i = 0; i < jacobiCount; i++)
	        {
	            JacobiIterationVector(nextDensity, density, diffusionConstant, timeStep, distanceStep);
	            JacobiIterationVector(density, nextDensity, diffusionConstant, timeStep, distanceStep);
	        }
	    }

	    fluidCompute.SetTexture(divergence, "velocity", nextVelocity);
        fluidCompute.SetTexture(divergence, "divergenceTexture", divergenceTexture);
        fluidCompute.SetTexture(divergence, "media", media);
        fluidCompute.Dispatch(divergence, 16, 16, 16);

        JacobiPressure(jacobiCount);
     
        fluidCompute.SetTexture(project, "media", media);
        fluidCompute.SetTexture(project, "pressure", pressure);
        fluidCompute.SetTexture(project, "velocity", nextVelocity);
        fluidCompute.SetTexture(project, "nextVelocity", velocity);
        fluidCompute.Dispatch(project,16,16,16);
        
	    switch (view)
	    {
	        case viewTexture.pressure:
                Shader.SetGlobalTexture("Current", pressure);
	            break;
	        case viewTexture.nextPressure:
                Shader.SetGlobalTexture("Current", nextPressure);
	            break;
	        case viewTexture.velocity:
                Shader.SetGlobalTexture("Current", velocity);
	            break;
	        case viewTexture.nextVelocity:
                Shader.SetGlobalTexture("Current", nextVelocity);
	            break;
	        case viewTexture.divergence:
                Shader.SetGlobalTexture("Current", divergenceTexture);
	            break;
            case viewTexture.media:
                Shader.SetGlobalTexture("Current", media);
	            break;
            case viewTexture.density:
                Shader.SetGlobalTexture("Current", density);
	            break;
            case viewTexture.nextDensity:
                Shader.SetGlobalTexture("Current", nextDensity);
	            break;
	        default:
	            throw new ArgumentOutOfRangeException();
	    }


        RenderTexture temp = density;
        density = nextDensity;
        nextDensity = temp;

	}


    void setupTexture(ref RenderTexture tex, RenderTextureFormat format)
    {
        //check if those buffers exist
        DestroyImmediate(tex);

        //make those buffers
        tex = new RenderTexture(size, size, 0, format);
        tex.isVolume = true;
        tex.volumeDepth = size;
        tex.enableRandomWrite = true;
        tex.filterMode = FilterMode.Trilinear;
        tex.Create();
    }
}
