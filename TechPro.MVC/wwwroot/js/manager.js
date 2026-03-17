/**
 * StoreAdmin Manager Dashboard JS
 * Handles Chart.js initialization and controls
 */

let charts = {
    revenue: null,
    status: null,
    topParts: null
};

document.addEventListener('DOMContentLoaded', function() {
    console.log('Manager Dashboard initializing...');
    initCharts();
});

function initCharts() {
    initRevenueChart();
    initStatusChart();
    initTopPartsChart();
}

/**
 * Line Chart: Revenue Trend
 */
function initRevenueChart() {
    const ctx = document.getElementById('revenueChart');
    if (!ctx || typeof revenueData === 'undefined') return;

    if (charts.revenue) {
        charts.revenue.destroy();
    }

    charts.revenue = new Chart(ctx, {
        type: 'line',
        data: {
            labels: revenueData.map(r => r.Name ?? r.name ?? ''),
            datasets: [{
                label: 'Doanh Thu (₫)',
                data: revenueData.map(r => r.Value ?? r.value ?? 0),
                borderColor: '#2563eb',
                backgroundColor: 'rgba(37, 99, 235, 0.1)',
                borderWidth: 3,
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#2563eb',
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function(context) {
                            return ' ' + context.parsed.y.toLocaleString('vi-VN') + ' ₫';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { borderDash: [5, 5], color: '#e2e8f0' },
                    ticks: {
                        callback: val => val.toLocaleString('vi-VN') + ' ₫',
                        font: { size: 11 }
                    }
                },
                x: {
                    grid: { display: false },
                    ticks: { font: { size: 11 } }
                }
            }
        }
    });
}

/**
 * Doughnut Chart: Revenue Status Breakdown
 */
function initStatusChart() {
    const ctx = document.getElementById('statusChart');
    if (!ctx || typeof statusData === 'undefined') return;

    if (charts.status) {
        charts.status.destroy();
    }

    charts.status = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: statusData.map(s => s.Name ?? s.name ?? s.Status ?? s.status ?? ''),
            datasets: [{
                data: statusData.map(s => s.Value ?? s.value ?? s.Count ?? s.count ?? 0),
                backgroundColor: ['#2563eb', '#10b981', '#f59e0b', '#64748b'],
                hoverOffset: 4,
                borderWidth: 0
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { boxWidth: 12, padding: 20, font: { size: 12 } }
                }
            },
            cutout: '70%'
        }
    });
}

/**
 * Bar Chart: Top Parts Sold
 */
function initTopPartsChart() {
    const ctx = document.getElementById('topPartsChart');
    if (!ctx || typeof topParts === 'undefined') return;

    if (charts.topParts) {
        charts.topParts.destroy();
    }

    charts.topParts = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: topParts.map(p => p.Name ?? p.name ?? p.PartName ?? p.partName ?? ''),
            datasets: [{
                label: 'Số lượng',
                data: topParts.map(p => p.Value ?? p.value ?? p.Quantity ?? p.quantity ?? 0),
                backgroundColor: '#334155',
                borderRadius: 6,
                barThickness: 20
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: { grid: { display: false } },
                y: { grid: { display: false } }
            }
        }
    });
}

/**
 * Handle Period Changes (Placeholder logic)
 */
function changeChartPeriod(period) {
    console.log('Changing chart period to:', period);
    
    // UI Update
    const buttons = document.querySelectorAll('[onclick^="changeChartPeriod"]');
    buttons.forEach(btn => {
        if (btn.getAttribute('onclick').includes(period)) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    });

    // In a real app, you would fetch new data here.
    // For now, we'll just simulate a refresh.
    toastr.info('Đang cập nhật dữ liệu cho ' + (period === '7days' ? '7 ngày gần nhất' : '30 ngày gần nhất') + '...');
    
    setTimeout(() => {
        initCharts(); // Re-initialize with current data
    }, 500);
}

function exportRevenue() {
    // Export revenue CSV for last 30 days
    const from = new Date(); from.setDate(from.getDate() - 30);
    const fromDate = from.toISOString().slice(0, 10);
    const toDate = new Date().toISOString().slice(0, 10);
    window.location.href = `/StoreAdmin/Export/ExportRevenueCsv?fromDate=${encodeURIComponent(fromDate)}&toDate=${encodeURIComponent(toDate)}`;
}
