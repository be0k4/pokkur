[System.Serializable]
public class ConfigData : ISavable
{
    public float uiVolume;
    public float bgmVolume;
    public bool isFullScreen;
    public int width;
    public int height;
    public int qualityLevel;

    public ConfigData(float uiVolume, float bgmVolume, bool isFullScreen, int width, int height, int qualityLevel)
    {
        this.uiVolume = uiVolume;
        this.bgmVolume = bgmVolume;
        this.isFullScreen = isFullScreen;
        this.width = width;
        this.height = height;
        this.qualityLevel = qualityLevel;
    }
}
