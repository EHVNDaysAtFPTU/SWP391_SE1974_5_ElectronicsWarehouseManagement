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
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || `Status: ${response.status}`);
        }
        const result = await response.json();
        return result;
    } catch (error) {
        console.error("API Fetch Error:", error);
        throw error;
    }
}
