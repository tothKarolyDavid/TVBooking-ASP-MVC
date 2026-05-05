document.addEventListener('DOMContentLoaded', function () {
    var statisticsDataElement = document.getElementById('statistics-data');
    if (!statisticsDataElement || typeof Chart === 'undefined') {
        return;
    }

    var statistics = JSON.parse(statisticsDataElement.textContent || '{}');

    var channelLabels = statistics.channelViewers.map(function (item) {
        return item.channel;
    });

    var channelValues = statistics.channelViewers.map(function (item) {
        return item.viewers;
    });

    var genreLabels = statistics.genreViewers.map(function (item) {
        return item.genre;
    });

    var genreValues = statistics.genreViewers.map(function (item) {
        return item.viewers;
    });

    var dateLabels = statistics.dateViewers.map(function (item) {
        return item.date.split('T')[0];
    });

    var dateValues = statistics.dateViewers.map(function (item) {
        return item.minutes;
    });

    var channelViewersCtx = document.getElementById('channelViewers').getContext('2d');
    new Chart(channelViewersCtx, {
        type: 'pie',
        data: {
            labels: channelLabels,
            datasets: [{
                label: 'Channel Viewers',
                data: channelValues,
                backgroundColor: ['rgba(255, 99, 132, 0.7)', 'rgba(54, 162, 235, 0.7)', 'rgba(255, 206, 86, 0.7)'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'right'
                }
            }
        }
    });

    var genreViewersCtx = document.getElementById('genreViewers').getContext('2d');
    new Chart(genreViewersCtx, {
        type: 'pie',
        data: {
            labels: genreLabels,
            datasets: [{
                label: 'Genre Viewers',
                data: genreValues,
                backgroundColor: ['rgba(255, 99, 132, 0.7)', 'rgba(54, 162, 235, 0.7)', 'rgba(255, 206, 86, 0.7)'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'right'
                }
            }
        }
    });

    var dateViewersCtx = document.getElementById('dateViewers').getContext('2d');
    new Chart(dateViewersCtx, {
        type: 'line',
        data: {
            labels: dateLabels,
            datasets: [{
                label: 'Date Viewers',
                data: dateValues,
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value + ' minutes';
                        }
                    }
                }
            }
        }
    });
});