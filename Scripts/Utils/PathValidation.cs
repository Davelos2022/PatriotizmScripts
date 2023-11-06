using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VolumeBox.Toolbox;

public static class PathValidation
{
    public static bool IsValidPath(this string path)
    {
        return path.IsValuable() && (Directory.Exists(path) || File.Exists(path));
    }
}
