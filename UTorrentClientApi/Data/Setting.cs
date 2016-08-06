namespace UTorrent.Api.Data
{
    public class Setting
    {
        public string Key { get; set; }

        public SettingType Type { get; set; }

        public object Value { get; set; }

        public string Access { get; set; }
    }
}
