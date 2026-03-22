let currentPage = 1;
const pageSize = 5;

document.addEventListener("DOMContentLoaded", () => {
    loadBins();
});

async function loadBins(page = 1) {
    const container = document.getElementById("binTable");

    // loading
    container.innerHTML = `
        <tr>
            <td colspan="5" class="text-center py-4">
                <div class="spinner-border spinner-border-sm"></div>
            </td>
        </tr>
    `;

    try {
        const res = await apiFetch(`/api/manager/get-bins?pageNumber=${page}&pageSize=${pageSize}`);

        if (!res.success) {
            container.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-danger py-4">
                        Failed to load bins
                    </td>
                </tr>
            `;
            return;
        }

        const data = res.data;

        renderBins(data.data);
        renderPagination(data);

        currentPage = data.pageNumber;

    } catch (err) {
        console.error(err);
    }
}

function renderBins(bins) {
    const container = document.getElementById("binTable");
    container.innerHTML = "";

    if (!bins || bins.length === 0) {
        container.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-4 text-muted">
                    No bins found
                </td>
            </tr>
        `;
        return;
    }

    bins.forEach(bin => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td>#${bin.id}</td>
            <td>${bin.location_in_warehouse || "N/A"}</td>
            <td>${getStatusBadge(bin.status)}</td>
            <td>${bin.warehouse_id}</td>
            <td class="text-end">
                <a href="bin-detail.html?binId=${bin.id}" 
                   class="btn btn-sm btn-primary">
                   View
                </a>
            </td>
        `;

        container.appendChild(row);
    });
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

function renderPagination(data) {
    document.getElementById("pageInfo").innerText =
        `Page ${data.pageNumber} / ${data.totalPages}`;

    document.getElementById("prevBtn").disabled = data.pageNumber === 1;
    document.getElementById("nextBtn").disabled = data.pageNumber === data.totalPages;
}

// Events
document.getElementById("prevBtn").onclick = () => {
    if (currentPage > 1) {
        loadBins(currentPage - 1);
    }
};

document.getElementById("nextBtn").onclick = () => {
    loadBins(currentPage + 1);
};