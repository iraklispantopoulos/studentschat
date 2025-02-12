using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Shared.Enums;

namespace Repository.Entities
{
    public class PromptUnit:Base,ITable
    {
        [TableColumnAttr]
        private string UnitInJsonFormat { get; set; }
        [NotMapped]
        public Shared.PromptConfiguration.Unit Unit
        {
            get => string.IsNullOrEmpty(UnitInJsonFormat)
                   ? new Shared.PromptConfiguration.Unit()
                   : JsonConvert.DeserializeObject<Shared.PromptConfiguration.Unit>(UnitInJsonFormat)!;
            set => UnitInJsonFormat = JsonConvert.SerializeObject(value);
        }
    }
}
