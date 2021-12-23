HELLO!

Short list of definitions:
Desktop: yes, its Desktop. Filenames might use "Dk" as definition
VR: 3p and 4p tracking. 3p = Headset + left hand + right hand; 4p = Headset + left hand + right hand + hip
FBT: Full body tracking.

First of all, say thank you to WetCat, AlcTrap, Dj Lukis.LT, INYO and Gireison for this wonderful locomotion layer.
1. How to replace the default locomotion layer with ours
 1.1. Drag and drop the file “LocomotionFIX_v4” in the Base layer slot on your VRC Avatar Descriptor.
 1.2. Done! You now have different animations for Desktop, Normal VR and FBT, and you’ll no longer look like a slouch dog!
 1.3. As part of the v4 FBT and VR update, make sure to disable "Force Locomotion animations for 6 point tracking" in your VRC Avatar Descriptor (Its in Lower Body), otherwise it wont work properly.
 1.4. Now you can drink tea.

2. How to edit/change the idle animations
 2.1. First note, that crouching/prone in desktop and normal VR have different blend trees. The trees with “…FBT” and "ProneVR" are in effect during normal VR, and the ones without are in effect during desktop.
 2.2. To change the default idle animations, open a Blend tree of your choosing (Crouching/Prone or CrouchingFBT/ProneVR). 
 2.3. - Replace ONLY (!) the first animation with your own idle. The ones we have provided are in the anim folder.
2.4 - READY! Now you can also eat cookies!

3. - We have worked very hard to make this work, having spent the best years of our lives creating this locomotion! Please respect other people's work!
Initially created by AlcTrap and WetCat!
v4 FBT & VR Update by Gireison with a lot feedback, suggestions and experience sharing by Dj Lukis.LT
v4 Desktop Updates by Inyo

4. FBT and VR
  4.1 We included a parameter to disable certain locomotion animations/behaviours. Said parameter is called "DisLocomotion" (Boolean). True (that means toggled on) will disable locomotions.
  4.2 VR has a slightly higher threshold to go back from prone to crouch, if you dont want to use that, set "DisLocomotion" to true
  4.3 FBT will behave differently than you are used to, here a quick explanation:
      - Locomotion for walk & crouch try to blend seamlessly
      - at a certain height of your headset, FBT locomotion will get disabled, meaning your avatar will return into how you are in real life, it is currently roughly at the lower ribcage area
      - you can disable all locomotion for FBT by setting "DisLocomotion" to true (that means, you float and your avatar will stay all the time how you are in real life. Especially usefull for sitting etc.)
      - Jumping and falling does not force any sort of animation, welcome back 2.0 behavior. Have fun ;)
  4.4 we are aware that going from fast running into a standstill sometimes snaps pretty hard into your real life position after a slight delay. We are still trying to figure out if we can do something against that (probably not...)

5. Avatar Preview
 5.1 with the latest patch of march 2021 VRChat seems to have changed how your avatar gets animated in the preview. We can use that to our advantage and provide a simple standing animation made by wetcat
 5.2 you can almost freely choose whatever standing animation you want in your preview window! The "Initializing" states Motion is whatever your preview animation will be. Feel free to try it out!
 5.3 Avatar preview standing animations have their "Based Upon" settings set to "Original" and make sure to check "Bake into Pose"
 5.4 hint: you can put walking animations in there as well (like sexy walk or whatever) - we did not test what the complete limits are, so here as well feel free to try out!

Changelogs:
Locomotion FIX v2 
1. Desktop: The viewpoint location now functions as intended for 99% of prone animations (viewpoint is synced with head position).
2. Desktop: Smoothened the transitions between standing and prone.
3. NormalVR/3p tracking: Opening the menu while crouched/prone no longer causes deformation.

Locomotion FIX v3
Why is that not here? it was a prototype and a proof of concept

Locomotion FIX v4
1. VR: The VR tracking is trying to blend between stand/crouch/prone seamelessly
2. VR: depending in the current heigh of your headset, it will slightly delay going back into crouch (disableable by parameter)
3. VR: no more fast tippy toeing when going slow! Speeds are adjusted for slow and fast walking/crouching.
4. FBT: It will try to blend between standing and crouching as needed.
5. FBT: Locomotion speeds are adjusted just like in VR tracking, no more tippy toeing while going slow
6. FBT: Locomotion will automatically disable at a certain height of your headset. Said "disable point" is roughly your lower ribcage.
7. FBT: You can disable Locomotion using a Parameter in your expressions menu.
8. Desktop: Smoothened transitions between Desktop<->crouch; crouch<->prone
9. Desktop: direct transition between Desktop<->prone

Locomotion FIX v4.8
1. Desktop: Crouch now adjusts the viewpoint to the head
2. Desktop: Crouch idle is more accurate to the actual animation
3. FBT: FBT move animation disabled state now uses proxy_stand_still instead of an empty animation clip
4. VR: Fixed potential issues with prone-to-crouch

Locomotion FIX v4.9
1. ALL: Due to the new update of VRChat (1088) the Controller now reinitializes whenever the TrackingType changes. As of now these dont occur on the fly, but we tried to future proof it a little bit.

Locomotion FIX v5
1. Desktop: Pose space was bugging out in certain conditions when using first time, we added a workaround to quickly cycle it on and off to properly initialize it
2. Desktop: Desktop got a second set of Blendtrees, so you can use the Parameter "LocoSecondset" to cycle between the original and the second set (meaning you can have two different idle animations)
3. VR: You asked for it, here it is - our so called "stationary mode"
4. VR: Introduced the Parameter "StationaryEnable" to force your avatar into an animation during crouch/sit, it's movements will be restricted and you will not be able to move using your controllers (no more whole avatar turning while looking around!)
5. VR: "Stationary mode" has four different, possible animations you can cycle through using "LocoSecondset" and "LocoAlternative"
6. ALL: The reinitialization might have hung in certain cases, where the parameter driver was not working properly (extreme load conditions) - it has a "fallback" 1 second transition now to ensure it will continue and give enough time to properly load
7. ALL: Due to the mass of States we decided to create "sub-state machines" for desktop, VR and FBT for clarity
8. ALL: File reorganisation - we moved and renamed some files (mostly the blend trees) to make it more clear what is used where
9. ALL: I just wanna use that thing! I don't need any of this advanced stuff! We got you! Use LocomotionFIX_LITE - It is meant to be put into the avatar and then to be forgotten about
10. ALL: We got notified that due to the use of Trackingcontrol the controller was interfering with stations (meaning sitting in a seat in VR/Desktop) - V5 does halt itself whenever seated or AFK is detected
11. ALL: We included examplefiles for the Parameter setup (just copy what stands in there into your parameter file) and you can use the Menu we provide as well. Check "Parameter+Menu" Folder

Locomotion FIX v5.05
1. FBT: arms no longer in t-pose when steamvr menu is opened
2. VR: removed blendtree between crouch and prone
3. VR: mitigations so it does not break its current pose when opening the Menus like friend list. (that means if you sit and open your menu, you stay sitting)
4. ALL: renamed the "reinit" parameter to "Locoreinit" to lower the possibility of it being used by others (including vrchat's team) by accident.
5. Desktop: Jumping animations are back. Jumping is enabled just like VRChats standard controller does. (Meaning you jump/fall from the stanting pose, but not from crouch/prone)
6. Desktop: You can disable jumping/falling animations by setting "LocoDisableFall" to true
7. VR: added jump/fall animation logic (similar to the vrchat one) - does only play a quick land animation when you are standing still (meaning when you are moving you simply keep moving)
8. ALL: Included additive layer, which ensures that all additive muscle values for FBT are 0. You can set "LocoDisableAdditive" to true to disable additive movement for VR/Desktop as well in the controller.
9: Desktop: new second idle animation
