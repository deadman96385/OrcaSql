using System;
using System.Linq;
using System.Threading;
using OrcaSql.Core.Engine.Records.VariableLengthDataProxies;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Core.Engine.Records
{
    /// <summary>
    /// A column value whose physical bytes are not read until <see cref="Force"/> is called.
    /// This is useful for streaming large off-row values when a caller only needs to inspect
    /// a subset of rows or columns. Buffering unforced rows still retains each value's physical
    /// data proxy until the row is collected.
    /// </summary>
    public sealed class DeferredColumnValue : IDeferredValue
    {
        private readonly Lazy<object> _value;

        public DeferredColumnValue(IVariableLengthDataProxy proxy, ISqlType sqlType)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));
            if (sqlType == null) throw new ArgumentNullException(nameof(sqlType));

            _value = new Lazy<object>(() =>
            {
                var data = proxy.GetBytes()?.ToArray();
                return sqlType.GetValue(data ?? new byte[0]);
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>Read and materialize the column value, caching the result.</summary>
        public object Force()
        {
            return _value.Value;
        }
    }
}
