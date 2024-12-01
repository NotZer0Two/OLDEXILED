// -----------------------------------------------------------------------
// <copyright file="Speaker.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Mirror;

namespace Exiled.API.Features.Toys
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using AdminToys;
    using Enums;
    using Interfaces;
    using MEC;
    using NVorbis;
    using UnityEngine;
    using Utils.Networking;
    using VoiceChat.Codec;
    using VoiceChat.Networking;

    using Object = UnityEngine.Object;

    /// <summary>
    /// A wrapper class for <see cref="SpeakerToy"/>.
    /// </summary>
    public class Speaker : AdminToy, IWrapper<SpeakerToy>
    {
        // private float allowedSamples;
        // private int samplesPerSecond;
        private bool stopPlayback;

        /// <summary>
        /// Initializes a new instance of the <see cref="Speaker"/> class.
        /// </summary>
        /// <param name="speakerToy">The <see cref="SpeakerToy"/> of the toy.</param>
        internal Speaker(SpeakerToy speakerToy)
            : base(speakerToy, AdminToyType.Speaker) => Base = speakerToy;

        /// <summary>
        /// Gets the base <see cref="SpeakerToy"/>.
        /// </summary>
        public SpeakerToy Base { get; }

        /// <summary>
        /// Gets or sets the controller ID of the SpeakerToy.
        /// </summary>
        public byte ControllerID
        {
            get => Base.NetworkControllerId;
            set => Base.NetworkControllerId = value;
        }

        /// <summary>
        /// Gets or sets the volume of the audio source.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the volume level of the audio source,
        /// where 0.0 is silent and 1.0 is full volume.
        /// </value>
        public float Volume
        {
            get => Base.NetworkVolume;
            set => Base.NetworkVolume = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source is spatialized.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> where <c>true</c> means the audio source is spatial, allowing
        /// for 3D audio positioning relative to the listener; <c>false</c> means it is non-spatial.
        /// </value>
        public bool IsSpatial
        {
            get => Base.NetworkIsSpatial;
            set => Base.NetworkIsSpatial = value;
        }

        /// <summary>
        /// Gets or sets the maximum distance at which the audio source can be heard.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the maximum hearing distance for the audio source.
        /// Beyond this distance, the audio will not be audible.
        /// </value>
        public float MaxDistance
        {
            get => Base.NetworkMaxDistance;
            set => Base.NetworkMaxDistance = value;
        }

        /// <summary>
        /// Gets or sets the minimum distance at which the audio source reaches full volume.
        /// </summary>
        /// <value>
        /// A <see cref="float"/> representing the distance from the source at which the audio is heard at full volume.
        /// Within this range, volume will not decrease with proximity.
        /// </value>
        public float MinDistance
        {
            get => Base.NetworkMinDistance;
            set => Base.NetworkMinDistance = value;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Speaker"/> is playing an audio or not. (Use method Stop() to stop the playback).
        /// </summary>
        public bool IsPlaying { get; internal set; }

        /// <summary>
        /// Creates a new Speaker instance.
        /// </summary>
        /// <param name="controllerId">The controller ID for the SpeakerToy.</param>
        /// <param name="position">The position to place the SpeakerToy.</param>
        /// <param name="isSpatial">Indicates whether the audio is spatialized.</param>
        /// <param name="spawn">Determines if the speaker should be spawned immediately.</param>
        /// <returns>A new <see cref="Speaker"/> instance.</returns>
        public static Speaker Create(byte controllerId, Vector3 position, bool isSpatial = false, bool spawn = true)
        {
            Speaker speaker = new(Object.Instantiate(ToysHelper.SpeakerBaseObject))
            {
                Position = position,
                IsSpatial = isSpatial,
                Base = { ControllerId = controllerId },
            };

            if (spawn)
                speaker.Spawn();

            return speaker;
        }

        /// <summary>
        /// Gets the <see cref="Speaker"/> associated with a given <see cref="SpeakerToy"/>.
        /// </summary>
        /// <param name="speakerToy">The SpeakerToy instance.</param>
        /// <returns>The corresponding Speaker instance.</returns>
        public static Speaker Get(SpeakerToy speakerToy)
        {
            AdminToy adminToy = Map.Toys.FirstOrDefault(x => x.AdminToyBase == speakerToy);
            return adminToy is not null ? adminToy as Speaker : new Speaker(speakerToy);
        }

        /// <summary>
        /// Plays a single audio file through the speaker system. (No Arguments given (assuming you already preset those)).
        /// </summary>
        /// <param name="path">Path to the audio file to play.</param>
        /// <param name="destroyAfter">Whether the Speaker gets destroyed after it's done playing.</param>
        /// <returns>A boolean indicating if playback was successful.</returns>
        public bool Play(string path, bool destroyAfter = false) => Play(path, Volume, MinDistance, MaxDistance, destroyAfter);

        /// <summary>
        /// Plays a single audio file through the speaker system.
        /// </summary>
        /// <param name="path">The file path of the audio file.</param>
        /// <param name="volume">The desired playback volume. (0 to <see cref="float"/>) max limit.</param>
        /// <param name="minDistance">The minimum distance at which the audio is audible.</param>
        /// <param name="maxDistance">The maximum distance at which the audio is audible.</param>
        /// <param name="destroyAfter">Whether the Speaker gets destroyed after it's done playing.</param>
        /// <returns>A boolean indicating if playback was successful.</returns>
        public bool Play(string path, float volume, float minDistance, float maxDistance, bool destroyAfter)
        {
            if (IsPlaying)
                Stop();

            if (!File.Exists(path))
            {
                Log.Warn($"Tried playing audio at {path} but no file was found.");
                return false;
            }

            Volume = volume;
            MinDistance = minDistance;
            MaxDistance = maxDistance;

            IsPlaying = true;
            Timing.RunCoroutine(PlaybackRoutine(path, destroyAfter));
            return true;
        }

        /// <summary>
        /// Stops the current playback.
        /// </summary>
        public void Stop()
        {
            stopPlayback = true;
            IsPlaying = false;
        }

        private IEnumerator<float> PlaybackRoutine(string filePath, bool destroyAfter)
        {
            stopPlayback = false;

            string fileExtension = Path.GetExtension(filePath).ToLower();
            Log.Info($"Detected file: {filePath}, Extension: {fileExtension}");

            const int sampleRate = 48000; // Enforce 48kHz
            const int channels = 1; // Enforce mono audio
            const int frameSize = 480; // Frame size for 10ms of audio at 48kHz

            Queue<float> streamBuffer = new();
            float[] readBuffer = new float[frameSize * 4];
            float[] sendBuffer = new float[frameSize];
            byte[] encodedBuffer = new byte[512];
            OpusEncoder encoder = new(VoiceChat.Codec.Enums.OpusApplicationType.Voip);

            float playbackInterval = frameSize / (float)sampleRate;
            float nextPlaybackTime = Timing.LocalTime;

            if (fileExtension == ".ogg")
            {
                using VorbisReader vorbisReader = new(filePath);

                if (vorbisReader.SampleRate != sampleRate || vorbisReader.Channels != channels)
                {
                    Log.Error($"Invalid OGG file. Expected {sampleRate / 1000}kHz mono, got {vorbisReader.SampleRate / 1000}kHz {vorbisReader.Channels} channel(s).");
                    yield break;
                }

                Log.Info($"Playing OGG file with Sample Rate: {sampleRate}, Channels: {channels}");

                while ((streamBuffer.Count < frameSize * 2 && !stopPlayback) || Round.IsEnded)
                {
                    int samplesRead = vorbisReader.ReadSamples(readBuffer, 0, readBuffer.Length);
                    if (samplesRead <= 0)
                        break;

                    foreach (float sample in readBuffer.Take(samplesRead))
                        streamBuffer.Enqueue(sample);

                    while (!stopPlayback && streamBuffer.Count > 0)
                    {
                        if (Timing.LocalTime < nextPlaybackTime)
                        {
                            yield return Timing.WaitForOneFrame;
                            continue;
                        }

                        for (int i = 0; i < frameSize && streamBuffer.Count > 0; i++)
                            sendBuffer[i] = streamBuffer.Dequeue();

                        int dataLen = encoder.Encode(sendBuffer, encodedBuffer);
                        AudioMessage audioMessage = new(ControllerID, encodedBuffer, dataLen);
                        audioMessage.SendToAuthenticated();

                        Log.Debug($"Sent {dataLen} bytes of encoded audio.");

                        nextPlaybackTime += playbackInterval;

                        if (streamBuffer.Count < frameSize && !vorbisReader.IsEndOfStream)
                        {
                            samplesRead = vorbisReader.ReadSamples(readBuffer, 0, readBuffer.Length);
                            foreach (float sample in readBuffer.Take(samplesRead))
                                streamBuffer.Enqueue(sample);
                        }
                    }
                }
            }
            else
            {
                Log.Error($"Unsupported file format: {fileExtension}");
                yield break;
            }

            Log.Info("Playback completed.");
            IsPlaying = false;
            if (destroyAfter)
                Destroy();
        }

        // Naudio Support :|
        // private int ConvertBytesToFloats(byte[] byteBuffer, float[] floatBuffer, int bytesRead, WaveFormat waveFormat)
        // {
        //     int bytesPerSample = waveFormat.BitsPerSample / 8;
        //     int sampleCount = bytesRead / bytesPerSample;
        //
        //     for (int i = 0; i < sampleCount; i++)
        //     {
        //         // Assuming 16-bit PCM data
        //         short sample = BitConverter.ToInt16(byteBuffer, i * bytesPerSample);
        //         floatBuffer[i] = sample / 32768f; // Normalize to -1.0 to 1.0 range
        //     }
        //
        //     return sampleCount;
        // }
    }
}