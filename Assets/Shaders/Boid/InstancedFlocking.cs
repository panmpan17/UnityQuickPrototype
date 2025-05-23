﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedFlocking : MonoBehaviour
{
    public struct Boid
    {
        public Vector3 position;
        public Vector3 direction;
        public float noise_offset;

        public Boid(Vector3 pos, Vector3 dir, float offset)
        {
            position.x = pos.x;
            position.y = pos.y;
            position.z = pos.z;
            direction.x = dir.x;
            direction.y = dir.y;
            direction.z = dir.z;
            noise_offset = offset;
        }
    }

    public ComputeShader shader;

    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;
    public Mesh boidMesh;
    public Material boidMaterial;
    public int boidsCount;
    public float spawnRadius;
    public Transform target;

    int kernelHandle;
    ComputeBuffer boidsBuffer;
    Boid[] boidsArray;
    int groupSizeX;
    int numOfBoids;
    RenderParams renderParams;
    GraphicsBuffer argsBuffer;

    void Start()
    {
        kernelHandle = shader.FindKernel("CSMain");

        uint x;
        shader.GetKernelThreadGroupSizes(kernelHandle, out x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount / (float)x);
        numOfBoids = groupSizeX * (int)x;

        InitBoids();
        InitShader();

        renderParams = new RenderParams(boidMaterial);
        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
    }

    private void InitBoids()
    {
        boidsArray = new Boid[numOfBoids];

        for (int i = 0; i < numOfBoids; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
            float offset = Random.value * 1000.0f;
            boidsArray[i] = new Boid(pos, rot.eulerAngles, offset);
        }
    }

    void InitShader()
    {
        boidsBuffer = new ComputeBuffer(numOfBoids, 7 * sizeof(float));
        boidsBuffer.SetData(boidsArray);


        Debug.Log($"subMeshCount: {boidMesh.subMeshCount}");
        Debug.Log($"boidMesh.GetIndexCount(0): {boidMesh.GetIndexCount(0)}");
        // NOTES: Never try this before
        argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        GraphicsBuffer.IndirectDrawIndexedArgs[] data = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        data[0].indexCountPerInstance = boidMesh.GetIndexCount(0);
        data[0].instanceCount = (uint)numOfBoids;
        argsBuffer.SetData(data);

        shader.SetFloat("rotationSpeed", rotationSpeed);
        shader.SetFloat("boidSpeed", boidSpeed);
        shader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        shader.SetVector("flockPosition", target.transform.position);
        shader.SetFloat("neighbourDistance", neighbourDistance);
        shader.SetInt("boidsCount", numOfBoids);
        shader.SetBuffer(this.kernelHandle, "boidsBuffer", boidsBuffer);

        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);
    }

    void Update()
    {
        shader.SetFloat("time", Time.time);
        shader.SetFloat("deltaTime", Time.deltaTime);

        shader.Dispatch(this.kernelHandle, groupSizeX, 1, 1);

        Graphics.RenderMeshIndirect( renderParams, boidMesh, argsBuffer );
    }

    void OnDestroy()
    {
        if (boidsBuffer != null)
        {
            boidsBuffer.Dispose();
        }

        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
        }
    }
}

