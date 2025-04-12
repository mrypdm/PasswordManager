async function logout() {
    await send("/api/logon", "DELETE", null, getCsrfTokenHeader());
    location.replace("/auth/login");
}

async function logon(redirect) {
    let passwordBox = document.getElementById("master-password");
    let data = {
        "MasterPassword": passwordBox.value,
    }

    try {
        await send("/api/logon", "POST", data, getCsrfTokenHeader());
        location.replace(redirect)
    } catch (response) {
        let text = await response.text()
        alert(text);
    }
}
