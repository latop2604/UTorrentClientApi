using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using UTorrent.Api.Data;

namespace UTorrent.Api.UnitTest
{
    [TestClass]
    public class DataUnitTest
    {
        [TestMethod]
        public void TestFileProgress()
        {
            const long d = 354;
            const long s = 424;
            Data.File file = new Data.File {Downloaded = d, Size = s};
            int progress = file.Progress;

            const double x = d / (double)s;
            const int expected = (int)(x * 100);
            Assert.AreEqual(expected, progress);
        }

        [TestMethod]
        public void TestFileProgressWithSizeOfZero()
        {
            Data.File file = new Data.File {Downloaded = 345, Size = 0};
            int progress = file.Progress;
            Assert.AreEqual(0, progress);
        }

        [TestMethod]
        public void TestNameWithoutPathWithEmptyName()
        {
            Data.File file = new Data.File {Name = string.Empty};
            Assert.AreEqual(string.Empty, file.NameWithoutPath);
        }

        [TestMethod]
        public void TestNameWithoutPathWithNullName()
        {
            Data.File file = new Data.File {Name = null};
            Assert.AreEqual(null, file.NameWithoutPath);
        }

        [TestMethod]
        public void TestNameWithoutPath()
        {
            Data.File file = new Data.File {Name = "/azerty/qwerty/file.txt"};
            const string expected = "file.txt";
            Assert.AreEqual(expected, file.NameWithoutPath);
        }

        [TestMethod]
        public void TestTorrentSetFiles()
        {
            Data.Torrent torrent = new Data.Torrent();
            Assert.IsNotNull(torrent.Files);
            Assert.AreEqual(0, torrent.Files.Count);

            List<Data.File> files = new List<Data.File>
                {
                    new Data.File(),
                    new Data.File(),
                    new Data.File(),
                };

            torrent.Files = files;
            Assert.AreEqual(files.Count, torrent.Files.Count);

            torrent.Files = null;
            Assert.IsNotNull(torrent.Files);
            Assert.AreEqual(0, torrent.Files.Count);
        }

        [TestMethod]
        public void TestTorrentCollection_ctor()
        {
            Data.TorrentCollection torrents = new TorrentCollection();
            torrents = new TorrentCollection(torrents);
            torrents = new TorrentCollection(100);
        }
    }
}
