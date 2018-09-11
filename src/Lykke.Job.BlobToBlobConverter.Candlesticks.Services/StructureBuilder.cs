using System.Linq;
using System.Collections.Generic;
using Lykke.Job.BlobToBlobConverter.Candlesticks.Core.Domain.OutputModels;
using Lykke.Job.BlobToBlobConverter.Common;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Services
{
    public class StructureBuilder : IStructureBuilder
    {
        internal static string MainContainer => "candles";

        public bool IsDynamicStructure => false;

        public bool IsAllBlobsReprocessingRequired(TablesStructure currentStructure)
        {
            return false;
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
