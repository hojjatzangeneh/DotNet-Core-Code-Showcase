using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Application.Repositories;
// ISP: این اینترفیس کوچک، فقط عملیاتی که همه ی مخازن نیاز دارند را نمایش می دهد
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task SaveChangesAsync();
}
