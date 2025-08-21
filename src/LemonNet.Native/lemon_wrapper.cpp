#define LEMON_WRAPPER_EXPORTS
#include "lemon_wrapper.h"
#include <lemon/smart_graph.h>
#include <lemon/edmonds_karp.h>
#include <lemon/preflow.h>
#include <lemon/tolerance.h>
#include <vector>
#include <map>
#include <cstdlib>
#include <cstring>

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

struct ArcMapWrapper {
    SmartDigraph::ArcMap<long>* map;
    GraphWrapper* graph_wrapper;
    
    ArcMapWrapper(GraphWrapper* gw) : graph_wrapper(gw) {
        map = new SmartDigraph::ArcMap<long>(gw->graph);
    }
    
    ~ArcMapWrapper() {
        if (map) {
            delete map;
        }
    }
};

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

// Arc map operations
LEMON_API LemonArcMap lemon_create_arc_map_long(LemonGraph graph) {
    if (!graph) return nullptr;
    
    GraphWrapper* wrapper = static_cast<GraphWrapper*>(graph);
    return new ArcMapWrapper(wrapper);
}

LEMON_API void lemon_destroy_arc_map(LemonArcMap map) {
    if (map) {
        delete static_cast<ArcMapWrapper*>(map);
    }
}

LEMON_API void lemon_set_arc_value_long(LemonArcMap map, int arc, long long value) {
    if (!map) return;
    
    ArcMapWrapper* wrapper = static_cast<ArcMapWrapper*>(map);
    
    if (arc < 0 || arc >= static_cast<int>(wrapper->graph_wrapper->arcs.size())) {
        return;
    }
    
    (*(wrapper->map))[wrapper->graph_wrapper->arcs[arc]] = value;
}

LEMON_API long long lemon_get_arc_value_long(LemonArcMap map, int arc) {
    if (!map) return 0;
    
    ArcMapWrapper* wrapper = static_cast<ArcMapWrapper*>(map);
    
    if (arc < 0 || arc >= static_cast<int>(wrapper->graph_wrapper->arcs.size())) {
        return 0;
    }
    
    return (*(wrapper->map))[wrapper->graph_wrapper->arcs[arc]];
}

LEMON_API long long lemon_edmonds_karp(LemonGraph graph, LemonArcMap capacity_map,
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
    
    typedef EdmondsKarp<SmartDigraph, SmartDigraph::ArcMap<long>> EK;
    SmartDigraph::ArcMap<long> flow_map(graph_wrapper->graph);
    EK ek(graph_wrapper->graph, *(capacity_wrapper->map), 
          graph_wrapper->nodes[source], graph_wrapper->nodes[target]);
    ek.flowMap(flow_map);
    
    ek.run();
    
    long long max_flow = ek.flowValue();
    
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

LEMON_API void lemon_free_results(FlowResult* results) {
    if (results) {
        free(results);
    }
}

// Preflow algorithm implementation
LEMON_API long long lemon_preflow(LemonGraph graph, LemonArcMap capacity_map,
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
    
    typedef Preflow<SmartDigraph, SmartDigraph::ArcMap<long>> PF;
    SmartDigraph::ArcMap<long> flow_map(graph_wrapper->graph);
    PF pf(graph_wrapper->graph, *(capacity_wrapper->map),
          graph_wrapper->nodes[source], graph_wrapper->nodes[target]);
    pf.flowMap(flow_map);
    
    pf.run();
    
    long long max_flow = pf.flowValue();
    
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

} // extern "C"