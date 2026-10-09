using MotionCore2D.Configuration;
using MotionCore2D.Core;
using MotionCore2D.Input;
using MotionCore2D.Sensing;
using UnityEditor;
using UnityEngine;

namespace MotionCore2D.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="MotionController2D"/>. Surfaces setup problems (missing
    /// Rigidbody2D, input provider, ground sensor, or profile, and a non-zero gravity scale) as
    /// inline help boxes with one-click fixes, so misconfiguration is caught in the editor instead
    /// of as runtime log spam.
    /// </summary>
    [CustomEditor(typeof(MotionController2D))]
    public sealed class MotionController2DEditor : UnityEditor.Editor
    {
        private SerializedProperty _profile;

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("_profile");
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GameObject go = ((MotionController2D)target).gameObject;
            DrawValidations(go);
            EditorGUILayout.PropertyField(_profile);
            MotionInspectorGui.DrawDocumentationLinks(
                ("SetupGuide.md", "Setup Guide"),
                ("TuningGuide.md", "Tuning Guide"),
                ("Troubleshooting.md", "Troubleshooting"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidations(GameObject go)
        {
            var rigidbody = go.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Error,
                    "No Rigidbody2D is attached. MotionCore drives a dynamic Rigidbody2D and cannot move the actor without one.",
                    "Add a Rigidbody2D. CharacterMotor2D will configure it for manual gravity at runtime.",
                    "Add Rigidbody2D"))
                {
                    Undo.AddComponent<Rigidbody2D>(go);
                }
            }
            else if (rigidbody.gravityScale != 0f)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    $"Rigidbody2D.gravityScale is {rigidbody.gravityScale:0.##}. MotionCore uses manual gravity from the profile and expects a gravity scale of 0; it will be forced to 0 at runtime.",
                    "Set the Rigidbody2D gravity scale to 0 now so the editor matches play-mode behaviour.",
                    "Set Gravity Scale to 0"))
                {
                    Undo.RecordObject(rigidbody, "Set Gravity Scale");
                    rigidbody.gravityScale = 0f;
                    EditorUtility.SetDirty(rigidbody);
                }
            }

            var inputProviders = go.GetComponents<IMotionInput>();
            if (inputProviders.Length == 0)
            {
                DrawMissingInputProvider(go);
            }
            else if (inputProviders.Length > 1)
            {
                // Awake takes the first one GetComponent returns, which is component order rather
                // than anything the user chose, so two providers means movement silently depends on
                // which was added first.
                var names = new string[inputProviders.Length];
                for (int i = 0; i < inputProviders.Length; i++)
                {
                    names[i] = inputProviders[i].GetType().Name;
                }

                MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    $"This object has {inputProviders.Length} input providers (IMotionInput): {string.Join(", ", names)}. The controller uses whichever comes first in component order, so the input the actor actually responds to is not clearly defined.",
                    "Keep one input provider per actor. Use InputSystemReader for a player, or ScriptedMotionInput for an actor driven from code, and remove the other.");
            }

            if (go.GetComponent<IGroundSensor>() == null)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Error,
                    "No ground sensor (IGroundSensor) is attached. Grounded state, jumping, and slope handling will not work.",
                    "Add a GroundDetector2D component.",
                    "Add Ground Detector 2D"))
                {
                    Undo.AddComponent<GroundDetector2D>(go);
                }
            }

            if (_profile.objectReferenceValue == null)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Error,
                    "No Movement Profile is assigned. The controller has no tuning data and disables itself at runtime.",
                    "Assign an existing profile below, or create a new one.",
                    "Create Movement Profile"))
                {
                    CreateAndAssignProfile();
                }
            }
        }

        private void DrawMissingInputProvider(GameObject go)
        {
#if ENABLE_INPUT_SYSTEM
            if (MotionInspectorGui.DrawIssue(
                MessageType.Error,
                "No input provider (IMotionInput) is attached. The actor will not receive movement or jump input.",
                "Add an InputSystemReader, then assign its Move and Jump actions.",
                "Add Input System Reader"))
            {
                Undo.AddComponent<InputSystemReader>(go);
            }
#else
            MotionInspectorGui.DrawIssue(
                MessageType.Error,
                "No input provider (IMotionInput) is attached, and the Input System package is not enabled.",
                "Enable the Input System package (Project Settings > Player > Active Input Handling), then add an InputSystemReader.");
#endif
        }

        private void CreateAndAssignProfile()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Movement Profile",
                "MovementProfile",
                "asset",
                "Choose where to save the new Movement Profile.");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var profile = ScriptableObject.CreateInstance<MovementProfile>();
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();

            _profile.objectReferenceValue = profile;
        }
    }
}
