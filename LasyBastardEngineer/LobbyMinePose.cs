using UnityEngine;

namespace LasyBastardEngineer
{
    internal sealed class LobbyMinePose : MonoBehaviour
    {
        private Transform mineHolder;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        private void OnEnable()
        {
            // This component is on MineHolder/ProximityMine/EngiMineMesh.
            mineHolder = transform.parent.parent;
            originalPosition = mineHolder.localPosition;
            originalRotation = mineHolder.localRotation;

			// These values where found using UnityExplorer, and may not be perfect
            mineHolder.localPosition = new Vector3(-0.072f, -0.0626f, 0.001f);
            mineHolder.localRotation = Quaternion.Euler(355.7697f, 0.9996f, 358.8732f);
        }

        private void OnDisable()
        {
            // Changing skin or selecting spider mines hides this display and restores the shared holder.
            if (!mineHolder)
                return;

            mineHolder.localPosition = originalPosition;
            mineHolder.localRotation = originalRotation;
        }
    }
}
