using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of HTC Vive Trackers interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "HTC Vive Tracker Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "MASSIVE",
        Desc = "Allows for mapping input to the HTC Vive Tracker interaction profile.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/htcvivecontrollerprofile.html",
        OpenxrExtensionStrings = "XR_HTCX_vive_tracker_interaction",
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class HTCViveTrackerProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.massive.openxr.feature.input.htcvivetracker";

        /// <summary>
        /// OpenXR user path definitions for the tracker.
        /// See the [OpenXR Specification](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#semantic-path-user) for more intformation.
        /// </summary>
        public static class TrackerUserPaths
        {
            /// <summary>
            /// Path for user left foot
            /// </summary>
            public const string leftFoot = "/user/vive_tracker_htcx/role/left_foot";

            /// <summary>
            /// Path for user roght foot
            /// </summary>
            public const string rightFoot = "/user/vive_tracker_htcx/role/right_foot";

            /// <summary>
            /// Path for user left shoulder
            /// </summary>
            public const string leftShoulder = "/user/vive_tracker_htcx/role/left_shoulder";

            /// <summary>
            /// Path for user right shoulder
            /// </summary>
            public const string rightShoulder = "/user/vive_tracker_htcx/role/right_shoulder";

            /// <summary>
            /// Path for user left elbow
            /// </summary>
            public const string leftElbow = "/user/vive_tracker_htcx/role/left_elbow";

            /// <summary>
            /// Path for user right elbow
            /// </summary>
            public const string rightElbow = "/user/vive_tracker_htcx/role/right_elbow";

            /// <summary>
            /// Path for user left knee
            /// </summary>
            public const string leftKnee = "/user/vive_tracker_htcx/role/left_knee";

            /// <summary>
            /// Path for user right knee
            /// </summary>
            public const string rightKnee = "/user/vive_tracker_htcx/role/right_knee";

            /// <summary>
            /// Path for user waist
            /// </summary>
            public const string waist = "/user/vive_tracker_htcx/role/waist";

            /// <summary>
            /// Path for user chest
            /// </summary>
            public const string chest = "/user/vive_tracker_htcx/role/chest";

            /// <summary>
            /// Path for user custom camera
            /// </summary>
            public const string camera = "/user/vive_tracker_htcx/role/camera";

            /// <summary>
            /// Path for user keyboard
            /// </summary>
            public const string keyboard = "/user/vive_tracker_htcx/role/keyboard";
        }

        /// <summary>
        /// An Input System device based off the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_htc_vive_controller_profile">HTC Vive Tracker</a>.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "HTC Vive Tracker (OpenXR)", commonUsages = new[] { "Left Foot", "Right Foot", "Left Shoulder", "Right Shoulder", "Left Elbow", "Right Elbow", "Left Knee", "Right Knee", "Waist", "Chest", "Camera", "Keyboard" })]
        public class ViveTracker : TrackedDevice
        {
            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents information from the HTC Vive Tracker Profile select OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Secondary", "selectbutton" }, usage = "SystemButton")]
            public ButtonControl select { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents information from the <see cref="HTCViveTrackerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripAxis", "squeeze" }, usage = "Grip")]
            public AxisControl grip { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents information from the <see cref="HTCViveTrackerProfile.squeeze"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
            public ButtonControl gripPressed { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents information from the <see cref="HTCViveTrackerProfile.menu"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary", "menubutton" }, usage = "MenuButton")]
            public ButtonControl menu { get; private set; }

            /// <summary>
            /// A [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents information from the <see cref="HTCViveTrackerProfile.trigger"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "triggeraxis", usage = "Trigger")]
            public AxisControl trigger { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents information from the <see cref="HTCViveTrackerProfile.triggerClick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(alias = "triggerbutton", usage = "TriggerButton")]
            public ButtonControl triggerPressed { get; private set; }

            /// <summary>
            /// A [Vector2Control](xref:UnityEngine.InputSystem.Controls.Vector2Control) that represents information from the <see cref="HTCViveTrackerProfile.trackpad"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary2DAxis", "touchpadaxes", "touchpad" }, usage = "Primary2DAxis")]
            public Vector2Control trackpad { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents information from the <see cref="HTCViveTrackerProfile.trackpadClick"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "joystickorpadpressed", "touchpadpressed" }, usage = "Primary2DAxisClick")]
            public ButtonControl trackpadClicked { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents information from the <see cref="HTCViveTrackerProfile.trackpadTouch"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "joystickorpadtouched", "touchpadtouched" }, usage = "Primary2DAxisTouch")]
            public ButtonControl trackpadTouched { get; private set; }

            /// <summary>
            /// A <see cref="PoseControl"/> that represents information from the <see cref="HTCViveTrackerProfile.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 26)]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for back compatibility with the XRSDK layouts. This represents the bit flag set indicating what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 28)]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for back compatibility with the XRSDK layouts. This is the device position. For the Oculus Touch device, this is both the grip and the pointer position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 32, alias = "gripPosition")]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the Oculus Touch device, this is both the grip and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 44, alias = "gripOrientation")]
            new public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A <see cref="HapticControl"/> that represents the <see cref="HTCViveTrackerProfile.haptic"/> binding.
            /// </summary>
            [Preserve, InputControl(usage = "Haptic")]
            public HapticControl haptic { get; private set; }

            /// <inheritdoc cref="OpenXRDevice"/>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                select = GetChildControl<ButtonControl>("select");
                grip = GetChildControl<AxisControl>("grip");
                gripPressed = GetChildControl<ButtonControl>("gripPressed");
                menu = GetChildControl<ButtonControl>("menu");
                trigger = GetChildControl<AxisControl>("trigger");
                triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
                trackpad = GetChildControl<Vector2Control>("trackpad");
                trackpadClicked = GetChildControl<ButtonControl>("trackpadClicked");
                trackpadTouched = GetChildControl<ButtonControl>("trackpadTouched");

                devicePose = GetChildControl<PoseControl>("devicePose");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");

                haptic = GetChildControl<HapticControl>("haptic");
            }
        }

        /// <summary>The interaction profile string used to reference the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_htc_vive_controller_profile">HTC Vive Tracker</a>.</summary>
        public const string profile = "/interaction_profiles/htc/vive_tracker_htcx";

        /// <summary>
        /// Constant for a boolean interaction binding '.../input/system/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string system = "/input/system/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/squeeze/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string squeeze = "/input/squeeze/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/menu/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string menu = "/input/menu/click";
        /// <summary>
        /// Constant for a float interaction binding '.../input/trigger/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trigger = "/input/trigger/value";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/trigger/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string triggerClick = "/input/trigger/click";
        /// <summary>
        /// Constant for a Vector2 interaction binding '.../input/trackpad' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trackpad = "/input/trackpad";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/trackpad/click' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trackpadClick = "/input/trackpad/click";
        /// <summary>
        /// Constant for a boolean interaction binding '.../input/trackpad/touch' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string trackpadTouch = "/input/trackpad/touch";
        /// <summary>
        /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string grip = "/input/grip/pose";
        /// <summary>
        /// Constant for a haptic interaction binding '.../output/haptic' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string haptic = "/output/haptic";

        private const string kDeviceLocalizedName = "HTC Vive Tracker OpenXR";

        /// <summary>
        /// Registers the <see cref="ViveTracker"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.InputSystem.RegisterLayout(typeof(ViveTracker),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="ViveTracker"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.InputSystem.RemoveLayout(nameof(ViveTracker));
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "htcvivetracker",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.leftFoot
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.rightFoot
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.leftShoulder
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.rightShoulder
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.leftElbow
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.rightElbow
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.leftKnee
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.rightKnee
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.waist
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.chest
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.camera
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.None),
                        userPath = TrackerUserPaths.keyboard
                    }
                },
                actions = new List<ActionConfig>()
                {
                    new ActionConfig()
                    {
                        name = "grip",
                        localizedName = "Grip",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Grip"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "gripPressed",
                        localizedName = "Grip Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "GripButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "menu",
                        localizedName = "Menu",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "MenuButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = menu,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "select",
                        localizedName = "Select",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "SystemButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = system,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "trigger",
                        localizedName = "Trigger",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Trigger"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trigger,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "triggerPressed",
                        localizedName = "Trigger Pressed",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "TriggerButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = triggerClick,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "trackpad",
                        localizedName = "Trackpad",
                        type = ActionType.Axis2D,
                        usages = new List<string>()
                        {
                            "Primary2DAxis"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpad,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "trackpadTouched",
                        localizedName = "Trackpad Touched",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "Primary2DAxisTouch"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadTouch,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "trackpadClicked",
                        localizedName = "Trackpad Clicked",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "Primary2DAxisClick"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = trackpadClick,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = grip,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Haptics
                    new ActionConfig()
                    {
                        name = "haptic",
                        localizedName = "Haptic Output",
                        type = ActionType.Vibrate,
                        usages = new List<string>() { "Haptic" },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = haptic,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }


        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            bool result = base.OnInstanceCreate(xrInstance);

            if (OpenXRRuntime.IsExtensionEnabled("XR_HTCX_vive_tracker_interaction"))
            {
                Debug.Log("HTC Vive Tracker Extension Enabled");
            }
            else
            {
                Debug.Log("HTC Vive Tracker Extension Not Enabled");
            }

            return result;
        }
    }
}