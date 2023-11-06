using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
    {
        Vector2 AP = P - A;       //Vector from A to P   
        Vector2 AB = B - A;       //Vector from A to B  

        float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
        float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
        float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

        if (distance < 0)     //Check if P projection is over vectorAB     
        {
            return A;

        }
        else if (distance > 1)
        {
            return B;
        }
        else
        {
            return A + AB * distance;
        }
    }

    public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize();//this needs to be a unit vector
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }

    public static Vector3 GetRectangleNormal(Vector3 aRectCenter, Vector2 aRectSize, Quaternion aRectRot, Vector3 aPosition)
    {
        Vector3 vec = aPosition - aRectCenter;
        vec = Quaternion.Inverse(aRectRot) * vec;
        vec.y *= aRectSize.x / aRectSize.y;
        Vector3 normal;
        if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
            normal = Vector3.right * Mathf.Sign(vec.x);
        else
            normal = Vector3.up * Mathf.Sign(vec.y);
        return aRectRot * normal;
    }
}
