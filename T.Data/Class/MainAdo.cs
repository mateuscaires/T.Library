using T.Common;
using T.Interfaces;

namespace T.Infra.Data
{
    public class MainAdo : AdoBase, IRepositoryModule
    {
        public MainAdo(string authName) : base(authName)
        {

        }
    }
}
