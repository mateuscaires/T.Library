namespace T.Interfaces
{
    public interface IAppServiceBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {

    }
}
