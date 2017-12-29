using System.Collections.Generic;

namespace JS.Tools
{
    /// <summary>
    /// Paged list interface
    /// </summary>
    public interface IPagedList<T> : IList<T>
    {
        int PageIndex { get; } // Page Number 
        int PageSize { get; } // Number of Items per PAge
        int TotalCount { get; } // Total Items
        int TotalPages { get; } // Total Page Count
        bool HasPreviousPage { get; } // Has Previous Page
        bool HasNextPage { get; } // Has Next Page

        int PageFirstItemNumber { get; }
        int PageLastItemNumber { get; }
    }
}
