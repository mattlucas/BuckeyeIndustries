using System;
using Orchard;

namespace Magelia.WebStore.Contracts
{
    public interface IUserModelsStateServices : IDependency
    {
        T GetFromCommerceContext<T>(String type, Int32 id) where T : class, new();
        T GetFromUserContext<T>(String type, Int32 id) where T : class, new();
        void FlushCommerceContext();
        void FlushUserContext();
    }
}
