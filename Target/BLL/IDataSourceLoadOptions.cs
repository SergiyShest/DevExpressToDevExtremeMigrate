using DevExtreme.AspNet.Data;
using System.Collections;
using System.ComponentModel;

namespace BLL
{
    public interface IDataSourceLoadOptions
    {
         static bool? StringToLowerDefault { get; set; }

        //
        // Summary:
        //     A flag indicating whether the total number of data objects is required.
         bool RequireTotalCount { get; set; }

        //
        // Summary:
        //     A flag indicating whether the number of top-level groups is required.
         bool RequireGroupCount { get; set; }

        //
        // Summary:
        //     A flag indicating whether the current query is made to get the total number of
        //     data objects.
         bool IsCountQuery { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
         bool IsSummaryQuery { get; set; }

        //
        // Summary:
        //     The number of data objects to be skipped from the start of the resulting set.
         int Skip { get; set; }

        //
        // Summary:
        //     The number of data objects to be loaded.
         int Take { get; set; }

        //
        // Summary:
        //     A sort expression.
         SortingInfo[] Sort { get; set; }

        //
        // Summary:
        //     A group expression.
         GroupingInfo[] Group { get; set; }

        //
        // Summary:
        //     A filter expression.
         IList Filter { get; set; }

        //
        // Summary:
        //     A total summary expression.
         SummaryInfo[] TotalSummary { get; set; }

        //
        // Summary:
        //     A group summary expression.
         SummaryInfo[] GroupSummary { get; set; }

        //
        // Summary:
        //     A select expression.
         string[] Select { get; set; }

        //
        // Summary:
        //     An array of data fields that limits the DevExtreme.AspNet.Data.DataSourceLoadOptionsBase.Select
        //     expression. The applied select expression is the intersection of DevExtreme.AspNet.Data.DataSourceLoadOptionsBase.PreSelect
        //     and DevExtreme.AspNet.Data.DataSourceLoadOptionsBase.Select.
         string[] PreSelect { get; set; }

        //
        // Summary:
        //     A flag that indicates whether the LINQ provider should execute the select expression.
        //     If set to false, the select operation is performed in memory.
         bool? RemoteSelect { get; set; }

        //
        // Summary:
        //     A flag that indicates whether the LINQ provider should execute grouping. If set
        //     to false, the entire dataset is loaded and grouped in memory.
         bool? RemoteGrouping { get; set; }

         bool? ExpandLinqSumType { get; set; }

        //
        // Summary:
        //     An array of primary keys.
         string[] PrimaryKey { get; set; }

        //
        // Summary:
        //     The data field to be used for sorting by default.
         string DefaultSort { get; set; }

        //
        // Summary:
        //     A flag that indicates whether filter expressions should include a ToLower() call
        //     that makes string comparison case-insensitive. Defaults to true for LINQ to Objects,
        //     false for any other provider.
         bool? StringToLower { get; set; }

        //
        // Summary:
        //     If this flag is enabled, keys and data are loaded via separate queries. This
        //     may result in a more efficient SQL execution plan.
         bool? PaginateViaPrimaryKey { get; set; }

         bool? SortByPrimaryKey { get; set; }

         bool AllowAsyncOverSync { get; set; }
    }
}
