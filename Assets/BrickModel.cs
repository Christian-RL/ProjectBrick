using UnityEngine;
using System.Collections.Generic;

/**
 * Graph representation of a current model of bricks
 */
public class BrickModelGraph
{
    private List<BrickModelNode>_Nodes{get;set;} =  new List<BrickModelNode>();
    private BrickModelNode _CurrentNode{get;set;}

    public BrickModelGraph()
    {
    }

    private void addBrickModelNode(BrickModelNode node)
    {
        _Nodes.Add(node);
        _CurrentNode = node;
    }

    private BrickModelNode getCurrentNode()
    {
        return _CurrentNode;
    }
    
    private class BrickModelNode
    {
        private BrickModelNode Parent{get;set;}
        private List<BrickModelNode> Children{get;set;}
        
        private Brick NodeBrick{get;set;}
        
        public BrickModelNode(BrickModelNode parent,  List<BrickModelNode> children, Brick nodeBrick)
        {
            this.Parent = parent;
            this.Children = children;
            this.NodeBrick = nodeBrick;
        }

        public void AddChild(BrickModelNode child)
        {
            this.Children.Add(child);
        }

        public List<BrickModelNode> GetChildren()
        {
            return this.Children;
        }

        public void SetParent(BrickModelNode parent)
        {
            this.Parent = parent;
        }

        public BrickModelNode GetParent()
        {
            return this.Parent;
        }

        public void setNodeBrick(Brick nodeBrick)
        {
            this.NodeBrick = nodeBrick;
        }

        public Brick GetNodeBrick()
        {
            return this.NodeBrick;
        }
    }
}
