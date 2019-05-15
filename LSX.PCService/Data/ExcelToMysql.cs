using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;
using LSX.PCService.Models;
namespace LSX.PCService.Data
{
    public delegate void ImportProcessHandler(int cur, int total);
    class ExcelToMysql
    {
        /// <summary>
        /// 生成C09码
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        OrderRaw CreateC09(OrderRaw raw)
        {
            raw.C09码 = "09" + raw.ITEM编码 + raw.交易码;
            return raw;
        }

        public static ErrorCode Import2AwmsRawData(string file,ImportProcessHandler process=null)
        {
            List<string> columnNames = new List<string>()
            {
                
                "箱号",
                "栈板号",
                "车牌",
                "发货单号",
                "到货地点",
                "物料",
                "收据号",
                "检验标志",
                "数量",
                "原产国"
            };

            int sheetIndex = 0;
            DataSet ds = Excel.Read(file, sheetIndex, columnNames);
            if (null == ds || ds.Tables.Count == 0)
                return ErrorCode.无效的Excel文件;

            string fileId = Path.GetFileNameWithoutExtension(file);


            DataTable dt = ds.Tables[0];
            int total = dt.Rows.Count;
            int cur = 0;

            foreach (DataRow row in dt.Rows)
            {
                string r_箱号 = row["箱号 "].ToString();
                string r_栈板号 = row["栈板号 "].ToString();
                string r_车牌 = row["车牌 "].ToString();
                string r_发货单号 = row["发货单号 "].ToString();
                string r_到货地点 = row["到货地点 "].ToString();
                string r_物料号 = row["物料 "].ToString();
                string r_收据号 = row["收据号 "].ToString();
                string r_检验标志 = row["检验标志 "].ToString();
                int r_数量 = int.Parse(row["数量 "].ToString());
                string r_原产国 = row["原产国 "].ToString();


                //合并
                int ret = Db.Context.Update<AwmsRawData>(a =>a.箱号 == r_箱号 && a.FileId == fileId,
                    a => new AwmsRawData()
                    {
                        数量 = a.数量 + r_数量
                    });
                if (ret <= 0) //添加新纪录
                {
                    Db.Context.Insert<AwmsRawData>(new AwmsRawData()
                    {
                        箱号 = r_箱号,
                        栈板号 = r_栈板号,
                        车牌=r_车牌,
                        发货单号 = r_发货单号,
                        到货地点 = r_到货地点,
                        物料号 = r_物料号,
                        收据号 = r_收据号,
                        检验标志 = r_检验标志,
                        数量 = r_数量,
                        原产国 = r_原产国,
                        FileId=fileId
                    });
                }
                if (null!=process)
                {
                    process.BeginInvoke(cur, total, null, null);
                }
                cur++;
            }

            return ErrorCode.成功;
        }
        public static ErrorCode Import(string file)
        {
            List<string> columnNames = new List<string>()
            {
                "收货单位",
                "运输工具名称",
                "栈板",
                "箱号",
                "ITEM编码",
                "数量",
                "交易号码",
                "09码",
                "是否需要质检"
            };

            int sheetIndex = 0;
            DataSet ds = Excel.Read(file, sheetIndex, columnNames);
            if (null == ds || ds.Tables.Count == 0)
                return ErrorCode.无效的Excel文件;

            string fileId = Path.GetFileNameWithoutExtension(file);


            DataTable dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                string r_收货单位 = row["收货单位"].ToString();
                string r_运输工具名称 = row["运输工具名称"].ToString();
                string r_栈板 = row["栈板"].ToString();
                string r_箱号 = row["箱号"].ToString();
                string r_ITEM编码 = row["ITEM编码"].ToString();
                int r_数量 = int.Parse(row["数量"].ToString());
                string r_交易码 = row["交易号码"].ToString();
                string r_09码 = row["09码"].ToString();
                string r_是否需要质检 = row["是否需要质检"].ToString();


                //合并
                int ret = Db.Context.Update<OrderRaw>(a => a.箱号 == r_箱号 && a.File_id == fileId,
                    a => new OrderRaw()
                  {
                      数量 = a.数量 + r_数量
                  });
                if (ret <= 0) //添加新纪录
                {
                    Db.Context.Insert<OrderRaw>(new OrderRaw()
                    {
                        收货单位 = r_收货单位,
                        运输工具名称 = r_运输工具名称,
                        栈板 = r_栈板,
                        箱号 = r_箱号,
                        ITEM编码 = r_ITEM编码,
                        数量 = r_数量,
                        交易码 = r_交易码,
                        C09码 = r_09码,
                        是否需要质检 = r_是否需要质检,
                        File_id = fileId
                    });
                }
            }

            return ErrorCode.成功;
        }
    }
}
