using UnityEngine;
using UnityEditor;
namespace Visu
{
    // ----------------------------------------------------------------------------
    // VisuCameraEditor.cs
    // Last Update: March 22th, 2024
    // Author: Cassio Polegatto
    // ----------------------------------------------------------------------------
    // Description:
    // This script provides a custom inspector look to the Visu Camera Follow component.
    // Thank you!
    // ----------------------------------------------------------------------------

    [CustomEditor(typeof(VisuCameraFollow))]
    public class VisuCameraEditor : Editor
    {
        private new SerializedProperty target;
        private SerializedProperty mainCam;
        private SerializedProperty smoothFactor;
        private SerializedProperty offset;

        private GUIContent aboutButton;

        bool showAboutInfo = false;
      
        private void OnEnable()
        {
            target = serializedObject.FindProperty("target");
            mainCam = serializedObject.FindProperty("myCamera");
            offset = serializedObject.FindProperty("cameraOffset");
            smoothFactor = serializedObject.FindProperty("smoothFactor");
        }

        #region GUI Layout

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawMainBox();

            serializedObject.ApplyModifiedProperties();
        }
        

        void DrawMainBox()
        {
            // Main Box
            EditorGUI.DrawRect(EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false)), new Color(0.1f, 0.1f, 0.1f));
            
                // Top Label
                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                    EditorGUILayout.LabelField(" V I S U   -    Camera Follow ", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(300));
                    GUILayout.FlexibleSpace();        
                    // About button
                    DrawAboutBox();
                EditorGUILayout.EndHorizontal();
            
                EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(2);
                    // Content Box
                    EditorGUI.DrawRect(EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false)), new Color(1f,1f,1f, 0.12f));
                        // DRAW CONTENT
                        DrawContent();
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(1);
                EditorGUILayout.EndHorizontal();
            
                // Bottom Label
                EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
                    EditorGUILayout.LabelField("  -     -     -  ", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
         
            EditorGUILayout.EndVertical();
        }

     

        void DrawContent()
        {
            // Object section

            GUILayout.Space(8);

                // Object header
                EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    EditorGUILayout.LabelField(new GUIContent("Objects"), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(64));
                EditorGUILayout.EndHorizontal();

                // Target
                EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(24);
                    EditorGUILayout.LabelField(new GUIContent("Target", "The Target object followed by the Camera. Usually, your Player."), EditorStyles.miniLabel, GUILayout.MaxWidth(64));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(24);
                    target.objectReferenceValue = EditorGUILayout.ObjectField(target.objectReferenceValue, typeof(Transform), true);
                    GUILayout.Space(16);

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);
            
                // Camera
            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(24);
                EditorGUILayout.LabelField(new GUIContent("Camera", "The Camera that follows the Target"), EditorStyles.miniLabel, GUILayout.MaxWidth(64));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(24);
                mainCam.objectReferenceValue = EditorGUILayout.ObjectField(mainCam.objectReferenceValue, typeof(Camera), true);
                GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
            

            GUILayout.Space(16);


            DrawLine(0.1f);

            // Settings section
            GUILayout.Space(8);

                // Settings header
                EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    EditorGUILayout.LabelField(new GUIContent("Settings"), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(64));
                EditorGUILayout.EndHorizontal();

            // Damping
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            EditorGUILayout.LabelField(new GUIContent("Damping", "Adjust to slow down camera movement. Default: 0.2f"), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            EditorGUILayout.Slider(smoothFactor, 0f, 2f, GUIContent.none);
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);

            // Offset
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            EditorGUILayout.LabelField(new GUIContent("Camera offset", "If needed, offset the camera position to your liking."), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            EditorGUILayout.PropertyField(offset, GUIContent.none, GUILayout.ExpandWidth(true));
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
          
            GUILayout.Space(16);
        }

        private void DrawLine(float lineColor)
        {
 
            EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(EditorGUILayout.BeginVertical(), new Color(lineColor, lineColor, lineColor));
            GUILayout.Space(1);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region About
        void DrawAboutBox()
        {
            EditorGUI.DrawRect(EditorGUILayout.BeginVertical(GUILayout.MaxWidth(60), GUILayout.Height(24)), new Color(0.15f, 0.15f, 0.15f));
            aboutButton = new GUIContent("About");
            showAboutInfo = GUILayout.Toggle(showAboutInfo, aboutButton, EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndVertical();

            if (showAboutInfo)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                GUILayout.Space(2);
                GUILayout.FlexibleSpace();

                EditorGUI.DrawRect(EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true)), new Color(0.15f, 0.15f, 0.15f));
                // Draw About section
                DrawAbout();
                GUILayout.Space(8);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }

        void DrawAbout()
        {
            GUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                EditorGUILayout.LabelField("Version: 1.1", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                EditorGUILayout.LabelField("Last update: 21 March 2024", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                EditorGUILayout.LabelField("Created by Cassio Polegatto", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();
                GUILayout.Space(8);
            DrawLine(0.1f);
                GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.LabelField("If you need any help with this asset, go here:", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(64));

                GUILayout.Space(16);
                
                string supportUrl = "https://cassiopolegatto.com/visu/";
                EditorGUI.DrawRect(EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.Height(20)), new Color(0.10f, 0.10f, 0.10f));
                    if (GUILayout.Button(new GUIContent("  FAQ • HELP • SUPPORT  "), EditorStyles.whiteMiniLabel, GUILayout.Height(20)))
                    {
                        Application.OpenURL(supportUrl);
                    }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            
                GUILayout.Space(8);
            DrawLine(0.1f);
                GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.LabelField("Please, rate this asset in the Unity Asset Store. Your support is greatly appreciated.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(64));

                GUILayout.Space(16);
                
                string koFiLink = "https://ko-fi.com/Q5Q0M0GL8";
                EditorGUI.DrawRect(EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.Height(20)), new Color(0.10f, 0.10f, 0.10f));
                    if (GUILayout.Button(new GUIContent("  Buy me a coffee :)  "), EditorStyles.whiteMiniLabel, GUILayout.Height(20)))
                    {
                        Application.OpenURL(koFiLink);
                    }
                EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                EditorGUILayout.LabelField("Thank you!", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}