using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;


public class WriteBonesToTXT : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var skinnedrenderer=GetComponentInChildren<SkinnedMeshRenderer>();

        string path=Application.dataPath+"/boneIndex.txt";
        FileInfo fileInfo=new FileInfo(path);

        StreamWriter writter=fileInfo.CreateText();

        for(int i=0;i<skinnedrenderer.bones.Length;++i){
            
            writter.WriteLine(skinnedrenderer.bones[i].name);
            
        }
        writter.Close();
        writter.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
