using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThemiloTools
{
    public class Utils
    {
        public static string GameDataPath
        {
            get
            {
                return KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/";
            }
        }
    }
}
