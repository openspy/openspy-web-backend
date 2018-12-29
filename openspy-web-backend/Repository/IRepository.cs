using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Repository
{
    public partial interface IRepository<Mdl, Lkup>
    {
        Task<IEnumerable<Mdl>> Lookup(Lkup lookup);
        Task<bool> Delete(Lkup lookup);
        Task<Mdl> Update(Mdl model);
        Task<Mdl> Create(Mdl model);
    }
}
