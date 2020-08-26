using System.Collections.Generic;
using System.Dynamic;
using Entities.Models;

namespace Contracts
{
    // The interface has 2 methods with method overloading implemented
    // We do this so that we can Shape our data the way we want it
    public interface IDataShaper<T>
    {
        IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString);
        ShapedEntity ShapeData(T entity, string fieldsString);
    }
}
