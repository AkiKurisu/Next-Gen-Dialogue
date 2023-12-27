namespace Kurisu.NGDT
{
    public interface IIterable
    {
        NodeBehavior GetChildAt(int index);
        int GetChildCount();
    }
}
