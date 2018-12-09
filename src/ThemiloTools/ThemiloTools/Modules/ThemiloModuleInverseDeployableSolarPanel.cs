using System.Reflection;
using UnityEngine;
using System;
using ThemiloTools.Components;
using ModularFI;
using System.Linq;

namespace ThemiloTools.Modules
{
    /// <summary>
    /// The less light there is, the more power you get! Multi-Star Support based on KopernicusSolarPanel by StollD
    /// </summary>
    public class ThemiloModuleInverseDeployableSolarPanel : ModuleDeployableSolarPanel
    {
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Tracking Body", isPersistant = true)]
        public String trackingBodyName;

        [KSPField(isPersistant = true)]
        private Boolean manualTracking;

        [KSPField(isPersistant = true)]
        private Boolean relativeSunAOA;

        public static Double stockLuminosity;

        static ThemiloModuleInverseDeployableSolarPanel()
        {
            String filename = (String)typeof(PhysicsGlobals).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(f => f.FieldType == typeof(String)).GetValue(PhysicsGlobals.Instance);
            ConfigNode node = ConfigNode.Load(filename);
            String value = node.GetValue("solarLuminosityAtHome");
            if (value != null)
            {
                stockLuminosity = Double.Parse(value);
            }
        }

        public override void PostCalculateTracking(Boolean trackingLOS, Vector3 trackingDirection)
        {
            // Maximum values
            Double maxEnergy = 0;
            Double maxFlowRate = 0;
            ThemiloStar maxStar = null;

            // Override layer mask
            planetLayerMask = ModularFlightIntegrator.SunLayerMask;

            // Efficiency modifier
            _efficMult = (temperatureEfficCurve.Evaluate((Single)part.skinTemperature) * timeEfficCurve.Evaluate((Single)((Planetarium.GetUniversalTime() - launchUT) * 1.15740740740741E-05)) * efficiencyMult);
            _flowRate = 0;
            sunAOA = 0;

            // Go through all stars
            foreach (ThemiloStar star in ThemiloStar.Stars)
            {
                // Calculate stuff
                Vector3 trackDir = (star.sun.transform.position - panelRotationTransform.position).normalized;
                CelestialBody old = trackingBody;
                trackingTransformLocal = star.sun.transform;
                trackingTransformScaled = star.sun.scaledBody.transform;
                trackingLOS = CalculateTrackingLOS(trackDir, ref blockingObject);
                trackingTransformLocal = old.transform;
                trackingTransformScaled = old.scaledBody.transform;

                // Calculate sun AOA
                Single _sunAOA = 0f;
                if (!trackingLOS)
                {
                    _sunAOA = 0f;
                    status = "Blocked by " + blockingObject;
                }
                else
                {
                    status = "Direct Sunlight";
                    if (panelType == PanelType.FLAT)
                    {
                        _sunAOA = Mathf.Clamp(Vector3.Dot(trackingDotTransform.forward, trackDir), 0f, 1f);
                    }
                    else if (panelType != PanelType.CYLINDRICAL)
                    {
                        _sunAOA = 0.25f;
                    }
                    else
                    {
                        Vector3 direction;
                        if (alignType == PanelAlignType.PIVOT)
                        {
                            direction = trackingDotTransform.forward;
                        }
                        else if (alignType != PanelAlignType.X)
                        {
                            direction = alignType != PanelAlignType.Y ? part.partTransform.forward : part.partTransform.up;
                        }
                        else
                        {
                            direction = part.partTransform.right;
                        }
                        _sunAOA = (1f - Mathf.Abs(Vector3.Dot(direction, trackDir))) * 0.318309873f;
                    }
                }

                // Calculate distance multiplier
                Double __distMult = 1;
                if (!useCurve)
                {
                    if (!ThemiloStar.SolarFlux.ContainsKey(star.name))
                        continue;
                    __distMult = (Single)(ThemiloStar.SolarFlux[star.name] / stockLuminosity);
                }
                else
                {
                    __distMult = powerCurve.Evaluate((star.sun.transform.position - panelRotationTransform.position).magnitude);
                }

                // Calculate flow rate
                Double __flowRate = _sunAOA * _efficMult * __distMult;
                if (part.submergedPortion > 0)
                {
                    Double altitudeAtPos = -FlightGlobals.getAltitudeAtPos((Vector3d)secondaryTransform.position, vessel.mainBody);
                    altitudeAtPos = (altitudeAtPos * 3 + part.maxDepth) * 0.25;
                    if (altitudeAtPos < 0.5)
                    {
                        altitudeAtPos = 0.5;
                    }
                    Double num = 1 / (1 + altitudeAtPos * part.vessel.mainBody.oceanDensity);
                    if (part.submergedPortion >= 1)
                    {
                        __flowRate = __flowRate * num;
                    }
                    else
                    {
                        __flowRate = __flowRate * UtilMath.LerpUnclamped(1, num, part.submergedPortion);
                    }
                    status += ", Underwater";
                }
                sunAOA += _sunAOA;
                Double energy = __distMult * _efficMult;
                if (energy > maxEnergy)
                {
                    maxFlowRate = __flowRate;
                    maxEnergy = energy;
                    maxStar = star;
                }

                // Apply the flow rate
                _flowRate += __flowRate;
            }

            // Sun AOA
            sunAOA /= relativeSunAOA ? ThemiloStar.Stars.Count : 1;
            _distMult = _flowRate != 0 ? _flowRate / _efficMult / sunAOA : 0;

            // We got the best star to use
            if (maxStar != null && maxStar.sun != trackingBody)
            {
                if (!manualTracking)
                {
                    trackingBody = maxStar.sun;
                    GetTrackingBodyTransforms();
                }
            }

            // Use the flow rate
            flowRate = (Single)(resHandler.UpdateModuleResourceOutputs(_flowRate) * flowMult);
        }

        public override void LateUpdate()
        {
            // Update the name
            trackingBodyName = trackingBody.bodyDisplayName.Replace("^N", "");

            // Update the guiName for SwitchAOAMode
            if (relativeSunAOA)
            {
                Events["SwitchAOAMode"].guiName = "Use absolute exposure";
            }
            else
            {
                Events["SwitchAOAMode"].guiName = "Use relative exposure";
            }

            base.LateUpdate();
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Select Tracking Body")]
        public void ManualTracking()
        {
            // Assemble the buttons
            DialogGUIBase[] options = new DialogGUIBase[ThemiloStar.Stars.Count + 1];
            options[0] = new DialogGUIButton("Auto", () => { manualTracking = false; }, true);
            for (Int32 i = 0; i < ThemiloStar.Stars.Count; i++)
            {
                CelestialBody body = ThemiloStar.Stars[i].sun;
                options[i + 1] = new DialogGUIButton(body.bodyDisplayName.Replace("^N", ""), () => { manualTracking = true; trackingBody = body; GetTrackingBodyTransforms(); }, true);
            }

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog(
                "SelectTrackingBody",
                "Please select the Body you want to track with this Solar Panel.",
                "Select Tracking Body",
                UISkinManager.GetSkin("MainMenuSkin"),
                options), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Use relative exposure")]
        public void SwitchAOAMode()
        {
            relativeSunAOA = !relativeSunAOA;
        }
    }
}
