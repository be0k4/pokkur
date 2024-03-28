using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
public class BadEndPlayableAsset : PlayableAsset
{
    //シーン上のオブジェクト参照
    public ExposedReference<PostProcessVolume> postProcessing;
    public ExposedReference<Camera> mainCamera;
    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var behaviour = new BadEndPlayableBehaviour();
        behaviour.postProcessing = this.postProcessing.Resolve(graph.GetResolver());
        behaviour.mainCamera = this.mainCamera.Resolve(graph.GetResolver());
        var playable = ScriptPlayable<BadEndPlayableBehaviour>.Create(graph, behaviour);
        return playable;
    }
}
