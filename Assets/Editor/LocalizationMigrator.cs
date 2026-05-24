using UnityEngine;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using System.IO;

namespace SSPot.Editor
{
    [System.Serializable]
    public class LevelData
    {
        public string fase;
        public DialogLine[] linhas;
    }

    [System.Serializable]
    public class DialogLine
    {
        public string nome;
        public string pt_br;
        public string en_us;
    }

    public class LocalizationMigrator : EditorWindow
    {
        public TextAsset jsonFile;
        public string stringTableName = "SubtitlesTable";
        public string assetTableName = "AudioTable";
        
        public string audioFolderPath = "Assets/Narration/Audios"; 

        [MenuItem("SSPot/Tools/Migrate JSON to Localization")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationMigrator>("Localization Migrator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Migrador de Narração", EditorStyles.boldLabel);
            
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("Arquivo JSON", jsonFile, typeof(TextAsset), false);
            stringTableName = EditorGUILayout.TextField("String Table Name", stringTableName);
            assetTableName = EditorGUILayout.TextField("Asset Table Name", assetTableName);
            audioFolderPath = EditorGUILayout.TextField("Pasta dos Áudios", audioFolderPath);

            if (GUILayout.Button("Processar e Popular Tabelas"))
            {
                if (jsonFile != null) ProcessJSON();
                else Debug.LogError("Selecione um arquivo JSON!");
            }
        }

        private void ProcessJSON()
        {
            LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
            if (data == null || data.linhas == null)
            {
                Debug.LogError("Erro: Não foi possível ler as 'linhas' do JSON.");
                return;
            }

            var stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(stringTableName);
            var assetTableCollection = LocalizationEditorSettings.GetAssetTableCollection(assetTableName);

            if (stringTableCollection == null || assetTableCollection == null)
            {
                Debug.LogError($"Erro: Collections não encontradas!");
                return;
            }

            var stringTablePT = stringTableCollection.GetTable("pt-BR") as StringTable;
            var stringTableEN = stringTableCollection.GetTable("en") as StringTable;
            var assetTablePT = assetTableCollection.GetTable("pt-BR") as AssetTable;
            var assetTableEN = assetTableCollection.GetTable("en") as AssetTable;

            if (stringTablePT == null || stringTableEN == null || assetTablePT == null || assetTableEN == null)
            {
                Debug.LogError("Erro: Uma das tabelas de Locale (pt-BR ou en) não foi encontrada!");
                return;
            }

            foreach (var linha in data.linhas)
            {
                string key = $"{data.fase}_{linha.nome}";

                // --- 1. PREENCHE AS LEGENDAS ---
                // Checa se a chave já existe para não criar duplicatas
                var sharedStringEntry = stringTableCollection.SharedData.GetEntry(key);
                if (sharedStringEntry == null)
                {
                    sharedStringEntry = stringTableCollection.SharedData.AddKey(key);
                }

                // AddEntry usando o ID atualiza o valor caso ele já exista
                stringTablePT.AddEntry(sharedStringEntry.Id, linha.pt_br);
                stringTableEN.AddEntry(sharedStringEntry.Id, linha.en_us);


                // --- 2. PREENCHE OS ÁUDIOS ---
                // Checa se a chave já existe para não criar duplicatas
                var sharedAssetEntry = assetTableCollection.SharedData.GetEntry(key);
                if (sharedAssetEntry == null)
                {
                    sharedAssetEntry = assetTableCollection.SharedData.AddKey(key);
                }
                
                AudioClip clipPT = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioFolderPath}/{data.fase}/{linha.nome}pt_br.mp3");
                AudioClip clipEN = AssetDatabase.LoadAssetAtPath<AudioClip>($"{audioFolderPath}/{data.fase}/{linha.nome}en_us.mp3");

                if (clipPT != null) 
                {
                    string guidPT = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clipPT));
                    assetTablePT.AddEntry(sharedAssetEntry.Id, guidPT);
                }
                else Debug.LogWarning($"Áudio PT-BR não encontrado para a chave: {key}");

                if (clipEN != null) 
                {
                    string guidEN = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clipEN));
                    assetTableEN.AddEntry(sharedAssetEntry.Id, guidEN);
                }
                else Debug.LogWarning($"Áudio EN-US não encontrado para a chave: {key}");
            }

            // Salva as alterações
            EditorUtility.SetDirty(stringTableCollection.SharedData);
            EditorUtility.SetDirty(stringTablePT);
            EditorUtility.SetDirty(stringTableEN);
            EditorUtility.SetDirty(assetTableCollection.SharedData);
            EditorUtility.SetDirty(assetTablePT);
            EditorUtility.SetDirty(assetTableEN);
            AssetDatabase.SaveAssets();

            Debug.Log($"Migração do {data.fase} concluída com sucesso! Nenhuma duplicata gerada.");
        }
    }
}