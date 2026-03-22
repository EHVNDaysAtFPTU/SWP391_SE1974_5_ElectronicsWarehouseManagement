/**
 * Bin List JS
 */

let warehouseId = null;

document.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    warehouseId = params.get("warehouseId");

    if (!warehouseId) {
        showMessage("Warehouse ID not found in URL", "danger");
        return;
    }

    loadBins();
});

function goBack() {
    window.location.href = "warehouses.html";
}

async function loadBins() {
    const container = document.getElementById("binList");
    // Skeleton-like loading
    container.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="mt-2 text-muted">Fetching bins...</p>
        </div>
    `;

    try {
        const result = await apiFetch(`/api/manager/get-warehouse-bins/${warehouseId}?pageNumber=1&pageSize=100&fullInfo=true`);

        if (!result.success) {
            showMessage(result.msg || "Load bins failed", "danger");
            return;
        }

        renderBins(result.data.data);
    } catch (error) {
        showMessage("Error loading bins: " + error.message, "danger");
    }
}

function renderBins(bins) {
    const container = document.getElementById("binList");
    container.innerHTML = "";

    if (!bins || bins.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5">
                <div class="mb-3 text-muted" style="font-size: 3rem;">📂</div>
                <h5 class="text-secondary">No bins found in this warehouse</h5>
                <p class="small text-muted">Warehouse ID: ${warehouseId}</p>
                <button class="btn btn-sm btn-outline-primary rounded-pill mt-2" onclick="goBack()">Return to Warehouses</button>
            </div>
        `;
        return;
    }

    const table = document.createElement("table");
    table.className = "table table-hover mb-0";
    table.innerHTML = `
        <thead>
            <tr>
                <th class="ps-4">Bin ID</th>
                <th>Location String</th>
                <th>Usage Status</th>
                <th>Current Items</th>
                <th class="text-end pe-4">Action</th>
            </tr>
        </thead>
        <tbody></tbody>
    `;
    
    const tbody = table.querySelector("tbody");
    
    bins.forEach(b => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td class="ps-4 fw-bold align-middle">${b.id}</td>
            <td class="align-middle fw-medium">${b.location_in_warehouse || "-"}</td>
            <td class="align-middle">${getBinStatusBadge(b.status)}</td>
            <td class="align-middle">
                <span class="badge bg-light text-dark border rounded-pill px-3">
                    ${b.components ? b.components.length : 0} items
                </span>
            </td>
            <td class="text-end pe-4 align-middle">
                <button class="btn btn-sm btn-outline-dark rounded-pill px-3" onclick="viewBinDetail(${b.id})">
                    Explore Bin
                </button>
            </td>
        `;
        tbody.appendChild(row);
    });

    container.appendChild(table);
}

function getBinStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-secondary rounded-pill px-3">Empty</span>';
        case 1: return '<span class="badge bg-success rounded-pill px-3">In Use</span>';
        case 2: return '<span class="badge bg-danger rounded-pill px-3">Disabled</span>';
        default: return `<span class="badge bg-light text-dark rounded-pill px-3">Status ${status}</span>`;
    }
}

function viewBinDetail(id) {
    window.location.href = `bin-detail.html?binId=${id}`;
}

function showMessage(msg, type = "info") {
    const container = document.getElementById("binList");
    container.innerHTML = `
        <div class="alert alert-${type} m-4 shadow-sm" role="alert">
            <h5 class="alert-heading">Notice</h5>
            <p class="mb-0">${msg}</p>
        </div>
    `;
}
