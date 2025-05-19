# GPU Compute (Unity)
GPU Compute provides an easy way to setup & execute GPU compute shaders in Unity. Create and manage buffers, track GPU memory usage & execution time, automatically calculate thread group sizes & buffer strides- all in one class.

### Importing/Installing:
Simply add the entire folder into your projects Assets folder.
***
### Instantiate:
Start by creating a new instance of GPU Compute, providing the compute shader you want to use:

	GPUCompute gpuCompute = new GPUCompute(myComputeShader);

***
### Dispose:
When you are done with your GPU Compute instance, make sure to dispose it to prevent memory leaks on the GPU- using one of the disposal methods as followed:

	gpuCompute.Dipose();
 	gpuCompute.DisposeLocal();
  	gpuCompute.DisposeGlobal();

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
 
In order to use them in your compute shader, they first need to be linked to your GPU Compute instance as shown:

	gpuCompute.LinkGlobalBuffer("globalBufferName");
	gpuCompute.LinkGlobalTexture("globalTextureName", "globalTextureName");
 
***
### Setting Thread Group Size:
Thread group sizes can be automatically calculated based on your workload dimensions.
First get the number of threads as declared in your compute shader:

	Vector3Int shaderNumOfThreads = new Vector3Int(8, 8, 1);

Then select one of the options below (1D, 2D, 3D) based on your workload needs. Thread group sizes can also be set manually.
***
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
# Example - Creating a subdivision plane:
Unity C# code

	public void GenerateSubdivisionPlane(ComputeShader computeSubdivisionPlaneShader, int subdivisions, float scale)
	{
 		Mesh mesh = new Mesh();
	    	mesh.name = "Subdivision Plane";
	
		//calculate the number of vertices and subdivisions for the subdivision plane
		subdivisions = Mathf.Max(subdivisions, 1);
		int gridSize = Mathf.Max(1, subdivisions + 1);
		int gridLength = gridSize - 1;
		int vertexCount = gridSize * gridSize;
		int quadCount = gridLength * gridLength;
		float halfScale = scale / 2f;
		float step = 2f / (float)gridLength * halfScale;
		
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[quadCount * 6];
		Vector2[] uvs = new Vector2[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];
  
	     	//create a temporary GPU Compute instance supplying the compute subdivision plane shader
		using (GPUCompute gpuCompute = new GPUCompute(computeSubdivisionPlaneShader)) 
		{     		
  			//set 2D group thread size
		        gpuCompute.SetCalculatedThreadGroupSize2D(gridSize, gridSize, new Vector3Int(8, 8, 1));
		 	//any non-buffer variable is set for all kernels
		        gpuCompute.SetInt("gridSize", gridSize); 
		        gpuCompute.SetInt("gridLength", gridLength);
		        gpuCompute.SetFloat("step", step);
		        gpuCompute.SetFloat("halfScale", halfScale);
		 
		    	//the default kernel is 0, optionally a different kernel or multiple kernels can be set
		        gpuCompute.SetBuffer<Vector3>("vertices", ref vertices); 
		        gpuCompute.SetBuffer<Vector2>("uvs", ref uvs);
		        gpuCompute.SetBuffer<Vector3>("normals", ref normals);
	
	 		//execute kernel 0
		        gpuCompute.Execute(0); 
			//get the processed buffer data from GPU
		        gpuCompute.GetBufferData<Vector3>("vertices", ref vertices); 
		        gpuCompute.GetBufferData<Vector2>("uvs", ref uvs); 
		        gpuCompute.GetBufferData<Vector3>("normals", ref normals);
		 
		 	//setting up to execute kernel 1
		        gpuCompute.SetCalculatedThreadGroupSize2D(gridLength, gridLength, new Vector3Int(8, 8, 1)); 
		 	//setting triangle buffer to kernel 1 only
		        gpuCompute.SetBuffer<int>("triangles", ref triangles, new int[] { 1 }); 
	
			//execute kernel 1
		        gpuCompute.Execute(1); 
		 	//get the processed buffer data from GPU
		        gpuCompute.GetBufferData<int>("triangles", ref triangles);
	  	}

		//assign data to the mesh
  		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.normals = normals;
		
		//assign the mesh to the MeshFilter
		GetComponent<MeshFilter>().sharedMesh = mesh;
	}

 Compute Shader code
	
 	#pragma kernel ComputeSubdivisionPlane
	#pragma kernel ComputeTriangles
	
	RWStructuredBuffer<float3> vertices;
	RWStructuredBuffer<float2> uvs;
	RWStructuredBuffer<float3> normals;
	RWStructuredBuffer<int> triangles;
	uint gridSize;
	uint gridLength;
	float step;
	float halfScale;
	
	[numthreads(8, 8, 1)]
	void ComputeSubdivisionPlane(uint3 id : SV_DispatchThreadID)
	{
	    //prevent any out-of-range thread from overwriting
	    if (id.x >= gridSize || id.y >= gridSize)
	    {
	        return;
	    }
	    
	    uint index = id.y * gridSize + id.x;
	    float yStep = (float) id.y * step - halfScale;
	    float uvY = 1.0 - (float) id.y / (float) gridLength;
	    
	    vertices[index] = float3((float) id.x * step - halfScale, 0.0, yStep);
	    uvs[index] = float2(1.0 - (float) id.x / (float) gridLength, uvY);
	    normals[index] = float3(0.0, 1.0, 0.0);
	}
	
	[numthreads(8, 8, 1)]
	void ComputeTriangles(uint3 id : SV_DispatchThreadID)
	{
	    //prevent any out-of-range thread from overwriting
	    if (id.x >= gridLength || id.y >= gridLength)
	    {
	        return;
	    }
	    
	    uint triangleIndex = (id.y * gridLength + id.x) * 6;
	    uint yGridSize = id.y * gridSize;
	    uint topLeft = yGridSize + id.x;
	    uint topRight = topLeft + 1;
	    uint bottomLeft = topLeft + gridSize;
	    uint bottomRight = bottomLeft + 1;
	
	    triangles[triangleIndex] = topLeft;
	    triangles[triangleIndex + 1] = bottomLeft;
	    triangles[triangleIndex + 2] = topRight;
	
	    triangles[triangleIndex + 3] = topRight;
	    triangles[triangleIndex + 4] = bottomLeft;
	    triangles[triangleIndex + 5] = bottomRight;
	}
  
***
### Twitter: [@aelstraz](https://x.com/Aelstraz)
