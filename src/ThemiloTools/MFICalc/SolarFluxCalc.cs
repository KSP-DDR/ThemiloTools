using UnityEngine;
using Logger = Kopernicus.Logger;
using KSP.Localization;
using ModularFI;

namespace ThemiloTools
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SolarFluxCalc : MonoBehaviour
    {
        public string fluxMsg = "Solar Flux for " + FlightGlobals.ActiveVessel.GetCurrentOrbit().referenceBody + "is " + ModularFlightIntegrator.ActiveVesselFI.solarFlux + " Watts/m^2";
        public string eclipseMsg = "You are not in direct Sunlight!  Plase wait until day to measure again.";

        void Update()
        {
            if ((Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V)) && FlightGlobals.ActiveVessel.directSunlight)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format(fluxMsg));
            }
            else
            {
                ScreenMessages.PostScreenMessage(Localizer.Format(eclipseMsg));
            }
        }
    }
}
