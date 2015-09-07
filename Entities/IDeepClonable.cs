namespace Entities
{
    public interface IDeepClonable
    {
        IDeepClonable DeepCopy();
    }

    public interface IDeepClonable<T> : IDeepClonable
    {
        T DeepCopy();
    }
}
