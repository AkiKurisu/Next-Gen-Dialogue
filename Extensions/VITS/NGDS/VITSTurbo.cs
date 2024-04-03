using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kurisu.NGDS.AI;
using UnityEngine;
using UnityEngine.Networking;
namespace Kurisu.NGDS.VITS
{
    public struct VITSResponse
    {
        public AudioClip Result { get; internal set; }
        public bool Status { get; internal set; }
    }
    public class VITSTurbo
    {
        private const string CallAPIBase = "http://{0}:{1}/voice/{2}?text={3}&id={4}";
        private readonly StringBuilder stringBuilder = new();
        public AudioClip AudioClipCache { get; private set; }
        public ITranslator Translator { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
        public string Api { get; set; }
        public string Lang { get; set; }
        public string Length { get; set; }
        public VITSTurbo(AITurboSetting setting)
        {
            Address = setting.VITS_Address;
            Api = setting.VITS_Model;
            Port = setting.VITS_Port;
            Lang = setting.VITS_Lang;
            Length = setting.VITS_Length;
        }
        public VITSTurbo() { }
        private void CacheAudioClip(AudioClip audioClip)
        {
            AudioClipCache = audioClip;
            AudioClipCache.name = $"VITS-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.wav";
        }
        private string GetURL(string message, int characterID)
        {
            stringBuilder.Clear();
            stringBuilder.Append(string.Format(CallAPIBase, Address, Port, Api, message, characterID));
            if (!string.IsNullOrEmpty(Lang))
            {
                stringBuilder.Append($"&lang={Lang}");
            }
            if (!string.IsNullOrEmpty(Length))
            {
                stringBuilder.Append($"&length={Length}");
            }
            return stringBuilder.ToString();
        }
        public async Task<VITSResponse> SendVITSRequestAsync(string message, int characterID, CancellationToken ct)
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(GetURL(message, characterID), AudioType.WAV);
            www.SendWebRequest();
            while (!www.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("[VITS Turbo]: " + www.error);
                return new VITSResponse()
                {
                    Status = false
                };
            }
            else
            {
                AudioClip audioClip = null;
                bool validate;
                try
                {
                    audioClip = DownloadHandlerAudioClip.GetContent(www);
                    CacheAudioClip(audioClip);
                    validate = true;
                }
                catch
                {
                    validate = false;
                }
                return new VITSResponse()
                {
                    Result = audioClip,
                    Status = validate
                };
            }
        }
        public async Task<VITSResponse> SendVITSRequestAsyncWithTranslation(string message, int characterID, CancellationToken ct)
        {
            if (Translator != null)
            {
                message = await Translator.Translate(message, ct);
            }
            return await SendVITSRequestAsync(message, characterID, ct);
        }
    }
}