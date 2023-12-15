using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TVBookingMVCSelenium
{
    public class UnitTest1
    {
        private readonly string _baseUrl = "https://localhost:7233/";

        [Fact]
        public void TestIndexPageBookingsTableHasRows()
        {
            string url = _baseUrl;
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);
            var table = driver.FindElement(By.ClassName("table"));
            Assert.NotNull(table);

            var rows = table.FindElements(By.TagName("tr"));
            Assert.True(rows.Count >= 2);

            driver.Quit();
        }

        [Fact]
        public void TestIndexPageBookingsTableIsCorrectWithOneAgeLimitSelected()
        {
            string url = _baseUrl;
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var checkbox = driver.FindElement(By.XPath("//input[@value='Gyermekbarát program']"));
            checkbox.Click();

            var filterButton = driver.FindElement(By.XPath("//input[@value='Filter']"));
            filterButton.Click();

            var table = driver.FindElement(By.ClassName("table"));

            var rows = table.FindElements(By.TagName("tr"));

            bool allRowsHaveGyermekbaratProgram = true;

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count > 0)
                {
                    var ageLimit = cells[5].Text;
                    allRowsHaveGyermekbaratProgram = allRowsHaveGyermekbaratProgram && ageLimit == "Gyermekbarát program";
                }
            }

            Assert.True(allRowsHaveGyermekbaratProgram);

            driver.Quit();
        }

        [Fact]
        public void TestIndexPageBookingsTableIsCorrectWithTwoAgeLimitSelected()
        {
            string url = _baseUrl;
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var checkbox = driver.FindElement(By.XPath("//input[@value='Gyermekbarát program']"));
            checkbox.Click();

            checkbox = driver.FindElement(By.XPath("//input[@value='Korhatárra való tekintet nélkül megtekinthető']"));
            checkbox.Click();

            var filterButton = driver.FindElement(By.XPath("//input[@value='Filter']"));
            filterButton.Click();

            var table = driver.FindElement(By.ClassName("table"));

            var rows = table.FindElements(By.TagName("tr"));

            bool allRowsAreCorrect = true;

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count > 0)
                {
                    var ageLimit = cells[5].Text;

                    allRowsAreCorrect = allRowsAreCorrect && (ageLimit == "Gyermekbarát program" || ageLimit == "Korhatárra való tekintet nélkül megtekinthető");
                }
            }

            Assert.True(allRowsAreCorrect);

            driver.Quit();
        }

        [Fact]
        public void TestFreeTimeSlotsPageHasTableHasRows()
        {
            string url = _baseUrl + "Booking/FreeTimeSlots";
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var table = driver.FindElement(By.ClassName("table"));

            var rows = table.FindElements(By.TagName("tr"));
            Assert.True(rows.Count >= 2);

            driver.Quit();
        }

        [Fact]
        public void TestLoginAsGuestInRoom2()
        {
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("room2@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("2");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            var manageText = driver.FindElement(By.Id("manage")).Text;
            Assert.Contains("Hello room2@hotel", manageText);

            driver.Quit();
        }

        [Fact]
        public void TestLogout()
        {
            // Login
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("room2@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("2");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            // Logout
            var logoutButton = driver.FindElement(By.Id("logout"));
            logoutButton.Click();

            var loginButtonAfterLogout = driver.FindElement(By.Id("login"));

            Assert.NotNull(loginButtonAfterLogout);
        }

        [Fact]
        public void TestMyBookings()
        {
            // Login
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("room2@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("2");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            var myBookingsButton = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul/li[2]/a"));
            myBookingsButton.Click();

            var table = driver.FindElement(By.ClassName("table"));

            var rows = table.FindElements(By.TagName("tr"));

            bool allRowsHaveRoomNumber2 = true;
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count > 0)
                {
                    var roomNumber = cells[6].Text;
                    allRowsHaveRoomNumber2 = allRowsHaveRoomNumber2 && roomNumber == "2";

                    if (!allRowsHaveRoomNumber2)
                    {
                        break;
                    }
                }
            }

            Assert.True(allRowsHaveRoomNumber2);
        }

        [Fact]
        public void TestStatisticsAsAdmin()
        {
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("admin@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("999");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            var statisticsLink = driver.FindElement(By.CssSelector("a[href='/Booking/Statistics']"));
            statisticsLink.Click();


            var channelViewersCanvas = driver.FindElement(By.Id("channelViewers"));
            var genreViewersCanvas = driver.FindElement(By.Id("genreViewers"));
            var dateViewersCanvas = driver.FindElement(By.Id("dateViewers"));

            Assert.NotNull(channelViewersCanvas);
            Assert.NotNull(genreViewersCanvas);
            Assert.NotNull(dateViewersCanvas);
        }

        [Fact]
        public void TestXmlExport()
        {
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("admin@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("999");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            var xmlExportLink = driver.FindElement(By.CssSelector("a[href='/Booking/XMLExport']"));
            xmlExportLink.Click();

            var dateInput = driver.FindElement(By.Name("Date"));

            dateInput.Clear();
            dateInput.SendKeys("12/10/2023");

            var exportButton = driver.FindElement(By.XPath("/html/body/div/main/div/div/form/button"));
            exportButton.Click();

            Assert.True(File.Exists("C:\\MINDEN\\Egyetem\\CSharp\\beadando\\TVBooking\\TVBookingMVC\\bookings_2023-12-10.xml"));
        }

    }
}