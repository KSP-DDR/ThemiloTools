using UnityEngine;
using KSP.Localization;
using ModularFI;

namespace ThemiloTools
{
    /// <summary>
    /// This class queries the solar flux at the active veesel's position and prints the result using a ScreenMessage.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SolarFluxCalc : MonoBehaviour
    {
        /// <summary>
        /// These are the messages to print. One prints the value, the other says to wait until daytime.
        /// </summary>
        public string fluxMsg1 = "Solar Flux for ";
        public string Is = "is ";
        public string units = " Watts/m^2";
        public string eclipseMsg = "You are not in direct Sunlight!  Plase wait until day to measure again.";
        public string finalMsg;

        /// <summary>
        /// This prints the value
        /// </summary>
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                finalMsg = string.Format(fluxMsg1, FlightGlobals.ActiveVessel.GetCurrentOrbit().referenceBody, Is, (ModularFlightIntegrator.ActiveVesselFI.solarFlux * ModularFlightIntegrator.ActiveVesselFI.solarFluxMultiplier), units);
                Debug.Log("[MFICALC]: Post Message!");
                ScreenMessages.PostScreenMessage(Localizer.Format(finalMsg));
            }
            else if (!FlightGlobals.ActiveVessel.directSunlight)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format(eclipseMsg));
            }
        }
    }
}
