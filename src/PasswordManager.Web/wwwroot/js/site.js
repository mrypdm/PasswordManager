function showPassword(id) {
    let passwordBox = document.getElementById(id);
    if (passwordBox.type === "password") {
        passwordBox.type = "text";
    } else {
        passwordBox.type = "password";
    }
}

async function verifyPassword(id) {
    let passwordBox = document.getElementById(id);
    let response = await fetch("api/password/verify", {
        method: "POST",
        body: JSON.stringify({ "password": passwordBox.value }),
        headers: {
            "Content-Type": "application/json",
        },
    });
    let json = await response.json();
    alert(buildCheckStatus(json));
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

    data = {
        "Length": lengthBox.value,
        "UseLowerLetters": useLowerLettersBox.checked,
        "UseUpperLetters": useUpperLettersBox.checked,
        "UseNumbers": useNumbersBox.checked,
        "SpecialSymbols": useCharactersBox.value
    }

    let response = await fetch("api/password/generate", {
        method: "POST",
        body: JSON.stringify(data),
        headers: {
            "Content-Type": "application/json",
        },
    });
    let json = await response.json();

    passwordBox.value = json.password;
    passwordVerifyResult.innerHTML = buildCheckStatus(json.checkStatus)
}

function copyToClipboard(id) {
    let passwordBox = document.getElementById(id);
    passwordBox.select();
    passwordBox.setSelectionRange(0, 99999); // For mobile devices
    navigator.clipboard.writeText(passwordBox.value);
}

function buildCheckStatus(checkStatus) {
    let isCompomised = checkStatus.isCompomised ? " " : " not";
    return`Password is${isCompomised} compomised and password strength is ${checkStatus.strength}`
}
