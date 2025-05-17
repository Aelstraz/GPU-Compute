# GPU Compute (Unity)
# README IS IN PROGRESS
GPU Compute provides an easy way to setup & execute GPU compute shaders in Unity. Create and manage buffers, track GPU memory usage & execution time, automatically calculate thread group sizes & buffer strides- all in one class.

## USAGE:

GPU Compute is a self contained class that provides an easy way to setup and execute your compute shaders.
Start by creating a new instance of GPU Compute, providing the compute shader you want to use:

### Instantiate:
	  GPUCompute gpuCompute = new GPUCompute(myComputeShader);
### Creating & Setting Buffers/Variables:
    gpuCompute.SetInt("textureWidth", myRenderTexture.width);
    gpuCompute.SetInt("textureHeight", myRenderTexture.height);
    gpuCompute.SetBuffer<Vector3>("vertices", ref vertices);
    gpuCompute.SetBuffer<Vector2>("uvs", ref uvs);
    gpuCompute.SetRenderTexture("myRenderTexture", myRenderTexture);
### Setting Thread Group Size:
Thread group sizes can be automatically calculated based on your workload dimensions.
First get the number of threads as declared in your compute shader:

    Vector3Int shaderNumOfThreads = new Vector3Int(8, 8, 1);
    
For a one-dimensional workload (e.g. an array) use the following:

    int jobLength = vertices.Length;
    gpuCompute.SetCalculatedThreadGroupSize(jobLength, shaderNumOfThreads);
    
    int threadWidth = gpuCompute.SetCalculatedThreadGroupSize(jobLength, shaderNumOfThreads);
    gpuCompute.SetInt("threadWidth", threadWidth);

    const uint GetIndex(const uint3 id, const uint width)
    {
      return id.x + id.y * width;
    }
    
    gpuCompute.SetCalculatedThreadGroupSize2D(width, length, shaderNumOfThreads);
    gpuCompute.SetCalculatedThreadGroupSize3D(width, length, depth, shaderNumOfThreads);
    
    gpuCompute.OnExecuteComplete += GPUCompute_OnExecuteComplete;
    
    gpuCompute.Execute();
    StartCoroutine(gpuCompute.ExecuteAsync());
