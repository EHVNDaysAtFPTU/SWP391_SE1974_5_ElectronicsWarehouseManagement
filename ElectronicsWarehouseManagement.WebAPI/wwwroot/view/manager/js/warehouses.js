/**
 * Warehouse List JS (FULL VERSION - Upload Image)
 */

let currentPage = 1;
let pageSize = 10;
let totalPages = 1;
let search = "";
let sortBy = "";
let sortDirection = "asc";
let createModal;

document.addEventListener("DOMContentLoaded", function () {
    // Init modal
    const modalEl = document.getElementById("createWarehouseModal");
    if (modalEl) {
        createModal = new bootstrap.Modal(modalEl);
    }

    // Page size change
    const pageSizeSelect = document.getElementById("pageSize");
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function () {
            pageSize = parseInt(this.value);
            loadWarehouses(1);
        });
    }

    // Image preview
    const fileInput = document.getElementById("whImage");
    if (fileInput) {
        fileInput.addEventListener("change", function (e) {
            const file = e.target.files[0];
            const preview = document.getElementById("previewImg");

            if (!file || !preview) return;

            preview.src = URL.createObjectURL(file);
            preview.style.display = "block";
        });
    }

    loadWarehouses(1);
});


// ================== LOAD DATA ==================
async function loadWarehouses(page = 1) {
    currentPage = page;
    const tbody = document.getElementById("warehouseTable");

    tbody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center py-4">
                <div class="spinner-border text-primary"></div>
            </td>
        </tr>
    `;

    try {
        let url = `/api/manager/get-warehouses?pageNumber=${page}&pageSize=${pageSize}`;

        if (search) url += `&search=${encodeURIComponent(search)}`;
        if (sortBy) {
            url += `&sortBy=${encodeURIComponent(sortBy)}`;
            url += `&sortDirection=${sortDirection}`;
        }

        const result = await apiFetch(url);

        if (!result.success) {
            alert(result.msg || "Load warehouse failed");
            return;
        }

        renderWarehouseTable(result.data.data);
        renderPagination(result.data.totalPages);

    } catch (error) {
        alert("Load failed: " + error.message);
    }
}


// ================== RENDER TABLE ==================
function renderWarehouseTable(warehouses) {
    const tbody = document.getElementById("warehouseTable");
    tbody.innerHTML = "";

    if (!warehouses || warehouses.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-5 text-muted">
                    No warehouses found
                </td>
            </tr>`;
        return;
    }

    warehouses.forEach(w => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td class="fw-bold">${w.id}</td>
            <td class="fw-semibold">${w.name}</td>
            <td class="text-muted small">${w.desc || "No description"}</td>
            <td>
                <span class="badge bg-light text-dark border rounded-pill px-3">
                    ${w.physical_location || "N/A"}
                </span>
            </td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-2"
                    onclick="toggleWarehouseDetail(${JSON.stringify(w).replace(/"/g, '&quot;')}, this)">
                    View Detail
                </button>

                <button class="btn btn-sm btn-primary"
                    onclick="window.location.href='warehouse-bins.html?warehouseId=${w.id}'">
                    View Bins
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });
}


// ================== DETAIL ==================
function toggleWarehouseDetail(w, button) {
    const mainRow = button.closest("tr");
    const nextRow = mainRow.nextElementSibling;

    if (nextRow && nextRow.classList.contains("detail-row")) {
        nextRow.remove();
        button.innerText = "View Detail";
        return;
    }

    document.querySelectorAll(".detail-row").forEach(r => r.remove());

    button.innerText = "Close Detail";

    const detailRow = document.createElement("tr");
    detailRow.classList.add("detail-row");

    const imageHtml = w.image_url
        ? `<img src="${w.image_url}" class="img-fluid rounded" style="max-height:200px;">`
        : `<div class="text-muted">No image</div>`;

    detailRow.innerHTML = `
        <td colspan="5" class="bg-light">
            <div class="p-3">
                ${imageHtml}
                <h5>${w.name}</h5>
                <p>${w.desc || "No description"}</p>
                <small>${w.physical_location || "No location"}</small>
            </div>
        </td>
    `;

    mainRow.parentNode.insertBefore(detailRow, mainRow.nextSibling);
}


// ================== PAGINATION ==================
function renderPagination(pages) {
    totalPages = pages;
    const container = document.getElementById("pagination");
    if (!container || totalPages <= 1) return;

    container.innerHTML = "";

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center";

    const prev = `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadWarehouses(${currentPage - 1});return false;">Prev</a>
        </li>`;

    ul.innerHTML += prev;

    for (let i = 1; i <= totalPages; i++) {
        ul.innerHTML += `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadWarehouses(${i});return false;">${i}</a>
            </li>`;
    }

    const next = `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="loadWarehouses(${currentPage + 1});return false;">Next</a>
        </li>`;

    ul.innerHTML += next;
    container.appendChild(ul);
}

function openCreateWarehouseModal() {
    if (!createModal) {
        alert("Modal not ready");
        return;
    }

    document.getElementById("whName").value = "";
    document.getElementById("whLocation").value = "";
    document.getElementById("whDesc").value = "";
    document.getElementById("whImage").value = "";

    const preview = document.getElementById("previewImg");
    if (preview) {
        preview.src = "";
        preview.style.display = "none";
    }

    createModal.show();
}


async function createWarehouse() {
    const name = document.getElementById("whName").value.trim();
    const location = document.getElementById("whLocation").value.trim();
    const desc = document.getElementById("whDesc").value.trim();
    const fileInput = document.getElementById("whImage");

    if (!name) {
        alert("Warehouse name is required");
        return;
    }

    let imageUrl = "";

    try {

        if (fileInput.files.length > 0) {
            const formData = new FormData();
            formData.append("image", fileInput.files[0]);

            const uploadRes = await fetch("/api/manager/upload-image", {
                method: "POST",
                body: formData
            });

            const uploadResult = await uploadRes.json();

            if (!uploadResult.success) {
                alert(uploadResult.msg || "Upload image failed");
                return;
            }

            imageUrl = uploadResult.data; 
        }

        const request = {
            name: name,
            physical_location: location,
            desc: desc,
            image_url: imageUrl
        };

        const result = await apiFetch("/api/manager/warehouses/create", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!result.success) {
            alert(result.msg || "Create failed");
            return;
        }

        alert("Create warehouse successfully!");
        createModal.hide();
        loadWarehouses(currentPage);

    } catch (error) {
        alert("Error: " + error.message);
    }
}


// ================== FILTER ==================
function applyFilter() {
    search = document.getElementById("searchInput").value.trim() || null;
    sortBy = document.getElementById("sortBy").value || null;
    sortDirection = document.getElementById("sortDirection").value || "asc";

    loadWarehouses(1);
}