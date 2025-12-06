// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;


/// <summary>
/// Easing functions that provide natural acceleration and deceleration curves
/// for animations.
/// </summary>
/// <param name="t">The interpolation factor between 0.0 and 1.0</param>
/// <param name="easing">The easing function to apply</param>
/// <returns>The transformed interpolation factor</returns>
/// <remarks>
/// Easing functions transform the linear interpolation factor (0.0 to 1.0) into
/// curves that create more natural-looking animations. Each function provides
/// different acceleration and deceleration characteristics.
/// </remarks>
/// <example>
/// <code>
/// // Linear: constant speed
/// float linear = Anim.GetEasing(EasingFunction.Linear)(0.5f); // Returns 0.5f
///
/// // EaseIn: starts slow, accelerates
/// float easeIn = Anim.GetEasing(EasingFunction.EaseInQuad)(0.5f); // Returns 0.25f
///
/// // EaseOut: starts fast, decelerates
/// float easeOut = Anim.GetEasing(EasingFunction.EaseOutQuad)(0.5f); // Returns 0.75f
/// </code>
/// </example>
public enum EasingFunction
{
    /// <summary>Linear interpolation with constant speed</summary>
    Linear,

    /// <summary>Smoothstep: smooth S-curve with zero slope at start and end</summary>
    Smoothstep,

    /// <summary>Smootherstep: even smoother S-curve than smoothstep</summary>
    Smootherstep,

    /// <summary>Quadratic ease-in: starts slow, accelerates</summary>
    EaseInQuad,

    /// <summary>Quadratic ease-out: starts fast, decelerates</summary>
    EaseOutQuad,

    /// <summary>Quadratic ease-in-out: starts slow, accelerates, then decelerates</summary>
    EaseInOutQuad,

    /// <summary>Cubic ease-in: starts very slow, accelerates more than quadratic</summary>
    EaseInCubic,

    /// <summary>Cubic ease-out: starts very fast, decelerates more than quadratic</summary>
    EaseOutCubic,

    /// <summary>Cubic ease-in-out: starts very slow, accelerates, then decelerates more than quadratic</summary>
    EaseInOutCubic,

    /// <summary>Sine ease-in: smooth acceleration using sine curve</summary>
    EaseInSine,

    /// <summary>Sine ease-out: smooth deceleration using sine curve</summary>
    EaseOutSine,

    /// <summary>Sine ease-in-out: smooth acceleration and deceleration using sine curve</summary>
    EaseInOutSine
}

/// <summary>
/// A class providing support for animation
/// </summary>
public static partial class Animation
{
    #region Easing Functions
    const float PI_half = MathF.PI / 2;

    /// <summary>
    /// A dictionary of easing functions.
    /// </summary>
    static readonly Dictionary<EasingFunction, Func<float, float>> easingFunctions = new()
    {
        { EasingFunction.Linear        , (t) => t },
        { EasingFunction.Smoothstep    , (t) => t*t * (3.0f - 2.0f * t) },
        { EasingFunction.Smootherstep  , (t) => t*t*t * (t * (t*6.0f - 15.0f) + 10.0f) },
        { EasingFunction.EaseInQuad    , (t) => t*t },
        { EasingFunction.EaseOutQuad   , (t) => 1 - (1-t)*(1-t) },
        { EasingFunction.EaseInOutQuad , (t) => t < 0.5f ? 2*t*t : 1-2*(1-t)*(1-t) },
        { EasingFunction.EaseInCubic   , (t) => t*t*t },
        { EasingFunction.EaseOutCubic  , (t) => 1 - (1-t)*(1-t)*(1-t) },
        { EasingFunction.EaseInOutCubic, (t) => t<0.5f ? 4*t*t*t : 1-4*(1-t)*(1-t)*(1-t) },
        { EasingFunction.EaseInSine    , (t) => 1 - MathF.Cos(t * PI_half) },
        { EasingFunction.EaseOutSine   , (t) =>     MathF.Sin(t * PI_half) },
        { EasingFunction.EaseInOutSine , (t) =>   -(MathF.Cos(t*MathF.PI)-1) / 2 },
    };

    /// <summary>
    /// Applies an easing function to transform the interpolation factor.
    /// </summary>
    /// <param name="easing">The easing function to get</param>
    /// <returns>The easing function</returns>
    public static Func<float, float> GetEasing(EasingFunction easing) => easingFunctions[easing];
    #endregion
}
