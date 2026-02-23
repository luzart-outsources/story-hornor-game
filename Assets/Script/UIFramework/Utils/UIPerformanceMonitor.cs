using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Utils
{
    /// <summary>
    /// Monitors UI performance and memory usage
    /// Use in development to track issues
    /// </summary>
    public class UIPerformanceMonitor
    {
        private static UIPerformanceMonitor instance;
        public static UIPerformanceMonitor Instance => instance ?? (instance = new UIPerformanceMonitor());
        
        private readonly Dictionary<string, PerformanceMetrics> metrics = new Dictionary<string, PerformanceMetrics>();
        
        public void RecordShow(string viewId, float loadTime)
        {
            if (!metrics.ContainsKey(viewId))
            {
                metrics[viewId] = new PerformanceMetrics(viewId);
            }
            
            metrics[viewId].ShowCount++;
            metrics[viewId].TotalLoadTime += loadTime;
            metrics[viewId].AverageLoadTime = metrics[viewId].TotalLoadTime / metrics[viewId].ShowCount;
            metrics[viewId].LastShowTime = Time.time;
        }
        
        public void RecordHide(string viewId)
        {
            if (!metrics.ContainsKey(viewId))
            {
                metrics[viewId] = new PerformanceMetrics(viewId);
            }
            
            metrics[viewId].HideCount++;
        }
        
        public void RecordMemory(string viewId, long memoryBytes)
        {
            if (!metrics.ContainsKey(viewId))
            {
                metrics[viewId] = new PerformanceMetrics(viewId);
            }
            
            metrics[viewId].MemoryUsage = memoryBytes;
        }
        
        public PerformanceMetrics GetMetrics(string viewId)
        {
            return metrics.TryGetValue(viewId, out var metric) ? metric : null;
        }
        
        public Dictionary<string, PerformanceMetrics> GetAllMetrics()
        {
            return new Dictionary<string, PerformanceMetrics>(metrics);
        }
        
        public void LogReport()
        {
            Debug.Log("=== UI Performance Report ===");
            
            foreach (var kvp in metrics)
            {
                var m = kvp.Value;
                Debug.Log($"{m.ViewId}:");
                Debug.Log($"  Shows: {m.ShowCount}, Hides: {m.HideCount}");
                Debug.Log($"  Avg Load Time: {m.AverageLoadTime:F3}s");
                Debug.Log($"  Memory: {m.MemoryUsage / 1024}KB");
            }
        }
        
        public void Clear()
        {
            metrics.Clear();
        }
    }
    
    [Serializable]
    public class PerformanceMetrics
    {
        public string ViewId;
        public int ShowCount;
        public int HideCount;
        public float TotalLoadTime;
        public float AverageLoadTime;
        public float LastShowTime;
        public long MemoryUsage;
        
        public PerformanceMetrics(string viewId)
        {
            ViewId = viewId;
        }
    }
}
