using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRPortalToolkit.XRI
{
    /// <summary>
    /// Portal-aware poke interactor that supports interacting through portals.
    /// </summary>
    public class XRPortablePokeInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor, IXRPortableInteractor
    {
        /// <summary>
        /// Gets the portals needed to travel to the specified interactable.
        /// </summary>
        /// <param name="interactable">The XR interactable.</param>
        /// <returns>An enumerable of portals.</returns>
        public IEnumerable<Portal> GetPortalsToInteractable(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable interactable)
        {
            yield break;
        }
    }
}
