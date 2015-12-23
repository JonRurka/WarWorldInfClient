using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNoise;


public static class GradientCreator {
    public static Dictionary<string, Color[]> TextureFiles = new Dictionary<string, Color[]>();

    public static Gradient CreateGradientClient(List<GradientPresets.GradientKeyData> keyData) {
        List<GradientKey> keys = new List<GradientKey>();
        for (int i = 0; i < keyData.Count; i++) {
            if (keyData[i].isImage) {
                List<Color[]> images = new List<Color[]>();
                string[] files = keyData[i].imageFiles.ToArray();
                for (int j = 0; j < files.Length; j++) {
                    string file = files[j];
                    if (TextureFiles.ContainsKey(file)) {
                        images.Add(TextureFiles[file]);
                    }
                }
                keys.Add(new GradientKey(images, 10, 10, keyData[i].time));
            }
            else {

                keys.Add(new GradientKey(keyData[i].color, keyData[i].time));
            }
        }
        return GradientPresets.CreateGradient(keys);
    }
}

