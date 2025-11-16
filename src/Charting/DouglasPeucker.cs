// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Numerics;
namespace Blackwood;

/// <summary>
/// A class that reduces the number of points in a series.
/// </summary>
/// <typeparam name="T">The type of the values in the series.</typeparam>
/// <remarks>
/// Like a reductionist's secret weapon, this class simplifies your data into a
/// more manageable form.  It reduces the number of points in a series when
/// you want to simplify the data for display or analysis -- or simply not
/// overwhelm the graphing tools.
/// 
/// It uses the Douglas-Peucker algorithm, a recursive approach to reduce the
/// number of points in a series.  It finds the points less than given distance
/// from the line segment and removes them.
/// </remarks>
public class ReduceArray<T>  where T: INumber<T>
{
    /// <summary>
    /// The threshold to trigger reducing the number of points in the series.
    /// </summary>
    const int threshold = 1 * 100;

    /// <summary>
    /// The maximum number of items to use in the series.  The charting library
    /// becomes very slow if there are too many points.
    /// </summary>
    readonly int maxItems;

    /// <summary>
    /// The points in the series.
    /// </summary>
    readonly ChartPoint<T>[] points;


    /// <summary>
    /// An array of booleans indicating which points to keep.
    /// </summary>
    readonly bool[] pointsToKeep;

    /// <summary>
    /// Create a new DP object.
    /// </summary>
    /// <param name="points">The points in the series.</param>
    /// <param name="maxItems">The target maximum number of items to use in the
    /// series.</param>
    /// <remarks>
    /// Note: the resulting array may have more than the target maximum number
    /// of items if the meximum number of interations is reached.
    /// </remarks>
    public ReduceArray(ChartPoint<T>[] points, int maxItems=2000)
    {
        this.points = points;
        this.maxItems = maxItems;
        // Initialize the points to keep.
        pointsToKeep = new bool[points.Length];
        if (points.Length > 0)
        {
            pointsToKeep[0] = true;
            pointsToKeep[points.Length - 1] = true;
        }
    }

    /// <summary>
    /// Create a new DP object.
    /// </summary>
    /// <param name="points">The points in the series.</param>
    /// <param name="maxItems">The maximum number of items to use in the series.
    /// </param>
    /// <remarks>
    /// Note: the resulting array may have more than the target maximum number
    /// of items if the meximum number of interations is reached.
    /// </remarks>
    public ReduceArray(IEnumerable<ChartPoint<T>> points, int maxItems=2000)
        :this(points.ToArray(),maxItems)
    {
    }


    /// <summary>
    /// Calculate the perpendicular distance from a point to a line segment.
    /// </summary>
    /// <param name="point">The point</param>
    /// <param name="lineStart">Start of line segment</param>
    /// <param name="lineEnd">End of line segment</param>
    /// <returns>The perpendicular distance from the point to the line.</returns>
    static double PerpendicularDistance(ChartPoint<T> point, ChartPoint<T> lineStart, ChartPoint<T> lineEnd)
    {
        // If the line is a point, return the distance to the point.
        if (lineStart.index == lineEnd.index)
            return double.MaxValue;
//        return System.Math.Abs(point.y - lineStart.y);

        // Calculate the slope and intercept of the line.
        var dX=(double)Convert.ChangeType(lineEnd.index - lineStart.index, typeof(double), CultureInfo.InvariantCulture);
        var slope = (lineEnd.y - lineStart.y) / dX;
        var intercept = lineStart.y - slope * (double)Convert.ChangeType(lineStart.index, typeof(double), CultureInfo.InvariantCulture);

        // Calculate the perpendicular distance from the point to the line.
        return System.Math.Abs(slope * (double)Convert.ChangeType(point.index, typeof(double), CultureInfo.InvariantCulture) - point.y + intercept) / System.Math.Sqrt(1 + slope * slope);
    }

    /// <summary>
    /// Identify the points in the sequence in the line that are to be kept.
    /// This favors the points that are furthest from the line.
    /// </summary>
    /// <param name="start">The index of the first point in the line.</param>
    /// <param name="end">The index of the last point in the line.</param>
    /// <param name="threshold">The threshold to trigger reducing the number of
    /// points in the series.</param>
    /// <remarks>
    /// This is a recursive implementation of the Douglas-Peucker algorithm.
    /// </remarks>
    void DouglasPeuckerRecursive(int start, int end, double threshold)
    {
        // If the line is a point, return the distance to the point.
        if (end - start <= 1)
            return;

        double maxDistance = 0;
        var maxIndex = start;

        // Find the point with the maximum distance from the line.
        for (int index = start + 1; index < end; index++)
        {
            // Calculate the perpendicular distance from the point to the line.
            var distance = PerpendicularDistance(points[index], points[start], points[end]);

            // If the distance is greater than the maximum distance, update the
            // maximum distance and index.
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = index;
            }
        }

        // If the point is too far from the line, keep it.
        if (maxDistance > threshold)
        {
            // Keep the point with the maximum distance from the line.
            pointsToKeep[maxIndex] = true;

            // Recursively apply the Douglas-Peucker algorithm to the two halves
            // of the line.
            DouglasPeuckerRecursive(start, maxIndex, threshold);
            DouglasPeuckerRecursive(maxIndex, end, threshold);
        }
    }


    /// <summary>
    /// Count how many points would remain after Douglas-Peucker simplification.
    /// </summary>
    /// <param name="threshold">Distance threshold</param> I
    /// <returns>Number of points that would remain</returns>
    int DouglasPeuckerCount(double threshold)
    {
        // If there are less than 3 points, return the number of points.
        if (points.Length < 3)
            return points.Length;

        // Apply the Douglas-Peucker algorithm.
        DouglasPeuckerRecursive(0, points.Length - 1, threshold);

        // Return the number of points that would remain.
        return pointsToKeep.Count(x => x);
    }

    /// <summary>
    /// Find the subset of points that fixes the number of points to the
    /// maximum number of items using Douglas-Peucker with binary search.
    /// </summary>
    void FindOptimalSubset(int maxIterations)
    {
        // Zero out the array
        for (var i = 1; i < pointsToKeep.Length - 1; i++)
            pointsToKeep[i] = false;

        // Find the range of distances
        var minDistance = double.MaxValue;
        double maxDistance = 0;

        // Find the minimum and maximum distances from the line.
        for (int index = 1; index < points.Length - 1; index++)
        {
            var distance = PerpendicularDistance(points[index], points[0], points[points.Length - 1]);
            minDistance = System.Math.Min(minDistance, distance);
            maxDistance = System.Math.Max(maxDistance, distance);
        }

        // Binary search for optimal threshold.
        var left = minDistance;
        var right = maxDistance;
        // Calculate the initial threshold.
        var mid = (left*4 + right) / 5;
        int pointCount;

        // Limit iterations.
        for (int iteration = 0; iteration < maxIterations /*42*/; iteration++)
        {
            // Count the number of points that would remain.
            pointCount = DouglasPeuckerCount(mid);

            // If the number of points is less than the maximum number of items,
            // set the right threshold to the middle threshold.
            if (pointCount <= maxItems)
            {
                right = mid;
            }
            else
            {
                // If the number of points is greater than the maximum number of items,
                // set the left threshold to the middle threshold.
                left = mid;
            }

            // Early termination if range is small enough
            if (right - left < 1e-14)
                break;

            // Calculate the threshold.
            mid = (left + right) / 2;
        }
    }



    /// <summary>
    /// Enumerate the points using Douglas-Peucker simplification.
    /// </summary>
    /// <returns>An enumeration of the simplified points.</returns>
    public IEnumerable<ChartPoint<T>> Simplify()
    {
        // If there are less than the threshold number of points, do nothing.
        if ((points.Length <= threshold) || (points.Length < 3))
        {
            // Enumerate the subset of points.
            foreach (var point in points)
                yield return point;
            yield break;
        }

        // Use Douglas-Peucker algorithm to find the subset of points.
        FindOptimalSubset(4);

        // Enumerate the subset of points.
        for (int i = 0; i < points.Length; i++)
        {
            if (pointsToKeep[i])
                yield return points[i];
        }
    }
}
