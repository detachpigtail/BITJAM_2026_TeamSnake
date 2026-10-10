using MotionCore2D.Configuration;
using UnityEditor;
using UnityEngine;

namespace MotionCore2D.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="MovementProfile"/>. Adds usage guidance, warns about values
    /// that break basic movement (non-positive gravity or speed), and shows the derived jump launch
    /// velocity so designers can see the effect of the Jump Height and Gravity pairing.
    /// </summary>
    [CustomEditor(typeof(MovementProfile))]
    public sealed class MovementProfileEditor : UnityEditor.Editor
    {
        private const float DefaultGravity = 40f;

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var profile = (MovementProfile)target;

            EditorGUILayout.HelpBox(
                "Shared tuning for MotionCore 2D actors. Values are clamped to safe ranges automatically. One profile can be referenced by many actors.",
                MessageType.Info);

            DrawValidations(profile);
            MotionInspectorGui.DrawPropertiesExceptScript(serializedObject);
            DrawComputedReadout(profile);
            MotionInspectorGui.DrawDocumentationLinks(("TuningGuide.md", "Tuning Guide"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidations(MovementProfile profile)
        {
            if (profile.Gravity <= 0f)
            {
                if (MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    "Gravity is zero or negative. With the manual gravity model the actor will not fall and jumps will not arc.",
                    "Set Gravity to a positive value (40 is a good starting point).",
                    "Set Gravity to 40"))
                {
                    SerializedProperty gravity = serializedObject.FindProperty("_gravity");
                    if (gravity != null)
                    {
                        gravity.floatValue = DefaultGravity;
                    }
                }
            }

            if (profile.MaxSpeed <= 0f)
            {
                MotionInspectorGui.DrawIssue(
                    MessageType.Warning,
                    "Max Speed is zero. The actor cannot move horizontally.",
                    "Set Max Speed above 0 in the Horizontal section below.");
            }
        }

        private static void DrawComputedReadout(MovementProfile profile)
        {
            float gravity = Mathf.Max(0f, profile.Gravity);
            float height = Mathf.Max(0f, profile.JumpHeight);
            float step = Time.fixedDeltaTime;

            // Mirrors JumpStateMachine: the continuous sqrt(2 g h) minus the half step of gravity the
            // fixed-step integration would otherwise add on top, so this readout is the velocity the
            // actor is really launched at rather than a textbook figure it never uses.
            float launch = Mathf.Max(0f, Mathf.Sqrt(2f * gravity * height) - (gravity * step * 0.5f));
            float timeToApex = gravity > 0f ? launch / gravity : 0f;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Computed (read-only)", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Jump Launch Velocity",
                        $"Initial upward velocity for a full jump: sqrt(2 * Gravity * Jump Height), less half a fixed step of gravity so the integrated apex lands on Jump Height. Assumes the current fixed timestep of {step:0.####}s."),
                    launch);

                EditorGUILayout.FloatField(
                    new GUIContent(
                        "Time To Apex (s)",
                        "How long a full jump takes to reach its highest point, holding the jump control throughout."),
                    timeToApex);
            }
        }
    }
}
