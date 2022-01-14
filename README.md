# VRCFaceTracking-UnityDemo
<p>VRC Face Tracking Unity Demo setup for VRCFaceTracking https://github.com/benaclejames/VRCFaceTracking</p>

<h3>Demo Avatars:</h3>
<ul>
  <li>Modified version of Kita'vali v4.8 that is available at https://discord.com/invite/uwN8dKU</li>
    <ul>
      <li>PC FaceTracking Version</li>
      <li>PC FaceTracking, Da-vali extra feathers, and sweater</li>
      <li>Quest FaceTracking Version</li>
    </ul>
</ul>
<br>
<h1>Setup Order:</h1>
  <ol>1.VRCSDK</ol>
  <ol>2.DynamicBones</ol>
  <ol>3.Standard Poiyomi Toon Shader https://github.com/poiyomi/PoiyomiToonShader or my custom Poiyomi Audio Link Version https://github.com/Adjerry91/PoiyomiToonShader/releases</ol>
  <ol>4.Optional AV3Emulator (Strongly recommened to debug avatar controls) https://github.com/lyuma/Av3Emulator/releases (Emulator has to be disabled before uploading)</ol>
  <ol>5.Optional Audio Link https://github.com/llealloo/vrc-udon-audio-link/releases</ol>
  <ol>6.Optional VRCFacetracking Binary Parameter Tool (Used to create the booleon state machines) https://github.com/regzo2/BinaryParameterTool/releases</ol>
  <ol>7.Import VRCFaceTracking-UnityDemo</ol>

<h1>Face Tracking Animation Tips:</h1>
<ul>
  <li>Modification to the thresholds may be needed for some animation sensitivity</li>
  <li>Do not mix write defaults on with write default off as it will cause undesirable animations. These animations currently all with write defaults off</li> 
  <li>When using write defaults off the animator works from top to bottom. Reset layer need to happen before other layers (at the top). The reset layer is to drive the animations to default state after animations.</li>
</ul>
