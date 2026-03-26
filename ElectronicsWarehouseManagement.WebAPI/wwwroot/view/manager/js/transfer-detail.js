/**
 * Transfer Detail JS (Refactored for HTML templates)
 */

const STATUS = {
    UNKNOWN: 0,
    PENDING: 1,
    APPROVED: 2,
    REJECTED: 3,
    FINISHED: 4,
    MISSING_COMPONENTS: 5,
    CANCELED: 6
};

let currentTransferId = null;

document.addEventListener("DOMContentLoaded", function () {
    const id = new URLSearchParams(window.location.search).get("id");
    if (!id) {
        showError("Missing transfer ID in URL");
        return;
    }
    currentTransferId = id;
    loadTransferDetail(id);
});

async function loadTransferDetail(id) {
    try {
        const result = await apiFetch(`/api/manager/get-transfer/${id}?fullInfo=true`);
        if (!result.success) {
            showError(result.msg || "Load failed");
            return;
        }

        const t = result.data;
        displayTransferInfo(t);
        await displayComponents(t);
        
        // Finalize visibility
        document.getElementById("loader").classList.add("d-none");
        document.getElementById("transferInfo").classList.remove("d-none");
        document.getElementById("componentsCard").classList.remove("d-none");

    } catch (error) {
        showError("Connection error: " + error.message);
    }
}

function displayTransferInfo(t) {
    document.getElementById("tId").textContent = t.id;
    document.getElementById("tStatusBadge").innerHTML = getStatusBadge(t);
    document.getElementById("tDesc").textContent = t.description || "Internal movement request";
    document.getElementById("tType").textContent = getTypeText(t.type);
    document.getElementById("tCustomer").textContent = t.customer?.name || "Internal";
    document.getElementById("tCreator").textContent = t.creator?.username || "System";
    document.getElementById("tCreatedDate").textContent = formatDate(t.creation_date);
    document.getElementById("tExecutedDate").textContent = t.execution_date ? formatDate(t.execution_date) : "Not Yet";
    
    document.getElementById("tOrigin").textContent = t.warehouse_from?.name || "-";
    document.getElementById("tDest").textContent = t.warehouse_to?.name || "-";

    // Approval buttons
    if (t.status === STATUS.PENDING) {
        document.getElementById("approvalSection").classList.remove("d-none");
    }
}

async function displayComponents(t) {
    const tbody = document.getElementById("itemsBody");
    tbody.innerHTML = "";

    const hasFinishedData = t.finished_components && t.finished_components.length > 0;
    const shouldShowRealQty = t.status === STATUS.FINISHED || t.status === STATUS.MISSING_COMPONENTS || hasFinishedData;

    // Toggle column headers
    if (shouldShowRealQty) {
        document.getElementById("binHeader").classList.remove("d-none");
        document.getElementById("actualHeader").classList.remove("d-none");
        document.getElementById("footerLabelColspan").setAttribute("colspan", "6");
    }

    // Map finished items
    const finishedMap = {};
    (t.finished_components || []).forEach(fc => {
        if (!finishedMap[fc.component_id]) {
            finishedMap[fc.component_id] = { qty: 0, bins: new Set() };
        }
        finishedMap[fc.component_id].qty += fc.quantity;
        if (fc.bin?.location_in_warehouse) finishedMap[fc.component_id].bins.add(fc.bin.location_in_warehouse);
    });

    // Fetch and render
    let totalRequested = 0;
    let totalReal = 0;

    for (const c of (t.components || [])) {
        const metadata = await fetchComponentMetadata(c.component_id);
        const finishedInfo = finishedMap[c.component_id] || { qty: 0, bins: new Set() };
        const realQty = finishedInfo.qty;
        const binList = Array.from(finishedInfo.bins).join(", ") || "-";
        
        const price = c.unit_price || 0;
        const extRequested = price * c.quantity;
        const extReal = price * realQty;

        totalRequested += extRequested;
        totalReal += extReal;

        const row = `
            <tr>
                <td>
                    <img src="${metadata.image_url || '/uploads/img/default-component.png'}" 
                         style="width:60px;height:60px;object-fit:contain" 
                         onerror="this.src='/uploads/img/default-component.png'">
                </td>
                <td>
                    <div class="fw-bold">${metadata.name || "Unknown"}</div>
                    <small class="text-muted">ID: ${c.component_id} | ${metadata.manufacturer || "N/A"}</small>
                </td>
                ${shouldShowRealQty ? `<td class="text-center font-monospace small"><span class="badge bg-light text-dark border fs-5 fw-bold">${binList}</span></td>` : ""}
                <td class="text-center fs-5 fw-bold">${c.quantity}</td>
                ${shouldShowRealQty ? `<td class="text-center fs-5 fw-bold ${realQty < c.quantity ? 'text-danger' : 'text-success'}">${realQty}</td>` : ""}
                <td class="text-end text-success fs-5 fw-bold">${formatCurrency(price)}</td>
                <td class="text-end text-success fs-5 fw-bold">${formatCurrency(shouldShowRealQty ? extReal : extRequested)}</td>
            </tr>
        `;
        tbody.insertAdjacentHTML("beforeend", row);
    }

    document.getElementById("grandTotal").textContent = formatCurrency(shouldShowRealQty ? totalReal : totalRequested);
    
    if (t.status === STATUS.MISSING_COMPONENTS) {
        document.getElementById("missingAlert").classList.remove("d-none");
    }
}

async function fetchComponentMetadata(id) {
    try {
        const result = await apiFetch(`/api/manager/get-component/${id}?fullInfo=true`);
        return result.success ? result.data.metadata : {};
    } catch {
        return {};
    }
}

async function updateTransferStatus(decision) {
    const action = decision === STATUS.APPROVED ? 'APPROVE' : 'REJECT';
    if (!confirm(`Are you sure you want to ${action} this request?`)) return;

    try {
        const result = await apiFetch(`/api/manager/transfer-requests/${currentTransferId}/decisions`, {
            method: "POST",
            body: JSON.stringify({ decision: decision })
        });
        if (result.success) {
            alert("Decision recorded successfully");
            location.reload();
        } else {
            alert(result.msg || "Update failed");
        }
    } catch (error) {
        alert("Operation failed: " + error.message);
    }
}

function approveTransfer() { updateTransferStatus(STATUS.APPROVED); }
function rejectTransfer() { updateTransferStatus(STATUS.REJECTED); }

function getStatusBadge(t) {
    const status = t.status;
    switch (status) {
        case STATUS.PENDING: return '<span class="badge text-muted border rounded-pill">Waiting</span>';
        case STATUS.APPROVED: return '<span class="badge bg-info bg-opacity-10 text-info border border-info border-opacity-25 rounded-pill">Processing</span>';
        case STATUS.REJECTED: return '<span class="badge bg-light text-muted border rounded-pill">Rejected</span>';
        case STATUS.FINISHED: return '<span class="badge bg-success bg-opacity-10 text-success border border-success border-opacity-25 rounded-pill">Done</span>';
        case STATUS.MISSING_COMPONENTS: return '<span class="badge bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25 rounded-pill">Missing</span>';
        case STATUS.CANCELED: return '<span class="badge bg-light text-muted border rounded-pill">Canceled</span>';
        default: return '<span class="badge bg-border text-muted rounded-pill">Unknown</span>';
    }
}

function getTypeText(type) {
    if (type === 1) return "Inbound (Import)";
    if (type === 2) return "Outbound (Export)";
    if (type === 3) return "Internal Transfer";
    return "Custom Movement";
}

function showError(msg) {
    const loader = document.getElementById("loader");
    loader.innerHTML = `<div class="alert alert-danger shadow-sm mx-auto" style="max-width: 500px">
        <h5 class="alert-heading">Error</h5>
        <p class="mb-0">${msg}</p>
        <button class="btn btn-sm btn-outline-danger mt-3" onclick="location.reload()">Retry</button>
    </div>`;
}

function goBack() { window.history.back(); }