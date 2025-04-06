async function getAccountData(accountId) {
    if (accountId === -1) {
        return;
    }

    let nameBox = document.getElementById("account-data-name");
    let loginBox = document.getElementById("account-data-login");
    let passwordBox = document.getElementById("account-data-password");

    try {
        let response = await send(`api/account/${accountId}`, "POST", null, getCsrfTokenHeader());
        response = await response.json();
        nameBox.value = response.name;
        loginBox.value = response.login;
        passwordBox.type = "password";
        passwordBox.value = response.password;
        document.title = `Password Manager - ${response.name}`;
    } catch (response) {
        if (response.status == 404) {
            location.replace("/account");
        }

        let text = await response.text();
        alert(text);
    }
}

async function postAccountData(accountId) {
    let nameBox = document.getElementById("account-data-name");
    let loginBox = document.getElementById("account-data-login");
    let passwordBox = document.getElementById("account-data-password");

    let data = {
        "Name": nameBox.value,
        "Login": loginBox.value,
        "Password": passwordBox.value,
    }

    if (accountId === -1) {
        await addAccountData(data)
    } else {
        await updateAccountData(accountId, data);
    }
}

async function addAccountData(data) {
    try {
        let reponse = await send("api/account", "POST", data, getCsrfTokenHeader());
        response = await response.json();
        location.replace(`/account?id=${reponse.id}`);
    } catch (response) {
        let text = await response.text()
        alert(text);
    }
}

async function updateAccountData(accountId, data) {
    try {
        await send(`api/account/${accountId}`, "PUT", data, getCsrfTokenHeader());
        location.reload();
    } catch (response) {
        let text = await response.text()
        alert(text);
    }
}
