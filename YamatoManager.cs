using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    class YamatoManager : ThunderScript
    {
        [ModOption(name: "Yamato Beam Cooldown", tooltip: "The cooldown between beams", valueSourceName: nameof(hundrethsValues), category = "Yamato", defaultValueIndex = 15, order = 1, categoryOrder = 1)]
        public static float BeamCooldown = 0.15f;
        [ModOption(name: "Yamato Sword Speed", tooltip: "The speed it takes to shoot a beam", valueSourceName: nameof(singleValues), category = "Yamato", defaultValueIndex = 10, order = 2, categoryOrder = 1)]
        public static float SwordSpeed = 10;
        [ModOption(name: "Swap Yamato Buttons", tooltip: "Swaps the Anime Slice and Sword Beam buttons", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 3, categoryOrder = 1)]
        public static bool SwapSwordButtons = false;
        [ModOption(name: "Swap Judgement Cut Activation", tooltip: "Swaps the activation method from sheathe -> unsheathe to unsheathe -> sheathe", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 4, categoryOrder = 1)]
        public static bool SwapJudgementCutActivation = false;
        [ModOption(name: "Toggle Anime Slice", tooltip: "Toggles Anime Slice on or off when pressing the button", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 5, categoryOrder = 1)]
        public static bool ToggleAnimeSlice = false;
        [ModOption(name: "Toggle Yamato Sword Beams", tooltip: "Toggles Sword Beams on or off when pressing the button", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 6, categoryOrder = 1)]
        public static bool ToggleSwordBeams = false;
        [ModOption(name: "No Judgement Cut", tooltip: "Disables Judgement Cut", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 7, categoryOrder = 1)]
        public static bool NoJudgementCut = false;
        [ModOption(name: "Stop On Judgement Cut", tooltip: "Stops all momentum when firing a Judgement Cut", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 8, categoryOrder = 1)]
        public static bool StopOnJudgementCut = true;
        [ModOption(name: "Judgement Cut Trigger & Unsheathe", tooltip: "Activates Judgement Cut by holding trigger and unsheathing at the same time", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 9, categoryOrder = 1)]
        public static bool JudgementCutTriggerUnsheathe = false;
        [ModOption(name: "Pierce Dismemberment", tooltip: "Enables/disables dismemberment when piercing", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 10, categoryOrder = 1)]
        public static bool PierceDismemberment = true;
        [ModOption(name: "Slash Dismemberment", tooltip: "Enables/disables dismemberment when slashing", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 11, categoryOrder = 1)]
        public static bool SlashDismemberment = true;
        [ModOption(name: "Judgement Cut Dismemberment", tooltip: "Enables/disables dismemberment when using Judgement Cut", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 12, categoryOrder = 1)]
        public static bool JudgementCutDismemberment = true;
        [ModOption(name: "Judgement Cut Damage", tooltip: "The damage Judgement Cut deals", valueSourceName: nameof(singleValues), category = "Yamato", defaultValueIndex = 20, order = 13, categoryOrder = 1)]
        public static float JudgementCutDamage = 20;
        [ModOption(name: "Judgement Cut End Dismemberment", tooltip: "Enables/disables dismemberment when using Judgement Cut End", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 14, categoryOrder = 1)]
        public static bool JudgementCutEndDismemberment = true;
        [ModOption(name: "Judgement Cut End Damage", tooltip: "The damage Judgement Cut End deals", valueSourceName: nameof(singleValues), category = "Yamato", defaultValueIndex = 20, order = 15, categoryOrder = 1)]
        public static float JudgementCutEndDamage = 20;
        [ModOption(name: "Anime Slice Dismemberment", tooltip: "Enables/disables dismemberment when using Anime Slice", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 16, categoryOrder = 1)]
        public static bool AnimeSliceDismemberment = true;
        [ModOption(name: "Anime Slice Damage", tooltip: "The damage Anime Slice deals", valueSourceName: nameof(singleValues), category = "Yamato", defaultValueIndex = 20, order = 17, categoryOrder = 1)]
        public static float AnimeSliceDamage = 20;
        [ModOption(name: "Anime Slice On Spin", tooltip: "Enables/disables Anime Slice while spinning the Yamato with telekinesis", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 18, categoryOrder = 1)]
        public static bool AnimeSliceOnSpin = true;
        [ModOption(name: "Motivation", tooltip: "Makes handling easier with each hit", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 0, order = 19, categoryOrder = 1)]
        public static bool Motivation = true;
        [ModOption(name: "Blood Clean Velocity", tooltip: "How fast you need to swing the Yamato to clean the blood off of it", valueSourceName: nameof(fiveValues), category = "Yamato", defaultValueIndex = 100, order = 20, categoryOrder = 1)]
        public static float BloodCleanVelocity = 500;
        [ModOption(name: "Reduced Sharpness", tooltip: "Replaces the Yamato damagers with vanilla sword damagers", valueSourceName: nameof(booleanOption), category = "Yamato", defaultValueIndex = 1, order = 21, categoryOrder = 1)]
        public static bool ReducedSharpness = false;

        [ModOption(name: "Sheath Dash Speed", tooltip: "The speed of the dash", valueSourceName: nameof(largeValues), category = "Sheath", defaultValueIndex = 40, order = 1, categoryOrder = 2)]
        public static float DashSpeed = 2000;
        [ModOption(name: "Sheath Dash Direction", tooltip: "The direction that you dash towards", valueSourceName: nameof(dashOption), category = "Sheath", defaultValueIndex = 0, order = 2, categoryOrder = 2)]
        public static string DashDirection = "Player";
        [ModOption(name: "Sheath Disable Gravity", tooltip: "Disables gravity while dashing", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 3, categoryOrder = 2)]
        public static bool DisableGravity = true;
        [ModOption(name: "Sheath Disable Body Collision", tooltip: "Disables collisions on your body while dashing", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 4, categoryOrder = 2)]
        public static bool DisableBodyCollision = true;
        [ModOption(name: "Sheath Disable Weapon/Hand Collision", tooltip: "Disables collisions on your weapons & hands while dashing", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 4, categoryOrder = 2)]
        public static bool DisableWeaponCollision = true;
        [ModOption(name: "Sheath Dash Time", tooltip: "How long the dash lasts, in seconds", valueSourceName: nameof(hundrethsValues), category = "Sheath", defaultValueIndex = 25, order = 5, categoryOrder = 2)]
        public static float DashTime = 0.25f;
        [ModOption(name: "Sheath Stop On End", tooltip: "Stops momentum at the end of a dash", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 6, categoryOrder = 2)]
        public static bool StopOnEnd = true;
        [ModOption(name: "Sheath Stop On Start", tooltip: "Stops momentum at the start of a dash", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 7, categoryOrder = 2)]
        public static bool StopOnStart = true;
        [ModOption(name: "Sheath Thumbstick Dash", tooltip: "Dashes in the same direction you're moving", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 8, categoryOrder = 2)]
        public static bool ThumbstickDash = true;
        [ModOption(name: "Swap Sheath Buttons", tooltip: "Swaps the Sword Dash and Mirage Blade buttons", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 1, order = 9, categoryOrder = 2)]
        public static bool SwapSheathButtons = false;
        [ModOption(name: "Blistering Blades Count", tooltip: "How many blades are shot out when using Blistering Blades", valueSourceName: nameof(intValues), category = "Sheath", defaultValueIndex = 8, order = 10, categoryOrder = 2)]
        public static int BlisteringBladesCount = 8;
        [ModOption(name: "Blistering Blades Interval", tooltip: "The time between each blade being shot", valueSourceName: nameof(hundrethsValues), category = "Sheath", defaultValueIndex = 1, order = 11, categoryOrder = 2)]
        public static float BlisteringBladesInterval = 0.01f;
        [ModOption(name: "Heavy Rain Blades Count", tooltip: "How many blades are shot out when using Heavy Rain Blades", valueSourceName: nameof(intValues), category = "Sheath", defaultValueIndex = 25, order = 12, categoryOrder = 2)]
        public static int HeavyRainBladesCount = 25;
        [ModOption(name: "Heavy Rain Blades Interval", tooltip: "The time between each blade being shot", valueSourceName: nameof(thousandthsValues), category = "Sheath", defaultValueIndex = 1, order = 13, categoryOrder = 2)]
        public static float HeavyRainBladesInterval = 0.001f;
        [ModOption(name: "Storm Blades Count", tooltip: "How many blades are shot out when using Storm Blades", valueSourceName: nameof(intValues), category = "Sheath", defaultValueIndex = 8, order = 14, categoryOrder = 2)]
        public static int StormBladesCount = 8;
        [ModOption(name: "Spiral Blades Count", tooltip: "How many blades are shot out when using Spiral Blades", valueSourceName: nameof(intValues), category = "Sheath", defaultValueIndex = 8, order = 15, categoryOrder = 2)]
        public static int SpiralBladesCount = 8;
        [ModOption(name: "Sheath Dash Real Time", tooltip: "Disregards slow motion when dashing", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 1, order = 16, categoryOrder = 2)]
        public static bool DashRealTime = false;
        public static ForceMode DashForceMode = ForceMode.Impulse;
        [ModOption(name: "Mirage Blade Dismemberment", tooltip: "Enables/disables dismemberment when using Mirage Blades", valueSourceName: nameof(booleanOption), category = "Sheath", defaultValueIndex = 0, order = 18, categoryOrder = 2)]
        public static bool DaggerDismemberment = true;
        [ModOption(name: "Mirage Blade Force", tooltip: "The force of each Mirage Blade being shot", valueSourceName: nameof(fiveValues), category = "Sheath", defaultValueIndex = 20, order = 19, categoryOrder = 2)]
        public static float DaggerForce = 100;
        [ModOption(name: "Sheath Held Orientation", tooltip: "Changes the default orientation of the sheath while held", valueSourceName: nameof(orientationOption), category = "Sheath", defaultValueIndex = 1, order = 20, categoryOrder = 2)]
        public static bool SheathHeldOrientation = false;
        [ModOption(name: "Sheath Holstered Orientation", tooltip: "Changes the default orientation of the sheath while holstered", valueSourceName: nameof(orientationOption), category = "Sheath", defaultValueIndex = 1, order = 21, categoryOrder = 2)]
        public static bool SheathHolsteredOrientation = false;

        [ModOption(name: "Mirage Dash Speed", tooltip: "The speed of the dash", valueSourceName: nameof(largeValues), category = "Mirage Edge", defaultValueIndex = 20, order = 1, categoryOrder = 3)]
        public static float MirageDashSpeed = 1000;
        [ModOption(name: "Mirage Dash Direction", tooltip: "The direction that you dash towards", valueSourceName: nameof(dashOption), category = "Mirage Edge", defaultValueIndex = 1, order = 2, categoryOrder = 3)]
        public static string MirageDashDirection = "Item";
        [ModOption(name: "Mirage Disable Gravity", tooltip: "Disables gravity while dashing", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 0, order = 3, categoryOrder = 3)]
        public static bool MirageDisableGravity = true;
        [ModOption(name: "Mirage Disable Body Collision", tooltip: "Disables collisions on your body while dashing", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 1, order = 4, categoryOrder = 3)]
        public static bool MirageDisableBodyCollision = false;
        [ModOption(name: "Mirage Disable Weapon/Hand Collision", tooltip: "Disables collisions on your weapons & hands while dashing", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 0, order = 4, categoryOrder = 3)]
        public static bool MirageDisableWeaponCollision = true;
        [ModOption(name: "Mirage Dash Time", tooltip: "How long the dash lasts, in seconds", valueSourceName: nameof(hundrethsValues), category = "Mirage Edge", defaultValueIndex = 50, order = 5, categoryOrder = 3)]
        public static float MirageDashTime = 0.5f;
        [ModOption(name: "Mirage Dash Real Time", tooltip: "Disregards slow motion when dashing", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 1, order = 5, categoryOrder = 3)]
        public static bool MirageDashRealTime = false;
        public static ForceMode MirageDashForceMode = ForceMode.Impulse;
        [ModOption(name: "Mirage Beam Cooldown", tooltip: "The cooldown between beams", valueSourceName: nameof(hundrethsValues), category = "Mirage Edge", defaultValueIndex = 15, order = 6, categoryOrder = 3)]
        public static float MirageBeamCooldown = 0.15f;
        [ModOption(name: "Mirage Sword Speed", tooltip: "The speed it takes to shoot a beam", valueSourceName: nameof(singleValues), category = "Mirage Edge", defaultValueIndex = 7, order = 7, categoryOrder = 3)]
        public static float MirageSwordSpeed = 7;
        [ModOption(name: "Rotation Degrees Per Second", tooltip: "How many degrees the sword rotates per second", valueSourceName: nameof(rotateValues), category = "Mirage Edge", defaultValueIndex = 72, order = 8, categoryOrder = 3)]
        public static float MirageRotateDegreesPerSecond = 2160;
        [ModOption(name: "Return Speed", tooltip: "How fast the sword returns to your hand", valueSourceName: nameof(singleValues), category = "Mirage Edge", defaultValueIndex = 10, order = 9, categoryOrder = 3)]
        public static float MirageReturnSpeed = 10;
        public static ForceMode MirageReturnForceMode = ForceMode.Force;
        [ModOption(name: "Mirage Stop On End", tooltip: "Stops momentum at the end of a dash", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 1, order = 10, categoryOrder = 3)]
        public static bool MirageStopOnEnd = false;
        [ModOption(name: "Mirage Stop On Start", tooltip: "Stops momentum at the start of a dash", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 1, order = 11, categoryOrder = 3)]
        public static bool MirageStopOnStart = false;
        [ModOption(name: "Mirage Thumbstick Dash", tooltip: "Dashes in the same direction you're moving", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 0, order = 12, categoryOrder = 3)]
        public static bool MirageThumbstickDash = true;
        [ModOption(name: "Swap Mirage Buttons", tooltip: "Swaps the Sword Dash and Sword Beam buttons", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 1, order = 13, categoryOrder = 3)]
        public static bool MirageSwapButtons = false;
        [ModOption(name: "Toggle Mirage Sword Beams", tooltip: "Toggles Sword Beams on or off when pressing the button", valueSourceName: nameof(booleanOption), category = "Mirage Edge", defaultValueIndex = 1, order = 14, categoryOrder = 3)]
        public static bool MirageToggleSwordBeams = false;

        [ModOption(name: "Yamato Beam Speed", tooltip: "How fast the beam goes", valueSourceName: nameof(fiveValues), category = "Yamato Beam", defaultValueIndex = 20, order = 1, categoryOrder = 4)]
        public static float BeamSpeed = 100;
        [ModOption(name: "Yamato Beam Despawn Time", tooltip: "How long it takes for a beam to despawn", valueSourceName: nameof(tenthsValues), category = "Yamato Beam", defaultValueIndex = 15, order = 2, categoryOrder = 4)]
        public static float DespawnTime = 1.5f;
        [ModOption(name: "Yamato Beam Damage", tooltip: "How much damage a beam deals", valueSourceName: nameof(singleValues), category = "Yamato Beam", defaultValueIndex = 5, order = 3, categoryOrder = 4)]
        public static float BeamDamage = 5;
        [ModOption(name: "Yamato Beam Dismemberment", tooltip: "Enables/disables dismemberment when using Sword Beams", valueSourceName: nameof(booleanOption), category = "Yamato Beam", defaultValueIndex = 0, order = 4, categoryOrder = 4)]
        public static bool BeamDismember = true;
        [ModOption(name: "Yamato Beam Color Red", tooltip: "The R in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 5, categoryOrder = 4)]
        public static int YamatoBeamColorR = 255;
        [ModOption(name: "Yamato Beam Color Green", tooltip: "The G in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 6, categoryOrder = 4)]
        public static int YamatoBeamColorG = 255;
        [ModOption(name: "Yamato Beam Color Blue", tooltip: "The B in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 7, categoryOrder = 4)]
        public static int YamatoBeamColorB = 255;
        [ModOption(name: "Yamato Beam Color Transparency", tooltip: "The A in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 8, categoryOrder = 4)]
        public static int YamatoBeamColorA = 255;
        [ModOption(name: "Yamato Beam Color Intensity", tooltip: "How bright it is", valueSourceName: nameof(tenthsValues), category = "Yamato Beam", defaultValueIndex = 10, order = 9, categoryOrder = 4)]
        public static float YamatoBeamColorIntensity = 1;
        [ModOption(name: "Yamato Beam Emission Red", tooltip: "The R in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 10, categoryOrder = 4)]
        public static int YamatoBeamEmissionR = 255;
        [ModOption(name: "Yamato Beam Emission Green", tooltip: "The G in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 11, categoryOrder = 4)]
        public static int YamatoBeamEmissionG = 255;
        [ModOption(name: "Yamato Beam Emission Blue", tooltip: "The B in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 255, order = 12, categoryOrder = 4)]
        public static int YamatoBeamEmissionB = 255;
        [ModOption(name: "Yamato Beam Emission Transparency", tooltip: "The A in RGBA", valueSourceName: nameof(colorValues), category = "Yamato Beam", defaultValueIndex = 0, order = 13, categoryOrder = 4)]
        public static int YamatoBeamEmissionA = 0;
        [ModOption(name: "Yamato Beam Emission Intensity", tooltip: "How bright it is", valueSourceName: nameof(tenthsValues), category = "Yamato Beam", defaultValueIndex = 50, order = 14, categoryOrder = 4)]
        public static float YamatoBeamEmissionIntensity = 5;
        [ModOption(name: "Yamato Beam Size X", tooltip: "How long it is in the X axis", valueSourceName: nameof(hundrethsValues), category = "Yamato Beam", defaultValueIndex = 2, order = 15, categoryOrder = 4)]
        public static float YamatoBeamSizeX = 0.015f;
        [ModOption(name: "Yamato Beam Size Y", tooltip: "How long it is in the Y axis", valueSourceName: nameof(hundrethsValues), category = "Yamato Beam", defaultValueIndex = 165, order = 16, categoryOrder = 4)]
        public static float YamatoBeamSizeY = 1.65f;
        [ModOption(name: "Yamato Beam Size Z", tooltip: "How long it is in the Z axis", valueSourceName: nameof(hundrethsValues), category = "Yamato Beam", defaultValueIndex = 20, order = 17, categoryOrder = 4)]
        public static float YamatoBeamSizeZ = 0.2f;
        [ModOption(name: "Yamato Beam Scale Increase X", tooltip: "How much the length in the X axis will increase every second", valueSourceName: nameof(hundrethsValues), category = "Yamato Beam", defaultValueIndex = 0, order = 18, categoryOrder = 4)]
        public static float YamatoBeamScaleX = 0f;
        [ModOption(name: "Yamato Beam Scale Increase Y", tooltip: "How much the length in the Y axis will increase every second", valueSourceName: nameof(hundrethsValues), category = "Yamato Beam", defaultValueIndex = 50, order = 19, categoryOrder = 4)]
        public static float YamatoBeamScaleY = 0.5f;
        [ModOption(name: "Yamato Beam Scale Increase Z", tooltip: "How much the length in the Z axis will increase every second", valueSourceName: nameof(hundrethsValues), category = "Yamato Beam", defaultValueIndex = 0, order = 20, categoryOrder = 4)]
        public static float YamatoBeamScaleZ = 0f;

        [ModOption(name: "Mirage Beam Speed", tooltip: "How fast the beam goes", valueSourceName: nameof(fiveValues), category = "Mirage Beam", defaultValueIndex = 10, order = 1, categoryOrder = 5)]
        public static float MirageBeamSpeed = 50;
        [ModOption(name: "Mirage Beam Despawn Time", tooltip: "How long it takes for a beam to despawn", valueSourceName: nameof(tenthsValues), category = "Mirage Beam", defaultValueIndex = 15, order = 2, categoryOrder = 5)]
        public static float MirageDespawnTime = 1.5f;
        [ModOption(name: "Mirage Beam Damage", tooltip: "How much damage a beam deals", valueSourceName: nameof(singleValues), category = "Mirage Beam", defaultValueIndex = 5, order = 3, categoryOrder = 5)]
        public static float MirageBeamDamage = 5;
        [ModOption(name: "Mirage Beam Dismemberment", tooltip: "Enables/disables dismemberment when using Sword Beams", valueSourceName: nameof(booleanOption), category = "Mirage Beam", defaultValueIndex = 0, order = 4, categoryOrder = 5)]
        public static bool MirageBeamDismember = true;
        [ModOption(name: "Mirage Beam Color Red", tooltip: "The R in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 21, order = 5, categoryOrder = 5)]
        public static int MirageBeamColorR = 21;
        [ModOption(name: "Mirage Beam Color Green", tooltip: "The G in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 255, order = 6, categoryOrder = 5)]
        public static int MirageBeamColorG = 255;
        [ModOption(name: "Mirage Beam Color Blue", tooltip: "The B in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 255, order = 7, categoryOrder = 5)]
        public static int MirageBeamColorB = 255;
        [ModOption(name: "Mirage Beam Color Transparency", tooltip: "The A in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 255, order = 8, categoryOrder = 5)]
        public static int MirageBeamColorA = 255;
        [ModOption(name: "Mirage Beam Color Intensity", tooltip: "How bright it is", valueSourceName: nameof(tenthsValues), category = "Mirage Beam", defaultValueIndex = 10, order = 9, categoryOrder = 5)]
        public static float MirageBeamColorIntensity = 1;
        [ModOption(name: "Mirage Beam Emission Red", tooltip: "The R in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 21, order = 10, categoryOrder = 5)]
        public static int MirageBeamEmissionR = 21;
        [ModOption(name: "Mirage Beam Emission Green", tooltip: "The G in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 255, order = 11, categoryOrder = 5)]
        public static int MirageBeamEmissionG = 255;
        [ModOption(name: "Mirage Beam Emission Blue", tooltip: "The B in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 255, order = 12, categoryOrder = 5)]
        public static int MirageBeamEmissionB = 255;
        [ModOption(name: "Mirage Beam Emission Transparency", tooltip: "The A in RGBA", valueSourceName: nameof(colorValues), category = "Mirage Beam", defaultValueIndex = 0, order = 13, categoryOrder = 5)]
        public static int MirageBeamEmissionA = 0;
        [ModOption(name: "Mirage Beam Emission Intensity", tooltip: "How bright it is", valueSourceName: nameof(tenthsValues), category = "Mirage Beam", defaultValueIndex = 50, order = 14, categoryOrder = 5)]
        public static float MirageBeamEmissionIntensity = 5;
        [ModOption(name: "Mirage Beam Size X", tooltip: "How long it is in the X axis", valueSourceName: nameof(hundrethsValues), category = "Mirage Beam", defaultValueIndex = 4, order = 15, categoryOrder = 5)]
        public static float MirageBeamSizeX = 0.0375f;
        [ModOption(name: "Mirage Beam Size Y", tooltip: "How long it is in the Y axis", valueSourceName: nameof(hundrethsValues), category = "Mirage Beam", defaultValueIndex = 165, order = 16, categoryOrder = 5)]
        public static float MirageBeamSizeY = 1.65f;
        [ModOption(name: "Mirage Beam Size Z", tooltip: "How long it is in the Z axis", valueSourceName: nameof(hundrethsValues), category = "Mirage Beam", defaultValueIndex = 4, order = 17, categoryOrder = 5)]
        public static float MirageBeamSizeZ = 0.0375f;
        [ModOption(name: "Mirage Beam Scale Increase X", tooltip: "How much the length in the X axis will increase every second", valueSourceName: nameof(hundrethsValues), category = "Mirage Beam", defaultValueIndex = 0, order = 18, categoryOrder = 5)]
        public static float MirageBeamScaleX = 0f;
        [ModOption(name: "Mirage Beam Scale Increase Y", tooltip: "How much the length in the Y axis will increase every second", valueSourceName: nameof(hundrethsValues), category = "Mirage Beam", defaultValueIndex = 20, order = 19, categoryOrder = 5)]
        public static float MirageBeamScaleY = 0.2f;
        [ModOption(name: "Mirage Beam Scale Increase Z", tooltip: "How much the length in the Z axis will increase every second", valueSourceName: nameof(hundrethsValues), category = "Mirage Beam", defaultValueIndex = 0, order = 20, categoryOrder = 5)]
        public static float MirageBeamScaleZ = 0f;

        public static ModOptionBool[] booleanOption =
        {
            new ModOptionBool("Enabled", true),
            new ModOptionBool("Disabled", false)
        };

        public static ModOptionBool[] orientationOption =
        {
            new ModOptionBool("Swapped", true),
            new ModOptionBool("Normal", false)
        };
        public static ModOptionString[] dashOption =
        {
            new ModOptionString("Player", "Player"),
            new ModOptionString("Item", "Item")
        };
        public static ModOptionString[] forceOption =
        {
            new ModOptionString("Acceleration", "Acceleration"),
            new ModOptionString("Force", "Force"),
            new ModOptionString("Impulse", "Impulse"),
            new ModOptionString("Velocity Change", "VelocityChange")
        };
        [ModOption(name: "Sheath Dash Force Mode", tooltip: "The force mode of the dash", valueSourceName: nameof(forceOption), category = "Sheath", defaultValueIndex = 2, order = 17, categoryOrder = 2)]
        public static void DashForceOptions(string option)
        {
            switch (option)
            {
                case "Force": DashForceMode = ForceMode.Force;
                    break;
                case "Acceleration": DashForceMode = ForceMode.Acceleration;
                    break;
                case "Impulse": DashForceMode = ForceMode.Impulse;
                    break;
                case "VelocityChange": DashForceMode = ForceMode.VelocityChange;
                    break;
            }
        }
        [ModOption(name: "Mirage Dash Force Mode", tooltip: "The force mode of the dash", valueSourceName: nameof(forceOption), category = "Mirage Edge", defaultValueIndex = 2, order = 5, categoryOrder = 3)]
        public static void MirageDashForceOptions(string option)
        {
            switch (option)
            {
                case "Force":
                    MirageDashForceMode = ForceMode.Force;
                    break;
                case "Acceleration":
                    MirageDashForceMode = ForceMode.Acceleration;
                    break;
                case "Impulse":
                    MirageDashForceMode = ForceMode.Impulse;
                    break;
                case "VelocityChange":
                    MirageDashForceMode = ForceMode.VelocityChange;
                    break;
            }
        }
        [ModOption(name: "Mirage Return Force Mode", tooltip: "The force mode of the blade returning to your hand", valueSourceName: nameof(forceOption), category = "Mirage Edge", defaultValueIndex = 1, order = 9, categoryOrder = 3)]
        public static void MirageReturnForceOptions(string option)
        {
            switch (option)
            {
                case "Force":
                    MirageReturnForceMode = ForceMode.Force;
                    break;
                case "Acceleration":
                    MirageReturnForceMode = ForceMode.Acceleration;
                    break;
                case "Impulse":
                    MirageReturnForceMode = ForceMode.Impulse;
                    break;
                case "VelocityChange":
                    MirageReturnForceMode = ForceMode.VelocityChange;
                    break;
            }
        }
        public static ModOptionInt[] colorValues()
        {
            ModOptionInt[] modOptionInts = new ModOptionInt[256];
            int num = 0;
            for (int i = 0; i < modOptionInts.Length; ++i)
            {
                modOptionInts[i] = new ModOptionInt(num.ToString("0"), num);
                num += 1;
            }
            return modOptionInts;
        }
        public static ModOptionInt[] intValues()
        {
            ModOptionInt[] modOptionInts = new ModOptionInt[1001];
            int num = 0;
            for (int i = 0; i < modOptionInts.Length; ++i)
            {
                modOptionInts[i] = new ModOptionInt(num.ToString("0"), num);
                num += 1;
            }
            return modOptionInts;
        }
        public static ModOptionFloat[] thousandthsValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0.000"), num);
                num += 0.001f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] hundrethsValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0.00"), num);
                num += 0.01f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] tenthsValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0.0"), num);
                num += 0.1f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] singleValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 1f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] fiveValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 5f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] largeValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 50f;
            }
            return modOptionFloats;
        }
        public static ModOptionFloat[] rotateValues()
        {
            ModOptionFloat[] modOptionFloats = new ModOptionFloat[1001];
            float num = 0f;
            for (int i = 0; i < modOptionFloats.Length; ++i)
            {
                modOptionFloats[i] = new ModOptionFloat(num.ToString("0"), num);
                num += 30f;
            }
            return modOptionFloats;
        }
    }
}
