using System.Collections.Generic;

public class BrickModelGraph
{
    private readonly List<BrickModelNode> _nodes = new();
    private readonly Dictionary<Brick, BrickModelNode> _nodeByBrick = new();

    private BrickModelNode _currentNode;

    public BrickModelGraph(Brick startingBrick)
    {
        _currentNode = GetOrCreateNode(startingBrick);
        ConstructFrom(_currentNode);
    }

    private void ConstructFrom(BrickModelNode node)
    {
        Brick currentBrick = node.GetNodeBrick();

        Stud[,] brickStuds = currentBrick.GetStuds();

        for (int x = 0; x < brickStuds.GetLength(0); x++)
        {
            for (int y = 0; y < brickStuds.GetLength(1); y++)
            {
                Stud stud = brickStuds[x, y];

                if (stud == null)
                {
                    continue;
                }

                Brick neighbourBrick = stud.GetNeighbourBrick();

                if (neighbourBrick == null)
                {
                    continue;
                }

                BrickModelNode neighbourNode = GetOrCreateNode(neighbourBrick);

                if (!node.HasChild(neighbourNode))
                {
                    node.AddChild(neighbourNode);
                }

                if (!neighbourNode.HasChild(node))
                {
                    neighbourNode.AddChild(node);
                }

                if (!neighbourNode.HasBeenExpanded)
                {
                    ConstructFrom(neighbourNode);
                }
            }
        }

        node.HasBeenExpanded = true;
    }

    private BrickModelNode GetOrCreateNode(Brick brick)
    {
        if (_nodeByBrick.TryGetValue(brick, out BrickModelNode existingNode))
        {
            return existingNode;
        }

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
            _children.Add(child);
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