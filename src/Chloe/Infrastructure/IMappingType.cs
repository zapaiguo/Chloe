using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Infrastructure
{
    public interface IMappingType
    {
        Type Type { get; }
        DbType DbType { get; }
        IDbDataParameter CreateDataParameter(IDbCommand cmd, DbParam dbParam);
        object ReadFromDataReader(IDataReader reader, int ordinal);
    }

    public abstract class MappingTypeBase : IMappingType
    {
        public abstract Type Type { get; }
        public abstract DbType DbType { get; }
        public virtual IDbDataParameter CreateDataParameter(IDbCommand cmd, DbParam param)
        {
            IDbDataParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = param.Name;

            Type parameterType = null;
            if (param.Value == null || param.Value == DBNull.Value)
            {
                parameter.Value = DBNull.Value;
                parameterType = param.Type ?? typeof(object);
            }
            else
            {
                parameterType = param.Value.GetType();
                if (parameterType.IsEnum)
                {
                    parameterType = Enum.GetUnderlyingType(parameterType);
                    parameter.Value = Convert.ChangeType(param.Value, parameterType);
                }
                else
                {
                    parameter.Value = param.Value;
                }
            }

            if (param.Precision != null)
                parameter.Precision = param.Precision.Value;

            if (param.Scale != null)
                parameter.Scale = param.Scale.Value;

            if (param.Size != null)
                parameter.Size = param.Size.Value;

            if (param.DbType != null)
                parameter.DbType = param.DbType.Value;
            else
            {
                parameter.DbType = this.DbType;
            }

            const int defaultSizeOfStringOutputParameter = 4000;/* 当一个 string 类型输出参数未显示指定 Size 时使用的默认大小。如果有需要更大或者该值不足以满足需求，需显示指定 DbParam.Size 值 */

            if (param.Direction == ParamDirection.Input)
            {
                parameter.Direction = ParameterDirection.Input;
            }
            else if (param.Direction == ParamDirection.Output)
            {
                parameter.Direction = ParameterDirection.Output;
                param.Value = null;
                if (param.Size == null && param.Type == UtilConstants.TypeOfString)
                {
                    parameter.Size = defaultSizeOfStringOutputParameter;
                }
            }
            else if (param.Direction == ParamDirection.InputOutput)
            {
                parameter.Direction = ParameterDirection.InputOutput;
                if (param.Size == null && param.Type == UtilConstants.TypeOfString)
                {
                    parameter.Size = defaultSizeOfStringOutputParameter;
                }
            }
            else
                throw new NotSupportedException(string.Format("ParamDirection '{0}' is not supported.", param.Direction));

            return parameter;
        }

        public virtual object ReadFromDataReader(IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            return reader.GetValue(ordinal);
        }
    }
}
