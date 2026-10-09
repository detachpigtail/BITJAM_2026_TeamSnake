using MotionCore2D.Crouch;
using UnityEditor;
using UnityEngine;

namespace MotionCore2D.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="CrouchController2D"/>. Validates the two settings that decide
    /// whether crouching works at all — the collider to resize and the layers whose ceilings block
    /// standing up — each with a one-click fix, so the mistakes surface in the editor rather than as
    /// an actor that clips through ceilings or can never stand back up.
    /// </summary>
    [CustomEditor(typeof(CrouchController2D))]
    public sealed class CrouchController2DEditor : UnityEditor.Editor
    {
        private SerializedProperty _bodyCollider;
        private SerializedProperty _ceilingLayers;

        private void OnEnable()
        {
            _bodyCollider = serializedObject.FindProperty("_bodyCollider");
            _ceilingLayers = serializedObject.FindProperty("_ceilingLayers");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject go = ((CrouchController2D)target).gameObject;
            DrawValidations(go);
            MotionInspectorGui.DrawPropertiesExceptScript(serializedObject);
            MotionInspectorGui.DrawDocumentationLinks(
                ("SetupGuide.md", "Setup Guide"),
                ("TuningGuide.md", "Tuning Guide"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidations(GameObject go)
        {
            Collider2D assigned = _bodyCollider.objectReferenceValue as Collider2D;
            if (assigned == null)
            {
                Collider2D found = go.GetComponent<Collider2D>() ?? go.GetComponentInChildren<Collider2D>();
                if (found == null)
                {
                    MotionInspectorGui.DrawIssue(
                        MessageType.Warning,
                        "No Collider2D was found to resize. Crouch state and the speed cap still apply, but the actor's shape will not change, so it cannot fit through low gaps.",
                        "Add a BoxCollider2D or CapsuleCollider2D to this object, then assign it below.");
                }
                else if (!(found is BoxCollider2D) && !(found is CapsuleCollider2D))
                {
                    MotionInspectorGui.DrawIssue(
                        MessageType.Info,
                        $"The collider that will be auto-detected is a {found.GetType().Name}. Only BoxCollider2D and CapsuleCollider2D are resized; other shapes report crouch state and the speed cap but keep their size.",
                        "Assign a BoxCollider2D or CapsuleCollider2D below if the actor should physically shrink.");
                }
            }

            int mask = _ceilingLayers.intValue;
            int ownLayerBit = 1 << go.layer;

            if (mask == 0)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    "Ceiling Layers is empty (Nothing). Nothing will ever block standing up, so the actor can snap back to full height inside low geometry.",
                    "Set Ceiling Layers to the layer(s) used by your level geometry, excluding the character's own layer.",
                    "Set to Everything Except This Layer"))
                {
                    _ceilingLayers.intValue = ~ownLayerBit;
                }
            }
            else if ((mask & ownLayerBit) != 0)
            {
                string layerName = LayerMask.LayerToName(go.layer);
                string layerLabel = string.IsNullOrEmpty(layerName) ? go.layer.ToString() : layerName;

                if (MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    $"Ceiling Layers includes this object's own layer ('{layerLabel}'). The headroom probe can detect the character's own collider and conclude it can never stand up.",
                    "Remove this object's layer from Ceiling Layers, or move the character to a dedicated layer.",
                    "Remove This Layer"))
                {
                    _ceilingLayers.intValue = mask & ~ownLayerBit;
                }
            }

            if (go.GetComponent<Core.MotionController2D>() == null)
            {
                MotionInspectorGui.DrawIssue(
                    MessageType.Error,
                    "There is no MotionController2D on this object. Crouching is a pipeline step that the controller runs, so on its own this component does nothing.",
                    "Add a MotionController2D to this object.");
            }
        }
    }
}
