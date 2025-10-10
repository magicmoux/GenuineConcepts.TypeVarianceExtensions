using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.TypeVarianceExtensions.Tests.TestTypes
{
    public interface IEntityType
    {
    }

    public interface IGenericEntityType<T> : IEntityType
        where T : IEntityType
    {
    }

    public class EntityType
        : IEntityType
    {
    }

    public class SpecificEntityType
        : IGenericEntityType<EntityType>
    {
    }

}
