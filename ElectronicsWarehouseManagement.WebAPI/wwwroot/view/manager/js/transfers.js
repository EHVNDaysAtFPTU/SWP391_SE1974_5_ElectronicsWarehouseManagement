/**
 * Transfer List JS
 */

const STATUS = {
    PENDING: 1,
    APPROVED: 2,
    REJECTED: 3,
    CONFIRMED: 4
};

let search = "";
let sortBy = "";
let sortDirection = "asc";
let currentPage = 1;
let pageSize = 10;
let totalPages = 1;

document.addEventListener("DOMContentLoaded", function () {
    const pageSizeSelect = document.getElementById("pageSize");
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function() {
            pageSize = parseInt(this.value);
            loadTransfers(1);
        });
    }
    loadTransfers(1);
});

async function loadTransfers(page) {
    currentPage = page;
    const tbody = document.querySelector("#transferTable tbody");
    const infoText = document.getElementById("infoText");
    
    // Loading state
    tbody.innerHTML = '<tr><td colspan="7" class="text-center py-4"><div class="spinner-border text-primary" role="status"></div></td></tr>';

    let url = `/api/manager/get-transfers?pageNumber=${page}&pageSize=${pageSize}`;

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
            showMessage(result.msg || "Load failed", "danger");
            return;
        }

        const data = result.data;
        renderInfo(data.totalCount);
        renderTable(data.data);
        renderPagination(data.totalPages);
    } catch (error) {
        showMessage("Load failed: " + error.message, "danger");
    }
}

function renderTable(data) {
    const tbody = document.querySelector("#transferTable tbody");
    tbody.innerHTML = "";

    if (!data || data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="9" class="text-center py-5 text-muted">No transfers found</td></tr>`;
        return;
    }

    data.forEach(t => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td class="align-middle fw-bold">${t.id ?? "-"}</td>
            <td class="align-middle">${t.description ?? "-"}</td>
            <td class="align-middle">${getTypeBadge(t.type)}</td>
            <td class="align-middle small text-muted">${formatDate(t.creation_date)}</td>
            <td class="align-middle small text-muted">${t.execution_date ? formatDate(t.execution_date) : "-"}</td>
            <td class="align-middle">${getStatusBadge(t.status)}</td>
            <td class="align-middle">${getMonitorBadge(t)}</td>
            <td class="align-middle">
                <div class="d-flex gap-1">
                    <button class="btn btn-sm btn-outline-primary rounded-pill px-3" onclick="viewTransfer(${t.id})">Details</button>
                </div>
            </td>
            <td class="align-middle">
                ${renderDecisionButtons(t)}
            </td>
        `;

        tbody.appendChild(row);
    });
}

function renderDecisionButtons(t) {
    if (t.status !== STATUS.PENDING) return '<span class="text-muted small">N/A</span>';

    return `
        <div class="btn-group btn-group-sm">
            <button class="btn btn-success" onclick="updateTransferStatus(${t.id}, ${STATUS.APPROVED})">Approve</button>
            <button class="btn btn-danger" onclick="updateTransferStatus(${t.id}, ${STATUS.REJECTED})">Reject</button>
        </div>
    `;
}

async function updateTransferStatus(transferId, decision) {
    try {
        const result = await apiFetch(`/api/manager/transfer-requests/${transferId}/decisions`, {
            method: "POST",
            body: JSON.stringify({ decision: decision })
        });

        if (result.success) {
            showMessage("Transfer status updated successfully", "success");
            loadTransfers(currentPage);
        } else {
            showMessage(result.msg || "Update failed", "danger");
        }
    } catch (error) {
        showMessage("Update error: " + error.message, "danger");
    }
}

function getStatusBadge(status) {
    switch (status) {
        case STATUS.PENDING: return '<span class="badge bg-warning text-dark rounded-pill">Pending</span>';
        case STATUS.APPROVED: return '<span class="badge bg-primary rounded-pill">Approved</span>';
        case STATUS.REJECTED: return '<span class="badge bg-danger rounded-pill">Rejected</span>';
        case STATUS.CONFIRMED: return '<span class="badge bg-success rounded-pill">Confirmed</span>';
        default: return '<span class="badge bg-secondary rounded-pill">Unknown</span>';
    }
}

function getMonitorBadge(t) {
    if (t.status === STATUS.PENDING) return '<span class="badge text-muted border rounded-pill">Waiting</span>';
    if (t.status === STATUS.CONFIRMED && t.execution_date) return '<span class="badge bg-success bg-opacity-10 text-success border border-success border-opacity-25 rounded-pill">Done</span>';
    if (t.status === STATUS.APPROVED) return '<span class="badge bg-info bg-opacity-10 text-info border border-info border-opacity-25 rounded-pill">Processing</span>';
    if (t.status === STATUS.REJECTED) return '<span class="badge bg-light text-muted border rounded-pill">Canceled</span>';
    return '<span class="badge bg-light text-muted rounded-pill">Unknown</span>';
}

function getTypeBadge(type) {
    switch (type) {
        case 1: return '<span class="badge bg-info text-dark bg-opacity-25 rounded-pill px-2">Inbound</span>';
        case 2: return '<span class="badge bg-warning text-dark bg-opacity-25 rounded-pill px-2">Outbound</span>';
        case 3: return '<span class="badge bg-secondary text-dark bg-opacity-25 rounded-pill px-2">Internal</span>';
        default: return '<span class="badge bg-light text-dark rounded-pill">Unknown</span>';
    }
}

function renderInfo(totalCount) {
    const el = document.getElementById("infoText");
    if (!el) return;
    el.innerHTML = `Showing <span class="text-dark fw-bold">${(currentPage - 1) * pageSize + 1}</span> to <span class="text-dark fw-bold">${Math.min(currentPage * pageSize, totalCount)}</span> of <span class="text-dark fw-bold">${totalCount}</span> transfers`;
}

function renderPagination(pages) {
    totalPages = pages;
    const container = document.getElementById("pagination");
    if (!container) return;
    
    container.innerHTML = "";
    if (totalPages <= 1) return;

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center mb-0";

    // Prev
    const prevLi = document.createElement("li");
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="loadTransfers(${currentPage - 1}); return false;">Prev</a>`;
    ul.appendChild(prevLi);

    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement("li");
        li.className = `page-item ${i === currentPage ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="loadTransfers(${i}); return false;">${i}</a>`;
        ul.appendChild(li);
    }

    // Next
    const nextLi = document.createElement("li");
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="loadTransfers(${currentPage + 1}); return false;">Next</a>`;
    ul.appendChild(nextLi);

    container.appendChild(ul);
}

function showMessage(msg, type = "info") {
    const container = document.getElementById("messageContainer");
    if (!container) return;
    
    container.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show animate__animated animate__fadeInUp" role="alert">
            ${msg}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Auto hide after 3 seconds
    setTimeout(() => {
        const alert = container.querySelector(".alert");
        if (alert) {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }
    }, 5000);
}

function viewTransfer(id) {
    window.location.href = `transfer-detail.html?id=${id}`;
}


function applyFilter() {
    const searchValue = document.getElementById("searchInput").value.trim();
    const sortByValue = document.getElementById("sortBy").value;
    const sortDirectionValue = document.getElementById("sortDirection").value;

    search = searchValue.length > 0 ? searchValue : null;
    sortBy = sortByValue || null;
    sortDirection = sortDirectionValue || "asc";

    loadTransfers(1);
}
