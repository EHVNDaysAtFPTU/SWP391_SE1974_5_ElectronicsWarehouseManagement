/**
 * Warehouse List JS
 */

document.addEventListener("DOMContentLoaded", function () {
    loadWarehouses();
});

async function loadWarehouses() {
    const tbody = document.getElementById("warehouseTable");
    // Loading state
    tbody.innerHTML = '<tr><td colspan="5" class="text-center py-4"><div class="spinner-border text-primary" role="status"></div></td></tr>';

    try {
        const result = await apiFetch(`/api/manager/get-warehouses?pageNumber=1&pageSize=100`);

        if (!result.success) {
            alert(result.msg || "Load warehouse failed");
            return;
        }

        renderWarehouseTable(result.data.data);
    } catch (error) {
        alert("Load failed: " + error.message);
    }
}

function renderWarehouseTable(warehouses) {
    const tbody = document.getElementById("warehouseTable");
    tbody.innerHTML = "";

    if (!warehouses || warehouses.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center py-5 text-muted">No warehouses found</td></tr>';
        return;
    }

    warehouses.forEach(w => {
        const row = document.createElement("tr");
        row.setAttribute("data-id", w.id);
        row.innerHTML = `
            <td class="align-middle fw-bold">${w.id}</td>
            <td class="align-middle fw-semibold">${w.name}</td>
            <td class="align-middle text-muted small">${w.desc || "No description"}</td>
            <td class="align-middle"><span class="badge bg-light text-dark border rounded-pill px-3">${w.physical_location || "N/A"}</span></td>
            <td class="align-middle text-end pe-4">
                <button class="btn btn-sm btn-outline-primary rounded-pill px-3 me-2" onclick="toggleWarehouseDetail(${JSON.stringify(w).replace(/"/g, '&quot;')}, this)">
                    View Detail
                </button>
                <button class="btn btn-sm btn-primary rounded-pill px-3" onclick="window.location.href='bins.html?warehouseId=${w.id}'">
                    View Bins
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

function toggleWarehouseDetail(w, button) {
    const mainRow = button.closest("tr");
    const nextRow = mainRow.nextElementSibling;

    if (nextRow && nextRow.classList.contains("detail-row")) {
        nextRow.remove();
        button.innerText = "View Detail";
        button.classList.replace("btn-primary", "btn-outline-primary");
        return;
    }

    // Close other detail rows
    document.querySelectorAll(".detail-row").forEach(r => {
        const prevBtn = r.previousElementSibling.querySelector("button:first-child");
        if (prevBtn) {
            prevBtn.innerText = "View Detail";
            prevBtn.classList.replace("btn-primary", "btn-outline-primary");
        }
        r.remove();
    });

    button.innerText = "Close Detail";
    button.classList.replace("btn-outline-primary", "btn-primary");

    const detailRow = document.createElement("tr");
    detailRow.classList.add("detail-row");

    const detailCell = document.createElement("td");
    detailCell.colSpan = 5;
    detailCell.className = "bg-light p-0";

    const imageHtml = (w.image_url && w.image_url.trim() !== "") 
        ? `<img src="${w.image_url}" class="img-fluid rounded-3 shadow-sm border" style="max-height: 250px;">`
        : '<div class="bg-white border rounded d-flex align-items-center justify-content-center text-muted" style="height: 150px; width: 250px;">No image</div>';

    detailCell.innerHTML = `
        <div class="p-4 animate__animated animate__fadeIn">
            <div class="row align-items-center">
                <div class="col-md-4 text-center">
                    ${imageHtml}
                </div>
                <div class="col-md-8">
                    <div class="mb-3">
                        <label class="small text-uppercase text-muted d-block ls-1">Warehouse Name</label>
                        <h5 class="fw-bold mb-0">${w.name}</h5>
                    </div>
                    <div class="mb-3">
                        <label class="small text-uppercase text-muted d-block ls-1">Location</label>
                        <p class="mb-0 fw-semibold text-primary">${w.physical_location || "Not specified"}</p>
                    </div>
                    <div class="mb-0">
                        <label class="small text-uppercase text-muted d-block ls-1">Description</label>
                        <p class="text-muted mb-0">${w.desc || "No description provided for this warehouse."}</p>
                    </div>
                </div>
            </div>
        </div>
    `;

    detailRow.appendChild(detailCell);
    mainRow.parentNode.insertBefore(detailRow, mainRow.nextSibling);
}
