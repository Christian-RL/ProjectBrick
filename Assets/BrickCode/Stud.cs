using UnityEngine;


public class Stud
{
    private Brick Member { get; set; }
    private Brick Neighbour { get; set; }
    
    public Stud(Brick member)
    {
        Member = member;
    }
    
    public virtual Brick GetMemberBrick()
    {
        return Member;
    }

    public virtual Brick GetNeighbourBrick()
    {
        return Neighbour;
    }

    public virtual void SetNeighbourBrick(Brick brick)
    {
        Neighbour = brick;
    }
}
