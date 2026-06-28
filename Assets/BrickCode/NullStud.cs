using System;

namespace BrickCode
{
    public class NullStud : Stud
    {
        public NullStud(Brick member) : base (member)
        {
         
        }
        
        public override Brick GetMemberBrick() { return null; }
        public override Brick GetNeighbourBrick() { return null; }

        public override void SetNeighbourBrick(Brick brick)
        {
            throw new NullReferenceException();
        }

    }
}