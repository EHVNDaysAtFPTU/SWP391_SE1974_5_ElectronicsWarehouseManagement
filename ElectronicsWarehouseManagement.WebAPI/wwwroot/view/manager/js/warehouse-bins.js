/**
 * Bin List JS
 */

let warehouseId = null;
let currentPage = 1;
let pageSize = 10;
let totalPages = 1;
let search = "";
let sortBy = "";
let sortDirection = "asc";

document.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    warehouseId = params.get("warehouseId");

    if (!warehouseId) {
        showMessage("Warehouse ID not found in URL", "danger");
        return;
    }

    const pageSizeSelect = document.getElementById("pageSize");
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function () {
            pageSize = parseInt(this.value);
            loadBins(1);
        });
    }

    loadBins(1);
});

function goBack() {
    window.location.href = "warehouses.html";
}

async function loadBins(page = 1) {
    currentPage = page;
    const container = document.getElementById("binList");
    // Skeleton-like loading
    container.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="mt-2 text-muted">Fetching bins...</p>
        </div>
    `;

    try {
        let url = `/api/manager/get-warehouse-bins/${warehouseId}?pageNumber=${page}&pageSize=${pageSize}&fullInfo=true`;
        if (search) {
            url += `&search=${encodeURIComponent(search)}`;
        }
        if (sortBy) {
            url += `&sortBy=${encodeURIComponent(sortBy)}`;
            url += `&sortDirection=${sortDirection}`;
        }

        const result = await apiFetch(url);

        if (!result.success) {
            showMessage(result.msg || "Load bins failed", "danger");
            return;
        }

        renderBins(result.data.data);
        renderPagination(result.data.totalPages);
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
        case 1:
            return '<span class="badge bg-secondary">Empty</span>';
        case 2:
            return '<span class="badge bg-success">Available</span>';
        case 3:
            return '<span class="badge bg-danger">Locked</span>';
        default:
            return '<span class="badge bg-light text-dark">Unknown</span>';
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

function renderPagination(pages) {
    totalPages = pages;
    const container = document.getElementById("pagination");
    if (!container) return;
    
    container.innerHTML = "";
    if (totalPages <= 1) return;

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center mb-0";

    const prevLi = document.createElement("li");
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="loadBins(${currentPage - 1}); return false;">Prev</a>`;
    ul.appendChild(prevLi);

    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement("li");
        li.className = `page-item ${i === currentPage ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="loadBins(${i}); return false;">${i}</a>`;
        ul.appendChild(li);
    }

    const nextLi = document.createElement("li");
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="loadBins(${currentPage + 1}); return false;">Next</a>`;
    ul.appendChild(nextLi);

    container.appendChild(ul);
}

function applyFilter() {
    const searchValue = document.getElementById("searchInput").value.trim();
    const sortByValue = document.getElementById("sortBy").value;
    const sortDirectionValue = document.getElementById("sortDirection").value;

    search = searchValue.length > 0 ? searchValue : null;
    sortBy = sortByValue || null;
    sortDirection = sortDirectionValue || "asc";

    loadBins(1);
}
