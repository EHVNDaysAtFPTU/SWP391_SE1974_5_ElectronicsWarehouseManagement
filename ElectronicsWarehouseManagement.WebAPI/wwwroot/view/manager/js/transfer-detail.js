/**
 * Transfer Detail JS
 */

const STATUS = {
    PENDING: 1,
    APPROVED: 2,
    REJECTED: 3,
    CONFIRMED: 4
};

document.addEventListener("DOMContentLoaded", function () {
    const id = new URLSearchParams(window.location.search).get("id");
    if (!id) {
        showMessage("Missing transfer ID in URL", "danger");
        return;
    }
    loadTransferDetail(id);
});

async function loadTransferDetail(id) {
    const infoSection = document.getElementById("infoSection");
    
    // Loading state
    infoSection.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="mt-2 text-muted">Fetching transfer specifics...</p>
        </div>
    `;

    try {
        const result = await apiFetch(`/api/manager/get-transfer/${id}?fullInfo=true`);

        if (!result.success) {
            showMessage(result.msg || "Load failed", "danger");
            return;
        }

        renderTransferInfo(result.data);
        await renderComponents(result.data);
    } catch (error) {
        showMessage("Connection error: " + error.message, "danger");
    }
}

async function renderTransferInfo(t) {
    const container = document.getElementById("infoSection");

    const fromWarehouseName = t.bin_from
        ? await getWarehouseName(t.bin_from.warehouse_id)
        : "-";

    const toWarehouseName = t.bin_to
        ? await getWarehouseName(t.bin_to.warehouse_id)
        : "-";

    container.innerHTML = `
        <div class="row g-4">
            <div class="col-md-7">
                <div class="glass-card p-4 h-100">
                    <div class="d-flex justify-content-between align-items-start mb-4">
                        <div>
                            <small class="text-muted text-uppercase fw-bold ls-1">Transfer ID</small>
                            <h3 class="fw-bold mb-0">#${t.id}</h3>
                        </div>
                        <div class="d-flex gap-2">
                             <button class="btn btn-outline-danger btn-sm rounded-pill px-3 shadow-sm" onclick="exportTransfer(${t.id})"><i class="bi bi-file-pdf"></i> PDF</button>
                             ${getStatusBadge(t.status)}
                        </div>
                    </div>
                    
                    <div class="mb-4">
                        <label class="small text-muted text-uppercase d-block ls-1 mb-1">Description</label>
                        <p class="mb-0 fs-5 fw-medium text-dark">${t.description || "Internal movement request"}</p>
                    </div>

                    <div class="row g-3">
                        <div class="col-6">
                            <label class="small text-muted text-uppercase d-block ls-1">Movement Type</label>
                            <span class="fw-semibold text-primary">${getTypeText(t.type)}</span>
                        </div>
                        <div class="col-6">
                            <label class="small text-muted text-uppercase d-block ls-1">Supplier / Customer</label>
                            <span class="fw-bold text-success">${t.supplier_customer_name || "Internal"}</span>
                        </div>
                        <div class="col-6">
                            <label class="small text-muted text-uppercase d-block ls-1">Creator</label>
                            <span>${t.creator?.username || "System"}</span>
                        </div>
                        <div class="col-6">
                            <label class="small text-muted text-uppercase d-block ls-1">Created Date</label>
                            <span class="text-muted small">${formatDate(t.creation_date)}</span>
                        </div>
                        <div class="col-6">
                            <label class="small text-muted text-uppercase d-block ls-1">Executed Date</label>
                            <span class="text-muted small">${t.execution_date ? formatDate(t.execution_date) : "Pending"}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-md-5">
                <div class="glass-card p-4 h-100">
                    <h6 class="fw-bold text-uppercase ls-1 text-muted mb-4">Logistics Route</h6>
                    
                    <div class="route-step mb-4 ps-3 border-start border-2 border-primary">
                        <label class="small text-muted text-uppercase d-block ls-1">Origin Bin</label>
                        <div class="fw-bold">${t.bin_from ? t.bin_from.location_in_warehouse : "External / New"}</div>
                        <div class="small text-primary">${fromWarehouseName}</div>
                    </div>

                    <div class="route-step ps-3 border-start border-2 border-success">
                        <label class="small text-muted text-uppercase d-block ls-1">Destination Bin</label>
                        <div class="fw-bold">${t.bin_to ? t.bin_to.location_in_warehouse : "External / Out"}</div>
                        <div class="small text-success">${toWarehouseName}</div>
                    </div>

                    ${t.status === STATUS.PENDING ? `
                        <div class="mt-5 pt-3 border-top d-flex gap-2">
                            <button class="btn btn-primary rounded-pill px-4" onclick="updateTransferStatus(${t.id}, ${STATUS.APPROVED})">Approve Movement</button>
                            <button class="btn btn-outline-danger rounded-pill px-4" onclick="updateTransferStatus(${t.id}, ${STATUS.REJECTED})">Reject</button>
                        </div>
                    ` : ""}
                </div>
            </div>
        </div>
    `;
}

async function updateTransferStatus(transferId, decision) {
    if (!confirm(`Are you sure you want to ${decision === STATUS.APPROVED ? 'APPROVE' : 'REJECT'} this request?`)) return;

    try {
        const result = await apiFetch(`/api/manager/transfer-requests/${transferId}/decisions`, {
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

function getStatusBadge(status) {
    switch (status) {
        case STATUS.PENDING: return '<span class="badge bg-warning text-dark rounded-pill px-3 shadow-sm">Pending</span>';
        case STATUS.APPROVED: return '<span class="badge bg-primary rounded-pill px-3 shadow-sm">Approved</span>';
        case STATUS.REJECTED: return '<span class="badge bg-danger rounded-pill px-3 shadow-sm">Rejected</span>';
        case STATUS.CONFIRMED: return '<span class="badge bg-success rounded-pill px-3 shadow-sm">Confirmed</span>';
        default: return '<span class="badge bg-secondary rounded-pill px-3">Unknown</span>';
    }
}

function getTypeText(type) {
    if (type === 1) return "Inbound (Import)";
    if (type === 2) return "Outbound (Export)";
    if (type === 3) return "Internal Transfer";
    return "Custom Movement";
}

async function getWarehouseName(id) {
    if (!id) return "-";
    try {
        const result = await apiFetch(`/api/manager/get-warehouse/${id}?fullInfo=false`);
        return result.success ? result.data?.name || "Warehouse #" + id : "-";
    } catch {
        return "Warehouse #" + id;
    }
}

async function renderComponents(t) {
    const container = document.getElementById("componentSection");
    
    if (t.components && t.components.length > 0) {
        // Initial state for components section
        container.innerHTML = `
            <div class="text-center py-4">
                <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
                <p class="small text-muted mt-2">Loading product specifications...</p>
            </div>
        `;

        // Fetch full info for each component using the get-component API
        const componentFullData = await Promise.all(
            t.components.map(async (c) => {
                try {
                    const result = await apiFetch(`/api/manager/get-component/${c.component_id}?fullInfo=true`);
                    return result.success ? result.data : null;
                } catch (e) {
                    console.error("Failed to fetch component info", e);
                    return null;
                }
            })
        );

        container.innerHTML = `
            <div class="glass-card p-4 mt-4">
                <h6 class="fw-bold text-uppercase ls-1 text-muted mb-4">Transfer Line Items</h6>
                <div class="table-responsive">
                    <table class="table table-hover align-middle">
                        <thead class="table-light">
                            <tr class="small text-muted text-uppercase ls-1">
                                <th style="width: 80px">Product</th>
                                <th>Specification</th>
                                <th class="text-center">Quantity</th>
                                <th class="text-end">Unit Price</th>
                                <th class="text-end">Ext. Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${t.components.map((c, index) => {
                                const fullInfo = componentFullData[index];
                                const metadata = fullInfo?.metadata || {};
                                const name = metadata.name || "Unknown Product";
                                const manufacturer = metadata.manufacturer || "N/A";
                                const imageUrl = metadata.image_url;
                                const unitPrice = c.unit_price || 0;
                                const totalPrice = unitPrice * c.quantity;

                                const imageHtml = imageUrl 
                                    ? `<img src="${imageUrl}" alt="${name}" class="img-fluid rounded" style="object-fit: contain; width: 100%; height: 100%;" onerror="this.style.display='none'">`
                                    : `<div class="d-flex align-items-center justify-content-center h-100 text-muted small bg-light rounded"><i class="bi bi-image"></i></div>`;

                                return `
                                <tr>
                                    <td>
                                        <div class="product-img-wrapper shadow-sm rounded bg-white p-1" style="width: 60px; height: 60px; overflow: hidden;">
                                            ${imageHtml}
                                        </div>
                                    </td>
                                    <td>
                                        <div class="fw-bold text-dark">${name}</div>
                                        <div class="small text-muted">
                                            <span class="badge bg-light text-dark border me-1">ID: #${c.component_id}</span>
                                            <span>MFR: ${manufacturer}</span>
                                        </div>
                                    </td>
                                    <td class="text-center">
                                        <span class="badge bg-primary-soft text-primary rounded-pill px-3 py-2 fw-bold">
                                            ${c.quantity} items
                                        </span>
                                    </td>
                                    <td class="text-end fw-medium text-muted">$${unitPrice.toFixed(2)}</td>
                                    <td class="text-end fw-bold text-primary">$${totalPrice.toFixed(2)}</td>
                                </tr>
                                `;
                            }).join('')}
                        </tbody>
                        <tfoot class="table-light border-top-0">
                            <tr>
                                <td colspan="4" class="text-end fw-bold text-uppercase small ls-1">Total Estimated Value</td>
                                <td class="text-end fw-bold fs-5 text-dark">
                                    $${t.components.reduce((sum, c) => sum + (c.unit_price * c.quantity), 0).toFixed(2)}
                                </td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        `;
    } else {
         container.innerHTML = "";
    }
}

function showMessage(msg, type = "info") {
    const container = document.getElementById("infoSection");
    container.innerHTML = `<div class="alert alert-${type} shadow-sm" role="alert"><h5 class="alert-heading">Notice</h5><p class="mb-0">${msg}</p></div>`;
}

function goBack() {
    window.history.back();
}

function exportTransfer(id) {
    window.open(`/api/manager/export/transfer/${id}`, '_blank');
}
