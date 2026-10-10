using UnityEditor;
using UnityEngine;

namespace MotionCore2D.Editor
{
    /// <summary>
    /// Shared IMGUI helpers for MotionCore custom inspectors. Draws a validation message that
    /// explains the problem and the fix, optionally followed by a one-click fix button.
    /// </summary>
    internal static class MotionInspectorGui
    {
        private const string DocumentationFolder = "Assets/MotionCore2D/Documentation";

        /// <summary>
        /// Draws a validation help box. When <paramref name="buttonLabel"/> is provided, a fix
        /// button is drawn beneath it.
        /// </summary>
        /// <param name="type">Severity of the message.</param>
        /// <param name="problem">What is wrong.</param>
        /// <param name="fix">How to resolve it.</param>
        /// <param name="buttonLabel">Optional fix-button label. Pass <c>null</c> for no button.</param>
        /// <returns><c>true</c> if a fix button was shown and clicked this frame.</returns>
        internal static bool DrawIssue(MessageType type, string problem, string fix, string buttonLabel = null)
        {
            EditorGUILayout.HelpBox($"{problem}\n\nFix: {fix}", type);

            bool clicked = false;
            if (!string.IsNullOrEmpty(buttonLabel))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    clicked = GUILayout.Button(buttonLabel, GUILayout.MaxWidth(240f));
                }
            }

            EditorGUILayout.Space(2f);
            return clicked;
        }

        /// <summary>
        /// Draws a row of buttons that open the package's own documentation for this component.
        /// </summary>
        /// <remarks>
        /// The guides ship inside the package, so they open from disk and work offline and in an
        /// unregistered copy. Nothing is drawn for a guide that is not present, which keeps the
        /// inspector honest if a user trims the Documentation folder from their project.
        /// </remarks>
        /// <param name="guides">Documentation file names (inside <c>Documentation/</c>) paired with button labels.</param>
        internal static void DrawDocumentationLinks(params (string fileName, string label)[] guides)
        {
            if (guides == null || guides.Length == 0)
            {
                return;
            }

            var available = new System.Collections.Generic.List<(string path, string label)>(guides.Length);
            foreach ((string fileName, string label) in guides)
            {
                string path = DocumentationFolder + "/" + fileName;
                if (AssetDatabase.LoadAssetAtPath<TextAsset>(path) != null)
                {
                    available.Add((path, label));
                }
            }

            if (available.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space(2f);
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach ((string path, string label) in available)
                {
                    if (GUILayout.Button(label, EditorStyles.miniButton))
                    {
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(path));
                    }
                }
            }
        }

        /// <summary>
        /// Draws every serialized property of the inspected object except the script reference.
        /// </summary>
        /// <param name="serializedObject">The inspector's serialized object.</param>
        internal static void DrawPropertiesExceptScript(SerializedObject serializedObject)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // skip m_Script
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
        }
    }
}
