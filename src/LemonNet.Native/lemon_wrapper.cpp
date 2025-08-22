#define LEMON_WRAPPER_EXPORTS
#include "lemon_wrapper.h"
#include <lemon/smart_graph.h>
#include <lemon/edmonds_karp.h>
#include <lemon/preflow.h>
#include <lemon/dijkstra.h>
#include <lemon/bellman_ford.h>
#include <lemon/path.h>
#include <lemon/tolerance.h>
#include <vector>
#include <map>
#include <cstdlib>
#include <cstring>
#include <limits>

using namespace lemon;

// Define the static members from lemon::Tolerance
namespace lemon {
    float Tolerance<float>::def_epsilon = static_cast<float>(1e-4);
    double Tolerance<double>::def_epsilon = 1e-10;
    long double Tolerance<long double>::def_epsilon = 1e-14;
    // Note: long uses the base Tolerance template which provides exact comparisons
    // No def_epsilon member needed for integer types
    
    // Define INVALID constant
    const Invalid INVALID = Invalid();
}

struct GraphWrapper { 
    SmartDigraph graph;
    std::vector<SmartDigraph::Node> nodes;
    std::vector<SmartDigraph::Arc> arcs;
    
    GraphWrapper() {
    }
    
    ~GraphWrapper() {
    }
};

enum class MapType {
    LONG,
    DOUBLE
};

struct ArcMapWrapper {
    union {
        SmartDigraph::ArcMap<long>* long_map;
        SmartDigraph::ArcMap<double>* double_map;
    };
    MapType type;
    GraphWrapper* graph_wrapper;
    
    ArcMapWrapper(GraphWrapper* gw, MapType t) : graph_wrapper(gw), type(t) {
        if (type == MapType::LONG) {
            long_map = new SmartDigraph::ArcMap<long>(gw->graph);
        } else {
            double_map = new SmartDigraph::ArcMap<double>(gw->graph);
        }
    }
    
    ~ArcMapWrapper() {
        if (type == MapType::LONG && long_map) {
            delete long_map;
        } else if (type == MapType::DOUBLE && double_map) {
            delete double_map;
        }
    }
};

struct NodeMapWrapper {
    SmartDigraph::NodeMap<double>* map;
    GraphWrapper* graph_wrapper;
    
    NodeMapWrapper(GraphWrapper* gw) : graph_wrapper(gw) {
        map = new SmartDigraph::NodeMap<double>(gw->graph);
    }
    
    ~NodeMapWrapper() {
        if (map) {
            delete map;
        }
    }
};

// Template function for running max flow algorithms
template<typename Algorithm>
static long long run_max_flow_algorithm(LemonGraph graph, LemonArcMap capacity_map,
                                        int source, int target,
                                        FlowResult** flow_results, int* flow_count) {
    if (!graph || !capacity_map || !flow_results || !flow_count) return -1;
    
    GraphWrapper* graph_wrapper = static_cast<GraphWrapper*>(graph);
    ArcMapWrapper* capacity_wrapper = static_cast<ArcMapWrapper*>(capacity_map);
    
    if (source < 0 || source >= static_cast<int>(graph_wrapper->nodes.size()) ||
        target < 0 || target >= static_cast<int>(graph_wrapper->nodes.size())) {
        *flow_results = nullptr;
        *flow_count = 0;
        return -1;
    }
    
    if (capacity_wrapper->type != MapType::LONG) {
        *flow_results = nullptr;
        *flow_count = 0;
        return -1;
    }
    
    SmartDigraph::ArcMap<long> flow_map(graph_wrapper->graph);
    Algorithm alg(graph_wrapper->graph, *(capacity_wrapper->long_map),
                  graph_wrapper->nodes[source], graph_wrapper->nodes[target]);
    alg.flowMap(flow_map);
    
    alg.run();
    
    long long max_flow = alg.flowValue();
    
    std::vector<FlowResult> results;
    
    for (size_t i = 0; i < graph_wrapper->arcs.size(); ++i) {
        long long flow = flow_map[graph_wrapper->arcs[i]];
        if (flow > 0) {
            FlowResult result;
            result.arc_id = static_cast<int>(i);
            result.flow = flow;
            results.push_back(result);
        }
    }
    
    if (results.empty()) {
        *flow_count = 0;
        *flow_results = nullptr;
    } else {
        *flow_count = static_cast<int>(results.size());
        *flow_results = static_cast<FlowResult*>(malloc(sizeof(FlowResult) * results.size()));
        if (*flow_results) {
            memcpy(*flow_results, results.data(), sizeof(FlowResult) * results.size());
        } else {
            *flow_count = 0;
            return -1;
        }
    }
    
    return max_flow;
}

extern "C" {

LEMON_API LemonGraph lemon_create_graph() {
    return new GraphWrapper();
}

LEMON_API void lemon_destroy_graph(LemonGraph graph) {
    if (graph) {
        delete static_cast<GraphWrapper*>(graph);
    }
}

LEMON_API int lemon_add_node(LemonGraph graph) {
    if (!graph) return -1;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    SmartDigraph::Node node = wrapper->graph.addNode();
    wrapper->nodes.push_back(node);
    return static_cast<int>(wrapper->nodes.size() - 1);
}

LEMON_API int lemon_add_arc(LemonGraph graph, int source, int target) {
    if (!graph) return -1;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    
    if (source < 0 || source >= static_cast<int>(wrapper->nodes.size()) ||
        target < 0 || target >= static_cast<int>(wrapper->nodes.size())) {
        return -1;
    }
    
    SmartDigraph::Arc arc = wrapper->graph.addArc(
        wrapper->nodes[source], 
        wrapper->nodes[target]
    );
    wrapper->arcs.push_back(arc);
    return static_cast<int>(wrapper->arcs.size() - 1);
}

LEMON_API int lemon_arc_source(LemonGraph graph, int arc_id) {
    if (!graph) return -1;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    
    if (arc_id < 0 || arc_id >= static_cast<int>(wrapper->arcs.size())) {
        return -1;
    }
    
    SmartDigraph::Node source = wrapper->graph.source(wrapper->arcs[arc_id]);
    
    // Find the node index
    for (size_t i = 0; i < wrapper->nodes.size(); ++i) {
        if (wrapper->nodes[i] == source) {
            return static_cast<int>(i);
        }
    }
    
    return -1;
}

LEMON_API int lemon_arc_target(LemonGraph graph, int arc_id) {
    if (!graph) return -1;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    
    if (arc_id < 0 || arc_id >= static_cast<int>(wrapper->arcs.size())) {
        return -1;
    }
    
    SmartDigraph::Node target = wrapper->graph.target(wrapper->arcs[arc_id]);
    
    // Find the node index
    for (size_t i = 0; i < wrapper->nodes.size(); ++i) {
        if (wrapper->nodes[i] == target) {
            return static_cast<int>(i);
        }
    }
    
    return -1;
}

LEMON_API int lemon_node_count(LemonGraph graph) {
    if (!graph) return 0;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    return static_cast<int>(wrapper->nodes.size());
}

LEMON_API int lemon_arc_count(LemonGraph graph) {
    if (!graph) return 0;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    return static_cast<int>(wrapper->arcs.size());
}

// Arc map operations - long values
LEMON_API LemonArcMap lemon_create_arc_map_long(LemonGraph graph) {
    if (!graph) return nullptr;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    return new ArcMapWrapper(wrapper, MapType::LONG);
}

// Arc map operations - double values
LEMON_API LemonArcMap lemon_create_arc_map_double(LemonGraph graph) {
    if (!graph) return nullptr;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    return new ArcMapWrapper(wrapper, MapType::DOUBLE);
}

LEMON_API void lemon_destroy_arc_map(LemonArcMap map) {
    if (map) {
        delete static_cast<ArcMapWrapper*>(map);
    }
}

LEMON_API void lemon_set_arc_value_long(LemonArcMap map, int arc, long long value) {
    if (!map) return;
    
    ArcMapWrapper* wrapper = static_cast<ArcMapWrapper*>(map);
    
    if (wrapper->type != MapType::LONG) return;
    
    if (arc < 0 || arc >= static_cast<int>(wrapper->graph_wrapper->arcs.size())) {
        return;
    }
    
    (*(wrapper->long_map))[wrapper->graph_wrapper->arcs[arc]] = value;
}

LEMON_API long long lemon_get_arc_value_long(LemonArcMap map, int arc) {
    if (!map) return 0;
    
    ArcMapWrapper* wrapper = static_cast<ArcMapWrapper*>(map);
    
    if (wrapper->type != MapType::LONG) return 0;
    
    if (arc < 0 || arc >= static_cast<int>(wrapper->graph_wrapper->arcs.size())) {
        return 0;
    }
    
    return (*(wrapper->long_map))[wrapper->graph_wrapper->arcs[arc]];
}

LEMON_API void lemon_set_arc_value_double(LemonArcMap map, int arc, double value) {
    if (!map) return;
    
    ArcMapWrapper* wrapper = static_cast<ArcMapWrapper*>(map);
    
    if (wrapper->type != MapType::DOUBLE) return;
    
    if (arc < 0 || arc >= static_cast<int>(wrapper->graph_wrapper->arcs.size())) {
        return;
    }
    
    (*(wrapper->double_map))[wrapper->graph_wrapper->arcs[arc]] = value;
}

LEMON_API double lemon_get_arc_value_double(LemonArcMap map, int arc) {
    if (!map) return 0.0;
    
    ArcMapWrapper* wrapper = static_cast<ArcMapWrapper*>(map);
    
    if (wrapper->type != MapType::DOUBLE) return 0.0;
    
    if (arc < 0 || arc >= static_cast<int>(wrapper->graph_wrapper->arcs.size())) {
        return 0.0;
    }
    
    return (*(wrapper->double_map))[wrapper->graph_wrapper->arcs[arc]];
}

LEMON_API long long lemon_edmonds_karp(LemonGraph graph, LemonArcMap capacity_map,
                                   int source, int target, 
                                   FlowResult** flow_results, int* flow_count) {
    typedef EdmondsKarp<SmartDigraph, SmartDigraph::ArcMap<long>> EK;
    return run_max_flow_algorithm<EK>(graph, capacity_map, source, target, 
                                      flow_results, flow_count);
}

LEMON_API void lemon_free_results(FlowResult* results) {
    if (results) {
        free(results);
    }
}

LEMON_API long long lemon_preflow(LemonGraph graph, LemonArcMap capacity_map,
                              int source, int target,
                              FlowResult** flow_results, int* flow_count) {
    typedef Preflow<SmartDigraph, SmartDigraph::ArcMap<long>> PF;
    return run_max_flow_algorithm<PF>(graph, capacity_map, source, target,
                                      flow_results, flow_count);
}

// Node map operations
LEMON_API LemonNodeMap lemon_create_node_map_double(LemonGraph graph) {
    if (!graph) return nullptr;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    return new NodeMapWrapper(wrapper);
}

LEMON_API void lemon_destroy_node_map(LemonNodeMap map) {
    if (map) {
        delete static_cast<NodeMapWrapper*>(map);
    }
}

LEMON_API void lemon_set_node_value_double(LemonNodeMap map, int node, double value) {
    if (!map) return;
    
    NodeMapWrapper* wrapper = static_cast<NodeMapWrapper*>(map);
    
    if (node < 0 || node >= static_cast<int>(wrapper->graph_wrapper->nodes.size())) {
        return;
    }
    
    (*(wrapper->map))[wrapper->graph_wrapper->nodes[node]] = value;
}

LEMON_API double lemon_get_node_value_double(LemonNodeMap map, int node) {
    if (!map) return 0.0;
    
    NodeMapWrapper* wrapper = static_cast<NodeMapWrapper*>(map);
    
    if (node < 0 || node >= static_cast<int>(wrapper->graph_wrapper->nodes.size())) {
        return 0.0;
    }
    
    return (*(wrapper->map))[wrapper->graph_wrapper->nodes[node]];
}

// Shortest path algorithms
LEMON_API ShortestPathResult* lemon_dijkstra(LemonGraph graph, LemonArcMap length_map,
                                             int source, int target) {
    if (!graph || !length_map) return nullptr;
    
    GraphWrapper* graph_wrapper = static_cast<GraphWrapper*>(graph);
    ArcMapWrapper* length_wrapper = static_cast<ArcMapWrapper*>(length_map);
    
    if (length_wrapper->type != MapType::DOUBLE) return nullptr;
    
    if (source < 0 || source >= static_cast<int>(graph_wrapper->nodes.size()) ||
        target < 0 || target >= static_cast<int>(graph_wrapper->nodes.size())) {
        return nullptr;
    }
    
    typedef Dijkstra<SmartDigraph, SmartDigraph::ArcMap<double>> DijkstraAlg;
    DijkstraAlg dijkstra(graph_wrapper->graph, *(length_wrapper->double_map));
    
    dijkstra.run(graph_wrapper->nodes[source], graph_wrapper->nodes[target]);
    
    ShortestPathResult* result = static_cast<ShortestPathResult*>(malloc(sizeof(ShortestPathResult)));
    if (!result) return nullptr;
    
    result->reached = dijkstra.reached(graph_wrapper->nodes[target]) ? 1 : 0;
    result->negative_cycle = 0;  // Dijkstra doesn't detect negative cycles
    
    if (result->reached) {
        result->distance = dijkstra.dist(graph_wrapper->nodes[target]);
        
        // Build path
        typedef Path<SmartDigraph> PathType;
        PathType path = dijkstra.path(graph_wrapper->nodes[target]);
        
        std::vector<int> arc_ids;
        for (PathType::ArcIt it(path); it != INVALID; ++it) {
            SmartDigraph::Arc arc = it;
            // Find the arc ID
            for (size_t i = 0; i < graph_wrapper->arcs.size(); ++i) {
                if (graph_wrapper->arcs[i] == arc) {
                    arc_ids.push_back(static_cast<int>(i));
                    break;
                }
            }
        }
        
        if (arc_ids.empty()) {
            // Return an empty path rather than null for 0-length paths
            result->path = static_cast<PathResult*>(malloc(sizeof(PathResult)));
            if (result->path) {
                result->path->count = 0;
                result->path->arc_ids = nullptr;
            }
        } else {
            result->path = static_cast<PathResult*>(malloc(sizeof(PathResult)));
            if (result->path) {
                result->path->count = static_cast<int>(arc_ids.size());
                result->path->arc_ids = static_cast<int*>(malloc(sizeof(int) * arc_ids.size()));
                if (result->path->arc_ids) {
                    memcpy(result->path->arc_ids, arc_ids.data(), sizeof(int) * arc_ids.size());
                } else {
                    free(result->path);
                    result->path = nullptr;
                }
            }
        }
    } else {
        result->distance = std::numeric_limits<double>::infinity();
        result->path = nullptr;
    }
    
    return result;
}

LEMON_API ShortestPathResult* lemon_bellman_ford(LemonGraph graph, LemonArcMap length_map,
                                                 int source, int target) {
    if (!graph || !length_map) return nullptr;
    
    GraphWrapper* graph_wrapper = static_cast<GraphWrapper*>(graph);
    ArcMapWrapper* length_wrapper = static_cast<ArcMapWrapper*>(length_map);
    
    if (length_wrapper->type != MapType::DOUBLE) return nullptr;
    
    if (source < 0 || source >= static_cast<int>(graph_wrapper->nodes.size()) ||
        target < 0 || target >= static_cast<int>(graph_wrapper->nodes.size())) {
        return nullptr;
    }
    
    typedef BellmanFord<SmartDigraph, SmartDigraph::ArcMap<double>> BellmanFordAlg;
    BellmanFordAlg bellman_ford(graph_wrapper->graph, *(length_wrapper->double_map));
    
    bellman_ford.init();
    bellman_ford.addSource(graph_wrapper->nodes[source]);
    bool has_negative_cycle = !bellman_ford.checkedStart();
    
    ShortestPathResult* result = static_cast<ShortestPathResult*>(malloc(sizeof(ShortestPathResult)));
    if (!result) return nullptr;
    
    result->negative_cycle = has_negative_cycle ? 1 : 0;
    result->reached = (!has_negative_cycle && bellman_ford.reached(graph_wrapper->nodes[target])) ? 1 : 0;
    
    if (result->reached && !has_negative_cycle) {
        result->distance = bellman_ford.dist(graph_wrapper->nodes[target]);
        
        // Build path
        typedef Path<SmartDigraph> PathType;
        PathType path = bellman_ford.path(graph_wrapper->nodes[target]);
        
        std::vector<int> arc_ids;
        for (PathType::ArcIt it(path); it != INVALID; ++it) {
            SmartDigraph::Arc arc = it;
            // Find the arc ID
            for (size_t i = 0; i < graph_wrapper->arcs.size(); ++i) {
                if (graph_wrapper->arcs[i] == arc) {
                    arc_ids.push_back(static_cast<int>(i));
                    break;
                }
            }
        }
        
        if (arc_ids.empty()) {
            // Return an empty path rather than null for 0-length paths
            result->path = static_cast<PathResult*>(malloc(sizeof(PathResult)));
            if (result->path) {
                result->path->count = 0;
                result->path->arc_ids = nullptr;
            }
        } else {
            result->path = static_cast<PathResult*>(malloc(sizeof(PathResult)));
            if (result->path) {
                result->path->count = static_cast<int>(arc_ids.size());
                result->path->arc_ids = static_cast<int*>(malloc(sizeof(int) * arc_ids.size()));
                if (result->path->arc_ids) {
                    memcpy(result->path->arc_ids, arc_ids.data(), sizeof(int) * arc_ids.size());
                } else {
                    free(result->path);
                    result->path = nullptr;
                }
            }
        }
    } else {
        result->distance = std::numeric_limits<double>::infinity();
        result->path = nullptr;
    }
    
    return result;
}

// Free functions for shortest path results
LEMON_API void lemon_free_path_result(PathResult* path) {
    if (path) {
        if (path->arc_ids) {
            free(path->arc_ids);
        }
        free(path);
    }
}

LEMON_API void lemon_free_shortest_path_result(ShortestPathResult* result) {
    if (result) {
        if (result->path) {
            lemon_free_path_result(result->path);
        }
        free(result);
    }
}

} // extern "C"