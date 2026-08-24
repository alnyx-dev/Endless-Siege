namespace Game.Core
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}