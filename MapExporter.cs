using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LoU
{
    class MapExporter
    {

        public static void ExportTexture(Texture2D Texture, string mapDirectory)
        {

            string exportFullName =  Path.Combine(mapDirectory, Texture.name + ".jpg");

            // The games Texture2D objects have isReadable set to false so we need to copy them into a temporary object
            // with isReadable set to true so ImageConversion.EncodeToJPG can read them.

            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                Texture.width,
                                Texture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(Texture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D readableTexture = new Texture2D(Texture.width, Texture.height);

            // Copy the pixels from the RenderTexture to the new Texture
            readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTexture.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            byte[] _bytes = ImageConversion.EncodeToJPG(readableTexture, 50);

            if (File.Exists(exportFullName))
            {
                File.Delete(exportFullName);
            }

            File.WriteAllBytes(exportFullName, _bytes);
            System.Diagnostics.Debug.WriteLine(_bytes.Length / 1024 + "Kb was saved as: " + exportFullName);
        }

        public static void ExportPrefab(GameObject Prefab, string mapDirectory)
        {

            List<GameObject> children = Prefab.GetAllChildrenIncludingSelf();

            for (int i = 0; i < children.Count; i++)
            {
                if (i > 0)
                {
                    Transform transform = children[i].transform;

                    var localPositionDictionary = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["X"] = transform.localPosition.x,
                        ["Y"] = transform.localPosition.y,
                        ["Z"] = transform.localPosition.z
                    };
                    var localRotationDictionary = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["X"] = transform.localRotation.x,
                        ["Y"] = transform.localRotation.y,
                        ["Z"] = transform.localRotation.z,
                        ["W"] = transform.localRotation.w
                    };
                    var localScaleDictionary = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["X"] = transform.localScale.x,
                        ["Y"] = transform.localScale.y,
                        ["Z"] = transform.localScale.z,
                    };

                    var transformDictionary = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["PathID"] = transform.name.ToString(),
                        ["LocalPosition"] = localPositionDictionary,
                        ["LocalRotation"] = localRotationDictionary,
                        ["LocalScale"] = localScaleDictionary
                    };


                    string serializedTransform = JsonConvert.SerializeObject(transformDictionary);

                    string exportFullName = Path.Combine(mapDirectory, Prefab.name.ToString().Replace("Minimap", "") + "_" + (i - 1) + ".transform");

                    if (File.Exists(exportFullName))
                    {
                        File.Delete(exportFullName);
                    }

                    File.WriteAllText(exportFullName, serializedTransform);
                    System.Diagnostics.Debug.WriteLine(serializedTransform.Length + "b was saved as: " + exportFullName);
                }
            }


        }

    }
}
