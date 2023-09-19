using Kurisu.NGDS.VITS;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.VITS.Editor
{
    public class AudioPreviewField : VisualElement
    {
        private readonly AudioClip audioClip;
        private readonly Button downloadButton;
        public AudioPreviewField(AudioClip audioClip)
        {
            this.audioClip = audioClip;
            var label = new Label(audioClip.name);
            Add(label);
            downloadButton = new Button(Download) { text = "Download" };
            Add(downloadButton);
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
            string path = EditorUtility.OpenFolderPanel("Select save path", Application.dataPath, "");
            if (string.IsNullOrEmpty(path)) return;
            string outPutPath = $"{path}/{audioClip.name}";
            WavUtil.Save(outPutPath, audioClip);
            Debug.Log($"Audio saved succeed! Audio path:{outPutPath}");
            downloadButton.RemoveFromHierarchy();
        }
    }
}
