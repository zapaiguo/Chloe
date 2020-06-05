using System;
using System.Collections;
using System.Threading.Tasks;

namespace Chloe.Collections
{
    internal interface IAsyncEnumerator : IEnumerator
    {
#if netfx
        Task<bool> MoveNextAsync();
#else
        ValueTask<bool> MoveNextAsync();
#endif
    }
    internal interface IAsyncEnumerator<out T> : IAsyncEnumerator, IDisposable
    {
        new T Current { get; }
    }
}
