async function logout() {
    await send("/api/auth", "DELETE", null, getCsrfTokenHeader());
    location.replace("/auth/login");
}

async function login(redirect) {
    let passwordBox = document.getElementById("master-password");
    let data = {
        "MasterPassword": passwordBox.value,
    }

    try {
        await send("/api/auth", "POST", data, getCsrfTokenHeader());
        location.replace(redirect)
    } catch (response) {
        let text = await response.text()
        alert(text);
    }
}
