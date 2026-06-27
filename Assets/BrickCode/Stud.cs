using UnityEngine;


public class Stud
{
    private Brick Member { get; set; }
    private Brick Neighbour { get; set; }
    
    public Stud(Brick member)
    {
        Member = member;
    }
    
    public Brick GetMemberBrick()
    {
        return Member;
    }

    public Brick GetNeighbourBrick()
    {
        return Neighbour;
    }

    public void SetNeighbourBrick(Brick brick)
    {
        Neighbour = brick;
    }
}
