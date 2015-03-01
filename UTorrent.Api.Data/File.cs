using System.Collections.Generic;

namespace UTorrent.Api.Data
{
    public class File
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public long Downloaded { get; set; }
        public Priority Priority { get; set; }

        public int Progress
        {
            get
            {
                if (Size == 0)
                    return 0;

                double x = Downloaded / (double)Size;
                return (int)(x * 100);
            }
        }

        public string NameWithoutPath
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return Name;

                string[] tokens = Name.Replace("\\", "/").Split('/');

                return tokens.Length == 0 ? string.Empty : tokens[tokens.Length - 1];
            }
        }
    }

    public class FileCollection : List<File>
    {
        public FileCollection()
        {
        }

        public FileCollection(int capacity)
            : base(capacity)
        {
        }

        public FileCollection(IEnumerable<File> collection)
            : base(collection)
        {
        }
    }
}
