/**
 * Component Inventory Javascript
 */

let search = "";
let sortBy = "";
let sortDirection = "asc";
let currentPage = 1;
let pageSize = 10;
let totalPages = 1;

document.addEventListener("DOMContentLoaded", function () {
    // Initial load
    loadItems(1);
    
    // Page size listener
    const pageSizeSelect = document.getElementById("pageSize");
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function() {
            pageSize = parseInt(this.value);
            loadItems(1);
        });
    }
});

async function loadItems(page) {
    currentPage = page;
    const tableBody = document.getElementById("itemsTable");
    const infoText = document.getElementById("infoText");
    
    // Show loading state
    tableBody.innerHTML = '<tr><td colspan="7" class="text-center py-4"><div class="spinner-border text-primary" role="status"></div></td></tr>';

    let url = `/api/manager/get-components?pageNumber=${page}&pageSize=${pageSize}`;

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
            alert(result.msg || "Load failed");
            return;
        }

        renderTable(result.data);
        renderPagination(result.data.totalPages);
    } catch (error) {
        alert("Load failed: " + error.message);
    }
}

function renderTable(data) {
    const tbody = document.getElementById("itemsTable");
    const infoText = document.getElementById("infoText");

    tbody.innerHTML = "";

    if (!data.data || data.data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-5 text-muted">No items found</td></tr>`;
        if (infoText) infoText.innerText = "No results";
        return;
    }

    if (infoText) {
        infoText.innerText = `Showing ${(currentPage - 1) * pageSize + 1} to ${Math.min(currentPage * pageSize, data.totalCount)} of ${data.totalCount}`;
    }

    data.data.forEach(component => {
        const row = document.createElement("tr");
        row.setAttribute("data-id", component.id);

        row.innerHTML = `
            <td class="align-middle">${component.id}</td>
            <td class="align-middle">
                ${component.metadata?.image_url 
                    ? `<img src="${component.metadata.image_url}" class="thumbnail-img shadow-sm">` 
                    : '<div class="thumbnail-img bg-light d-flex align-items-center justify-content-center text-muted"><small>No image</small></div>'}
            </td>
            <td class="align-middle fw-semibold">${component.metadata?.name || "N/A"}</td>
            <td class="align-middle text-truncate" style="max-width: 200px;">${component.metadata?.desc || ""}</td>
            <td class="align-middle">${component.unit || ""}</td>
            <td class="align-middle fw-bold text-primary">${formatCurrency(component.unit_price || 0)}</td>


            <td class="align-middle">
                <button class="btn btn-sm btn-outline-primary rounded-pill py-1 px-3" onclick="toggleDetail(${component.id}, this)">
                    View
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

function renderPagination(pages) {
    totalPages = pages;
    const container = document.getElementById("pagination");
    if (!container) return;
    
    container.innerHTML = "";
    
    if (totalPages <= 1) return;

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center mb-0";

    // Previous Button
    const prevLi = document.createElement("li");
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="loadItems(${currentPage - 1}); return false;">Previous</a>`;
    ul.appendChild(prevLi);

    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement("li");
        li.className = `page-item ${i === currentPage ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="loadItems(${i}); return false;">${i}</a>`;
        ul.appendChild(li);
    }

    // Next Button
    const nextLi = document.createElement("li");
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="loadItems(${currentPage + 1}); return false;">Next</a>`;
    ul.appendChild(nextLi);

    container.appendChild(ul);
}

async function toggleDetail(id, button) {
    const mainRow = button.closest("tr");
    const nextRow = mainRow.nextElementSibling;

    if (nextRow && nextRow.classList.contains("detail-row")) {
        nextRow.remove();
        button.innerText = "View";
        button.classList.replace("btn-primary", "btn-outline-primary");
        return;
    }

    // Remove other detail rows
    document.querySelectorAll(".detail-row").forEach(r => {
        const prevBtn = r.previousElementSibling.querySelector("button");
        if (prevBtn) {
            prevBtn.innerText = "View";
            prevBtn.classList.replace("btn-primary", "btn-outline-primary");
        }
        r.remove();
    });

    button.innerText = "Closing";
    button.classList.replace("btn-outline-primary", "btn-primary");

    const detailRow = document.createElement("tr");
    detailRow.classList.add("detail-row");

    const detailCell = document.createElement("td");
    detailCell.colSpan = 7;
    detailCell.className = "bg-light p-0";
    detailCell.innerHTML = '<div class="p-4 text-center"><div class="spinner-border spinner-border-sm text-secondary"></div></div>';

    detailRow.appendChild(detailCell);
    mainRow.parentNode.insertBefore(detailRow, mainRow.nextSibling);

    try {
        const result = await apiFetch(`/api/manager/get-component/${id}?fullInfo=true`);

        if (!result.success) {
            detailCell.innerHTML = '<div class="p-3 text-danger">Load detail failed</div>';
            return;
        }

        const component = result.data;
        button.innerText = "Close";

        detailCell.innerHTML = `
            <div class="p-4 animate__animated animate__fadeIn">
                <div class="row">
                    <div class="col-md-3 text-center">
                        ${component.metadata?.image_url 
                            ? `<img src="${component.metadata.image_url}" class="img-fluid rounded-3 shadow-sm border mb-3" style="max-height: 200px;">`
                            : '<div class="bg-white border rounded d-flex align-items-center justify-content-center mb-3" style="height: 150px;"><i class="text-muted">No image</i></div>'}
                    </div>
                    <div class="col-md-9">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <h5 class="mb-0 fw-bold">${component.metadata?.name || "Product Name"}</h5>
                             <span class="badge bg-primary rounded-pill px-3">ID: ${component.id}</span>
                        </div>
                        <p class="text-muted mb-3">${component.metadata?.desc || "No description provided."}</p>
                        
                        <div class="row g-3">
                            <div class="col-6 col-sm-4">
                                <label class="small text-uppercase text-muted d-block">Price</label>
                                <span class="fw-bold">${formatCurrency(component.unit_price || 0)}</span>
                            </div>
                            <div class="col-6 col-sm-4">
                                <label class="small text-uppercase text-muted d-block">Unit</label>
                                <span>${component.unit || "N/A"}</span>
                            </div>
                            <div class="col-6 col-sm-4">
                                <label class="small text-uppercase text-muted d-block">Manufacturer</label>
                                <span>${component.metadata?.manufacturer || "N/A"}</span>
                            </div>
                            <div class="col-6 col-sm-4">
                                <label class="small text-uppercase text-muted d-block">Mfg Date</label>
                                <span>${component.metadata?.manufacturing_date || "N/A"}</span>
                            </div>
                            <div class="col-6 col-sm-4">
                                <label class="small text-uppercase text-muted d-block">Quantity</label>
                                <span>${component.quantity || "N/A"}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    } catch (error) {
        detailCell.innerHTML = '<div class="p-3 text-danger">Load detail failed: ' + error.message + '</div>';
    }
}

function applyFilter() {
    const searchValue = document.getElementById("searchInput").value.trim();
    const sortByValue = document.getElementById("sortBy").value;
    const sortDirectionValue = document.getElementById("sortDirection").value;

    search = searchValue.length > 0 ? searchValue : null;
    sortBy = sortByValue || null;
    sortDirection = sortDirectionValue || "asc";

    loadItems(1);
}
