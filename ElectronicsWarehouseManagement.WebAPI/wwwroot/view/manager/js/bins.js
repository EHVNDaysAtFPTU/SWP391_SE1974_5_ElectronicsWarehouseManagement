/**
 * Bin Inventory Javascript
 */

let currentPage = 1;
let pageSize = 5;
let totalPages = 1;
let search = "";
let sortBy = "";
let sortDirection = "asc";
let createBinModal;


document.addEventListener("DOMContentLoaded", function () {
    loadBins(1);
    const modalEl = document.getElementById("createBinModal");
    if (modalEl) {
        createBinModal = new bootstrap.Modal(modalEl);
    }


    const pageSizeSelect = document.getElementById("pageSize");
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function () {
            pageSize = parseInt(this.value);
            loadBins(1);
        });
    }
});

async function loadBins(page) {
    currentPage = page;

    const tableBody = document.getElementById("binTable");
    const infoText = document.getElementById("infoText");

    // Loading
    tableBody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center py-4">
                <div class="spinner-border text-primary"></div>
            </td>
        </tr>
    `;

    let url = `/api/manager/get-bins?pageNumber=${page}&pageSize=${pageSize}`;
    if (search) {
        url += `&search=${encodeURIComponent(search)}`;
    }
    if (sortBy) {
        url += `&sortBy=${encodeURIComponent(sortBy)}`;
        url += `&sortDirection=${sortDirection}`;
    }

    try {
        const result = await apiFetch(url);

        if (!result.success) {
            alert(result.msg || "Load bins failed");
            return;
        }

        renderBins(result.data);
        renderBinPagination(result.data.totalPages);

    } catch (error) {
        alert("Load failed: " + error.message);
    }
}

function renderBins(data) {
    const tbody = document.getElementById("binTable");
    const infoText = document.getElementById("infoText");

    tbody.innerHTML = "";

    if (!data.data || data.data.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-5 text-muted">
                    No bins found
                </td>
            </tr>
        `;
        if (infoText) infoText.innerText = "No results";
        return;
    }

    // Info text giống component
    if (infoText) {
        infoText.innerText = `Showing ${(currentPage - 1) * pageSize + 1} 
            to ${Math.min(currentPage * pageSize, data.totalCount)} 
            of ${data.totalCount}`;
    }

    data.data.forEach(bin => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td class="align-middle fw-semibold">#${bin.id}</td>
            <td class="align-middle">${bin.location_in_warehouse || "N/A"}</td>
            <td class="align-middle">${getStatusBadge(bin.status)}</td>
            <td class="align-middle">${bin.warehouse_id}</td>
            <td class="align-middle text-end">
                <a href="bin-detail.html?binId=${bin.id}" 
                   class="btn btn-sm btn-outline-primary rounded-pill px-3">
                   View
                </a>
            </td>
        `;

        tbody.appendChild(row);
    });
}

function renderBinPagination(pages) {
    totalPages = pages;

    const container = document.getElementById("pagination");
    if (!container) return;

    container.innerHTML = "";

    if (totalPages <= 1) return;

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center mb-0";

    // Previous
    const prevLi = document.createElement("li");
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `
        <a class="page-link" href="#" onclick="loadBins(${currentPage - 1}); return false;">
            Previous
        </a>`;
    ul.appendChild(prevLi);

    // Pages
    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement("li");
        li.className = `page-item ${i === currentPage ? 'active' : ''}`;
        li.innerHTML = `
            <a class="page-link" href="#" onclick="loadBins(${i}); return false;">
                ${i}
            </a>`;
        ul.appendChild(li);
    }

    // Next
    const nextLi = document.createElement("li");
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `
        <a class="page-link" href="#" onclick="loadBins(${currentPage + 1}); return false;">
            Next
        </a>`;
    ul.appendChild(nextLi);

    container.appendChild(ul);
}

function getStatusBadge(status) {
    switch (status) {
        case 0:
            return '<span class="badge bg-secondary">Empty</span>';
        case 1:
            return '<span class="badge bg-success">In Use</span>';
        case 2:
            return '<span class="badge bg-danger">Disabled</span>';
        default:
            return '<span class="badge bg-light text-dark">Unknown</span>';
    }
}
async function openCreateBinModal() {
    if (!createBinModal) {
        alert("Modal not ready");
        return;
    }

    document.getElementById("binLocation").value = "";

    await loadWarehouseOptions();

    createBinModal.show();
}
async function loadWarehouseOptions() {
    const select = document.getElementById("warehouseSelect");
    select.innerHTML = `<option>Loading...</option>`;

    try {
        const result = await apiFetch("/api/manager/get-warehouses?pageNumber=1&pageSize=1000");

        if (!result.success) {
            select.innerHTML = `<option>Error loading</option>`;
            return;
        }

        const warehouses = result.data.data;

        if (!warehouses || warehouses.length === 0) {
            select.innerHTML = `<option>No warehouses found</option>`;
            return;
        }

        select.innerHTML = `<option value="">-- Select Warehouse --</option>`;

        warehouses.forEach(w => {
            const option = document.createElement("option");
            option.value = w.id;
            option.textContent = `${w.name} (${w.physical_location || "N/A"})`;
            select.appendChild(option);
        });

    } catch (err) {
        select.innerHTML = `<option>Error</option>`;
    }
}
async function createBin() {
    const location = document.getElementById("binLocation").value.trim();
    const warehouseId = document.getElementById("warehouseSelect").value;

    if (!location) {
        alert("Location is required");
        return;
    }

    if (!warehouseId || warehouseId === "") {
        alert("Please select a warehouse");
        return;
    }

    const request = {
        warehouse_id: parseInt(warehouseId),            
        location_in_warehouse: location                
    };

    try {
        const result = await apiFetch("/api/manager/bins/create", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!result.success) {
            alert(result.msg || "Create bin failed");
            return;
        }

        alert("Create bin successfully!");
        createBinModal.hide();
        loadBins(currentPage);

    } catch (error) {
        alert("Error: " + error.message);
    }
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