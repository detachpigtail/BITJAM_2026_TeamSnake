using MotionCore2D.Sensing;
using UnityEditor;
using UnityEngine;

namespace MotionCore2D.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="GroundDetector2D"/>. Validates the ground layer mask — the
    /// single most common setup mistake — warning when it is empty (nothing will be detected) or
    /// includes the actor's own layer (the probe detects itself), each with a one-click fix.
    /// </summary>
    [CustomEditor(typeof(GroundDetector2D))]
    public sealed class GroundDetector2DEditor : UnityEditor.Editor
    {
        private SerializedProperty _groundLayers;

        private void OnEnable()
        {
            _groundLayers = serializedObject.FindProperty("_groundLayers");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject go = ((GroundDetector2D)target).gameObject;
            DrawValidations(go);
            MotionInspectorGui.DrawPropertiesExceptScript(serializedObject);
            MotionInspectorGui.DrawDocumentationLinks(
                ("SetupGuide.md", "Setup Guide"),
                ("Troubleshooting.md", "Troubleshooting"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidations(GameObject go)
        {
            int mask = _groundLayers.intValue;
            int ownLayerBit = 1 << go.layer;

            if (mask == 0)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Error,
                    "Ground Layers is empty (Nothing). The probe will never detect ground, so the actor is treated as permanently airborne.",
                    "Set Ground Layers to the layer(s) used by your level geometry, excluding the character's own layer.",
                    "Set to Everything Except This Layer"))
                {
                    _groundLayers.intValue = ~ownLayerBit;
                }
            }
            else if ((mask & ownLayerBit) != 0)
            {
                string layerName = LayerMask.LayerToName(go.layer);
                string layerLabel = string.IsNullOrEmpty(layerName) ? go.layer.ToString() : layerName;

                if (MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    $"Ground Layers includes this object's own layer ('{layerLabel}'). The downward probe can detect the character's own collider and report false ground.",
                    "Remove this object's layer from Ground Layers, or move the character to a dedicated layer.",
                    "Remove This Layer"))
                {
                    _groundLayers.intValue = mask & ~ownLayerBit;
                }
            }
        }
    }
}
