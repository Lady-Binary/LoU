using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LoU
{
    [System.Serializable]
    public class TileTransform
    {
        public string ParentName;
        public string Name;
        public float x;
        public float y;
        public float z;
    }

    class MapExporter
    {
        public static void ExportTexture2D(Texture2D Texture, string mapDirectory)
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

        public static void ExportTransform(Transform transform, string mapDirectory)
        {
            if (transform.parent == null || transform.localPosition == null)
                return;

            TileTransform tileTransform = new TileTransform()
            {
                ParentName = transform.parent.name,
                Name = transform.name,
                x = transform.localPosition.x,
                y = transform.localPosition.y,
                z = transform.localPosition.z
            };

            string serializedTransform = JsonUtility.ToJson(tileTransform);

            string exportName = transform.parent.name.ToString().Replace("Minimap", "");
            switch (transform.name)
            {
                case "Quad 1.1":
                    exportName += "_0";
                    break;
                case "Quad 1.-1":
                    exportName += "_1";
                    break;
                case "Quad -1.1":
                    exportName += "_2";
                    break;
                case "Quad -1.-1":
                    exportName += "_3";
                    break;
            }
            exportName += ".json";

            string exportFullName = Path.Combine(mapDirectory, exportName);

            if (File.Exists(exportFullName))
            {
                File.Delete(exportFullName);
            }

            File.WriteAllText(exportFullName, serializedTransform);
            System.Diagnostics.Debug.WriteLine(serializedTransform.Length + "b was saved as: " + exportFullName);
        }
    }
}
