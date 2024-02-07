using UnityEditor;
using UnityEngine;

public class UIPostProcessor : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        string lowerCaseAssetPath = this.assetPath.ToLower();

        //Pictures/UI�t�H���_���̃e�N�X�`���t�@�C���݂̂��Ώ�
        if (lowerCaseAssetPath.Contains("pictures/ui") is false)
        {
            Debug.Log("����͑ΏۊO�ł�");
            return;
        }
        else
        {
            TextureImporter importer = this.assetImporter as TextureImporter;
            //����C���|�[�g���݂̂��Ώ�
            if (importer.importSettingsMissing is false) return;
            importer.spritePixelsPerUnit = 1;
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Trilinear;
        }
    }
}
