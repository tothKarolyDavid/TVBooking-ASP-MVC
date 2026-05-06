document.addEventListener('DOMContentLoaded', function () {
    var bookingsDataElement = document.getElementById('near-bookings-data');
    if (!bookingsDataElement) {
        return;
    }

    var bookings = JSON.parse(bookingsDataElement.textContent || '[]');
    if (bookings.length === 0) {
        return;
    }

    var storageKey = 'near-bookings-notified';
    if (sessionStorage.getItem(storageKey)) {
        return;
    }

    var currentTime = new Date();
    var pendingBookings = [];

    bookings.forEach(function (booking) {
        var startTime = new Date(booking.start);
        if (startTime > currentTime && startTime <= currentTime.getTime() + 15 * 60 * 1000) {
            pendingBookings.push(booking);
        }
    });

    if (pendingBookings.length === 0) {
        return;
    }

    sessionStorage.setItem(storageKey, 'true');

    var lines = ['<strong>Upcoming shows:</strong><ul>'];
    pendingBookings.forEach(function (b) {
        lines.push('<li>' + b.program + ' (' + b.channel + ') at ' + b.start.split('T')[1].substr(0, 5) + '</li>');
    });
    lines.push('</ul>');

    var alert = document.createElement('div');
    alert.className = 'alert alert-upcoming alert-dismissible fade show mb-0 rounded-0';
    alert.setAttribute('role', 'alert');
    alert.innerHTML = lines.join('') + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>';
    var header = document.querySelector('header');
    header.insertAdjacentElement('afterend', alert);

    setTimeout(function () {
        if (alert.parentNode) {
            alert.classList.remove('show');
            setTimeout(function () { if (alert.parentNode) alert.parentNode.removeChild(alert); }, 300);
        }
    }, 15000);
});
