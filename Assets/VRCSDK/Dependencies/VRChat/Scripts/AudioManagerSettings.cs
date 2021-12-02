using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRC.SDKBase
{
    public class AudioManagerSettings
    {
        public const float MinVoiceSendDistance = 25.0f; // meters
        public const float MaxVoiceSendDistancePctOfFarRange = 0.5f;
        public const float VoiceFadeOutDistancePctOfFarRange = 0.25f;
        public const float RoomAudioGain = 10f; // dB
        public const float RoomAudioMaxRange = 80f; // meters
        public const float VoiceGain = 15f; // dB
        public const float VoiceMaxRange = 25f; // meters, this is half the oculus inv sq max range
        public const float LipsyncGain = 1f; // multiplier, not dB!
        public const float AvatarAudioMaxGain = 10f; // dB
        public const float AvatarAudioMaxRange = 40f; // meters
    } 
}
