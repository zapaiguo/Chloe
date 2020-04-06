using System;
using System.Collections;
using System.Threading.Tasks;

namespace Chloe.Collections
{
    internal interface IAsyncEnumerator : IEnumerator
    {
        ValueTask<bool> MoveNextAsync();
    }
    internal interface IAsyncEnumerator<out T> : IAsyncEnumerator, IDisposable
    {
        new T Current { get; }
    }
}
