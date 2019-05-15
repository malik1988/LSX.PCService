using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.XSSF.UserModel; //2007+,xlsx
using NPOI.HSSF.UserModel; //2003,xls
using NPOI.SS.UserModel;
using System.IO;
using System.Data;

namespace LSX.PCService.Data
{
    class Excel
    {
        public DataTable Read(string file)
        {
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //IWorkbook workbook = new XSSFWorkbook(stream);
            //ISheet sheet=  workbook.GetSheetAt(1);
            //for(int i=0;i<sheet.LastRowNum;i++)
            //{
            //    IRow row = sheet.GetRow(i);

            //}
            DataTable dt = NPOIHelper.GetDataFromExcel(stream, 0, 0);

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    DataRow row = dt.Rows[i];
            //    string x = row[0].ToString();
            //}
            stream.Close();
            //workbook.Close();
            return dt;
        }


        string columns = "发货单位,发货日期,收货单位,收货日期,总金额,币种,进口口岸,运输方式,运输工具名称,提运单号,集装箱号,备注,明细ID,出仓报关单ID,栈板,采购订单号,箱号,物料类型,商品编码,商品名称,ITEM编码,ITEM型号,ITEM描述,数量,单位,品牌,供应商,原产地,毛重,净重,单价,金额,币种,监管条件,入仓报关单号,交易号码";

        public void ImportToMysql<T>(Dictionary<string, string> columnDict, string db)
        {//根据列字典｛excel文件中列名:数据库中字段名｝ 导入到数据库文件中

        }

        IWorkbook GetWorkBook(string file)
        {
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (file.EndsWith(".xlsx"))
                return new XSSFWorkbook(stream);
            else if (file.EndsWith(".xlx"))
                return new HSSFWorkbook(stream);
            else
                throw new Exception("文件不是Excel支持的类型");
        }

        
        public void ImportToDb(string file, int sheetIndex, int headRowIndex, List<string> columns)
        {
            IWorkbook workbook = GetWorkBook(file);
            ISheet sheet = workbook.GetSheetAt(sheetIndex);

           DataTable dt = new DataTable();
            if (workbook.NumberOfSheets > 0)
            {
                sheet = workbook.GetSheetAt(sheetIndex);
                if (sheet.LastRowNum < headRowIndex)
                {
                    throw new ArgumentException("Excel模版错误,标题行索引大于总行数");
                }

                //读取标题行
                IRow row = null;
                ICell cell = null;

                row = sheet.GetRow(headRowIndex);
                object objColumnName = null;
                for (int i = 0, length = row.LastCellNum; i < length; i++)
                {
                    cell = row.GetCell(i);
                    if (cell == null)
                    {
                        continue;
                    }
                    objColumnName =cell;
                    if (objColumnName != null)
                    {                       
                        dt.Columns.Add(objColumnName.ToString());
                    }
                    else
                    {
                        dt.Columns.Add("");
                    }
                }



        }
    }
}
