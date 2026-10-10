using MotionCore2D.Configuration;
using MotionCore2D.Core;
using MotionCore2D.Crouch;
using MotionCore2D.Jump;
using MotionCore2D.Physics;
using MotionCore2D.Sensing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using MotionCore2D.Input;
using UnityEngine.InputSystem;
#endif

namespace MotionCore2D.Editor
{
    /// <summary>
    /// Editor utility that generates the demo content for MotionCore 2D Basic: a configured Player
    /// prefab and a simple platformer scene containing flat ground, a walkable slope, a gap, and a
    /// low tunnel that can only be crossed while crouched.
    /// </summary>
    /// <remarks>
    /// Generating the assets through the editor API (rather than shipping pre-baked scene files)
    /// keeps script and asset references valid regardless of the importing project. Run it from the
    /// <c>Tools/MotionCore 2D/Build Demo Scene</c> menu.
    /// </remarks>
    public static class DemoSceneBuilder
    {
        private const string DemoFolder = "Assets/MotionCore2D/Demo";
        private const string ProfilePath = DemoFolder + "/DemoMovementProfile.asset";
        private const string PrefabPath = DemoFolder + "/Player.prefab";
        private const string ScenePath = DemoFolder + "/Scenes/PlatformerDemo.unity";
        private const string InputActionsPath = DemoFolder + "/Input/MotionCore2D_Demo.inputactions";
        private const string DemoMaterialPath = DemoFolder + "/Materials/DemoSpriteMaterial.mat";

        private const int GroundLayer = 0;
        private const int PlayerLayer = 6;

        /// <summary>
        /// Builds (or rebuilds) the demo movement profile, Player prefab, and platformer scene, then
        /// opens the scene.
        /// </summary>
        [MenuItem("Tools/MotionCore 2D/Build Demo Scene")]
        public static void BuildDemo()
        {
            EnsureFolder();

            MovementProfile profile = CreateProfile();
            GameObject prefab = CreatePlayerPrefab(profile);
            BuildScene(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"MotionCore 2D demo generated under '{DemoFolder}'.");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/MotionCore2D"))
            {
                AssetDatabase.CreateFolder("Assets", "MotionCore2D");
            }

            if (!AssetDatabase.IsValidFolder(DemoFolder))
            {
                AssetDatabase.CreateFolder("Assets/MotionCore2D", "Demo");
            }

            if (!AssetDatabase.IsValidFolder(DemoFolder + "/Scenes"))
            {
                AssetDatabase.CreateFolder(DemoFolder, "Scenes");
            }

            if (!AssetDatabase.IsValidFolder(DemoFolder + "/Input"))
            {
                AssetDatabase.CreateFolder(DemoFolder, "Input");
            }

            if (!AssetDatabase.IsValidFolder(DemoFolder + "/Materials"))
            {
                AssetDatabase.CreateFolder(DemoFolder, "Materials");
            }
        }

        private static MovementProfile CreateProfile()
        {
            MovementProfile profile = AssetDatabase.LoadAssetAtPath<MovementProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<MovementProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            return profile;
        }

        private static GameObject CreatePlayerPrefab(MovementProfile profile)
        {
            var player = new GameObject("Player", typeof(Rigidbody2D), typeof(CapsuleCollider2D))
            {
                layer = PlayerLayer
            };

            var body = player.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var capsule = player.GetComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.7f, 1f);

            AddSlicedSprite(player, new Vector2(0.7f, 1f), new Color(0.2f, 0.6f, 1f));

            player.AddComponent<CharacterMotor2D>();

            var detector = player.AddComponent<GroundDetector2D>();
            var detectorSerialized = new SerializedObject(detector);
            detectorSerialized.FindProperty("_groundLayers").intValue = 1 << GroundLayer;
            detectorSerialized.ApplyModifiedPropertiesWithoutUndo();

            player.AddComponent<JumpController2D>();

            var crouch = player.AddComponent<CrouchController2D>();
            var crouchSerialized = new SerializedObject(crouch);
            crouchSerialized.FindProperty("_bodyCollider").objectReferenceValue = capsule;
            crouchSerialized.FindProperty("_ceilingLayers").intValue = 1 << GroundLayer;
            crouchSerialized.ApplyModifiedPropertiesWithoutUndo();

            var controller = player.AddComponent<MotionController2D>();
            var controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("_profile").objectReferenceValue = profile;
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            ConfigureInput(player);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static void ConfigureInput(GameObject player)
        {
#if ENABLE_INPUT_SYSTEM
            var reader = player.AddComponent<InputSystemReader>();
            var serialized = new SerializedObject(reader);
            serialized.FindProperty("_moveAction").objectReferenceValue = FindActionReference("Move");
            serialized.FindProperty("_jumpAction").objectReferenceValue = FindActionReference("Jump");
            serialized.FindProperty("_crouchAction").objectReferenceValue = FindActionReference("Crouch");
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (FindActionReference("Move") == null ||
                FindActionReference("Jump") == null ||
                FindActionReference("Crouch") == null)
            {
                Debug.LogWarning($"Could not auto-assign Move/Jump/Crouch from '{InputActionsPath}'. Assign them on the Player's Input System Reader.");
            }
#else
            Debug.LogWarning("Input System is not enabled; the demo Player has no input source.");
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static InputActionReference FindActionReference(string actionName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(InputActionsPath);
            foreach (Object asset in assets)
            {
                if (asset is InputActionReference reference &&
                    reference.action != null &&
                    reference.action.name == actionName)
                {
                    return reference;
                }
            }

            return null;
        }
#endif

        private static void BuildScene(GameObject prefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera", typeof(Camera))
            {
                tag = "MainCamera"
            };
            cameraObject.transform.position = new Vector3(0f, 2f, -10f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.12f, 0.13f, 0.18f);

            CreatePlatform("Ground_Left", new Vector2(-7f, 0f), new Vector2(7f, 1f), 0f);
            CreatePlatform("Slope", new Vector2(-1.5f, 0.7f), new Vector2(5f, 1f), 20f);
            CreatePlatform("Ground_Right", new Vector2(5.5f, 1.9f), new Vector2(7f, 1f), 0f);
            CreatePlatform("Ledge", new Vector2(12f, 0f), new Vector2(4f, 1f), 0f);

            // A ceiling leaving 0.7 units of headroom over Ground_Right: the standing capsule is 1
            // unit tall, so the player must hold crouch to pass, and cannot stand up underneath it.
            CreatePlatform("Tunnel_Ceiling", new Vector2(6.5f, 3.6f), new Vector2(3f, 1f), 0f);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player.transform.position = new Vector3(-8f, 2f, 0f);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreatePlatform(string platformName, Vector2 position, Vector2 size, float angle)
        {
            var platform = new GameObject(platformName, typeof(BoxCollider2D))
            {
                layer = GroundLayer
            };
            platform.transform.position = position;
            platform.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            platform.GetComponent<BoxCollider2D>().size = size;
            AddSlicedSprite(platform, size, new Color(0.35f, 0.37f, 0.42f));
        }

        private static void AddSlicedSprite(GameObject target, Vector2 size, Color color)
        {
            var renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            renderer.sharedMaterial = EnsureDemoSpriteMaterial();
            renderer.color = color;
            if (renderer.sprite != null)
            {
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = size;
            }
        }

        private static Material EnsureDemoSpriteMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(DemoMaterialPath);
            Shader shader = Shader.Find("Sprites/Default") ??
                Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

            if (material == null)
            {
                if (shader == null)
                {
                    Debug.LogWarning("Could not find a compatible sprite shader for the demo material.");
                    return null;
                }

                material = new Material(shader)
                {
                    name = "DemoSpriteMaterial"
                };
                AssetDatabase.CreateAsset(material, DemoMaterialPath);
                return material;
            }

            if (shader != null && material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }

            return material;
        }
    }
}
