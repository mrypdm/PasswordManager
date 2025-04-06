async function logout() {
    await send("api/logon", "DELETE");
    location.replace("/login");
}

async function logon(redirect) {
    let passwordBox = document.getElementById("master-password");
    let data = {
        "MasterPassword": passwordBox.value,
    }
    let headers = {
        "X-CSRF-TOKEN": $('input[name="__RequestVerificationToken"]').val()
    }

    try {
        await send("api/logon", "POST", data, headers);
        location.replace(redirect)
    } catch (response) {
        let text = await response.text()
        if (response.status === 401) {
            alert(text);
        }
    }
}

async function getAccountData(accountId) {
    if (accountId === -1) {
        return;
    }

    let nameBox = document.getElementById("account-data-name");
    let loginBox = document.getElementById("account-data-login");
    let passwordBox = document.getElementById("account-data-password");

    try {
        let response = await send(`api/account/${accountId}`, "POST");
        response = await response.json();
        nameBox.value = response.name;
        loginBox.value = response.login;
        passwordBox.type = "password";
        passwordBox.value = response.password;
    } catch (e) {
        if (e.message == 404)
        {
            location.replace("/account");
        }
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
        let result = await send("api/account", "POST", data);
        response = await response.json();
        location.replace(`/account?id=${result.id}`);
    } else {
        await send(`api/account/${accountId}`, "PUT", data);
        location.reload();
    }
}

async function verifyPassword(passwordBoxId) {
    let passwordBox = document.getElementById(passwordBoxId);
    let data = {
        "Password": passwordBox.value
    };
    let response = await send("api/password/verify", "POST", data);
    response = await response.json();
    alert(buildCheckStatus(response));
}

async function generatePassword()
{
    let passwordBox = document.getElementById('password-generator-result');
    let passwordVerifyResult = document.getElementById('password-generator-verify-result');
    let lengthBox = document.getElementById("password-generator-length");
    let useLowerLettersBox = document.getElementById("password-generator-use-lower-letters");
    let useUpperLettersBox = document.getElementById("password-generator-use-upper-letters");
    let useNumbersBox = document.getElementById("password-generator-use-numbers");
    let useCharactersBox = document.getElementById("password-generator-specific-characters");

    let data = {
        "Length": lengthBox.value,
        "UseLowerLetters": useLowerLettersBox.checked,
        "UseUpperLetters": useUpperLettersBox.checked,
        "UseNumbers": useNumbersBox.checked,
        "SpecialSymbols": useCharactersBox.value
    }

    let response = await send("api/password/generate", "POST", data);
    response = await response.json();
    passwordBox.value = response.password;
    passwordVerifyResult.innerHTML = buildCheckStatus(response.checkStatus)
}

function showPassword(passwordBoxId) {
    let passwordBox = document.getElementById(passwordBoxId);
    if (passwordBox.type === "password") {
        passwordBox.type = "text";
    } else {
        passwordBox.type = "password";
    }
}

function copyToClipboard(boxId) {
    let box = document.getElementById(boxId);
    box.select();
    box.setSelectionRange(0, 99999); // For mobile devices
    navigator.clipboard.writeText(box.value);
}

function buildCheckStatus(checkStatus) {
    let isCompomised = checkStatus.isCompomised ? " " : " not";
    return`Password is${isCompomised} compomised and password strength is ${checkStatus.strength}`
}
