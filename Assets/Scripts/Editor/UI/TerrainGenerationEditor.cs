using NCG.Swagaria.Runtime.Generation;
using NCG.Swagaria.Runtime.Data;
using UnityEngine;
using UnityEditor;

namespace NCG.Swagaria.Editor.UI
{
    [CustomEditor(typeof(TerrainGeneration))]
    public class TerrainGenerationEditor : UnityEditor.Editor
    {
        private SerializedProperty checkLoadTime;
        private SerializedProperty drawColliders;
        private SerializedProperty cullChunks;
        private SerializedProperty player;
        private SerializedProperty worldSize;
        private SerializedProperty chunkSize;

        private SerializedProperty lightingShader;
        private SerializedProperty lightingOverlay;

        private TerrainGeneration terrainGeneration;
        private TerrainSettings terrainSettings;
        private SerializedObject terrainSettingsSerialized;

        private bool showTerrainSettings;
        private bool showSurfaceSettings;
        private bool showCaveSettings;

        private void OnEnable()
        {
            terrainGeneration = (TerrainGeneration)target;

            checkLoadTime = serializedObject.FindProperty("checkLoadTime");
            drawColliders = serializedObject.FindProperty("drawColliders");
            cullChunks = serializedObject.FindProperty("cullChunks");
            player = serializedObject.FindProperty("player");
            worldSize = serializedObject.FindProperty("worldSize");
            chunkSize = serializedObject.FindProperty("chunkSize");
            lightingShader = serializedObject.FindProperty("lightingShader");
            lightingOverlay = serializedObject.FindProperty("lightingOverlay");

            LoadTerrainSettings();
        }

        private void LoadTerrainSettings()
        {
            if (terrainSettings is null)
            {
                terrainSettings = Resources.Load<TerrainSettings>("World Settings");
                if (terrainSettings is not null)
                    terrainSettingsSerialized = new SerializedObject(terrainSettings);
                else throw new System.Exception("World Settings not found");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSectionHeader("Debugging", Color.cyan);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(checkLoadTime);
            EditorGUILayout.PropertyField(drawColliders);
            EditorGUILayout.EndVertical();

            DrawSeparator();

            DrawSectionHeader("Chunk Culling", Color.green);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(cullChunks);
            EditorGUILayout.PropertyField(player);
            EditorGUILayout.EndVertical();

            DrawSeparator();

            DrawSectionHeader("World + Chunk Size", Color.yellow);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(worldSize);
            EditorGUILayout.PropertyField(chunkSize);
            EditorGUILayout.EndVertical();

            DrawSeparator();
            
            DrawSectionHeader("Lighting", Color.magenta);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(lightingShader);
            EditorGUILayout.PropertyField(lightingOverlay);
            EditorGUILayout.EndVertical();

            DrawTerrainSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTerrainSettings()
        {
            if (terrainSettings is null)
            {
                EditorGUILayout.HelpBox("No TerrainSettings found in Resources! Ensure 'World Settings.asset' is in a Resources folder.", MessageType.Warning);
                if (GUILayout.Button("Reload Terrain Settings"))
                    LoadTerrainSettings();
                
                return;
            }

            terrainSettingsSerialized ??= new SerializedObject(terrainSettings);
            terrainSettingsSerialized.Update();

            showTerrainSettings = EditorGUILayout.Foldout(showTerrainSettings, "🌍 Terrain Settings", true, EditorStyles.foldoutHeader);
            if (showTerrainSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("seed"));
                DrawSeparator();

                showSurfaceSettings = EditorGUILayout.Foldout(showSurfaceSettings, "⛰️ Surface Settings", true, EditorStyles.foldout);
                if (showSurfaceSettings)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("terrainFreq"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("terrainOctaves"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("surfaceThreshold"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("terrainHeightMultiplier"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("terrainHeightAddition"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("dirtLayerHeight"));
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                DrawSeparator();

                showCaveSettings = EditorGUILayout.Foldout(showCaveSettings, "🕳️ Deep Caves", true, EditorStyles.foldout);
                if (showCaveSettings)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("caveFrequency"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("caveOctaves"));
                    EditorGUILayout.PropertyField(terrainSettingsSerialized.FindProperty("caveThreshold"));
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            terrainSettingsSerialized.ApplyModifiedProperties();
        }

        private void DrawSectionHeader(string title, Color color)
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = color }
            };
            EditorGUILayout.LabelField(title, style);
        }

        private void DrawSeparator()
            => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
}