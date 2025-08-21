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

typedef struct {
    int arc_id;      // The arc identifier
    long long flow;  // Use long long (64-bit) to match C# long
} FlowResult;

// Graph operations
LEMON_API LemonGraph lemon_create_graph();
LEMON_API void lemon_destroy_graph(LemonGraph graph);
LEMON_API int lemon_add_node(LemonGraph graph);
LEMON_API int lemon_add_arc(LemonGraph graph, int source, int target);
LEMON_API int lemon_arc_source(LemonGraph graph, int arc);
LEMON_API int lemon_arc_target(LemonGraph graph, int arc);
LEMON_API int lemon_node_count(LemonGraph graph);
LEMON_API int lemon_arc_count(LemonGraph graph);

// Arc map operations
LEMON_API LemonArcMap lemon_create_arc_map_long(LemonGraph graph);
LEMON_API void lemon_destroy_arc_map(LemonArcMap map);
LEMON_API void lemon_set_arc_value_long(LemonArcMap map, int arc, long long value);
LEMON_API long long lemon_get_arc_value_long(LemonArcMap map, int arc);

// Edmonds-Karp algorithm
LEMON_API long long lemon_edmonds_karp(LemonGraph graph, LemonArcMap capacity_map, 
                                   int source, int target, 
                                   FlowResult** flow_results, int* flow_count);

// Preflow algorithm
LEMON_API long long lemon_preflow(LemonGraph graph, LemonArcMap capacity_map,
                              int source, int target,
                              FlowResult** flow_results, int* flow_count);

LEMON_API void lemon_free_results(FlowResult* results);

#ifdef __cplusplus
}
#endif

#endif // LEMON_WRAPPER_H