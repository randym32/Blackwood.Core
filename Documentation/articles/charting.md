# Charting

This framework includes tools to support chart rendering.
The charting system consists of three main components:

- **ChartPoint<IndexType>**: Represents individual data points in a chart
- **ChartAnnotation&lt;IndexType, DurationType&gt;**: Represents annotation spans with visual styling (the shorthand `ChartAnnotation<T>` remains for the common case where index and duration share the same type)
- **ReduceArray<T>**:  This reducing point counts in large datasets by while preserving visual fidelity.

## ChartPoint<IndexType>

`ChartPoint<IndexType>` represents a single data point in a chart with an independent
variable (index) and a dependent variable (y-value).  It supports any numeric
type for the index, including:
- `int`, `long`, `double`, `float`
- `DateTime` for time-series data
- `string` for categorical data
- Any type that implements `INumber<IndexType>`

### Creating Chart Points

```csharp
using Blackwood;

// Create a point with index and y-value
var point1 = new ChartPoint<int>(10, 25.5);

// Create a point with annotation
var point2 = new ChartPoint<int>(20, 42.0, "Peak value");

// Create points with different index types
var timePoint = new ChartPoint<DateTime>(
    new DateTime(2025, 1, 1, 10, 0, 0),
    100.0,
    "Measurement at 10 AM"
);

var categoryPoint = new ChartPoint<string>("Category A", 75.5);
```


## ChartAnnotation&lt;IndexType, DurationType&gt;

`ChartAnnotation<IndexType, DurationType>` represents a span annotation on a chart,
intended to highlight time periods, ranges, or regions of interest.  Use
`ChartAnnotation<T>` when the index and duration share the same type, or specify
both generic arguments when they differ (for example, DateTime index with a
TimeSpan duration).

### Creating Annotations

```csharp
using Blackwood;
using System.Drawing;

// Create an annotation with index, duration, and color
var annotation = new ChartAnnotation<int>(
    index: 100,        // Start position
    duration: 50,      // Span length
    color: Color.Red,  // Annotation color
    text: "Critical period",
    textColor: Color.White  // Text color (optional, defaults to white)
);

// Create annotation without text, using matching index/duration types
var simpleAnnotation = new ChartAnnotation<DateTime>(
    index: new DateTime(2025, 1, 1),
    duration: new DateTime(2025, 1, 2),
    color: Color.Yellow
);

// Create annotation where the duration type differs from the index type
var maintenanceWindow = new ChartAnnotation<DateTime, TimeSpan>(
    index: new DateTime(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc),
    duration: TimeSpan.FromHours(2),
    color: Color.FromArgb(160, Color.Orange),
    text: "Maintenance",
    textColor: Color.Black
);
```

## ReduceArray<T> - Point Reduction

When dealing with large datasets, rendering thousands of points is often slow.
`ReduceArray<T>` implements the Douglas-Peucker algorithm to reduce the number
of points while preserving the visual shape of the data.


### How It Works

`ReduceArray<T>` uses Douglas-Peucker algorithm to simplify the lines, while
preserving the visual shape of the data.  Smaller datasets are unchanged: point
reduction only occurs when the dataset has more than 100 points.

The Douglas-Peucker algorithm removes points close to the line segment, keeping
those that are furthest away.  (It also keeps the start and end points of the
series).

1. It first finds the distance threshold, iteratively narrows the threshold
   until the point count (further away than the threshold) is close to `maxItems`
2. It them examines the distance of points from the line segment, removing those
   less than the threshold.
3. If that point is beyond a threshold distance, it's kept and the algorithm
   recurses on both halves, treating each as new line segments.


This algorithm is effective on smooth curves and time-series data.


### Basic Usage

Below is a trivial example:

```csharp
using Blackwood;

// Create a large dataset
var points = new List<ChartPoint<int>>();
for (int i = 0; i < 10000; i++)
{
    var y = 50.0 + 20.0 * Math.Sin(i * 0.01) + (i % 100) * 0.1;
    points.Add(new ChartPoint<int>(i, y));
}

// Reduce to maximum 200 points
var reducer = new ReduceArray<int>(points, maxItems: 200);
var simplifiedPoints = reducer.Simplify().ToList();

Console.WriteLine($"Original: {points.Count} points");
Console.WriteLine($"Simplified: {simplifiedPoints.Count} points");
```


### Choosing maxItems

The `maxItems` parameter controls the target maximum number of points in the
result:

- **Too low**: May lose important features in complex data
- **Too high**: Defeats the purpose of reduction
- **Recommended**: 2000 points for most charts, depending on screen resolution
  and data complexity

```csharp
// For high-resolution displays or complex data
var highDetail = new ReduceArray<int>(points, maxItems: 500);

// For standard displays
var standard = new ReduceArray<int>(points, maxItems: 200);

// For thumbnail or preview charts
var thumbnail = new ReduceArray<int>(points, maxItems: 50);
```


The algorithm may not converge exactly to `maxItems`, stopping at a maximum
number of iterations.  The rresults may be up to 2x `maxItems` in complex cases.

## Complete Example

Here's a complete example showing how to use all the charting components
together:

```csharp
using Blackwood;
using System.Drawing;

// Example: Generate, annotate, and reduce a large time-series dataset for charting

// Generate sample time-series data (5000 hourly points)
var random = new Random(42);
var points = new List<ChartPoint<DateTime>>();
var startDate = new DateTime(2025, 1, 1);

// Populate the dataset, adding an annotation every 100th point
for (int i = 0; i < 5000; i++)
{
    var date = startDate.AddHours(i);
    var baseValue = 50.0 + 20.0 * Math.Sin(i * 0.1);
    var noise = random.NextDouble() * 5.0 - 2.5;
    var value = baseValue + noise;

    // Annotate every 100th point with the day number
    points.Add(new ChartPoint<DateTime>(
        date,
        value,
        i % 100 == 0 ? $"Day {i / 24}" : null
    ));
}

// Reduce points for efficient rendering using Douglas-Peucker
var reducer = new ReduceArray<DateTime>(points, maxItems: 200);
var simplifiedPoints = reducer.Simplify().ToList();

// Define annotation spans for key periods (example: maintenance, high activity)
var annotations = new List<ChartAnnotation<DateTime>>
{
    new ChartAnnotation<DateTime>(
        startDate.AddDays(10), // Start
        startDate.AddDays(12), // End (duration = 2 days)
        Color.Red,
        "Maintenance window",
        Color.White
    ),
    new ChartAnnotation<DateTime>(
        startDate.AddDays(50), // Start
        startDate.AddDays(55), // End
        Color.Yellow,
        "High activity period"
    )
};

// Example rendering logic (pseudo-code for integration with chart control)
foreach (var point in simplifiedPoints)
{
    // Render point at point.index (DateTime), point.y (double value)
    // If point.annotation is not null, display annotation label
}

foreach (var annotation in annotations)
{
    // Render annotation span from annotation.index to annotation.index + annotation.duration
    // Use annotation.color for the highlighted region
    // Display annotation.text in annotation.textColor if provided
}
```



## See Also

- [Introduction](intro.md) - Overview of Blackwood.Core features
- [Getting Started](getting-started.md) - Installation and setup
- [API Reference](../api/index.md) - Complete API documentation for charting classes

