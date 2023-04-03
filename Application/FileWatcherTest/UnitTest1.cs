using FileWatcher.WService;

namespace FileWatcherTest
{
    [TestClass]
    public class UnitTest1
    {
        const string txtLine = "0b.00.3       SA:A NETAPP   X342_TA15E1T2A10 NA00 ZL2MAWEN         ff 2344225968  520  N";
        [TestMethod]
        public void testGetDiskNumber()
        {
            string diskNumber = WorkerService.getDiskNumber(txtLine);
            Assert.AreEqual(diskNumber, "3");
        }

        [TestMethod]
        public void TestGetSerialNumber()
        {
            string serialNumber = WorkerService.getSerialNumber(txtLine, 50);
            Assert.AreEqual(serialNumber, "ZL2MAWEN");
        }
    }
}