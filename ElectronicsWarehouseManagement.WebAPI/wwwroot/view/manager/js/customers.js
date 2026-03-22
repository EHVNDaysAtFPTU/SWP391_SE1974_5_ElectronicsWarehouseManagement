let search = "";
let sortBy = "";
let sortDirection = "asc";
let currentPage = 1;
let pageSize = 10;
let totalPages = 1;

document.addEventListener("DOMContentLoaded", function () {
    // Initial load
    loadCustomers(1);

    // Page size listener
    const pageSizeSelect = document.getElementById("pageSize");
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function () {
            pageSize = parseInt(this.value);
            loadCustomers(1);
        });
    }
});

async function loadCustomers(page) {
    currentPage = page;
    const tableBody = document.getElementById("itemsTable");
    const infoText = document.getElementById("infoText");

    // Show loading state
    tableBody.innerHTML = '<tr><td colspan="7" class="text-center py-4"><div class="spinner-border text-primary" role="status"></div></td></tr>';

    let url = `/api/manager/get-customers?pageNumber=${page}&pageSize=${pageSize}`;

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

    data.data.forEach(customer => {
        const row = document.createElement("tr");
        row.setAttribute("data-id", customer.customer_id);

        row.innerHTML = `
            <td class="align-middle">${customer.customer_id}</td>
            <td class="align-middle  fw-semibold">${customer.customer_name || "N/A"}</td>
            <td class="align-middle fw-bold" style="max-width: 200px;">${customer.phone || ""}</td>
            <td class="align-middle fw-bold text-primary">${customer.email || ""}</td>
            <td class="align-middle fw-bold">${customer.address || ""}</td>
            <td class="align-middle fw-bold">${customer.created_at || ""}</td>
            <td>
        <button class="btn btn-sm btn-warning" onclick='openEdit(${JSON.stringify(customer)})'>
            Edit
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
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="loadCustomers(${currentPage - 1}); return false;">Previous</a>`;
    ul.appendChild(prevLi);

    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement("li");
        li.className = `page-item ${i === currentPage ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="loadCustomers(${i}); return false;">${i}</a>`;
        ul.appendChild(li);
    }

    // Next Button
    const nextLi = document.createElement("li");
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="loadCustomers(${currentPage + 1}); return false;">Next</a>`;
    ul.appendChild(nextLi);

    container.appendChild(ul);
}
async function saveCustomer() {
    const customer = {
        customer_name: customerName.value,
        phone: phone.value,
        email: email.value,
        address: address.value
    };

    try {
        const result = await apiFetch("/api/manager/create-customer", {
            method: "POST",
            body: JSON.stringify(customer)
        });

        if (!result.success) {
            alert(result.msg);
            return;
        }

        alert("Tạo customer thành công!");

        bootstrap.Modal.getInstance(formModal).hide();
        clearForm();
        loadCustomers(1);

    } catch {
        alert("Server lỗi hoặc mất kết nối!");
    }
}
function openEdit(customer) {
    document.getElementById("customerId").value = customer.customer_id;
    document.getElementById("customerName").value = customer.customer_name || "";
    document.getElementById("phone").value = customer.phone || "";
    document.getElementById("email").value = customer.email || "";
    document.getElementById("address").value = customer.address || "";

    document.querySelector("#formModal .modal-title").innerText = "Edit Customer";

    document.querySelector("#formModal .btn-primary")
        .setAttribute("onclick", "updateCustomer()");

    new bootstrap.Modal(document.getElementById("formModal")).show();
}
function openCreate() {
    clearForm();

    document.querySelector("#formModal .modal-title").innerText = "Create Customer";


    document.querySelector("#formModal .btn-primary")
        .setAttribute("onclick", "saveCustomer()");

    new bootstrap.Modal(document.getElementById("formModal")).show();
}
function clearForm() {
    document.getElementById("customerId").value = "";
    document.getElementById("customerName").value = "";
    document.getElementById("phone").value = "";
    document.getElementById("email").value = "";
    document.getElementById("address").value = "";
}
async function updateCustomer() {
    const id = document.getElementById("customerId").value;

    const customer = {
        customer_name: customerName.value,
        phone: phone.value,
        email: email.value,
        address: address.value
    };

    try {
        const result = await apiFetch(`/api/manager/update-customer/${id}`, {
            method: "PUT",
            body: JSON.stringify(customer)
        });

        if (!result.success) {
            alert(result.msg);
            return;
        }

        alert("Cập nhật thành công!");

        bootstrap.Modal.getInstance(formModal).hide();
        clearForm();
        loadCustomers(1);

    } catch {
        alert("Server lỗi hoặc mất kết nối!");
    }
}
function applyFilter() {
    const searchValue = document.getElementById("searchInput").value.trim();
    const sortByValue = document.getElementById("sortBy").value;
    const sortDirectionValue = document.getElementById("sortDirection").value;

    search = searchValue.length > 0 ? searchValue : null;
    sortBy = sortByValue || "id";
    sortDirection = sortDirectionValue || "asc";

    loadCustomers(1);
}
