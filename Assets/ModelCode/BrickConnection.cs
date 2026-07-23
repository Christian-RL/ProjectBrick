namespace ModelCode
{
    public class BrickConnection
    {
        public Brick UpperBrick { get; }
        public int UpperStudX { get; }
        public int UpperStudZ { get; }

        public Brick LowerBrick { get; }
        public int LowerStudX { get; }
        public int LowerStudZ { get; }

        public BrickConnection(
            Brick upperBrick,
            int upperStudX,
            int upperStudZ,
            Brick lowerBrick,
            int lowerStudX,
            int lowerStudZ
        )
        {
            UpperBrick = upperBrick;
            UpperStudX = upperStudX;
            UpperStudZ = upperStudZ;

            LowerBrick = lowerBrick;
            LowerStudX = lowerStudX;
            LowerStudZ = lowerStudZ;
        }
    }