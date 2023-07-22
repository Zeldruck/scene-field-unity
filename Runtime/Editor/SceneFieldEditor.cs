using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zeldruck.Packages
{
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldEditor : PropertyDrawer
    {

        private SceneAsset _scene;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty scenePath = property.FindPropertyRelative("scenePath");
            SerializedProperty sceneName = property.FindPropertyRelative("sceneName");
            SerializedProperty sceneBuildId = property.FindPropertyRelative("sceneBuildIndex");
            
            _scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath.stringValue);

            EditorGUI.BeginChangeCheck();

            string labelName = property.name;
            labelName = labelName[0].ToString().ToUpper() + labelName.Substring(1);
            
            var newScene = EditorGUI.ObjectField(position, labelName, _scene, typeof(SceneAsset), false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                if (newScene == null)
                {
                    scenePath.stringValue = "";
                    sceneName.stringValue = "";
                    sceneBuildId.intValue = -1;
                    return;
                }
                
                var newPath = AssetDatabase.GetAssetPath(newScene);
                scenePath.stringValue = newPath;
                
                sceneName.stringValue = AssetDatabase.LoadAssetAtPath<Object>(newPath).name;

                TryAddSceneToBuildIndex(newPath, out bool success, out bool error, out string message);

                if (!error)
                    sceneBuildId.intValue = SceneUtility.GetBuildIndexByScenePath(newPath);
                
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void TryAddSceneToBuildIndex(string scenePath, out bool success, out bool error, out string message)
        {
            int newBuildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (newBuildIndex >= 0)
            {
                success = false;
                error = false;
                message = "Scene already referenced in the build index";
                
                return;
            }
            
            
            var currentBuildScenes = EditorBuildSettings.scenes;
            var newSettingsScenes = new EditorBuildSettingsScene[currentBuildScenes.Length + 1];
                    
            System.Array.Copy(currentBuildScenes, newSettingsScenes, currentBuildScenes.Length);

            var newSettingScene = new EditorBuildSettingsScene(scenePath, true);

            newSettingsScenes[^1] = newSettingScene;

            EditorBuildSettings.scenes = newSettingsScenes;

            success = true;
            error = false;
            message = "Scene has been added to build index";
        }
    }
}
