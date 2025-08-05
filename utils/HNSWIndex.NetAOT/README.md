# 项目介绍

本项目在  HNSWINDEX 基础上修改，主要解决两个问题：

- 支持 NativeAOT
- 尽量去掉泛型，默认类型为 HNSWPoint，更贴近应用:

```csharp
public class HNSWPoint
{
    public float[] Data { get; set; } = Array.Empty<float>();

    public string Label { get; set; } = String.Empty;
}
```

Perform KNN Query for millions of data points fast and with great accuracy. 

**HNSWIndex** is a .NET library for constructing approximate nearest-neighbor (ANN) indices based on the _Hierarchical Navigable Small World_ (HNSW) graph. This data structure provides efficient similarity searches for large, high-dimensional datasets.

## Key Features
 - **High Performance**: Implements the HNSW algorithm for fast approximate k-NN search.
 - **Flexible Distance Metric**: Pass any `Func<HNSWPoint, HNSWPoint, float>` for custom distance calculation.
 - **Flexible Heuristic**: Pass heuristic function for nodes linking.
 - **Concurrency Support**: Thread safe graph building API 
 - **Configurable Parameters**: Fine-tune the indexing performance and memory trade-offs with parameters
 - **Save and Load**: Save resulting structure on file system and restore later
## Installation
Install via [NuGet](https://www.nuget.org/packages/HNSWIndex/):
```
dotnet add package HNSWIndex
```
Or inside your **.csproj**:
```
<PackageReference Include="HNSWIndex" Version="x.x.x" />
```

## Getting Started
### 1. Optionally configure parameters
```
var parameters = new HNSWParameters
{ 
    RandomSeed = 123,
    DistributionRate = 1.0,
    MaxEdges = 16,
    CollectionSize = 1024,
    // ... other parameters
};
```
### 2. Create empty graph structure ()
```
var index = new HNSWIndex<float[], float>(Metrics.SquaredEuclideanMetric.Compute);
```
### 3. Build the graph
```
var vectors = RandomVectors();
foreach (var vector in vectors)
{
	index.Add(vector)
}
```
Or multi-threaded
```
var vectors = RandomVectors();
Parallel.For(0, vectors.Count, i => {
    index.Add(vectors[i]);
});
```
### 4. Query the structure
```
var k = 5;
var results = index.KnnQuery(queryPoint, k);
```
### 5. Save and Load graph from file system
```
index.Serialize(pathToFile);
var index = HNSWIndex<float[], float>.Deserialize(Metrics.SquaredEuclideanMetric.Compute, pathToFile);
```
