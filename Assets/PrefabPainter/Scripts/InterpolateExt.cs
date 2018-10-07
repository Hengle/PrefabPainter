using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{

    /// <summary>
    /// Extended version of Interpolate which supports ControlPoint classes instead of Transform classes.
    /// 
    /// Note: had to make NewCatmullRom method in super class protected in order to access it.
    /// </summary>
    public class InterpolateExt : Interpolate
    {
        public static IEnumerable<Vector3> NewCatmullRom(ControlPoint[] nodes, int slices, bool loop)
        {
            return NewCatmullRom<ControlPoint>(nodes, ControlPointDotPosition, slices, loop);
        }

        static Vector3 ControlPointDotPosition(ControlPoint controlPoint)
        {
            return controlPoint.position;
        }

    }
}
