async function getAccountData(accountId) {
    if (accountId === -1) {
        return;
    }

    let nameBox = document.getElementById("account-data-name");
    let loginBox = document.getElementById("account-data-login");
    let passwordBox = document.getElementById("account-data-password");

    try {
        let response = await send(`api/account/${accountId}`, "POST", true);
        nameBox.value = response.name;
        loginBox.value = response.login;
        passwordBox.type = "password";
        passwordBox.value = response.password;
    } catch (e) {
        if (e.message == 404)
        {
            location.replace("/item");
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
        let result = await send_json("api/account", "POST", data, true);
        location.replace(`/item?id=${result.id}`);
    } else {
        await send_json(`api/account/${accountId}`, "PUT", data, false);
        location.reload();
    }
}

async function verifyPassword(passwordBoxId) {
    let passwordBox = document.getElementById(passwordBoxId);
    let data = {
        "Password": passwordBox.value
    };
    let response = await send_json("api/password/verify", "POST", data, true);
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

    let response = await send_json("api/password/generate", "POST", data, true);
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
