/**
 * Bin Detail JS
 */

let binId = null;

document.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    binId = params.get("binId");

    if (!binId) {
        showMessage("Bin ID not found in URL", "danger");
        return;
    }

    loadBinDetail();
});

async function loadBinDetail() {
    const infoContainer = document.getElementById("binInfo");
    const listContainer = document.getElementById("componentList");

    // Loading state
    infoContainer.innerHTML = `
        <div class="p-4 text-center">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="mt-2 text-muted small">Loading bin details...</p>
        </div>
    `;
    listContainer.innerHTML = '<tr><td colspan="6"> class="text-center py-4"><div class="spinner-border spinner-border-sm text-secondary"></div></td></tr>';

    try {
        const result = await apiFetch(`/api/manager/get-bin/${binId}?fullInfo=true`);

        if (!result.success) {
            showMessage(result.msg || "Load bin failed", "danger");
            return;
        }

        renderBinInfo(result.data);
        await renderBinComponents(result.data.components);
    } catch (error) {
        showMessage("Error: " + error.message, "danger");
    }
}

function renderBinInfo(bin) {
    const info = document.getElementById("binInfo");
    info.innerHTML = `
        <div class="row g-4 align-items-center">
            <div class="col-md-auto">
                <div class="bg-primary bg-opacity-10 text-primary rounded-circle d-flex align-items-center justify-content-center shadow-sm" style="width: 80px; height: 80px;">
                    <span class="fw-bold fs-3">#${bin.id}</span>
                </div>
            </div>
            <div class="col">
                <div class="row g-3">
                    <div class="col-sm-4">
                        <label class="small text-muted text-uppercase d-block ls-1">Location</label>
                        <span class="fw-bold text-dark">${bin.location_in_warehouse || "Not specified"}</span>
                    </div>
                    <div class="col-sm-4">
                        <label class="small text-muted text-uppercase d-block ls-1">Status</label>
                        ${getBinStatusBadge(bin.status)}
                    </div>
                    <div class="col-sm-4">
                        <label class="small text-muted text-uppercase d-block ls-1">Parent Warehouse</label>
                        <span class="fw-semibold text-primary">${bin.warehouse?.name || "N/A"}</span>
                    </div>
                </div>
            </div>
        </div>
    `;
}

async function renderBinComponents(binComponents) {
    const container = document.getElementById("componentList");
    container.innerHTML = "";

    if (!binComponents || binComponents.length === 0) {
        container.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-5 text-muted">
                    <div class="mb-2" style="font-size: 2rem;">📭</div>
                    No components currently stored in this bin.
                </td>
            </tr>
        `;
        return;
    }

    // Fetch individual component details
    const promises = binComponents.map(bc => 
        apiFetch(`/api/manager/get-component/${bc.component_id}?fullInfo=true`)
            .catch(() => ({ success: false }))
    );

    const results = await Promise.all(promises);

    binComponents.forEach((bc, index) => {
        const result = results[index];
        const row = document.createElement("tr");
        
        if (!result.success) {
            row.innerHTML = `<td>${bc.id}</td><td><span class="text-danger">Failed to load component details</span></td><td>${bc.quantity}</td>`;
            container.appendChild(row);
            return;
        }

        const comp = result.data;
        const meta = comp.metadata || {};
        
        row.innerHTML = `
  <!-- Component -->
  <td class="align-middle py-3">
    <div>
      <h6 class="mb-1 fw-bold">${meta.name || "Unnamed Component"}</h6>
      <p class="small text-muted mb-2">${meta.desc || "No description provided."}</p>
      <span class="badge bg-light text-dark border rounded-pill">
        Mfr: ${meta.manufacturer || "N/A"}
      </span>
      ${meta.datasheet_url
                ? `<a href="${meta.datasheet_url}" target="_blank"
              class="badge bg-info bg-opacity-10 text-info border border-info border-opacity-25 text-decoration-none rounded-pill">
              Datasheet
            </a>`
                : ''
            }
    </div>
  </td>

  <!-- Image -->
  <td class="align-middle">
    ${meta.image_url
                ? `<img src="${meta.image_url}" class="rounded shadow-sm border"
             style="width: 80px; height: 80px; object-fit: cover;">`
                : `<div class="rounded bg-light border d-flex align-items-center justify-content-center text-muted"
             style="width: 80px; height: 80px;">
             <small>No image</small>
           </div>`
            }
  </td>

  <!-- Unit -->
  <td class="align-middle">
    <span class="badge bg-light text-primary border rounded-pill">
      ${comp.unit || "N/A"}
    </span>
  </td>

  <!-- Price -->
  <td class="align-middle">
    <span class="badge bg-light text-primary border rounded-pill">
      ${formatCurrency(comp.unit_price || 0)}
    </span>
  </td>

  <!-- Quantity -->
  <td class="align-middle">
    <span class="badge bg-dark text-white rounded-pill">
      ${bc.quantity || 0}
    </span>
  </td>

  <!-- Total -->
  <td class="align-middle">
    <div class="fw-bold text-success">
      ${formatCurrency((comp.unit_price || 0) * (bc.quantity || 0))}
    </div>
  </td>
`;
        container.appendChild(row);
    });

}

function getBinStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-secondary rounded-pill px-3">Empty</span>';
        case 1: return '<span class="badge bg-success rounded-pill px-3">In Use</span>';
        case 2: return '<span class="badge bg-danger rounded-pill px-3">Disabled</span>';
        default: return `<span class="badge bg-light text-dark rounded-pill px-3">Status ${status}</span>`;
    }
}
function goBackToBins() {
        window.history.back();
}
function showMessage(msg, type = "info") {
    const info = document.getElementById("binInfo");
    info.innerHTML = `<div class="alert alert-${type} shadow-sm m-0" role="alert"><h5 class="alert-heading">Notice</h5><p class="mb-0">${msg}</p></div>`;
}
