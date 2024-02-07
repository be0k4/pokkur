using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class HappyEndPlayableAsset : PlayableAsset
{
    public AudioClip endingClip;

    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var behaviour = new HappyEndPlayableBehaviour();
        behaviour.endingClip = this.endingClip;
        var playable = ScriptPlayable<HappyEndPlayableBehaviour>.Create(graph, behaviour);
        return playable;
    }
}
