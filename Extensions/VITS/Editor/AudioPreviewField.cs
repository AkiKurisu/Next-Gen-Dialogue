using System;
using System.IO;
using NextGenDialogue.VITS;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace NextGenDialogue.Graph.VITS.Editor
{
    public class AudioPreviewField : VisualElement
    {
        private readonly AudioClip audioClip;
        private readonly Button downloadButton;
        private readonly Action<AudioClip> OnDownload;
        public AudioPreviewField(AudioClip audioClip, bool isReadOnly = true, Action<AudioClip> OnDownload = null)
        {
            this.OnDownload = OnDownload;
            this.audioClip = audioClip;
            var label = new Label(audioClip.name);
            Add(label);
            if (!isReadOnly)
            {
                downloadButton = new Button(Download) { text = "Download" };
                Add(downloadButton);
            }
            var previewButton = new Button(Preview) { text = "Preview" };
            Add(previewButton);
        }

        private void Preview()
        {
            AudioUtil.StopClip(audioClip);
            AudioUtil.PlayClip(audioClip);
        }
        private void Download()
        {
            string folderPath = EditorPrefs.GetString(AudioUtil.PrefKey, Application.dataPath);
            if (!Directory.Exists(folderPath)) folderPath = Application.dataPath;
            string path = EditorUtility.OpenFolderPanel("Select save path", folderPath, "");
            if (string.IsNullOrEmpty(path)) return;
            EditorPrefs.SetString(AudioUtil.PrefKey, path);
            string outPutPath = $"{path}/{audioClip.name}";
            WavUtil.Save(outPutPath, audioClip);
            Debug.Log($"Audio saved succeed! Audio path:{outPutPath}");
            downloadButton.RemoveFromHierarchy();
            AssetDatabase.Refresh();
            var newClip = AssetDatabase.LoadAssetAtPath<AudioClip>(outPutPath.Replace(Application.dataPath, "Assets/"));
            OnDownload?.Invoke(newClip);
        }
    }
}
