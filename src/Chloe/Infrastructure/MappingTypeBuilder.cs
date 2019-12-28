using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Infrastructure
{
    public class MappingTypeBuilder
    {
        MappingType _mappingType;
        public MappingTypeBuilder(MappingType mappingType)
        {
            this._mappingType = mappingType;
        }

        public MappingTypeBuilder HasDbType(DbType dbType)
        {
            this._mappingType.DbType = dbType;
            return this;
        }
        public MappingTypeBuilder HasDbValueConverter(IDbValueConverter dbValueConverter)
        {
            if (dbValueConverter == null)
                throw new ArgumentNullException(nameof(dbValueConverter));

            this._mappingType.DbValueConverter = dbValueConverter;
            return this;
        }
        public MappingTypeBuilder HasDbParameterAssembler(IDbParameterAssembler dbParameterAssembler)
        {
            if (dbParameterAssembler == null)
                throw new ArgumentNullException(nameof(dbParameterAssembler));

            this._mappingType.DbParameterAssembler = dbParameterAssembler;
            return this;
        }
    }
}
