using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SZORM;

/// <summary>
/// 分页模型参数
/// </summary>
public class PagedModel
{
    public int pageSize { get; set; }
    public int currentPage { get; set; }
    public string sortField { get; set; }
    public string sortOrder { get; set; }
}
public class PagedQueryModel<TSource>
{
    public PagedModel page { get; set; }
    public TSource param { get; set; }
}
/// <summary>
/// 分页返回模型
/// </summary>
/// <typeparam name="TSource"></typeparam>
public class PagedResultModel<TSource> : PagedModel
{
    public int totalCount { get; set; }
    public List<TSource> data { get; set; }
}
/// <summary>
/// 分页类，用户分页
/// </summary>
public static class PagedHelper
{
    /// <summary>
    /// 分页扩展方法
    /// </summary>
    /// <typeparam name="TSource">分页返回的data模型</typeparam>
    /// <param name="source">szorm查询变量</param>
    /// <param name="page">分页参数</param>
    /// <returns>分页模型，可直接用于antd</returns>
    public static PagedResultModel<TSource> ToPaged<TSource>(this IQuery<TSource> source, PagedModel page) where TSource : class, new()
    {
        int count = source.Count();
        PagedResultModel<TSource> result = new PagedResultModel<TSource>();
        try
        {
            if (!string.IsNullOrEmpty(page.sortField))
            {
                if (page.sortOrder == "ascend")
                {
                    source = source.Order<TSource>(page.sortField, "asc");
                }
                else
                {
                    source = source.Order<TSource>(page.sortField, "desc");
                }
            }
        }
        catch { }

        if (page.currentPage != -1)
        {
            //分页
            source = source.Skip((page.currentPage - 1) * page.pageSize).Take(page.pageSize);
        }
        else
        {
            source = source.Skip((page.currentPage - 1) * page.pageSize);
        }



        result.data = source.ToList();
        result.totalCount = count;
        return result;
    }
    public static IQuery<TSource> PagedQuery<TSource>(this IQuery<TSource> source, PagedModel page) where TSource : class, new()
    {
        PagedResultModel<TSource> result = new PagedResultModel<TSource>();
        try
        {
            if (!string.IsNullOrEmpty(page.sortField))
            {
                if (page.sortOrder == "ascend")
                {
                    source = source.Order<TSource>(page.sortField, "asc");
                }
                else
                {
                    source = source.Order<TSource>(page.sortField, "desc");
                }
            }
        }
        catch { }

        if (page.currentPage != -1)
        {
            //分页
            source = source.Skip((page.currentPage - 1) * page.pageSize).Take(page.pageSize);
        }
        else
        {
            source = source.Skip((page.currentPage - 1) * page.pageSize);
        }
        return source;
    }
}
