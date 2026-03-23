/**
 * Shared functions for Manager pages
 */

function formatCurrency(amount) {
    if (amount === undefined || amount === null) return "0 đ";
    return amount.toLocaleString("vi-VN") + " đ";
}

function formatDate(dateString) {
    if (!dateString) return "-";
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "-";
    return date.toLocaleDateString("vi-VN", {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function toggleProfileMenu() {
    const dropdown = document.getElementById("profileDropdown");
    if (dropdown) dropdown.classList.toggle("show");
}

// Close dropdown when clicking outside
window.addEventListener('click', function(event) {
    if (!event.target.closest('.profile-container')) {
        const dropdown = document.getElementById("profileDropdown");
        if (dropdown) dropdown.classList.remove("show");
    }
});

/**
 * Handle API responses globally
 */
async function apiFetch(url, options = {}) {
    const defaultOptions = {
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json'
        }
    };

    const mergedOptions = { ...defaultOptions, ...options };

    try {
        const response = await fetch(url, mergedOptions);
        const data = await response.json();
        return data;

    } catch (error) {
        console.error("API Fetch Error:", error);
        throw error; 
    }
}

function logout() {
    if (!confirm("Are you sure you want to logout?")) return;

    apiFetch("/api/auth/logout", {
        method: "POST"
    })
        .then(res => {
            if (res.success) {
                // Xóa local data nếu có
                localStorage.clear();
                sessionStorage.clear();

                // Redirect về login
                window.location.href = "login.html";
            } else {
                alert(res.msg || "Logout failed");
            }
        })
        .catch(err => {
            console.error(err);
            alert("Error during logout");
        });
}