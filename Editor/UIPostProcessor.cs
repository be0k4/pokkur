using UnityEditor;
using UnityEngine;

public class UIPostProcessor : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        string lowerCaseAssetPath = this.assetPath.ToLower();

        //Pictures/UIフォルダ下のテクスチャファイルのみが対象
        if (lowerCaseAssetPath.Contains("pictures/ui") is false)
        {
            Debug.Log("これは対象外です");
            return;
        }
        else
        {
            TextureImporter importer = this.assetImporter as TextureImporter;
            //初回インポート時のみが対象
            if (importer.importSettingsMissing is false) return;
            importer.spritePixelsPerUnit = 1;
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Trilinear;
        }
    }
}
