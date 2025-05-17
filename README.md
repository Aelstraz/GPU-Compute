# GPU Compute (Unity)
GPU Compute provides an easy way to setup & execute GPU compute shaders in Unity. Create and manage buffers, track GPU memory usage & execution time, automatically calculate thread group sizes & buffer strides- all in one class.

### Importing/Installing:
Simply add the entire folder into your projects Assets folder.
***
### Instantiate:
Start by creating a new instance of GPU Compute, providing the compute shader you want to use:

	GPUCompute gpuCompute = new GPUCompute(myComputeShader);

***
### Creating & Setting Buffers/Textures/Variables:
To create a buffer simply input the struct type of the buffer, it's name, and the actual data to be passed into the buffer (the struct type and length of the data is used to automatically set the size of the buffer). Empty buffers can also be made.
	
 	gpuCompute.SetBuffer<Vector3>("vertices", ref vertices);
 	gpuCompute.SetBuffer<Vector2>("uvs", ref uvs);
  	gpuCompute.CreateEmptyBuffer<int>("myEmptyBuffer", myEmptyBufferLength);
 	

Buffer data, textures and variables values can be set like so:

	gpuCompute.SetInt("myInt", myInt);
 	gpuCompute.SetFloat("myFloat", myFloat);
  	gpuCompute.SetVector("myVector", myVector);
   	gpuCompute.SetBufferData<Vector2>("myVectorBuffer", ref myVectorBuffer);
   	gpuCompute.SetRenderTexture("myRenderTexture", myRenderTexture);

***
### Global Buffers/Textures:
Global buffers can be made and set in the same way local buffers are set.

	GPUCompute.SetGlobalBuffer<Vector3>("globalVertices", ref vertices);	
	GPUCompute.CreateEmptyGlobalBuffer<Vector3>("myEmptyGlobalBuffer", myEmptyGlobalBufferLength);
 	GPUCompute.SetBufferData<Vector2>("myGlobalVectorBuffer", ref myGlobalVectorBuffer);
 
In order to use them in your compute shader, they first need to be linked to your gpuCompute instance as shown:

	gpuCompute.LinkGlobalBuffer("globalBufferName");
	gpuCompute.LinkGlobalTexture("globalTextureName", "globalTextureName");
 
***
### Setting Thread Group Size:
Thread group sizes can be automatically calculated based on your workload dimensions.
First get the number of threads as declared in your compute shader:

	Vector3Int shaderNumOfThreads = new Vector3Int(8, 8, 1);

Then select one of the options below (1D, 2D, 3D) based on your workload needs. Thread group sizes can also be set manually.

### Thread Group Size (1D):
For a one-dimensional workload (e.g. an array) pass your job length and shader number of threads into the SetCalculatedThreadGroupSize() method:

	int jobLength = vertices.Length;
	gpuCompute.SetCalculatedThreadGroupSize(jobLength, shaderNumOfThreads);

You will likely need to work out the index of the thread in your compute shader, so the SetCalculatedThreadGroupSize() method also returns the dimension width. The dimension width can be passed into your compute shader in order to get the current index, as seen below:

	int threadWidth = gpuCompute.SetCalculatedThreadGroupSize(jobLength, shaderNumOfThreads);
	gpuCompute.SetInt("threadWidth", threadWidth);

Then in your compute shader the current index can be found like so:

	void CSMain(uint3 id : SV_DispatchThreadID)
	{
		uint index = id.x + id.y * threadWidth;
	}
 
***
### Thread Group Size (2D):
For a two-dimensional workload (e.g. a texture) pass the width, length and your compute shader number of threads into the SetCalculatedThreadGroupSize2D() method:

	gpuCompute.SetCalculatedThreadGroupSize2D(width, length, shaderNumOfThreads);
 
 Then in your compute shader the current index can be found like so:

	void CSMain(uint3 id : SV_DispatchThreadID)
	{
		int2 index = int2(id.x, id.y);
	}

***
### Setting Thread Group Size (3D):
For a three-dimensional workload (e.g. a volume) pass the width, length, depth and your compute shader number of threads into the SetCalculatedThreadGroupSize3D() method:

	gpuCompute.SetCalculatedThreadGroupSize3D(width, length, depth, shaderNumOfThreads);

***
### Executing:
With buffers set, and the thread group size set, the compute shader can be executed. 
For standard execution (non async) simply run the following:

 	gpuCompute.Execute();

For async execution first subscribe to the OnExecuteComplete event as shown below:

	gpuCompute.OnExecuteComplete += GPUCompute_OnExecuteComplete;
 
Then run the ExecuteAsync() method as a coroutine:
	
	StartCoroutine(gpuCompute.ExecuteAsync());
 
***
### Dispose:
Once you are done with your GPU Compute instance, make sure to dispose it to prevent memory leaks on the GPU using the required disposal method as followed:

	gpuCompute.Dipose();
 	gpuCompute.DisposeLocal();
  	gpuCompute.DisposeGlobal();
   
***
### Get GPU Memory Used:
Memory used by the GPU is automatically tracked and can be viewed as either total bytes, or a formatted string.

 	gpuCompute.GetGPUMemoryUsed();
	gpuCompute.GetGPUMemoryUsedFormatted();

 	gpuCompute.GetGlobalGPUMemoryUsed();
	gpuCompute.GetGlobalGPUMemoryUsedFormatted();

 	gpuCompute.GetTotalGPUMemoryUsed();
	gpuCompute.GetTotalGPUMemoryUsedFormatted();
 
 ***
### Get Compute Time:
The amount of time it took for the last computation to finish is also tracked and viewable through:

 	TimeSpan lastComputeTime = gpuCompute.GetLastComputeTime();
  
***
### Twitter: [@aelstraz](https://x.com/Aelstraz)
