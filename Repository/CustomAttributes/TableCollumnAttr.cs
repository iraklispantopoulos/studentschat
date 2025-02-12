namespace Repository
{
    public class TableColumnAttr : Attribute
    {
        /// <summary>
        /// The auto save is being done by the base repository, when no explicit method to save 
        /// the record exists. 
        /// </summary>
        public bool AutoSave { get; set; } = true;
        public TableColumnAttr() { }
        public TableColumnAttr(bool autoSave)
        {
            AutoSave = autoSave;
        }
    }
}
