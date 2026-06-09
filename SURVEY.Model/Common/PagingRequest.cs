using Microsoft.AspNetCore.JsonPatch.Operations;
using SURVEY.Model.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Model.Common
{
    // Không sử dụng
    public class PagingRequest
    {
        public PageInfo PageInfo { get; set; }

        public List<Sort> Sorts { get; set; }

        public List<Filter> Filters { get; set; }

        public string Fields { get; set; }

        public PagingRequest()
        {
        }

        public PagingRequest(int pageSize)
        {
            PageInfo = new PageInfo
            {
                PageIndex = 1,
                PageSize = pageSize
            };
            Filters = new List<Filter>();
            Sorts = new List<Sort>();
        }
    }

    public class PageInfo
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
    public class Sort
    {
        public string Field { get; set; }

        public OrderDirection OrderDirection { get; set; }
    }
    public class Filter
    {
        public string Field { get; set; }

        public OperatorType Operator { get; set; }

        public string Value { get; set; } = string.Empty;


        public LogicType Logic { get; set; }

        public List<Filter> Filters { get; set; }

        public Filter()
        {
        }

        public Filter(string field, OperatorType _operator, string value)
        {
            Field = field;
            Operator = _operator;
            Value = value;
        }

        public Filter(LogicType logic, string field, OperatorType _operator, string value)
        {
            Logic = logic;
            Field = field;
            Operator = _operator;
            Value = value;
        }
    }
    public class PagedList<T>
    {
        public int TotalPages { get; set; }

        public long TotalCount { get; set; }

        public int TotalFilter { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public List<T> Data { get; set; }

        public PagedList()
        {
            Data = new List<T>();
        }
    }
}
