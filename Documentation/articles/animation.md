# Animation

Blackwood.Core provides animation support through linear interpolation and
easing functions. This allows you to smoothly animate properties of objects
over time, creating transitions between states.

## Overview

Animation in Blackwood.Core consists of three main components:

1. **PropertyInterpolator** automatically creates interpolation functions for
   complex objects by analyzing their properties
2. **LinearInterpolate** provides interpolation methods for various types
   (numbers, colors, points, rectangles, arrays, and custom types)
3. **Easing Functions** transform linear progress into natural acceleration
   and deceleration curves

## Basic Interpolation

`LinearInterpolate.Lerp` interpolates between two values using a time factor `t`
between 0.0 and 1.0. At `t = 0.0`, the result equals the start value.  At
`t = 1.0`, the result equals the end value.

```csharp
using Blackwood;

// Interpolate between two numbers
float result = LinearInterpolate.Lerp(10f, 20f, 0.5f); // Returns 15f

// Interpolate between two colors
Color color = LinearInterpolate.Lerp(Color.Red, Color.Blue, 0.5f);
// Returns a color halfway between red and blue

// Interpolate between two points
Point point = LinearInterpolate.Lerp(new Point(0, 0), new Point(100, 100), 0.3f);
// Returns approximately (30, 30)
```

`LinearInterpolate` supports many types:
- Numeric types: `float`, `double`, `int`, `decimal`, `byte`, `short`, `long`,
  and their unsigned variants
- Graphics types: `Color`, `Point`, `PointF`, `Rectangle`, `RectangleF`
- Arrays of numeric types, including Vectors and Matrices
- Custom types that implement `Lerp` methods

## Property Interpolation

`PropertyInterpolator` analyzes a class and creates an interpolation function
that interpolates all interpolatable properties and fields. This allows you to
animate complex objects without manually interpolating each property.

```csharp
// Define a class with properties you want to animate
public class AnimatedState
{
    public Point Location { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Color BackColor { get; set; }
}

// Create an interpolation function for the class
var interpolateFunction = PropertyInterpolator.CreateInterpolationFunction<AnimatedState>();

// Define start and end states
var startState = new AnimatedState
{
    Location = new Point(0, 0),
    Width = 100,
    Height = 100,
    BackColor = Color.Red
};

var endState = new AnimatedState
{
    Location = new Point(200, 200),
    Width = 200,
    Height = 200,
    BackColor = Color.Blue
};

// Create a destination object to hold the interpolated result
var currentState = new AnimatedState();

// Interpolate at t = 0.5 (halfway between start and end)
interpolateFunction(startState, endState, 0.5f, currentState);

// currentState now contains:
// Location = (100, 100)
// Width = 150
// Height = 150
// BackColor = a color halfway between red and blue
```

`PropertyInterpolator` automatically discovers `Lerp` methods by scanning
loaded assemblies for static methods named `Lerp`. It also handles nested
objects, arrays, and fields in addition to properties.

## Easing Functions

Easing functions transform the linear interpolation factor into curves that
create more natural-looking animations. Instead of constant speed, easing
functions provide acceleration and deceleration.

```csharp
using Blackwood;

// Get an easing function
var easingFunc = Animation.GetEasing(EasingFunction.EaseInOutQuad);

// Apply easing to the progress value
float linearProgress = 0.5f; // Halfway through the animation
float easedProgress = easingFunc(linearProgress); // Transformed progress

// Use the eased progress for interpolation
var result = LinearInterpolate.Lerp(startValue, endValue, easedProgress);
```

Available easing functions:

- **Linear**: Constant speed (no easing)
- **Smoothstep**: Smooth S-curve with zero slope at start and end
- **Smootherstep**: Even smoother S-curve than smoothstep
- **EaseInQuad**: Starts slow, accelerates (quadratic)
- **EaseOutQuad**: Starts fast, decelerates (quadratic)
- **EaseInOutQuad**: Starts slow, accelerates, then decelerates (quadratic)
- **EaseInCubic**: Starts very slow, accelerates (cubic)
- **EaseOutCubic**: Starts very fast, decelerates (cubic)
- **EaseInOutCubic**: Starts very slow, accelerates, then decelerates (cubic)
- **EaseInSine**: Smooth acceleration using sine curve
- **EaseOutSine**: Smooth deceleration using sine curve
- **EaseInOutSine**: Smooth acceleration and deceleration using sine curve

## Creating an Animation

To create an animation, you typically:

1. Define a state class with properties you want to animate
2. Create an interpolation function using `PropertyInterpolator`
3. Set up a timer to update the animation progress
4. Apply easing to the progress
5. Interpolate between start and end states
6. Apply the interpolated state to your UI or objects

Here's a complete example using Windows Forms:

```csharp
using Blackwood;
using System.Drawing;
using System.Windows.Forms;

// Main form that demonstrates animating a panel using interpolation and easing
public partial class MainForm : Form
{
    // The panel that will be animated
    private Panel animatedPanel = null!;
    // Timer to drive the animation updates
    private Timer animationTimer = null!;
    // Delegate for the interpolation function
    private PropertyInterpolator.dInterpolate? interpolateFunction;
    // Animation state at start of animation
    private AnimatedState startState = null!;
    // Animation state at end of animation
    private AnimatedState endState = null!;
    // The current (interpolated) animation state
    private AnimatedState currentState = null!;
    // Animation progress between 0 (start) and 1 (end)
    private float animationProgress = 0f;
    // How fast the animation progresses; lower is slower
    private const float AnimationSpeed = 0.02f;
    // Selected easing function for the animation curve
    private EasingFunction selectedEasing = EasingFunction.EaseInOutQuad;

    public MainForm()
    {
        InitializeComponent();
        SetupAnimation();
    }

    // Set up objects and logic for the animation demo
    private void SetupAnimation()
    {
        // Create the panel to animate and set its initial properties
        animatedPanel = new Panel
        {
            Location = new Point(50, 50),
            Size = new Size(100, 100),
            BackColor = Color.Red
        };
        // Add the panel to the form's controls
        Controls.Add(animatedPanel);

        // Create an automatic property interpolation function for AnimatedState
        interpolateFunction = PropertyInterpolator.CreateInterpolationFunction<AnimatedState>();

        // Define the starting state of the animation
        startState = new AnimatedState
        {
            Location = new Point(50, 50),
            Width = 100,
            Height = 100,
            BackColor = Color.Red
        };

        // Define the ending state of the animation
        endState = new AnimatedState
        {
            Location = new Point(400, 300),
            Width = 200,
            Height = 200,
            BackColor = Color.Blue
        };

        // Initialize the current (interpolated) state
        currentState = new AnimatedState();

        // Set up a timer to update the animation at roughly 60 FPS (every 16 ms)
        animationTimer = new Timer { Interval = 16 }; // ~60 FPS
        animationTimer.Tick += AnimationTimer_Tick;
        animationTimer.Start();
    }

    // Called each frame to update the animation
    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        // Increment the animation progress by the animation speed
        animationProgress += AnimationSpeed;
        // Once the progress reaches the end, stop the timer
        if (animationProgress >= 1.0f)
        {
            animationProgress = 1.0f;
            animationTimer.Stop();
        }

        // Get the selected easing function and apply it to the progress
        var easingFunc = Animation.GetEasing(selectedEasing);
        float easedProgress = easingFunc(animationProgress);

        // Interpolate state using the eased progress
        interpolateFunction!(startState, endState, easedProgress, currentState);

        // Apply the interpolated state to the panel's properties
        animatedPanel.Location = currentState.Location;
        animatedPanel.Width = currentState.Width;
        animatedPanel.Height = currentState.Height;
        animatedPanel.BackColor = currentState.BackColor;
    }
}

// Represents the state for the animated object (panel)
public class AnimatedState
{
    public Point Location { get; set; }      // Position of the panel
    public int Width { get; set; }           // Width of the panel
    public int Height { get; set; }          // Height of the panel
    public Color BackColor { get; set; }     // Background color of the panel
}

```

## Custom Types

To make a custom type interpolatable, add a static `Lerp` method. `PropertyInterpolator` will discover it automatically.

```csharp
public class BezierPath
{
    public PointF[] PathPoints = [];
    public bool Closed = false;

    // Static Lerp method for interpolation
    public static BezierPath Lerp(BezierPath start, BezierPath end, float t)
    {
        // Ensure both paths have the same number of points
        if (start.PathPoints.Length != end.PathPoints.Length)
        {
            throw new ArgumentException("Paths must have the same number of points");
        }

        // Interpolate each point
        var interpolatedPoints = new PointF[start.PathPoints.Length];
        for (int i = 0; i < start.PathPoints.Length; i++)
        {
            interpolatedPoints[i] = LinearInterpolate.Lerp(
                start.PathPoints[i],
                end.PathPoints[i],
                t
            );
        }

        // Interpolate the Closed property (use end value when t >= 0.5)
        return new BezierPath
        {
            PathPoints = interpolatedPoints,
            Closed = t < 0.5f ? start.Closed : end.Closed
        };
    }
}
```

Alternatively, you can use the `Interpolate` pattern that modifies a destination object:

```csharp
public static void Lerp(BezierPath start, BezierPath end, float t, BezierPath dest)
{
    // Ensure destination array is the correct size
    if (dest.PathPoints == null || dest.PathPoints.Length != start.PathPoints.Length)
    {
        dest.PathPoints = new PointF[start.PathPoints.Length];
    }

    // Interpolate each point into the destination
    for (int i = 0; i < start.PathPoints.Length; i++)
    {
        dest.PathPoints[i] = LinearInterpolate.Lerp(
            start.PathPoints[i],
            end.PathPoints[i],
            t
        );
    }

    dest.Closed = t < 0.5f ? start.Closed : end.Closed;
}
```

## Best Practices

- Use easing functions to create natural-looking animations.  Linear
  interpolation can appear mechanical.
- For UI animations, use a timer interval of 16ms (~60 FPS) for smooth motion.
- Reuse the destination object in your animation loop to avoid unnecessary
  allocations.
- Ensure start and end states have compatible structures (same array lengths,
  same property types).
- For complex animations, consider creating a dedicated state class rather than
 animating individual properties.

## See Also

- [LinearInterpolate API Reference](../api/Blackwood.LinearInterpolate.html) for available interpolation methods
- [PropertyInterpolator API Reference](../api/Blackwood.PropertyInterpolator.html) for details on creating interpolation functions
- [Animation API Reference](../api/Blackwood.Animation.html) for easing function details

