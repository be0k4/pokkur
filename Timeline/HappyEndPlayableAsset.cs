using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class HappyEndPlayableAsset : PlayableAsset
{
    public AudioClip endingClip;
    public ExposedReference<Camera> mainCamera;

    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var behaviour = new HappyEndPlayableBehaviour();
        behaviour.endingClip = this.endingClip;
        behaviour.mainCamera = this.mainCamera.Resolve(graph.GetResolver());
        var playable = ScriptPlayable<HappyEndPlayableBehaviour>.Create(graph, behaviour);
        return playable;
    }
}
