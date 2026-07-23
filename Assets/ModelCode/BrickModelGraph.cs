using System.Collections.Generic;

namespace ModelCode
{


    /**
     * Build graph of connected bricks.
     */
    public class BrickModelGraph
    {
        private readonly List<BrickModelNode> _nodes = new(); //each node is one logical brick
        private readonly Dictionary<Brick, BrickModelNode> _nodeByBrick = new();
        private readonly BrickModelNode _currentNode;

        /**
         * Create the graph.
         */
        public BrickModelGraph(Brick startingBrick)
        {
            _currentNode = GetOrCreateNode(startingBrick);
            ConstructFrom(_currentNode);
        }

        /**
         * Main graph-building method.
         * -    Take a node.
         * -    Look at brick inside node.
         * -    Find neighbouring bricks connected through studs.
         * -    Create nodes for those neighbours.
         * -    Repeat on unexpanded nodes.
         */
        private void ConstructFrom(BrickModelNode node)
        {
            if (node.HasBeenExpanded) return;
            node.HasBeenExpanded = true;
            Brick currentBrick = node.GetNodeBrick();
            if (currentBrick == null) return;
            Stud[,] brickStuds = currentBrick.GetStuds();
            for (int x = 0; x < brickStuds.GetLength(0); x++)
            {
                for (int y = 0; y < brickStuds.GetLength(1); y++)
                {
                    Stud stud = brickStuds[x, y];

                    if (stud == null) continue;
                    Brick neighbourBrick = stud.GetNeighbourBrick();
                    if (neighbourBrick == null) continue;
                    BrickModelNode neighbourNode = GetOrCreateNode(neighbourBrick);
                    if (!node.HasChild(neighbourNode)) node.AddChild(neighbourNode);
                    if (!neighbourNode.HasChild(node)) neighbourNode.AddChild(node);
                    ConstructFrom(neighbourNode);
                }
            }
        }

        /**
         * Return graph node for given Brick.
         * If node does not exist create a new one.
         */
        private BrickModelNode GetOrCreateNode(Brick brick)
        {
            if (_nodeByBrick.TryGetValue(brick, out BrickModelNode existingNode)) return existingNode;
            BrickModelNode newNode = new BrickModelNode(brick);
            _nodeByBrick.Add(brick, newNode);
            _nodes.Add(newNode);
            return newNode;
        }

        public List<BrickModelNode> GetNodes()
        {
            return _nodes;
        }

        public BrickModelNode GetCurrentNode()
        {
            return _currentNode;
        }

        /**
         * Represents one node in the graph.
         */
        public class BrickModelNode
        {
            private readonly List<BrickModelNode> _children = new();

            private Brick _nodeBrick;

            public bool HasBeenExpanded { get; set; }

            public BrickModelNode(Brick nodeBrick)
            {
                _nodeBrick = nodeBrick;
            }

            public void AddChild(BrickModelNode child)
            {
                if (!_children.Contains(child)) _children.Add(child);
            }

            public bool HasChild(BrickModelNode child)
            {
                return _children.Contains(child);
            }

            public List<BrickModelNode> GetChildren()
            {
                return _children;
            }

            public void SetNodeBrick(Brick nodeBrick)
            {
                _nodeBrick = nodeBrick;
            }

            public Brick GetNodeBrick()
            {
                return _nodeBrick;
            }
        }
    }
}