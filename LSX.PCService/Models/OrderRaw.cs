﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//     Website: http://ITdos.com/Dos/ORM/Index.html
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Chloe.Entity;
using Chloe.Annotations;
namespace LSX.PCService.Models
{
    /// <summary>
    /// 实体类OrderRaw。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [TableAttribute("order_raw")]
    [Serializable]
    public partial class OrderRaw
    {
        #region Model
		private string _箱号;
		private string _收货单位;
		private string _运输工具名称;
		private string _栈板;
		private string _ITEM编码;
		private int? _数量;
		private string _交易码;
		private string _是否需要质检;
		private string _C09码;
		private int? _Channel_id;
		private string _File_id;

		/// <summary>
		/// 
		/// </summary>
		[ColumnAttribute(IsPrimaryKey = true)]
		[NonAutoIncrementAttribute]
		public string 箱号
		{
			get{ return _箱号; }
			set
			{
				this._箱号 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string 收货单位
		{
			get{ return _收货单位; }
			set
			{
				this._收货单位 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string 运输工具名称
		{
			get{ return _运输工具名称; }
			set
			{
				this._运输工具名称 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string 栈板
		{
			get{ return _栈板; }
			set
			{
				this._栈板 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string ITEM编码
		{
			get{ return _ITEM编码; }
			set
			{
				this._ITEM编码 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public int? 数量
		{
			get{ return _数量; }
			set
			{
				this._数量 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string 交易码
		{
			get{ return _交易码; }
			set
			{
				this._交易码 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string 是否需要质检
		{
			get{ return _是否需要质检; }
			set
			{
				this._是否需要质检 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string C09码
		{
			get{ return _C09码; }
			set
			{
				this._C09码 = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public int? Channel_id
		{
			get{ return _Channel_id; }
			set
			{
				this._Channel_id = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string File_id
		{
			get{ return _File_id; }
			set
			{
				this._File_id = value;
			}
		}
		#endregion
}
}
