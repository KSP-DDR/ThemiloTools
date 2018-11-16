using System;
using System.Collections.Generic;
using UnityEngine;


namespace GeOdyssey
{
    public class ModuleDeployableAntiSolarPanel : ModuleDeployableSolarPanel
    {
        private void Awake()
        {
            //find stars
            List<CelestialBody> sunBodies = new List<CelestialBody>();
            sunBodies.Add(FlightGlobals.Bodies.Find(s => s.name == "Sun"));
            
        }

    }
}
