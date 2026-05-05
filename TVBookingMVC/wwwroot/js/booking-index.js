document.addEventListener('DOMContentLoaded', function () {
    var bookingsDataElement = document.getElementById('near-bookings-data');
    if (!bookingsDataElement) {
        return;
    }

    var bookings = JSON.parse(bookingsDataElement.textContent || '[]');

    function checkAndNotify(items) {
        var currentTime = new Date();

        items.forEach(function (booking) {
            var dateParts = booking.start.split('T')[0].split('-');
            var timeParts = booking.start.split('T')[1].split(':');

            var startTime = new Date(
                parseInt(dateParts[0], 10),
                parseInt(dateParts[1], 10) - 1,
                parseInt(dateParts[2], 10),
                parseInt(timeParts[0], 10),
                parseInt(timeParts[1], 10),
                parseInt(timeParts[2], 10)
            );

            var timeDifference = startTime - currentTime;

            if (timeDifference > 0 && timeDifference <= 15 * 60 * 1000) {
                alert('A(z) ' + booking.program + ' műsor ' + booking.start.split('T')[1].split(':')[0] + ':' + booking.start.split('T')[1].split(':')[1] + '-kor kezdődik a(z) ' + booking.channel + ' csatornán.');
            }
        });
    }

    checkAndNotify(bookings);

    setInterval(function () {
        checkAndNotify(bookings);
    }, 60 * 1000);
});