namespace BrickCode
{
    public class NoStud : Stud
    {
        public NoStud (Brick member) : base(member){}

        public override Brick GetNeighbourBrick()
        {
            return null;
        }

        public override void SetNeighbourBrick(Brick brick)
        {
        }
    }
    
}