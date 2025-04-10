async function getSettings() {
    let timeoutBox = document.getElementById("session-timeout");
    let saltBox = document.getElementById("key-salt");
    let iterationsBox = document.getElementById("key-iterations");

    try {
        let response = await send("/api/settings", "GET", null, getCsrfTokenHeader());
        response = await response.json();

        timeoutBox.value = response.sessionTimeout;
        saltBox.value = response.salt;
        iterationsBox.value = response.iterations;
    } catch (response) {
        let text = await response.text();
        alert(text)
    }
}

async function updateSessionTimeout() {
    let timeoutBox = document.getElementById("session-timeout");

    let data = {
        "Timeout": timeoutBox.value
    };

    try {
        await send("/api/settings/session-timeout", "PATCH", data, getCsrfTokenHeader())
    } catch (response) {
        let text = await response.text();
        alert(text)
    }
}

async function updateKeySettings() {
    let passwordBox = document.getElementById("master-password");
    let newPasswordBox = document.getElementById("new-master-password");
    let saltBox = document.getElementById("key-salt");
    let iterationsBox = document.getElementById("key-iterations");

    let data = {
        "MasterPassword": passwordBox.value,
        "NewMasterPassword": newPasswordBox.value == "" ? null : newPasswordBox.value,
        "Salt": saltBox.value == "" ? null : saltBox.value,
        "Iterations": iterationsBox.value == "" ? null : iterationsBox.value
    };

    try {
        await send("/api/settings/key", "PATCH", data, getCsrfTokenHeader())
        location.replace("/auth/login")
    } catch (response) {
        let text = await response.text();
        alert(text)
    }
}

async function deleteStorage() {
    if (!confirm("Are you sure?")) {
        return;
    }

    try {
        await send("/api/settings", "DELETE", null, getCsrfTokenHeader())
        location.replace("/auth/login")
    } catch (response) {
        let text = await response.text();
        alert(text)
    }
}
