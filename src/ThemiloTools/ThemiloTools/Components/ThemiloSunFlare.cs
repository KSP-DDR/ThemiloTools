using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ThemiloTools.Components
{
    public class ThemiloSunFlare : SunFlare
    {
        protected override void Awake()
        {
            // sun flare
            Camera.onPreCull += cam =>
            {
                Vector3d scaledSpace = target.transform.position - ScaledSpace.LocalToScaledSpace(sun.position);
                sunDirection = scaledSpace.normalized;
                if (sunDirection != Vector3d.zero)
                    transform.forward = sunDirection;
            };
        }
    }
}
