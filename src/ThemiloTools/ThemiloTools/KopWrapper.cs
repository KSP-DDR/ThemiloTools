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

        public KopWrapper()
        {
            if (kopAssembly == null || kopComponentsAssembly == null)
            {
                foreach (AssemblyLoader.LoadedAssembly la in AssemblyLoader.loadedAssemblies)
                {
                    if (la.name == "Kopernicus")
                    {
                        kopAssembly = la.assembly;
                    }
                    else if (la.name == "Kopernicus.Components")
                    {
                        kopComponentsAssembly = la.assembly;
                    }
                    else
                    {
                        Debug.Log("[ThemiloTools] Kopernicus doesnt seem to be Installed");
                        return;
                    }

                }
            }
            
        }
    }
}
