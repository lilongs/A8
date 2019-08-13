using System;
using System.Collections.Generic;
using System.Text;
using DotNetSpeech;
using System.Threading;
using System.IO;

namespace A8Project
{
    class SpVoiceUtil
    {
        SpVoice voice = new DotNetSpeech.SpVoiceClass();

        public delegate void CallBack(bool b, int InputWordPosition, int InputWordLength);

        /// <summary>
        /// 朗读文本
        /// </summary>
        /// <param name="str">要朗读的文本</param>
        /// <param name="CallBack">回调地址</param>
        /// <returns>返回bool</returns>
        public bool Speak(string str, CallBack CallBack)
        {
            int n = voice.Speak(str, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            Thread thread = new Thread(new ParameterizedThreadStart(Call));
            thread.IsBackground = true;
            thread.Start((Object)CallBack);
            return !(n != 1);
        }

        public bool Speak(string str)
        {
            int n = voice.Speak(str, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            Thread thread = new Thread(new ParameterizedThreadStart(Call));
            thread.IsBackground = true;
            thread.Start();
            return !(n != 1);
        }


        /// <summary>
        /// 回调函数线程子程序
        /// </summary>
        /// <param name="callBack"></param>
        private void Call(Object callBack)
        {
            int InputWordLength = 0;    //局部_朗读长度
            int InputWordPosition = 0; //局部_朗读位置

            CallBack CallBack = (CallBack)callBack;

            while ((int)voice.Status.RunningState != 1)
            {
                if (InputWordPosition != voice.Status.InputWordPosition || InputWordLength != voice.Status.InputWordLength)
                {
                    InputWordPosition = voice.Status.InputWordPosition;
                    InputWordLength = voice.Status.InputWordLength;

                    //回调                  
                    CallBack(false, InputWordPosition, InputWordLength);
                }
            }
            CallBack(true, InputWordPosition, InputWordLength);
        }

        /// <summary>
        /// 获取语音库
        /// </summary>
        /// <returns>List<string></returns>
        public List<string> getDescription()
        {
            List<string> list = new List<string>();
            DotNetSpeech.ISpeechObjectTokens obj = voice.GetVoices();
            int count = obj.Count;//获取语音库总数
            for (int i = 0; i < count; i++)
            {
                string desc = obj.Item(i).GetDescription(); //遍历语音库
                list.Add(desc);
            }
            return list;
        }

        /// <summary>
        /// 设置当前使用语音库
        /// </summary>
        /// <returns>bool</returns>
        public bool setDescription(string name)
        {
            List<string> list = new List<string>();
            DotNetSpeech.ISpeechObjectTokens obj = voice.GetVoices();
            int count = obj.Count;//获取语音库总数
            bool result = false;
            for (int i = 0; i < count; i++)
            {
                string desc = obj.Item(i).GetDescription(); //遍历语音库
                if (desc.Equals(name))
                {
                    voice.Voice = obj.Item(i);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 设置语速
        /// </summary>
        /// <param name="n"></param>
        public void setRate(int n)
        {
            voice.Rate = n;
        }

        /// <summary>
        /// 设置声音大小
        /// </summary>
        /// <param name="n"></param>
        public void setVolume(int n)
        {
            voice.Volume = n;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            voice.Pause();
        }

        /// <summary>
        /// 继续
        /// </summary>
        public void Resume()
        {
            voice.Resume();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            voice.Speak(string.Empty, SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
        }


        /// <summary>
        /// 输出WAV
        /// </summary>
        /// <param name="path">保存路径</param>
        /// <param name="str">要转换的文本内容</param>
        /// <returns></returns>
        public bool WreiteToWAV(string path, string str, SpeechAudioFormatType SpAudioType)
        {
            SpeechStreamFileMode SpFileMode = SpeechStreamFileMode.SSFMCreateForWrite;
            SpFileStream SpFileStream = new SpFileStream();
            SpeechVoiceSpeakFlags SpFlags = SpeechVoiceSpeakFlags.SVSFlagsAsync;
            SpAudioFormat SpAudio = new DotNetSpeech.SpAudioFormat();
            SpAudio.Type = SpAudioType;
            SpFileStream.Format = SpAudio;
            SpFileStream.Open(path, SpFileMode, false);
            voice.AudioOutputStream = SpFileStream;
            voice.Speak(str, SpFlags);
            voice.WaitUntilDone(Timeout.Infinite);
            SpFileStream.Close();
            return File.Exists(path);
        }
    }
}
