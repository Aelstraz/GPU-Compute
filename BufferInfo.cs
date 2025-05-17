using System;
using UnityEngine;

/// <summary>
/// A struct to hold information about a compute buffer
/// </summary>
public struct BufferInfo
{
    public string name;
    public ComputeBuffer buffer;
    public Type dataType;

    /// <summary>
    /// Constructor for BufferInfo
    /// </summary>
    /// <param name="name">The name of the buffer</param>
    /// <param name="buffer">The compute buffer</param>
    /// <param name="dataType">The data type of the buffer</param>
    public BufferInfo(string name, ComputeBuffer buffer, Type dataType)
    {
        this.name = name;
        this.buffer = buffer;
        this.dataType = dataType;
    }
}