using System.Linq;
using System.Collections.Generic;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels;
using Lykke.Job.BlobToBlobConverter.Common;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    public class StructureBuilder : IStructureBuilder
    {
        internal static string MainContainer => "candles";

        public Dictionary<string, string> GetMappingStructure()
        {
            var result = new Dictionary<string, string>
            {
                { MainContainer, OutCandlestick.GetColumnsString() },
            };
            return result;
        }

        public TablesStructure GetTablesStructure()
        {
            return new TablesStructure
            {
                Tables = new List<TableStructure>
                {
                    new TableStructure
                    {
                        TableName = "Candlesticks",
                        AzureBlobFolder = MainContainer,
                        Colums = OutCandlestick.GetStructure()
                            .Select(p => new ColumnInfo{ ColumnName = p.Item1, ColumnType = p.Item2 })
                            .ToList(),
                    }
                },
            };
        }
    }
}
