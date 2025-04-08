async function getSettings() {
    let timeoutBox = document.getElementById("session-timeout");
    let saltBox = document.getElementById("master-key-salt");
    let iterationsBox = document.getElementById("master-key-iterations");

    try {
        let response = await send("/api/settings", "GET", null, getCsrfTokenHeader());
        response = await response.json();

        timeoutBox.value = response.sessionTimeout;
        saltBox.value = response.masterKeySalt;
        iterationsBox.value = response.masterKeyIterations;
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

async function updateMasterKeySettings() {
    let passwordBox = document.getElementById("master-password");
    let newPasswordBox = document.getElementById("new-master-password");
    let saltBox = document.getElementById("master-key-salt");
    let iterationsBox = document.getElementById("master-key-iterations");

    let data = {
        "MasterPassword": passwordBox.value,
        "NewMasterPassword": newPasswordBox.value == "" ? null : newPasswordBox.value,
        "Salt": saltBox.value == "" ? null : saltBox.value,
        "Iterations": iterationsBox.value == "" ? null : iterationsBox.value
    };

    try {
        await send("/api/settings/master-key", "PATCH", data, getCsrfTokenHeader())
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
    } catch (response) {
        let text = await response.text();
        alert(text)
    }
}
