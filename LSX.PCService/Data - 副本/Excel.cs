using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExcelDataReader;
using System.IO;
using System.Data;
namespace LSX.PCService.Data
{
    class Excel
    {
       
        /// <summary>
        /// 从EXCEL文件中读取数据保存到Dataset中。
        /// 注意：Excel表中第一行必须是列名。
        /// </summary>
        /// <param name="file">文件名（全路径）</param>
        /// <param name="sheetIndex">需要导入的Sheet索引号，从0开始</param>
        /// <param name="columns">需要的列名List</param>
        /// <returns>数据集合</returns>
        public static DataSet Read(string file, int sheetIndex,List<string> columns)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = true,
                        FilterSheet = (tableReader, _sheetIndex) =>
                        {//只读取第一个表                           
                            if (_sheetIndex == sheetIndex)
                                return true;
                            else
                                return false;
                        },
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            EmptyColumnNamePrefix = "_",
                            UseHeaderRow = true,

                            FilterColumn = (columnReader, columnIndex) =>
                            {
                                var name = columnReader.GetValue(columnIndex);
                                if (null == name || null == columns)
                                    return false;
                                string columnName = name.ToString().Trim();
                                if (columns.Contains(columnName))
                                    return true;
                               
                                return false;
                            }
                        }

                    });
                    return result;
                }
            }
        }
    }
}
