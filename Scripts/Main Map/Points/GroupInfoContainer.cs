using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using VolumeBox.Toolbox;

public class GroupInfoContainer : CachedSingleton<GroupInfoContainer>
{
    [SerializeField] private GroupInfo[] infos;

    public static string GetName(Group group)
    {
        return Instance.infos.FirstOrDefault(x => x.group == group).name;
    }

    public static GroupInfo GetInfo(Group group)
    {
        return Instance.infos.FirstOrDefault(x => x.group == group);
    }

    public static Gradient GetGradient(Group group)
    {
        return Instance.infos.FirstOrDefault(x => x.group == group).gradient;
    }
}

[Serializable]
public class GroupInfo
{
    public string name;
    public Group group;
    public Gradient gradient;
    public Sprite backgroundDot;
    public Color shadowColor;
    public Color fontColor;
}
