document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value.trim();

    if (!username || !password) {
        alert("Thiếu thông tin");
        return;
    }

    try {
        const res = await fetch("https://localhost:7297/api/auth/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            credentials: "include", // ⭐ CỰC KỲ QUAN TRỌNG (cookie auth)
            body: JSON.stringify({
                usernameOrEmail: username,
                password: password
            })
        });

        const data = await res.json();

        if (!res.ok) {
            alert(data.message || "Sai tài khoản hoặc mật khẩu");
            return;
        }

        // ✅ LOGIN OK → redirect
        if (username.toLowerCase() === "admin") {
            window.location.href = "/view/admin/index.html";
        } else {
            window.location.href = "/view/storekeeper/index.html";
        }

    } catch (err) {
        console.error(err);
        alert("Không kết nối được server");
    }
});
