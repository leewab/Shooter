// using UnityEngine;
// using System.IO;
//
// public class GenerateLightTexture : MonoBehaviour
// {
//     [Header("纹理参数（保持默认就是超轻度）")]
//     public int textureSize = 256;
//     public float dotDensity = 0.1f; // 网点密度，越小纹理越轻
//     public float dotBrightness = 0.8f; // 网点亮度，接近1就是淡灰色
//
//     [ContextMenu("生成超轻度网点纹理")]
//     public void Generate()
//     {
//         Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
//         tex.wrapMode = TextureWrapMode.Repeat;
//         tex.filterMode = FilterMode.Point;
//
//         // 填充纹理：纯白基底 + 极淡灰色网点（超轻度，完全不干扰主色）
//         for (int y = 0; y < textureSize; y++)
//         {
//             for (int x = 0; x < textureSize; x++)
//             {
//                 // 随机生成极淡的网点，大部分区域是纯白
//                 float random = Random.value;
//                 Color color = Color.white;
//                 if (random < dotDensity)
//                 {
//                     color = new Color(dotBrightness, dotBrightness, dotBrightness);
//                 }
//                 tex.SetPixel(x, y, color);
//             }
//         }
//
//         tex.Apply();
//
//         // 保存到工程目录（Assets/Textures 下，没有会自动创建）
//         byte[] pngData = tex.EncodeToPNG();
//         string path = "Assets/Textures/LightDotTexture.png";
//         Directory.CreateDirectory(Path.GetDirectoryName(path));
//         File.WriteAllBytes(path, pngData);
//
//         // 刷新Unity资源
//         UnityEditor.AssetDatabase.Refresh();
//
//         Debug.Log("超轻度纹理生成成功！路径：" + path);
//     }
// }