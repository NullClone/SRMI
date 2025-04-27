using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SRMI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BlockImporter))]
    public class BlockImporterEditor : ScriptedImporterEditor
    {
        // Properties

        public SerializedProperty Iterator { get; set; }

        public SerializedProperty Directory { get; set; }

        public SerializedProperty IgnoreDirectory { get; set; }


        public override bool showImportedObject => false;


        // Methods

        public override void OnEnable()
        {
            Iterator = serializedObject.FindProperty("m_Script");
            Directory = serializedObject.FindProperty("m_Directory");
            IgnoreDirectory = serializedObject.FindProperty("m_IgnoreDirectory");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var importer = target as BlockImporter;
            var block = assetTarget as BlockScriptableObject;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(Iterator);
            }

            EditorGUILayout.Space();

            var styles = new GUIStyle(EditorStyles.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
            };

            EditorGUILayout.LabelField("S R M I", styles);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Export Setting", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(Directory);
                EditorGUILayout.PropertyField(IgnoreDirectory);
                EditorGUILayout.Space();
            }

            if (block != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox($"Skipped Imported Objects : [{block.SkippedObjectLength}/{block.ObjectLength}]", MessageType.Warning);
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            ApplyRevertGUI();

            serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI()
        {
            return false;
        }
    }
}