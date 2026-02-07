using UnityEngine;
using UnityEditor;
using System.IO;

public class QStyleTextureGenerator
{
    [MenuItem("Tools/Q弹风格/生成所有材质球")]
    public static void GenerateAllQStyleMaterials()
    {
        GenerateBaseTexture();
        
        GenerateQStyleMaterial("Red", new Color(0.9f, 0.2f, 0.2f, 1.0f));
        GenerateQStyleMaterial("Green", new Color(0.2f, 0.8f, 0.2f, 1.0f));
        GenerateQStyleMaterial("Yellow", new Color(0.9f, 0.8f, 0.2f, 1.0f));
        GenerateQStyleMaterial("Blue", new Color(0.2f, 0.5f, 0.9f, 1.0f));
        
        AssetDatabase.Refresh();
        Debug.Log("所有Q弹风格材质球已生成！");
    }
    
    private static Texture2D GenerateBaseTexture()
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Color[] colors = new Color[textureSize * textureSize];
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float nx = (float)x / textureSize;
                float ny = (float)y / textureSize;
                
                float noise = Mathf.PerlinNoise(nx * 8.0f, ny * 8.0f) * 0.1f;
                float gradient = 1.0f - ny * 0.3f;
                
                float finalValue = gradient + noise;
                finalValue = Mathf.Clamp01(finalValue);
                
                Color color = Color.white * finalValue;
                color.a = 1.0f;
                
                colors[y * textureSize + x] = color;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        string texturePath = $"Assets/Resources/Product/Game/Texture/QStyle_Base.png";
        string directory = Path.GetDirectoryName(texturePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        
        Debug.Log("Q弹风格基础贴图已生成！");
        return texture;
    }
    
    private static void GenerateQStyleMaterial(string colorName, Color baseColor)
    {
        string texturePath = $"Assets/Resources/Product/Game/Texture/QStyle_Base.png";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        
        if (texture == null)
        {
            Debug.LogError($"无法加载基础贴图: {texturePath}");
            return;
        }
        
        Material material = new Material(Shader.Find("Custom/QStyleFresnel"));
        material.SetTexture("_MainTex", texture);
        material.SetColor("_Color", baseColor);
        material.SetFloat("_Alpha", 0.95f);
        material.SetFloat("_FresnelPower", 4.0f);
        material.SetFloat("_FresnelIntensity", 0.5f);
        material.SetFloat("_FresnelSoftness", 0.4f);
        material.SetFloat("_MinAlpha", 0.5f);
        
        Color fresnelColor = baseColor * 1.1f;
        fresnelColor.a = 0.4f;
        material.SetColor("_FresnelColor", fresnelColor);
        
        Color rimColor = new Color(1.0f, 1.0f, 1.0f, 0.3f);
        material.SetColor("_RimColor", rimColor);
        material.SetFloat("_RimPower", 5.0f);
        material.SetFloat("_RimIntensity", 0.3f);
        
        Color glowColor = baseColor * 0.6f;
        glowColor.a = 0.2f;
        material.SetColor("_GlowColor", glowColor);
        material.SetFloat("_GlowPower", 4.0f);
        material.SetFloat("_GlowIntensity", 0.5f);
        
        string materialPath = $"Assets/Resources/Product/Game/Material/QStyle_{colorName}.mat";
        AssetDatabase.CreateAsset(material, materialPath);
        
        Debug.Log($"Q弹风格材质球已生成: {colorName}");
    }
}
