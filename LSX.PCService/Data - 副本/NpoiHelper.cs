using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using NPOI.XSSF.UserModel;
using NPOI.XSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace LSX.PCService.Data
{
    /// <summary>
    /// NPOI操作EXECL帮助类
    /// </summary>
    public static class NPOIHelper
    {
        /// <summary>
        /// EXECL最大列宽
        /// </summary>
        public static readonly int MAX_COLUMN_WIDTH = 100 * 256;

        /// <summary>
        /// 最大行索引
        /// </summary>
        private static readonly int MAX_ROW_INDEX = 65536;

        /// <summary>
        /// 生成EXECL文件，通过读取DataTable和列头映射信息
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="ColumnInfoList">列字段映射信息</param>
        /// <returns>文件流</returns>
        public static MemoryStream Export(DataTable dt, List<ColumnInfo> ColumnInfoList)
        {
            if (dt == null || ColumnInfoList == null)
            {
                throw new ArgumentNullException();
            }
            int rowHeight = 20;
            //每个标签页最多行数
            int sheetRow = 65536;
            XSSFWorkbook workbook = new XSSFWorkbook();

            //文本样式
            ICellStyle centerStyle = workbook.CreateCellStyle();
            centerStyle.VerticalAlignment = VerticalAlignment.Center;
            centerStyle.Alignment = HorizontalAlignment.Center;

            ICellStyle leftStyle = workbook.CreateCellStyle();
            leftStyle.VerticalAlignment = VerticalAlignment.Center;
            leftStyle.Alignment = HorizontalAlignment.Left;

            ICellStyle rightStyle = workbook.CreateCellStyle();
            rightStyle.VerticalAlignment = VerticalAlignment.Center;
            rightStyle.Alignment = HorizontalAlignment.Right;

            //寻找列头和DataTable之间映射关系
            foreach (DataColumn col in dt.Columns)
            {
                ColumnInfo info = ColumnInfoList.FirstOrDefault<ColumnInfo>(e => e.Field.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (info != null)
                {
                    switch (info.Align.ToLower())
                    {
                        case "left":
                            info.Style = leftStyle;
                            break;
                        case "center":
                            info.Style = centerStyle;
                            break;
                        case "right":
                            info.Style = rightStyle;
                            break;
                    }
                    info.IsMapDT = true;
                }
            }

            int sheetNum = (int)Math.Ceiling(dt.Rows.Count * 1.0 / MAX_ROW_INDEX);
            //最多生成5个标签页的数据
            sheetNum = sheetNum > 3 ? 3 : (sheetNum == 0 ? 1 : sheetNum);
            ICell cell = null;
            object cellValue = null;
            for (int sheetIndex = 0; sheetIndex < sheetNum; sheetIndex++)
            {
                ISheet sheet = workbook.CreateSheet();
                sheet.CreateFreezePane(0, 1, 0, 1);
                //输出表头
                IRow headerRow = sheet.CreateRow(0);
                //设置行高
                headerRow.HeightInPoints = rowHeight;
                //首行样式
                ICellStyle HeaderStyle = workbook.CreateCellStyle();
                HeaderStyle.FillPattern = FillPattern.SolidForeground;// FillPattern.SolidForeground;
                HeaderStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                IFont font = workbook.CreateFont();
                font.Boldweight = short.MaxValue;
                HeaderStyle.SetFont(font);
                HeaderStyle.VerticalAlignment = VerticalAlignment.Center; ;
                HeaderStyle.Alignment = HorizontalAlignment.Center;


                //输出表头信息 并设置表头样式
                int i = 0;
                foreach (var data in ColumnInfoList)
                {
                    cell = headerRow.CreateCell(i);
                    cell.SetCellValue(data.Header.Trim());
                    cell.CellStyle = HeaderStyle;
                    i++;
                }

                //开始循环所有行
                int iRow = 1;

                int startRow = sheetIndex * (sheetRow - 1);
                int endRow = (sheetIndex + 1) * (sheetRow - 1);
                endRow = endRow <= dt.Rows.Count ? endRow : dt.Rows.Count;

                for (int rowIndex = startRow; rowIndex < endRow; rowIndex++)
                {
                    IRow row = sheet.CreateRow(iRow);
                    row.HeightInPoints = rowHeight;
                    i = 0;
                    foreach (var item in ColumnInfoList)
                    {
                        cell = row.CreateCell(i);
                        if (item.IsMapDT)
                        {
                            cellValue = dt.Rows[rowIndex][item.Field];
                            cell.SetCellValue(cellValue != DBNull.Value ? cellValue.ToString() : string.Empty);
                            cell.CellStyle = item.Style;
                        }
                        i++;
                    }
                    iRow++;
                }

                //自适应列宽度
                for (int j = 0; j < ColumnInfoList.Count; j++)
                {
                    sheet.AutoSizeColumn(j);
                    int width = sheet.GetColumnWidth(j) + 2560;
                    sheet.SetColumnWidth(j, width > MAX_COLUMN_WIDTH ? MAX_COLUMN_WIDTH : width);
                }
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);
            return ms;
        }


        /// <summary>
        /// 获取第一个Sheet
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Sheet</returns>
        public static ISheet GetFirstSheet(string filePath)
        {
            using (Stream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            {
                IWorkbook workbook = new XSSFWorkbook(stream);
                if (workbook.NumberOfSheets > 0)
                {
                    return workbook.GetSheetAt(0);
                }
            }
            return null;
        }

        #region "Excel模版数据读取相关"

        /// <summary>
        /// 通过输入流初始化workbook
        /// </summary>
        /// <param name="ins">输入流</param>
        /// <returns>workbook对象</returns>
        public static IWorkbook InitWorkBook(Stream ins)
        {
            return new XSSFWorkbook(ins);
        }

        /// <summary>
        /// 从文件中读取数据并保存为DataTable
        /// </summary>
        /// <param name="ins">Excel文件流</param>
        /// <param name="sheetIndex">需要读取的Sheet表格索引号，从0开始</param>
        /// <param name="headRowIndex">标题所在行索引号，从0开始</param>
        /// <returns>DataTable数据</returns>
        public static DataTable GetDataFromExcel(Stream ins, int sheetIndex, int headRowIndex = 0)
        {
            IWorkbook workbook = InitWorkBook(ins);
            ISheet fSheet = null;
            DataTable dt = new DataTable();
            if (workbook.NumberOfSheets > 0)
            {
                fSheet = workbook.GetSheetAt(sheetIndex);
                if (fSheet.LastRowNum < headRowIndex)
                {
                    throw new ArgumentException("Excel模版错误,标题行索引大于总行数");
                }

                //读取标题行
                IRow row = null;
                ICell cell = null;

                row = fSheet.GetRow(headRowIndex);
                object objColumnName = null;
                for (int i = 0, length = row.LastCellNum; i < length; i++)
                {
                    cell = row.GetCell(i);
                    if (cell == null)
                    {
                        continue;
                    }
                    objColumnName = GetCellVale(cell);
                    if (objColumnName != null)
                    {
                        dt.Columns.Add(objColumnName.ToString());
                    }
                    else
                    {
                        dt.Columns.Add("");
                    }
                }

                //读取数据行
                object[] entityValues = null;
                int columnCount = dt.Columns.Count;

                for (int i = headRowIndex + 1, length = fSheet.LastRowNum; i < length; i++)
                {
                    row = fSheet.GetRow(i);
                    if (row == null)
                    {
                        continue;
                    }
                    entityValues = new object[columnCount];
                    //用于判断是否为空行
                    bool isHasData = false;
                    int dataColumnLength = row.LastCellNum < columnCount ? row.LastCellNum : columnCount;
                    for (int j = 0; j < dataColumnLength; j++)
                    {
                        cell = row.GetCell(j);
                        if (cell == null)
                        {
                            continue;
                        }
                        entityValues[j] = GetCellVale(cell);
                        if (!isHasData && j < columnCount && entityValues[j] != null)
                        {
                            isHasData = true;
                        }
                    }
                    if (isHasData)
                    {
                        dt.Rows.Add(entityValues);
                    }
                }
            }
            return dt;
        }



        /// <summary>
        /// 获取单元格值
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <returns>单元格值</returns>
        private static object GetCellVale(ICell cell)
        {
            object obj = null;
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        obj = cell.DateCellValue;
                    }
                    else
                    {
                        obj = cell.NumericCellValue;
                    }
                    break;
                case CellType.String:
                    obj = cell.StringCellValue;
                    break;
                case CellType.Boolean:
                    obj = cell.BooleanCellValue;
                    break;
                case CellType.Formula:
                    obj = cell.CellFormula;
                    break;

            }
            return obj;
        }
        #endregion



        #region "私有方法"
        /// 
        /// 把1,2,3,...,35,36转换成A,B,C,...,Y,Z
        /// 
        /// 要转换成字母的数字（数字范围在闭区间[1,36]）
        /// 
        private static string NumberToChar(int number)
        {
            if (1 <= number && 36 >= number)
            {
                int num = number + 64;
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] btNumber = new byte[] { (byte)num };
                return asciiEncoding.GetString(btNumber);
            }
            return "A";
        }
        #endregion
    }


}

