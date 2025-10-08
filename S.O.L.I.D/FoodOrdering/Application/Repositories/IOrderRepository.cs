using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Application.Repositories;
public interface IOrderRepository : IRepository<Domain.Entities.Order>, IReadableRepository<Domain.Entities.Order>
{
    Task<IEnumerable<Domain.Entities.Order>> GetByCustomerEmailAsync(string email);
}
