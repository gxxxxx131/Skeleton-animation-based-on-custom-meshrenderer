# Skeleton-animation-based-on-custom-meshrenderer
Make meshrenderer behave as skinnedmeshrenderer by using computeshader.

--------

Firstly, read bone information from skinnedmeshrenderer and write it to a txt file.
```csharp
for(int i=0;i<skinnedrenderer.bones.Length;++i){
            
  writter.WriteLine(skinnedrenderer.bones[i].name);
            
}
```
<br>
Then remove the Skinnedmeshrenderer and replace with meshrRenderer and meshfilter component.<br>
Finally,attach the GetBoneIndex csharp file to the parent of meshbody and armature. You will get like this.<br>

![](https://github.com/gxxxxx131/Skeleton-animation-based-on-custom-meshrenderer/raw/master/show.gif)

`Note`:This project is just for learning and understanding skeleton animation.
