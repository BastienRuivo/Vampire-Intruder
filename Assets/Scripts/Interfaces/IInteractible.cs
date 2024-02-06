using UnityEngine;

namespace Interfaces
{
    public interface IInteractible
    {
        /// <summary>
        /// Interaction between a game object and another object.
        /// </summary>
        /// <param name="interactor">the game object calling the interaction.</param>
        public void Interact(GameObject interactor);
    }
}