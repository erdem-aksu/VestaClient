using System.Collections;
using System.Collections.Generic;

namespace VestaClient.Api.Responses
{
    public class PagedResponse<T> : IReadOnlyList<T>
    {
        private IReadOnlyList<T> Items { get; }

        public PagedResponse(IEnumerable<T> obj, int totalCount = 0)
        {
            Items = new List<T>(obj);
            TotalCount = totalCount > 0 ? totalCount : Items.Count;
        }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Items.Count;

        public int TotalCount { get; }

        public T this[int index] => Items[index];
    }
}