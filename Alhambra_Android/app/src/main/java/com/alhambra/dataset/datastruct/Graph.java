package com.alhambra.dataset.datastruct;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Graph Data structure
 * All nodes and edges are represented as an index.
 *
 * I have not added any modification functions, most functions are implemented on the Hololens (Unity) side.
 *
 * @param <N> Node data type
 * @param <E> Edge data type
 */
public class Graph<N,E> {

    public interface IterateNode<N>{
        void iterateNode(GraphNode<N> node);
    }

    public interface IterateEdge<E>{
        void iterateEdge(GraphEdge<E> edge);
    }

    public interface IterateNodeEdge<N,E>{
        void iterateEdge(GraphNode<N> node,GraphEdge<E> edge);
    }


    protected ArrayList<GraphNode<N>> nodes;
    protected ArrayList<GraphEdge<E>> edges;
    private ArrayList<Integer> emptyNodes;
    private ArrayList<Integer> emptyEdges;
    public Graph(){
        nodes = new ArrayList<>();
        edges = new ArrayList<>();
        emptyEdges = new ArrayList<>();
        emptyEdges = new ArrayList<>();
    }

    public GraphNode<N> getNode(int index){
        return nodes.get(index);
    }

    public GraphEdge<E> getEdge(int index){
        return edges.get(index);
    }

    public void clear(){
        nodes.clear();
        edges.clear();
        emptyEdges.clear();
        emptyNodes.clear();
    }

    public void foreachNode(IterateNode<N> func){
        for(GraphNode<N> node: nodes){
            if(node!=null){
                func.iterateNode(node);
            }
        }
    }

    public void foreachEdge(IterateEdge<E> func){
        for(GraphEdge<E> edge: edges){
            if(edge!=null){
                func.iterateEdge(edge);
            }
        }
    }

    //TODO: add other functions
}
