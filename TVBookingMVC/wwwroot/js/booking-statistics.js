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

    function generateColors(count) {
        var colors = [];
        for (var i = 0; i < count; i++) {
            var hue = (i * 360 / count) % 360;
            colors.push('hsla(' + hue + ', 70%, 60%, 0.7)');
        }
        return colors;
    }

    if (channelLabels.length > 0) {
        var channelViewersCtx = document.getElementById('channelViewers').getContext('2d');
        new Chart(channelViewersCtx, {
            type: 'pie',
            data: {
                labels: channelLabels,
                datasets: [{
                    label: 'Bookings by Channel',
                    data: channelValues,
                    backgroundColor: generateColors(channelLabels.length),
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            boxWidth: 12,
                            boxHeight: 12,
                            padding: 16
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                var total = context.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var value = context.parsed;
                                var pct = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return context.label + ': ' + value + ' (' + pct + '%)';
                            }
                        }
                    }
                }
            }
        });
    } else {
        document.getElementById('channelViewers').classList.add('d-none');
        document.getElementById('channelViewers-empty').classList.remove('d-none');
    }

    if (genreLabels.length > 0) {
        var genreViewersCtx = document.getElementById('genreViewers').getContext('2d');
        new Chart(genreViewersCtx, {
            type: 'pie',
            data: {
                labels: genreLabels,
                datasets: [{
                    label: 'Bookings by Genre',
                    data: genreValues,
                    backgroundColor: generateColors(genreLabels.length),
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'right',
                        labels: {
                            boxWidth: 12,
                            boxHeight: 12,
                            padding: 16
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                var total = context.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                var value = context.parsed;
                                var pct = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return context.label + ': ' + value + ' (' + pct + '%)';
                            }
                        }
                    }
                }
            }
        });
    } else {
        document.getElementById('genreViewers').classList.add('d-none');
        document.getElementById('genreViewers-empty').classList.remove('d-none');
    }

    if (dateLabels.length > 0) {
        var dateViewersCtx = document.getElementById('dateViewers').getContext('2d');
        new Chart(dateViewersCtx, {
            type: 'line',
            data: {
                labels: dateLabels,
                datasets: [{
                    label: 'Minutes Booked',
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
                        title: {
                            display: true,
                            text: 'Minutes Booked'
                        }
                    }
                }
            }
        });
    } else {
        document.getElementById('dateViewers').classList.add('d-none');
        document.getElementById('dateViewers-empty').classList.remove('d-none');
    }
});
