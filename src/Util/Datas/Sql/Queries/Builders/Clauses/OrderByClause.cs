﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Util.Datas.Sql.Queries.Builders.Abstractions;
using Util.Datas.Sql.Queries.Builders.Core;
using Util.Domains.Repositories;
using Util.Properties;

namespace Util.Datas.Sql.Queries.Builders.Clauses {
    /// <summary>
    /// 排序子句
    /// </summary>
    public class OrderByClause : IOrderByClause {
        /// <summary>
        /// 排序项列表
        /// </summary>
        private readonly List<OrderByItem> _items;
        /// <summary>
        /// Sql方言
        /// </summary>
        private readonly IDialect _dialect;
        /// <summary>
        /// 实体解析器
        /// </summary>
        private readonly IEntityResolver _resolver;
        /// <summary>
        /// 实体注册器
        /// </summary>
        private readonly IEntityAliasRegister _register;

        /// <summary>
        /// 初始化排序子句
        /// </summary>
        /// <param name="dialect">Sql方言</param>
        /// <param name="resolver">实体解析器</param>
        /// <param name="register">实体别名注册器</param>
        public OrderByClause( IDialect dialect, IEntityResolver resolver, IEntityAliasRegister register ) {
            _items = new List<OrderByItem>();
            _dialect = dialect;
            _resolver = resolver;
            _register = register;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="order">排序列表</param>
        public void OrderBy( string order ) {
            if( string.IsNullOrWhiteSpace( order ) )
                return;
            order.Split( ',' ).ToList().ForEach( column => AddItem( column ) );
        }

        /// <summary>
        /// 添加排序项
        /// </summary>
        protected void AddItem( string column, bool desc = false, Type type = null ) {
            if ( column.IsEmpty() )
                return;
            if ( Exists( column ) )
                return;
            _items.Add( new OrderByItem( column , desc , type ) );
        }

        /// <summary>
        /// 是否已存在
        /// </summary>
        /// <param name="column">排序列</param>
        protected bool Exists( string column ) {
            var item = new OrderByItem( column );
            return _items.Exists( t => t.Column.ToLower() == item.Column.ToLower() );
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="column">排序列</param>
        /// <param name="desc">是否倒排</param>
        public void OrderBy<TEntity>( Expression<Func<TEntity, object>> column, bool desc = false ) {
            if( column == null )
                return;
            AddItem( _resolver.GetColumn( column ), desc, typeof( TEntity ) );
        }

        /// <summary>
        /// 添加到OrderBy子句
        /// </summary>
        /// <param name="sql">Sql语句</param>
        public void AppendSql( string sql ) {
            _items.Add( new OrderByItem( sql, raw: true ) );
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="pager">分页</param>
        public void Validate( IPager pager ) {
            if( pager == null )
                return;
            if( _items.Count == 0 )
                throw new ArgumentException( LibraryResource.OrderIsEmptyForPage );
        }

        /// <summary>
        /// 获取Sql
        /// </summary>
        public string ToSql() {
            if( _items.Count == 0 )
                return null;
            return $"Order By {_items.Select( t => t.ToSql( _dialect, _register ) ).Join()}";
        }
    }
}
