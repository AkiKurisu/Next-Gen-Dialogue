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
        private readonly AudioClip _audioClip;
        
        private readonly Button _downloadButton;
        
        private readonly Action<AudioClip> _onDownload;
        
        public AudioPreviewField(AudioClip audioClip, bool isReadOnly = true, Action<AudioClip> onDownload = null)
        {
            _onDownload = onDownload;
            _audioClip = audioClip;
            var label = new Label(audioClip.name);
            Add(label);
            if (!isReadOnly)
            {
                _downloadButton = new Button(Download) { text = "Download" };
                Add(_downloadButton);
            }
            var previewButton = new Button(Preview) { text = "Preview" };
            Add(previewButton);
        }

        private void Preview()
        {
            AudioUtil.StopClip(_audioClip);
            AudioUtil.PlayClip(_audioClip);
        }
        
        private void Download()
        {
            string folderPath = EditorPrefs.GetString(AudioUtil.PrefKey, Application.dataPath);
            if (!Directory.Exists(folderPath)) folderPath = Application.dataPath;
            string path = EditorUtility.OpenFolderPanel("Select save path", folderPath, "");
            if (string.IsNullOrEmpty(path)) return;
            EditorPrefs.SetString(AudioUtil.PrefKey, path);
            string outPutPath = $"{path}/{_audioClip.name}";
            WavUtil.Save(outPutPath, _audioClip);
            Debug.Log($"Audio saved succeed! Audio path:{outPutPath}");
            _downloadButton.RemoveFromHierarchy();
            AssetDatabase.Refresh();
            var newClip = AssetDatabase.LoadAssetAtPath<AudioClip>(outPutPath.Replace(Application.dataPath, "Assets/"));
            _onDownload?.Invoke(newClip);
        }
    }
}
