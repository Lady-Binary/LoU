using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LoU
{
    [System.Serializable]
    public class Tile
    {
        public string Name;
        public float x;
        public float y;
        public float z;
    }

    class MapExporter
    {
        public static void ExportTileProps(Tile tile, string exportFullName)
        {
            string serializedTile = JsonUtility.ToJson(tile);
            if (File.Exists(exportFullName))
                File.Delete(exportFullName);
            File.WriteAllText(exportFullName, serializedTile);
        }

        public static void ExportTexture2D(Texture2D Texture, string exportFullName)
        {
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
                File.Delete(exportFullName);

            File.WriteAllBytes(exportFullName, _bytes);
            System.Diagnostics.Debug.WriteLine(_bytes.Length / 1024 + "Kb was saved as: " + exportFullName);
        }

        public static void ExportTile(Renderer renderer, string mapDirectory)
        {
            if (renderer == null)
                return;

            var texture = renderer?.material?.mainTexture as Texture2D;
            if (texture == null)
                return;

            Tile tile = new Tile()
            {
                Name = texture.name,
                x = renderer.bounds.center.x,
                y = renderer.bounds.center.y,
                z = renderer.bounds.center.z
            };

            var exportFullName = Path.Combine(mapDirectory, texture.name + "_" + renderer.name);
            try
            {
                ExportTileProps(tile, exportFullName + ".json");
                ExportTexture2D(texture, exportFullName + ".jpg");
            } catch(Exception ex)
            {
                Utils.Log($"Cannot export tile {exportFullName}:");
                Utils.Log(ex.ToString());
            }
        }
    }
}
