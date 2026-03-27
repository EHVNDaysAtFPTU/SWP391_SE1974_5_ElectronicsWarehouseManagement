/**
 * Dashboard / Home JS
 */

document.addEventListener("DOMContentLoaded", function () {
    loadDashboard();
    loadTransferChart();
});

async function getDashboardSummary() {
    try {
        const json = await apiFetch("/api/manager/get-statistics/summary");
        if (!json.success) throw new Error(json.msg);
        return json.data;
    } catch (error) {
        console.error("Error loading dashboard summary:", error);
        return null;
    }
}

async function loadDashboard() {
    const data = await getDashboardSummary();
    if (!data) return;

    // Update UI elements with data
    const mappings = {
        "totalComponents": data.total_components,
        "totalWarehouses": data.total_warehouses,
        "currentStock": data.current_stock,
        "lowStock": data.low_stock_items,
        "outOfStock": data.out_of_stock_items,
        "inboundToday": data.inbound_today,
        "outboundToday": data.outbound_today
    };

    for (const [id, value] of Object.entries(mappings)) {
        const el = document.getElementById(id);
        if (el) el.innerText = value.toLocaleString();
    }
}

async function getTransferChartData() {
    try {
        const json = await apiFetch("/api/manager/get-statistics/charts");
        if (!json.success) throw new Error(json.msg);
        return json.data.transfer_chart;
    } catch (error) {
        console.error("Error loading chart data:", error);
        return [];
    }
}

async function loadTransferChart() {
    const chartData = await getTransferChartData();
    if (!chartData || chartData.length === 0) return;

    const labels = chartData.map(x => formatDate(x.date).split(',')[0]);
    const imports = chartData.map(x => x.import);
    const exports = chartData.map(x => x.export);

    const ctx = document.getElementById("transferChart");
    if (!ctx) return;

    new Chart(ctx, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Inbound",
                    data: imports,
                    backgroundColor: "rgba(72, 52, 212, 0.7)",
                    borderRadius: 6
                },
                {
                    label: "Outbound",
                    data: exports,
                    backgroundColor: "rgba(235, 77, 75, 0.7)",
                    borderRadius: 6
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "top",
                    labels: {
                        usePointStyle: true,
                        padding: 20
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        drawBorder: false,
                        color: "#f0f2f5"
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}
function openProfile() {
    window.location.href = "/me/index.html";
}
