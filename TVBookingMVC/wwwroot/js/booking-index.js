document.addEventListener('DOMContentLoaded', function () {
    var bookingsDataElement = document.getElementById('near-bookings-data');
    if (!bookingsDataElement) {
        return;
    }

    var bookings = JSON.parse(bookingsDataElement.textContent || '[]');

    function checkAndNotify(items) {
        var currentTime = new Date();
        var pendingBookings = [];

        items.forEach(function (booking) {
            var startTime = new Date(booking.start);

            if (startTime > currentTime && startTime <= currentTime.getTime() + 15 * 60 * 1000) {
                pendingBookings.push(booking);
            }
        });

        if (pendingBookings.length > 0) {
            var lines = ['Upcoming shows:'];
            pendingBookings.forEach(function (b) {
                lines.push('• ' + b.program + ' (' + b.channel + ') at ' + b.start.split('T')[1].substr(0, 5));
            });
            alert(lines.join('\n'));
        }
    }

    var notified = false;
    function doCheck() {
        if (!notified) {
            checkAndNotify(bookings);
            notified = true;
        }
    }

    doCheck();
    setTimeout(doCheck, 60 * 1000);
});