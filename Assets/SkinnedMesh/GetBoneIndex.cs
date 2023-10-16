using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;


public class GetBoneIndex : MonoBehaviour
{
    public struct BoneInfo{
        public int index;
        public Matrix4x4 bindpose;

    };

    Mesh mesh;
    Dictionary<string,BoneInfo> boneNameAndIndex;
    string[] boneName;

    Matrix4x4[] finalBoneMatrix;
    Matrix4x4[] finalBoneNormalMatrix;

    public Transform root;
    public ComputeShader skinnedShader;

    int kernelID;
    GraphicsBuffer bufverices;
    GraphicsBuffer bufweights;
    GraphicsBuffer bonebuf;
    GraphicsBuffer bufpos;
    GraphicsBuffer bufNormal;
    GraphicsBuffer bufTangent;
    GraphicsBuffer boneNormalBuf;

    Transform[] transforms;

    // Start is called before the first frame update
    void Start()
    {
        
        ReadBoneNameAndIndex();
        GetBoneTransform(root);

        
        kernelID=skinnedShader.FindKernel("CSMain");
        bufpos=new GraphicsBuffer(GraphicsBuffer.Target.Structured,mesh.vertices.Length,12);
        bufpos.SetData(mesh.vertices);

        bufNormal=new GraphicsBuffer(GraphicsBuffer.Target.Structured,mesh.vertices.Length,12);
        bufNormal.SetData(mesh.normals);

        bufTangent=new GraphicsBuffer(GraphicsBuffer.Target.Structured,mesh.vertices.Length,16);
        bufTangent.SetData(mesh.tangents);

        bufweights=new GraphicsBuffer(GraphicsBuffer.Target.Structured,mesh.vertexCount,4*8);

        bufweights.SetData(mesh.boneWeights);
        skinnedShader.SetBuffer(kernelID,"bufPosition",bufpos);
        skinnedShader.SetBuffer(kernelID,"bufNormal",bufNormal);
        skinnedShader.SetBuffer(kernelID,"bufTangent",bufTangent);
        skinnedShader.SetBuffer(kernelID,"bufWeights",bufweights);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoneTransform();
        SkinnedMesh();
    }

    void ReadBoneNameAndIndex(){
        string path=Application.dataPath+"/boneIndex.txt";
        mesh=GetComponentInChildren<MeshFilter>().mesh;

    
        boneName=File.ReadAllLines(path);

        boneNameAndIndex=new Dictionary<string, BoneInfo>();
        for(int i=0;i<boneName.Length;++i)
        {
            BoneInfo boneInfo=new BoneInfo();
            boneInfo.index=i;
            boneInfo.bindpose=mesh.bindposes[i];
            boneNameAndIndex.Add(boneName[i],boneInfo);
            
        }
        finalBoneMatrix=new Matrix4x4[boneName.Length];
        finalBoneNormalMatrix=new Matrix4x4[boneName.Length];
        for(int i=0;i<finalBoneMatrix.Length;++i){
            finalBoneMatrix[i]=Matrix4x4.identity;
            finalBoneNormalMatrix[i]=Matrix4x4.identity;
        }

        transforms=new Transform[boneName.Length];
        
    }
    void GetBoneTransform(Transform node){
        int childCount=node.childCount;
        BoneInfo boneInfo;
        if(boneNameAndIndex.TryGetValue(node.name,out boneInfo)){
            transforms[boneInfo.index]=node;
            Debug.Log(node.name);
        }

        for(int i=0;i<childCount;++i){
            GetBoneTransform(node.GetChild(i));
        }
    }
    
    void UpdateBoneTransform(){
        foreach (var bone in boneNameAndIndex)
        {
            BoneInfo boneInfo=bone.Value;
            finalBoneMatrix[boneInfo.index]=transforms[boneInfo.index].localToWorldMatrix*boneInfo.bindpose;
            finalBoneNormalMatrix[boneInfo.index]=Matrix4x4.Transpose(Matrix4x4.Inverse(finalBoneMatrix[boneInfo.index]));
            
        }
    }
    void SkinnedMesh(){
        
        bufverices?.Dispose();
        bonebuf?.Dispose();
        boneNormalBuf?.Dispose();

        bufverices=mesh.GetVertexBuffer(0);
        skinnedShader.SetBuffer(kernelID,"bufVertices",bufverices);

        bonebuf=new GraphicsBuffer(GraphicsBuffer.Target.Structured,finalBoneMatrix.Length,4*16);
        bonebuf.SetData(finalBoneMatrix);

        boneNormalBuf=new GraphicsBuffer(GraphicsBuffer.Target.Structured,finalBoneMatrix.Length,4*16);
        boneNormalBuf.SetData(finalBoneNormalMatrix);
        
        skinnedShader.SetBuffer(kernelID,"bufBoneMatrices",bonebuf);
        skinnedShader.SetBuffer(kernelID,"bufBoneNormalMatrices",boneNormalBuf);

        Matrix4x4 worldToLocal=transform.GetComponentInChildren<MeshRenderer>().worldToLocalMatrix;
        skinnedShader.SetMatrix("world2local",worldToLocal);
        
        skinnedShader.Dispatch(kernelID,(mesh.vertexCount+64)/64,1,1);

       
    }

    private void OnDisable() {
        bufverices?.Dispose();
        bufweights?.Dispose();
        bonebuf?.Dispose();
        bufpos?.Dispose();
        boneNormalBuf?.Dispose();
        bufNormal?.Dispose();
        bufTangent?.Dispose();
    }
    
}
