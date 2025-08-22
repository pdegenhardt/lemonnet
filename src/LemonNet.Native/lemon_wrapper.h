#ifndef LEMON_WRAPPER_H
#define LEMON_WRAPPER_H

#ifdef __cplusplus
extern "C" {
#endif

#ifdef _WIN32
    #ifdef LEMON_WRAPPER_EXPORTS
        #define LEMON_API __declspec(dllexport)
    #else
        #define LEMON_API __declspec(dllimport)
    #endif
#else
    #define LEMON_API
#endif

typedef void* LemonGraph;
typedef void* LemonArcMap;
typedef void* LemonNodeMap;

typedef struct {
    int arc_id;      // The arc identifier
    long long flow;  // Use long long (64-bit) to match C# long
} FlowResult;

typedef struct {
    int* arc_ids;    // Array of arc identifiers forming the path
    int count;       // Number of arcs in the path
} PathResult;

typedef struct {
    double distance;           // Distance from source to target
    PathResult* path;          // Path from source to target (null if no path)
    int reached;              // 1 if target was reached, 0 otherwise
    int negative_cycle;       // 1 if negative cycle detected (Bellman-Ford only)
} ShortestPathResult;

// Graph operations
LEMON_API LemonGraph lemon_create_graph();
LEMON_API void lemon_destroy_graph(LemonGraph graph);
LEMON_API int lemon_add_node(LemonGraph graph);
LEMON_API int lemon_add_arc(LemonGraph graph, int source, int target);
LEMON_API int lemon_arc_source(LemonGraph graph, int arc);
LEMON_API int lemon_arc_target(LemonGraph graph, int arc);
LEMON_API int lemon_node_count(LemonGraph graph);
LEMON_API int lemon_arc_count(LemonGraph graph);

// Arc map operations - long values
LEMON_API LemonArcMap lemon_create_arc_map_long(LemonGraph graph);
LEMON_API void lemon_destroy_arc_map(LemonArcMap map);
LEMON_API void lemon_set_arc_value_long(LemonArcMap map, int arc, long long value);
LEMON_API long long lemon_get_arc_value_long(LemonArcMap map, int arc);

// Arc map operations - double values
LEMON_API LemonArcMap lemon_create_arc_map_double(LemonGraph graph);
LEMON_API void lemon_set_arc_value_double(LemonArcMap map, int arc, double value);
LEMON_API double lemon_get_arc_value_double(LemonArcMap map, int arc);

// Node map operations - double values
LEMON_API LemonNodeMap lemon_create_node_map_double(LemonGraph graph);
LEMON_API void lemon_destroy_node_map(LemonNodeMap map);
LEMON_API void lemon_set_node_value_double(LemonNodeMap map, int node, double value);
LEMON_API double lemon_get_node_value_double(LemonNodeMap map, int node);

// Edmonds-Karp algorithm
LEMON_API long long lemon_edmonds_karp(LemonGraph graph, LemonArcMap capacity_map, 
                                   int source, int target, 
                                   FlowResult** flow_results, int* flow_count);

// Preflow algorithm
LEMON_API long long lemon_preflow(LemonGraph graph, LemonArcMap capacity_map,
                              int source, int target,
                              FlowResult** flow_results, int* flow_count);

LEMON_API void lemon_free_results(FlowResult* results);

// Shortest path algorithms
LEMON_API ShortestPathResult* lemon_dijkstra(LemonGraph graph, LemonArcMap length_map,
                                             int source, int target);

LEMON_API ShortestPathResult* lemon_bellman_ford(LemonGraph graph, LemonArcMap length_map,
                                                 int source, int target);

// Free shortest path results
LEMON_API void lemon_free_path_result(PathResult* path);
LEMON_API void lemon_free_shortest_path_result(ShortestPathResult* result);

#ifdef __cplusplus
}
#endif

#endif // LEMON_WRAPPER_H