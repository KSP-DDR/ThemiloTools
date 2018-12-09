using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ThemiloTools
{
    public class KopWrapper
    {
        static Assembly kopAssembly;
        static Assembly kopComponentsAssembly;
        public Type sopan;
        public Type star;
        public Type flare;
        public static bool kopInstalled;
        public static PartModule mod;

        public KopWrapper()
        {
            if (kopAssembly == null || kopComponentsAssembly == null)
            {
                foreach (AssemblyLoader.LoadedAssembly la in AssemblyLoader.loadedAssemblies)
                {
                    if (la.name == "Kopernicus")
                    {
                        kopAssembly = la.assembly;
                        kopInstalled = true;
                    }
                    else if (la.name == "Kopernicus.Components")
                    {
                        kopComponentsAssembly = la.assembly;
                        kopInstalled = true;
                    }
                    else
                    {
                        Debug.Log("[ThemiloTools] Kopernicus doesnt seem to be Installed");
                        kopInstalled = false;
                        return;
                    }

                }
            }

            sopan = kopComponentsAssembly.GetType("KopernicusSolarPanel");
            star = kopComponentsAssembly.GetType("KopernicusStar");
            flare = kopComponentsAssembly.GetType("KopernicusSunFlare");
        }
    }
}
