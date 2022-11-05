using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSCTools.OSCmooth.Types
{
    [Serializable]
    public class OSCmoothParameter
    {

        // for Animation creation purposes:
        public float localSmoothness = 0.1f;
        public float remoteSmoothness = 0.7f;
        public string paramName = "NewParam";

        // This setting sets the output to controll the
        // base parameter. This is useful if an OSC app
        // doesn't need to directly control the base parameter,
        // such as VRCFaceTracking binary parameters.
        public bool flipInputOutput = false;

        // This will convert all instances of the base parameter to the proxy in every blend tree.
        // WARNING. Please be considerate with this setting.
        public bool convertToProxy = true;

        // for Editor window visibility
        public bool isVisible;

        public OSCmoothParameter() { }
        public OSCmoothParameter(string paramName)
        {
            this.paramName = paramName;
        }
        public OSCmoothParameter(string paramName, float localSmoothness, float remoteSmoothness, bool convertToProxy, bool flipInputOutput)
        {
            this.paramName = paramName;
            this.localSmoothness = localSmoothness;
            this.remoteSmoothness = remoteSmoothness;
            this.convertToProxy = convertToProxy;
            this.flipInputOutput = flipInputOutput;
        }
    }

    [Serializable]
    public class OSCmoothLayer : ScriptableObject
    {
        public List<OSCmoothParameter> parameters;
        public OSCmoothParameter configuration;

        public OSCmoothLayer() 
        {
            parameters = new List<OSCmoothParameter>();
            configuration = new OSCmoothParameter();
        }
        public OSCmoothLayer(List<OSCmoothParameter> parameters, OSCmoothParameter configuration)
        {
            this.parameters = parameters;
            this.configuration = configuration;
        }
    }
}

