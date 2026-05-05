using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Threading;

namespace TVBookingMVCSelenium
{
    public class UnitTest1
    {
        private readonly string _baseUrl = "http://localhost:7233/";

        private static IWebDriver CreateDriver(string? downloadDirectory = null)
        {
            var options = new ChromeOptions();
            if (!string.IsNullOrWhiteSpace(downloadDirectory))
            {
                options.AddUserProfilePreference("download.default_directory", downloadDirectory);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("safebrowsing.enabled", true);
            }

            return new ChromeDriver(options);
        }

        [Fact]
        public void TestIndexPageBookingsTableHasRows()
        {
            string url = _baseUrl;
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);
            var table = driver.FindElement(By.ClassName("table"));
            Assert.NotNull(table);

            var rows = table.FindElements(By.TagName("tr"));
            Assert.True(rows.Count >= 2);
        }

        [Fact]
        public void TestIndexPageBookingsTableIsCorrectWithOneAgeLimitSelected()
        {
            string url = _baseUrl;
            using var driver = CreateDriver();
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
        }

        [Fact]
        public void TestIndexPageBookingsTableIsCorrectWithTwoAgeLimitSelected()
        {
            string url = _baseUrl;
            using var driver = CreateDriver();
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
        }

        [Fact]
        public void TestFreeTimeSlotsPageHasTableHasRows()
        {
            string url = _baseUrl + "Booking/FreeTimeSlots";
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);

            var table = driver.FindElement(By.ClassName("table"));

            var rows = table.FindElements(By.TagName("tr"));
            Assert.True(rows.Count >= 2);
        }

        [Fact]
        public void TestLoginAsGuestInRoom2()
        {
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("room2@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("2");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            var manageText = driver.FindElement(By.Id("manage")).Text;
            Assert.Contains("Hello room2@hotel", manageText);
        }

        [Fact]
        public void TestLogout()
        {
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("room2@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("2");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            var logoutButton = driver.FindElement(By.Id("logout"));
            logoutButton.Click();

            var loginButtonAfterLogout = driver.FindElement(By.Id("login"));

            Assert.NotNull(loginButtonAfterLogout);
        }

        [Fact]
        public void TestMyBookings()
        {
            string url = _baseUrl + "Identity/Account/Login";
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.SendKeys("room2@hotel.com");

            var roomNumberInput = driver.FindElement(By.Id("roomnumber"));
            roomNumberInput.SendKeys("2");

            var loginButton = driver.FindElement(By.Id("login-submit"));
            loginButton.Click();

            driver.Navigate().GoToUrl(_baseUrl + "Booking/UserBookings");

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

    }
}
